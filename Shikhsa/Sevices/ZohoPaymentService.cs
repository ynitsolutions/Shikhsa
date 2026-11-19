using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models.Payment;
using Shikhsa.Repository;
using Shikhsa.ViewModels;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Shikhsa.Services
{
    public sealed record ZohoAccessToken(string AccessToken, DateTime ExpiresOnUtc);

    public sealed record ZohoSessionResult(
        bool Success,
        string? PaymentsSessionId,
        string? AccessKey,
        string? RawResponse,
        string? ErrorMessage);

    public sealed record ZohoSessionStatusResult(
        bool Success,
        string? PaymentSessionStatus,
        string? PaymentId,
        string? PaymentStatus,
        decimal? Amount,
         string? Currency,
        string? RawResponse,
        string? ErrorMessage);

    public sealed record ZohoCompleteResult(bool Success, bool AlreadyProcessed, long? ReceiptId, string Message);
    public sealed record ZohoPaymentDetailsResult(
    bool Success,
    string? PaymentId,
    string? PaymentSessionId,
    string? Status,
    decimal? Amount,
    string? Currency,
    string? RawResponse,
    string? ErrorMessage
);
    // Zoho gateway (token, session, signature) + payment confirm ke baad fee receipt.
    //
    // PaymentTransactionDetail columns (GatewayName = "ZOHO"):
    //   GatewayOrderId     = Zoho payments_session_id   (isi se lookup)
    //   GatewayPaymentId   = Zoho payment_id
    //   ReferenceNo        = Zoho payment_id (ATOM jaisa: gateway reference)
    //   GatewaySignature   = StudentFeeReceiptVM ka JSON snapshot (PaymentController.InitiateFromSession bharta hai)
    //   GatewayResponse    = Zoho raw response
    //   PaymentCompletedOn = atomic "claim" marker (double receipt rokne ke liye)
    //   Receipt ka link    = FeeReceipt.PaymentTransactionId
    public sealed class ZohoPaymentService
    {
        private const string GatewayName = "ZOHO";

        private const string StPending = "Pending";
        private const string StSuccess = "Success";
        private const string StFailed = "Failed";
        private const string StCanceled = "Canceled";

        private readonly ApplicationDbContext _context;
        private readonly ILogger<ZohoPaymentService> _logger;
        private readonly FeeHeadingRepository _feeRepo;
        private static readonly HttpClient Http = new();

        // In-memory token cache — single-instance app ke liye theek hai.
        // Multi-instance hosting me IDistributedCache use karna.
        private static ZohoAccessToken? _cachedToken;
        private static readonly SemaphoreSlim TokenLock = new(1, 1);

        // same purpose string as MastersController (Payment Gateway tab)
        private readonly IDataProtector _pgProtector;

        public ZohoPaymentService(
            ApplicationDbContext context,
            ILogger<ZohoPaymentService> logger,
            IDataProtectionProvider dataProtectionProvider,
            FeeHeadingRepository feeRepo)
        {
            _context = context;
            _logger = logger;
            _feeRepo = feeRepo;
            _pgProtector = dataProtectionProvider.CreateProtector("PaymentGatewaySettingValue");
        }

        // =========================================================
        // DB Settings  (School Master > Payment Gateway tab, key/value rows)
        // =========================================================

        public async Task<ZohoPaymentSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
        {
            var c = await GetGatewayConfigAsync(GatewayName, cancellationToken);

            if (!c.Any())
                throw new InvalidOperationException(
                    "Zoho payment gateway configuration was not found. Add and activate it in School Master > Payment Gateway.");

            string Get(string key, string fallback = "") =>
                c.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : fallback;

            var apiBase = Get("ApiBaseUrl", "https://payments.zoho.in/api/v1").TrimEnd('/');
            var isSandbox = apiBase.Contains("sandbox", StringComparison.OrdinalIgnoreCase);

            var settings = new ZohoPaymentSettings
            {
                AccountId = Get("AccountId"),
                ClientId = Get("ClientId"),
                ClientSecret = Get("ClientSecret"),
                RefreshToken = Get("RefreshToken"),
                SigningKey = Get("SigningKey"),
                WebhookSigningKey = Get("WebhookSigningKey"),
                AccountsBaseUrl = Get("AccountsBaseUrl", "https://accounts.zoho.in").TrimEnd('/'),
                ApiBaseUrl = apiBase,
                // sandbox ApiBaseUrl ho to checkout bhi sandbox host par (CheckoutBaseUrl na bhara ho tab)
                CheckoutBaseUrl = Get("CheckoutBaseUrl",
                    isSandbox ? "https://paymentssandbox.zoho.in" : "https://payments.zoho.in").TrimEnd('/'),
                SuccessUrl = Get("SuccessUrl"),
                FailureUrl = Get("FailureUrl"),
                ApiKey = Get("ApiKey")
            };

            if (string.IsNullOrWhiteSpace(settings.AccountId))
                throw new InvalidOperationException("Zoho AccountId is not configured.");

            if (string.IsNullOrWhiteSpace(settings.ClientId))
                throw new InvalidOperationException("Zoho ClientId is not configured.");

            if (string.IsNullOrWhiteSpace(settings.ClientSecret))
                throw new InvalidOperationException("Zoho ClientSecret is not configured.");

            return settings;
        }

        /// <summary>Decrypted key/value settings of an active gateway (empty if not found).</summary>
        private async Task<Dictionary<string, string>> GetGatewayConfigAsync(
            string gatewayName, CancellationToken cancellationToken = default)
        {
            var g = await _context.PaymentGatewaySettings
                .AsNoTracking()
                .Include(x => x.Values)
                .FirstOrDefaultAsync(x => x.GatewayName == gatewayName && x.IsActive, cancellationToken);

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (g == null) return result;

            foreach (var v in g.Values)
            {
                var value = v.SettingValue ?? string.Empty;

                if (v.IsSecret && value != string.Empty)
                {
                    try
                    {
                        value = _pgProtector.Unprotect(value);
                    }
                    catch (CryptographicException ex)
                    {
                        _logger.LogError(ex, "Zoho setting '{Key}' could not be decrypted.", v.SettingKey);
                        throw new InvalidOperationException(
                            $"Zoho setting '{v.SettingKey}' could not be decrypted. Please re-enter it in School Master > Payment Gateway.");
                    }
                }

                result[v.SettingKey] = value;
            }

            return result;
        }

        /// <summary>
        /// Saves (encrypted) the RefreshToken into the Zoho gateway settings.
        /// Used after the one-time authorization code exchange.
        /// </summary>
        public async Task SaveRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var g = await _context.PaymentGatewaySettings
                .Include(x => x.Values)
                .FirstOrDefaultAsync(x => x.GatewayName == GatewayName && x.IsActive, cancellationToken);

            if (g == null)
                throw new InvalidOperationException("Zoho payment gateway configuration was not found.");

            var row = g.Values.FirstOrDefault(v => v.SettingKey.Equals("RefreshToken", StringComparison.OrdinalIgnoreCase));
            var encrypted = _pgProtector.Protect(refreshToken);

            if (row == null)
            {
                g.Values.Add(new PaymentGatewaySettingValue
                {
                    SettingKey = "RefreshToken",
                    SettingValue = encrypted,
                    IsSecret = true
                });
            }
            else
            {
                row.SettingValue = encrypted;
                row.IsSecret = true;
            }

            g.UpdatedDate = DateTime.UtcNow;
            g.UpdatedBy = "ZohoOAuth";

            await _context.SaveChangesAsync(cancellationToken);

            _cachedToken = null;   // next call uses the new refresh token
        }

        // =========================================================
        // Step 1 (ONE-TIME): Authorization Code → Access + Refresh Token
        // =========================================================

        public async Task<(string AccessToken, string RefreshToken, int ExpiresIn)> ExchangeAuthorizationCodeAsync(
            string authorizationCode, string redirectUri, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(authorizationCode))
                throw new ArgumentException("Authorization code is required.", nameof(authorizationCode));

            if (string.IsNullOrWhiteSpace(redirectUri))
                throw new ArgumentException("Redirect URI is required.", nameof(redirectUri));

            var settings = await GetSettingsAsync(cancellationToken);

            var url =
                $"{settings.AccountsBaseUrl}/oauth/v2/token" +
                $"?code={Uri.EscapeDataString(authorizationCode)}" +
                $"&client_id={Uri.EscapeDataString(settings.ClientId)}" +
                $"&client_secret={Uri.EscapeDataString(settings.ClientSecret)}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                $"&grant_type=authorization_code";

            using var response = await Http.PostAsync(url, content: null, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Zoho code exchange failed. HTTP={StatusCode}, Response={Response}",
                    (int)response.StatusCode, body);

                throw new InvalidOperationException(
                    "Unable to exchange Zoho authorization code. Code expire ho chuka ho sakta hai (valid ~1 min) — fresh code lo.");
            }

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.TryGetProperty("error", out var errorEl))
            {
                throw new InvalidOperationException(
                    $"Zoho returned an OAuth error: {errorEl.GetString()}. Code already used/expired ho sakta hai, ya redirect_uri mismatch hai.");
            }

            var accessToken = root.TryGetProperty("access_token", out var at) ? at.GetString() : null;
            var refreshToken = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
            var expiresIn = root.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 3600;

            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new InvalidOperationException(
                    "Zoho response did not include access_token/refresh_token — authorization URL me access_type=offline confirm karo.");
            }

            // save the refresh token (encrypted) in the DB so GetAccessTokenAsync can use it
            try
            {
                await SaveRefreshTokenAsync(refreshToken, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Zoho refresh token could not be saved to the DB.");
                throw new InvalidOperationException(
                    "Zoho refresh token mil gaya, par DB me save nahi ho saka: " + ex.Message);
            }

            _cachedToken = new ZohoAccessToken(accessToken, DateTime.UtcNow.AddSeconds(expiresIn));

            return (accessToken, refreshToken, expiresIn);
        }

        // =========================================================
        // Step 4 (RECURRING): Access Token via Refresh Token, cached
        // =========================================================

        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            if (_cachedToken != null && _cachedToken.ExpiresOnUtc > DateTime.UtcNow.AddMinutes(2))
                return _cachedToken.AccessToken;

            await TokenLock.WaitAsync(cancellationToken);
            try
            {
                if (_cachedToken != null && _cachedToken.ExpiresOnUtc > DateTime.UtcNow.AddMinutes(2))
                    return _cachedToken.AccessToken;

                var settings = await GetSettingsAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(settings.RefreshToken))
                {
                    throw new InvalidOperationException(
                        "Zoho RefreshToken DB me set nahi hai — pehle /Payment/ZohoOAuthSetup se one-time setup complete karo.");
                }

                var baseUrl = settings.AccountsBaseUrl.TrimEnd('/');
                var url =
                    $"{baseUrl}/oauth/v2/token" +
                    $"?refresh_token={Uri.EscapeDataString(settings.RefreshToken)}" +
                    $"&client_id={Uri.EscapeDataString(settings.ClientId)}" +
                    $"&client_secret={Uri.EscapeDataString(settings.ClientSecret)}" +
                    $"&grant_type=refresh_token";

                using var response = await Http.PostAsync(url, content: null, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Zoho token refresh failed. HTTP={StatusCode}, Response={Response}",
                        (int)response.StatusCode, body);

                    throw new InvalidOperationException("Unable to refresh Zoho access token.");
                }

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                // Zoho kabhi HTTP 200 ke saath {"error": "..."} bhejta hai
                if (root.TryGetProperty("error", out var errEl))
                {
                    _logger.LogError("Zoho token refresh error: {Error}", errEl.GetString());
                    throw new InvalidOperationException($"Zoho token refresh error: {errEl.GetString()}");
                }

                var accessToken = root.GetProperty("access_token").GetString()
                    ?? throw new InvalidOperationException("Zoho token response missing access_token.");

                var expiresIn = root.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 3600;

                _cachedToken = new ZohoAccessToken(accessToken, DateTime.UtcNow.AddSeconds(expiresIn));

                return accessToken;
            }
            finally
            {
                TokenLock.Release();
            }
        }

        // =========================================================
        // Create Payment Session (Hosted Checkout)
        // =========================================================

        //public async Task<ZohoSessionResult> CreatePaymentSessionAsync(decimal amount, string currency, string email, string mobile, string description, string customerName = "", string phoneCountryCode = "IN", string udf1 = "", string udf2 = "", string udf3 = "", string udf4 = "", string udf5 = "", CancellationToken cancellationToken = default)
        //{
        //    if (amount <= 0)
        //        throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));

        //    try
        //    {
        //        var settings = await GetSettingsAsync(cancellationToken);

        //        if (string.IsNullOrWhiteSpace(settings.SuccessUrl) || string.IsNullOrWhiteSpace(settings.FailureUrl))
        //        {
        //            return new ZohoSessionResult(false, null, null, null,
        //                "Zoho SuccessUrl / FailureUrl configure nahi hai. School Master > Payment Gateway me bharo.");
        //        }

        //        var accessToken = await GetAccessTokenAsync(cancellationToken);

        //        // Zoho docs ke hisaab se: success_url, failure_url, description zaroori; baaki optional.
        //        // Khali/galat values bhejne se API 400 de sakti hai, isliye sirf valid values jaati hain.
        //        var hosted = new Dictionary<string, string>
        //        {
        //            ["description"] = description,
        //            ["success_url"] = settings.SuccessUrl,
        //            ["failure_url"] = settings.FailureUrl
        //        };

        //        var digits = new string((mobile ?? "").Where(char.IsDigit).ToArray());
        //        if (digits.Length > 10) digits = digits[^10..];
        //        if (digits.Length == 10) hosted["phone"] = digits;

        //        if (!string.IsNullOrWhiteSpace(email) && email.Contains('@'))
        //            hosted["email"] = email.Trim();

        //        void AddUdf(string key, string? value)
        //        {
        //            if (!string.IsNullOrWhiteSpace(value)) hosted[key] = value;
        //        }
        //        AddUdf("udf1", udf1);
        //        AddUdf("udf2", udf2);
        //        AddUdf("udf3", udf3);
        //        AddUdf("udf4", udf4);
        //        AddUdf("udf5", udf5);

        //        var payload = new
        //        {
        //            amount, // number, string nahi
        //            currency,
        //            description,
        //            configurations = new
        //            {
        //                allowed_payment_methods = new[] { "upi", "card" },
        //                hosted_checkout_parameters = hosted
        //            }
        //        };

        //        var json = JsonSerializer.Serialize(payload);

        //        var url = $"{settings.ApiBaseUrl}/paymentsessions?account_id={Uri.EscapeDataString(settings.AccountId)}";

        //        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        //        request.Headers.Add("Authorization", $"Zoho-oauthtoken {accessToken}");
        //        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        //        using var response = await Http.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
        //        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            _logger.LogWarning("Zoho create payment session failed. HTTP={StatusCode}, Response={Response}",
        //                (int)response.StatusCode, body);

        //            return new ZohoSessionResult(false, null, null, body,
        //                $"Zoho payment session API returned HTTP {(int)response.StatusCode}. {body}");
        //        }

        //        using var doc = JsonDocument.Parse(body);
        //        var root = doc.RootElement;

        //        if (!root.TryGetProperty("payments_session", out var session))
        //            return new ZohoSessionResult(false, null, null, body, "Zoho response did not contain payments_session.");

        //        var sessionId = session.TryGetProperty("payments_session_id", out var sid) ? sid.GetString() : null;
        //        var accessKey = session.TryGetProperty("access_key", out var ak) ? ak.GetString() : null;

        //        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(accessKey))
        //        {
        //            // Docs me 2 naam milte hain: hosted_checkout_parameters (guide) aur hosted_page_parameters (API reference).
        //            // access_key na aaye to payload me naam hosted_page_parameters karke try karein.
        //            return new ZohoSessionResult(false, sessionId, accessKey, body,
        //                "Zoho response missing payments_session_id or access_key.");
        //        }

        //        return new ZohoSessionResult(true, sessionId, accessKey, body, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Unexpected error while creating Zoho payment session.");
        //        return new ZohoSessionResult(false, null, null, null, ex.Message);
        //    }
        //}
        public async Task<ZohoSessionResult> CreatePaymentSessionAsync(decimal amount,string currency,string email,string mobile,string description,string referenceNumber = "",string invoiceNumber = "",string udf1 = "",string udf2 = "",string udf3 = "",string udf4 = "",string udf5 = "",CancellationToken cancellationToken = default)
        {
            if (amount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));

            try
            {
                var settings = await GetSettingsAsync(cancellationToken);
                var accessToken = await GetAccessTokenAsync(cancellationToken);

                // Zoho Checkout Widget uses 'meta_data' (max 5 key-value pairs) instead of hosted_checkout_parameters
                var metaData = new List<Dictionary<string, string>>();
                void AddMeta(string key, string? value)
                {
                    if (!string.IsNullOrWhiteSpace(value) && metaData.Count < 5)
                    {
                        metaData.Add(new Dictionary<string, string>
                        {
                            ["key"] = key,
                            ["value"] = value.Trim()
                        });
                    }
                }

                AddMeta("udf1", udf1);
                AddMeta("udf2", udf2);
                AddMeta("udf3", udf3);
                AddMeta("udf4", udf4);
                AddMeta("udf5", udf5);

                // Build clean payload for Checkout Widget (no hosted_checkout_parameters)
                var payload = new Dictionary<string, object>
                {
                    ["amount"] = amount,
                    ["currency"] = currency,
                    ["description"] = description
                };

                if (!string.IsNullOrWhiteSpace(referenceNumber))
                    payload["reference_number"] = referenceNumber.Trim();

                if (!string.IsNullOrWhiteSpace(invoiceNumber))
                    payload["invoice_number"] = invoiceNumber.Trim();

                if (metaData.Count > 0)
                    payload["meta_data"] = metaData;

                var json = JsonSerializer.Serialize(payload);
                var url = $"{settings.ApiBaseUrl}/paymentsessions?account_id={Uri.EscapeDataString(settings.AccountId)}";

                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Authorization", $"Zoho-oauthtoken {accessToken}");
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await Http.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Zoho create payment session failed. HTTP={StatusCode}, Response={Response}",
                        (int)response.StatusCode, body);

                    return new ZohoSessionResult(false, null, null, body,
                        $"Zoho payment session API returned HTTP {(int)response.StatusCode}. {body}");
                }

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                if (!root.TryGetProperty("payments_session", out var session))
                    return new ZohoSessionResult(false, null, null, body, "Zoho response did not contain payments_session.");

                var sessionId = session.TryGetProperty("payments_session_id", out var sid) ? sid.GetString() : null;

                // Checkout Widget only requires payments_session_id (access_key is not returned/needed)
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return new ZohoSessionResult(false, null, null, body,
                        "Zoho response missing payments_session_id.");
                }

                return new ZohoSessionResult(true, sessionId, null, body, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating Zoho payment session.");
                return new ZohoSessionResult(false, null, null, null, ex.Message);
            }
        }


        // =========================================================
        // Retrieve Payment Session (status check / reconciliation)
        // =========================================================

        public async Task<ZohoSessionStatusResult> RetrievePaymentSessionAsync(string paymentsSessionId, CancellationToken cancellationToken = default)
        {
            try
            {
                var settings = await GetSettingsAsync(cancellationToken);
                var accessToken = await GetAccessTokenAsync(cancellationToken);

                var url = $"{settings.ApiBaseUrl}/paymentsessions/{Uri.EscapeDataString(paymentsSessionId)}" +
                          $"?account_id={Uri.EscapeDataString(settings.AccountId)}";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Zoho-oauthtoken {accessToken}");

                using var response = await Http.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return new ZohoSessionStatusResult(false, null, null, null, null,null,body,
                        $"Zoho retrieve session API returned HTTP {(int)response.StatusCode}.");
                }

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                if (!root.TryGetProperty("payments_session", out var session))
                {
                    return new ZohoSessionStatusResult(false, null, null, null, null,null,body,
                        "Zoho response did not contain payments_session.");
                }

                static string? Str(JsonElement e, string name) =>
                    e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var v)
                        ? (v.ValueKind == JsonValueKind.String ? v.GetString()
                           : v.ValueKind == JsonValueKind.Null ? null : v.GetRawText())
                        : null;

                // Zoho docs: payments_session.status + payments[] { payment_id, status }
                var sessionStatus = Str(session, "status");
                string? paymentId = null, paymentStatus = null;

                if (session.TryGetProperty("payments", out var pays) && pays.ValueKind == JsonValueKind.Array)
                {
                    foreach (var p in pays.EnumerateArray())
                    {
                        var ps = Str(p, "status");
                        paymentId = Str(p, "payment_id");
                        paymentStatus = ps;
                        if (string.Equals(ps, "succeeded", StringComparison.OrdinalIgnoreCase))
                            break;   // succeeded attempt mil gaya; warna aakhri attempt
                    }
                }

                paymentId ??= Str(session, "payment_id");
                paymentStatus ??= Str(session, "payment_status");
                var currency = Str(session, "currency");
                decimal? amount = null;
                var amountText = Str(session, "amount");
                if (decimal.TryParse(amountText,NumberStyles.Any,CultureInfo.InvariantCulture,out var parsedAmount))
                {
                    amount = parsedAmount;
                }
                //if (session.TryGetProperty("amount", out var amt) &&
                //    decimal.TryParse(amt.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedAmt))
                //{
                //    amount = parsedAmt;
                //}

                return new ZohoSessionStatusResult(true, sessionStatus, paymentId, paymentStatus, amount, currency,body, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving Zoho payment session. SessionId={SessionId}", paymentsSessionId);
                return new ZohoSessionStatusResult(false, null, null, null, null, null,null, ex.Message);
            }
        }

        // =========================================================
        // Verify Redirect Signature (success_url / failure_url)
        // message = payments_session_id.payment_session_status.payment_id.payment_status.amount.mandate_id.udf1..udf5
        // =========================================================

        public async Task<bool> VerifyRedirectSignatureAsync(
            string paymentsSessionId, string paymentSessionStatus, string? paymentId, string? paymentStatus,
            string? amount, string? mandateId, string? udf1, string? udf2, string? udf3, string? udf4, string? udf5,
            string signature, CancellationToken cancellationToken = default)
        {
            var settings = await GetSettingsAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(settings.SigningKey))
            {
                _logger.LogWarning("Zoho SigningKey not configured — cannot verify redirect signature.");
                return false;
            }

            var message = string.Join(".", new[]
            {
                paymentsSessionId, paymentSessionStatus, paymentId ?? "", paymentStatus ?? "",
                amount ?? "", mandateId ?? "", udf1 ?? "", udf2 ?? "", udf3 ?? "", udf4 ?? "", udf5 ?? ""
            });

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(settings.SigningKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            var computed = Convert.ToHexString(hash).ToLowerInvariant();

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computed),
                Encoding.UTF8.GetBytes((signature ?? "").Trim().ToLowerInvariant()));
        }

        // =========================================================
        // Verify Webhook Signature (X-Zoho-Webhook-Signature)
        // =========================================================

        public async Task<bool> VerifyWebhookSignatureAsync(
            string signatureHeader, string rawPayload, CancellationToken cancellationToken = default)
        {
            var settings = await GetSettingsAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(settings.WebhookSigningKey))
            {
                _logger.LogWarning("Zoho WebhookSigningKey not configured — cannot verify webhook.");
                return false;
            }

            string? t = null, v = null;

            foreach (var part in signatureHeader.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = part.Split('=', 2);
                if (kv.Length != 2) continue;

                if (kv[0].Trim().Equals("t", StringComparison.OrdinalIgnoreCase)) t = kv[1].Trim();
                if (kv[0].Trim().Equals("v", StringComparison.OrdinalIgnoreCase)) v = kv[1].Trim();
            }

            if (string.IsNullOrWhiteSpace(t) || string.IsNullOrWhiteSpace(v))
            {
                _logger.LogWarning("Zoho webhook signature header missing t or v.");
                return false;
            }

            var dataToSign = $"{t}.{rawPayload}";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(settings.WebhookSigningKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
            var computed = Convert.ToHexString(hash).ToLowerInvariant();

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computed),
                Encoding.UTF8.GetBytes(v.ToLowerInvariant()));
        }

        // =========================================================
        // FEE PAYMENT: Complete — payment confirm hone par SaveCashFeeReceipt
        // (success redirect aur webhook dono yahi call karte hain; kitni bhi baar call ho, receipt ek hi banegi)
        // =========================================================

        //public async Task<ZohoCompleteResult> CompleteAsync(string paymentsSessionId, CancellationToken cancellationToken = default)
        public Task<ZohoCompleteResult> CompleteAsync(string paymentsSessionId,CancellationToken cancellationToken = default)
        {
            return CompleteAsync(paymentsSessionId, null, cancellationToken);
        }

        public async Task<ZohoCompleteResult> CompleteAsync(string paymentsSessionId,string? expectedPaymentId = null,CancellationToken cancellationToken = default)
        {
            var ct = cancellationToken;

            var txn = await _context.PaymentTransactionDetail.AsNoTracking()
                .FirstOrDefaultAsync(x => x.GatewayName == GatewayName && x.GatewayOrderId == paymentsSessionId, ct);

            if (txn == null)
                return new ZohoCompleteResult(false, false, null, "Unknown payment session.");

            if (txn.TransactionStatus == StSuccess)
                return new ZohoCompleteResult(true, true, await GetReceiptIdAsync(txn.PaymentTransactionId, ct), "Already processed.");

            // 1) Zoho se server-side confirm (redirect/webhook par akele bharosa nahi)
            var s = await RetrievePaymentSessionAsync(paymentsSessionId, ct);
            if (!s.Success)
                return new ZohoCompleteResult(false, false, null, s.ErrorMessage ?? "Zoho status check failed.");

            if (!IsPaid(s))
            {
                if (IsTerminalFailure(s) && txn.TransactionStatus == StPending)
                {
                    await _context.PaymentTransactionDetail.Where(x => x.PaymentTransactionId == txn.PaymentTransactionId)
                        .ExecuteUpdateAsync(u => u
                            .SetProperty(x => x.TransactionStatus, StFailed)
                            .SetProperty(x => x.TransactionError, "Zoho payment failed/expired")
                            .SetProperty(x => x.GatewayPaymentId, s.PaymentId)
                            .SetProperty(x => x.GatewayResponse, s.RawResponse)
                            .SetProperty(x => x.UpdatedDate, DateTime.Now)
                            .SetProperty(x => x.UpdatedBy, "ZohoGateway"), ct);
                }
                return new ZohoCompleteResult(false, false, null, "Payment successful nahi hai.");
            }

            // 2) Amount DB wale amount se match hona chahiye
            if (s.Amount.HasValue && s.Amount.Value != txn.Amount)
            {
                _logger.LogError("Zoho amount mismatch. Session={S} Zoho={Z} DB={D}", paymentsSessionId, s.Amount, txn.Amount);
                await _context.PaymentTransactionDetail.Where(x => x.PaymentTransactionId == txn.PaymentTransactionId)
                    .ExecuteUpdateAsync(u => u
                        .SetProperty(x => x.TransactionError, $"AMOUNT_MISMATCH zoho={s.Amount} db={txn.Amount}")
                        .SetProperty(x => x.GatewayPaymentId, s.PaymentId)
                        .SetProperty(x => x.GatewayResponse, s.RawResponse)
                        .SetProperty(x => x.UpdatedDate, DateTime.Now), ct);
                return new ZohoCompleteResult(false, false, null, "Zoho payment amount missing or mismatched.");
            }

            // 3) Atomic claim: redirect + webhook ek saath aaye to sirf ek hi receipt banegi
            var now = DateTime.Now;
            var claimed = await _context.PaymentTransactionDetail
                .Where(x => x.PaymentTransactionId == txn.PaymentTransactionId
                            && x.TransactionStatus == StPending
                            && x.PaymentCompletedOn == null)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.PaymentCompletedOn, now)
                    .SetProperty(x => x.GatewayPaymentId, s.PaymentId)
                    .SetProperty(x => x.GatewayResponse, s.RawResponse), ct);

            if (claimed == 0)
            {
                var latest = await _context.PaymentTransactionDetail.AsNoTracking()
                    .FirstAsync(x => x.PaymentTransactionId == txn.PaymentTransactionId, ct);

                var rid = await GetReceiptIdAsync(latest.PaymentTransactionId, ct);
                var done = latest.TransactionStatus == StSuccess;

                return new ZohoCompleteResult(done, true, rid,
                    done ? "Already processed." : "The payment is being processed. Please check for the receipt after a while.");
            }

            // 4) Receipt save (fee snapshot GatewaySignature se)
            long receiptId;
            try
            {
                var vm = BuildReceiptVm(txn);

                receiptId = _feeRepo.SaveCashFeeReceipt(vm);

                if (receiptId <= 0)
                    throw new InvalidOperationException($"SaveCashFeeReceipt returned {receiptId}.");
            }
            catch (Exception ex)
            {
                // Paisa kat chuka hai, receipt nahi bani: claim hatao taaki retry ho sake
                _logger.LogError(ex, "Receipt save failed after Zoho payment success. Session={S}", paymentsSessionId);
                await _context.PaymentTransactionDetail.Where(x => x.PaymentTransactionId == txn.PaymentTransactionId)
                    .ExecuteUpdateAsync(u => u
                        .SetProperty(x => x.PaymentCompletedOn, (DateTime?)null)
                        .SetProperty(x => x.TransactionError, "PAID_RECEIPT_PENDING: " + Trunc(ex.Message, 400))
                        .SetProperty(x => x.UpdatedDate, DateTime.Now), ct);

                return new ZohoCompleteResult(false, false, null,
                    "Payment has been received, but the receipt was not saved. Please inform the admin.");
            }

            // Receipt ban chuki hai: ab yahan koi error claim reset nahi karega (warna duplicate receipt banti)
            try
            {
                await _context.FeeReceipt.Where(x => x.FeeReceiptId == receiptId)
                    .ExecuteUpdateAsync(u => u.SetProperty(x => x.PaymentTransactionId, txn.PaymentTransactionId), ct);

                await _context.PaymentTransactionDetail.Where(x => x.PaymentTransactionId == txn.PaymentTransactionId)
                    .ExecuteUpdateAsync(u => u
                        .SetProperty(x => x.TransactionStatus, StSuccess)
                        .SetProperty(x => x.TransactionError, (string?)null)
                        .SetProperty(x => x.ReferenceNo, s.PaymentId)
                        .SetProperty(x => x.UpdatedDate, DateTime.Now)
                        .SetProperty(x => x.UpdatedBy, "ZohoGateway"), ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Receipt {R} saved but transaction update/link failed. Session={S}", receiptId, paymentsSessionId);
            }

            return new ZohoCompleteResult(true, false, receiptId, "Fee receipt saved.");
        }

        // =========================================================
        // Reconcile — atke hue Zoho rows retry (optional: background job / admin button)
        // =========================================================

        public async Task<int> RetryPendingAsync(CancellationToken cancellationToken = default)
        {
            var cutoff = DateTime.Now.AddMinutes(-5);
            var sessionIds = await _context.PaymentTransactionDetail.AsNoTracking()
                .Where(x => x.GatewayName == GatewayName
                            && x.TransactionStatus == StPending
                            && x.GatewayOrderId != null
                            && x.TxnDate < cutoff
                            && (x.TransactionError == null || !x.TransactionError.StartsWith("AMOUNT_MISMATCH")))
                .Select(x => x.GatewayOrderId!)
                .Take(50)
                .ToListAsync(cancellationToken);

            int done = 0;
            foreach (var id in sessionIds)
            {
                var r = await CompleteAsync(id, cancellationToken);
                if (r.Success && !r.AlreadyProcessed) done++;
            }
            return done;
        }

        // =========================================================
        // Helpers
        // =========================================================

        // InitiateFromSession me session ka StudentFeeReceiptVM JSON GatewaySignature me save hota hai
        // (ATOM ka Callback bhi session se wahi VM deserialize karke SaveCashFeeReceipt chalata hai).
        private static StudentFeeReceiptVM BuildReceiptVm(
     PaymentTransactionDetail txn)
        {
            if (string.IsNullOrWhiteSpace(txn.GatewaySignature))
            {
                throw new InvalidOperationException(
                    $"Fee snapshot missing. TransactionId: {txn.PaymentTransactionId}, " +
                    $"Gateway: {txn.GatewayName}");
            }

            try
            {
                return JsonSerializer.Deserialize<StudentFeeReceiptVM>(
                           txn.GatewaySignature)
                       ?? throw new InvalidOperationException(
                           "Fee snapshot deserialization returned null.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Invalid fee snapshot for transaction {txn.PaymentTransactionId}.",
                    ex);
            }
        }

        private Task<long?> GetReceiptIdAsync(long paymentTransactionId, CancellationToken ct) =>
            _context.FeeReceipt.AsNoTracking()
                .Where(x => x.PaymentTransactionId == paymentTransactionId)
                .Select(x => (long?)x.FeeReceiptId)
                .FirstOrDefaultAsync(ct);

        // Zoho: session status / payment status "succeeded" | "failed" | "in_progress" (bank transfer)

        /* private static readonly string[] PaidValues = { "succeeded" };
         private static readonly string[] FailedValues = { "failed", "expired", "cancelled", "canceled" };

         private static bool IsPaid(ZohoSessionStatusResult s) =>
             Matches(s.PaymentStatus, PaidValues) || Matches(s.PaymentSessionStatus, PaidValues);

         private static bool IsTerminalFailure(ZohoSessionStatusResult s) =>
             Matches(s.PaymentSessionStatus, FailedValues);

         private static bool Matches(string? value, string[] set) =>
             !string.IsNullOrWhiteSpace(value) && set.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);

         private static string? Trunc(string? v, int max) =>
             v == null ? null : (v.Length <= max ? v : v[..max]);*/
        private static readonly string[] PaidValues =
        {
    "succeeded"
};

        private static readonly string[] FailedValues =
        {
    "failed",
    "blocked"
};

        private static readonly string[] CanceledValues =
        {
    "canceled",
    "cancelled"
};


        //private static bool IsPaid(ZohoSessionStatusResult s)
        //{
        //    return Matches(s.PaymentStatus, PaidValues);
        //}
        private static bool IsPaid(ZohoSessionStatusResult s)
        {
            return !string.IsNullOrWhiteSpace(s.PaymentStatus)
                && PaidValues.Contains(
                    s.PaymentStatus.Trim(),
                    StringComparer.OrdinalIgnoreCase);
        }
        private static bool IsTerminalFailure(ZohoSessionStatusResult s)
        {
            return Matches(s.PaymentStatus, FailedValues);
        }

        private static bool IsCanceled(ZohoSessionStatusResult s)
        {
            return Matches(s.PaymentStatus, CanceledValues);
        }

        private static bool Matches(string? value, string[] set)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   set.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);
        }
        private static string? Trunc(string? v, int max) =>
          v == null ? null : (v.Length <= max ? v : v[..max]);
        //private static bool IsPaid(string? status) =>
        //    Matches(status, PaidValues);

        //private static bool IsFailed(string? status) =>
        //    Matches(status, FailedValues);

        //private static bool IsCanceled(string? status) =>
        //    Matches(status, CanceledValues);

        //private static bool Matches(string? value, string[] set) =>
        //    !string.IsNullOrWhiteSpace(value) &&
        //    set.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);
        //private static string? Trunc(string? v, int max) =>
        //     v == null ? null : (v.Length <= max ? v : v[..max]);
        public async Task<ZohoPaymentDetailsResult> RetrievePaymentAsync(string paymentId,CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(paymentId))
                {
                    return new ZohoPaymentDetailsResult(false, null, null, null, null, null, null, "Zoho PaymentId is missing.");
                }

                var settings = await GetSettingsAsync(cancellationToken);
                var accessToken = await GetAccessTokenAsync(cancellationToken);

                var url =
                    $"{settings.ApiBaseUrl}/payments/{Uri.EscapeDataString(paymentId)}" +
                    $"?account_id={Uri.EscapeDataString(settings.AccountId)}";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Zoho-oauthtoken {accessToken}");

                using var response = await Http.SendAsync(
                    request,
                    HttpCompletionOption.ResponseContentRead,
                    cancellationToken);

                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Zoho Retrieve Payment failed. HTTP={StatusCode}, PaymentId={PaymentId}, Response={Response}", (int)response.StatusCode, paymentId, body);

                    return new ZohoPaymentDetailsResult(
                        false, null, null, null, null, null, body,$"Zoho payment verification API returned HTTP {(int)response.StatusCode}.");
                }

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                if (!root.TryGetProperty("payment", out var payment))
                {
                    return new ZohoPaymentDetailsResult(false, null, null, null, null, null,body,"Zoho response did not contain payment.");
                }

                static string? Str(JsonElement e, string name)
                {
                    if (e.ValueKind != JsonValueKind.Object ||
                        !e.TryGetProperty(name, out var value))
                        return null;

                    return value.ValueKind switch
                    {
                        JsonValueKind.String => value.GetString(),
                        JsonValueKind.Null => null,
                        _ => value.GetRawText()
                    };
                }

                var amountText = Str(payment, "amount");

                decimal? amount = null;

                if (decimal.TryParse(
                    amountText,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var parsedAmount))
                {
                    amount = parsedAmount;
                }

                return new ZohoPaymentDetailsResult(true,Str(payment, "payment_id"),Str(payment, "payments_session_id"),Str(payment, "status"),amount,Str(payment, "currency"),body,null);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Zoho Retrieve Payment exception. PaymentId={PaymentId}",
                    paymentId);

                return new ZohoPaymentDetailsResult(false, null, null, null, null, null, null, "Unable to verify payment directly with Zoho.");
            }
        }
    }
}