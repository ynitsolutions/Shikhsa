using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models.Payment;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Shikhsa.Services
{
    //public sealed class ZohoPaymentSettings
    //{
    //    public string AccountId { get; set; } = "";
    //    public string ClientId { get; set; } = "";
    //    public string ClientSecret { get; set; } = "";
    //    public string RefreshToken { get; set; } = "";
    //    public string SigningKey { get; set; } = "";
    //    public string WebhookSigningKey { get; set; } = "";
    //    public string AccountsBaseUrl { get; set; } = "https://accounts.zoho.in";
    //    public string ApiBaseUrl { get; set; } = "https://payments.zoho.in/api/v1";
    //    public string CheckoutBaseUrl { get; set; } = "https://payments.zoho.in";
    //    public string SuccessUrl { get; set; } = "";
    //    public string FailureUrl { get; set; } = "";
    //}

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
        string? RawResponse,
        string? ErrorMessage);

    public sealed class ZohoPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ZohoPaymentService> _logger;
        private static readonly HttpClient Http = new();

        // In-memory token cache — single-instance app ke liye theek hai.
        // Multi-instance hosting me IDistributedCache use karna.
        private static ZohoAccessToken? _cachedToken;
        private static readonly SemaphoreSlim TokenLock = new(1, 1);

        public ZohoPaymentService(ApplicationDbContext context, ILogger<ZohoPaymentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =========================================================
        // DB Settings
        // =========================================================

        public async Task<ZohoPaymentSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
        {
            var settings = await _context.PaymentGatewaySettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.GatewayName == "ZOHO" && x.IsActive, cancellationToken);

            if (settings == null)
                throw new InvalidOperationException("Zoho payment gateway configuration was not found in database.");

            if (string.IsNullOrWhiteSpace(settings.MerchId))
                throw new InvalidOperationException("Zoho AccountId is not configured.");

            if (string.IsNullOrWhiteSpace(settings.RequestEncryptKey))
                throw new InvalidOperationException("Zoho ClientId is not configured.");

            if (string.IsNullOrWhiteSpace(settings.RequestSalt))
                throw new InvalidOperationException("Zoho ClientSecret is not configured.");

            return new ZohoPaymentSettings
            {
                AccountId = settings.MerchId,
                ClientId = settings.RequestEncryptKey,
                ClientSecret = settings.RequestSalt,
                //RefreshToken = settings.ZohoRefreshToken ?? "",
                SigningKey = settings.ResponseDecryptKey ?? "",
                WebhookSigningKey = "",
                AccountsBaseUrl = settings.AuthUrl,
                ApiBaseUrl = settings.AuthUrl,
                CheckoutBaseUrl = settings.AuthUrl,//settings.ZohoCheckoutBaseUrl,
                SuccessUrl = "https://reunion-suing-arise.ngrok-free.dev/Payment/Success",
                FailureUrl = "https://reunion-suing-arise.ngrok-free.dev/Payment/Failure"
            };
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
                        "Zoho RefreshToken DB me set nahi hai — pehle /Settings/ZohoOAuthSetup se one-time setup complete karo.");
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
        // Create Payment Session (Hosted Page)
        // =========================================================

        /* public async Task<ZohoSessionResult> CreatePaymentSessionAsync(decimal amount, string currency, string email, string mobile, string description,string udf1 = "", string udf2 = "", string udf3 = "", string udf4 = "", string udf5 = "",CancellationToken cancellationToken = default)
         {
             if (amount <= 0)
                 throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));

             try
             {
                 var settings = await GetSettingsAsync(cancellationToken);
                 var accessToken = await GetAccessTokenAsync(cancellationToken);

                 var payload = new
                 {
                     amount = amount.ToString("0.00", CultureInfo.InvariantCulture),
                     currency,
                     configurations = new
                     {
                         hosted_checkout_parameters = new
                         {
                             phone = mobile,
                             description,
                             email,
                             success_url = settings.SuccessUrl,
                             failure_url = settings.FailureUrl,
                             udf1,
                             udf2,
                             udf3,
                             udf4,
                             udf5
                         }
                     }
                 };

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
                         $"Zoho payment session API returned HTTP {(int)response.StatusCode}.");
                 }

                 using var doc = JsonDocument.Parse(body);
                 var root = doc.RootElement;

                 if (!root.TryGetProperty("payments_session", out var session))
                     return new ZohoSessionResult(false, null, null, body, "Zoho response did not contain payments_session.");

                 var sessionId = session.TryGetProperty("payments_session_id", out var sid) ? sid.GetString() : null;
                 var accessKey = session.TryGetProperty("access_key", out var ak) ? ak.GetString() : null;

                 if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(accessKey))
                 {
                     return new ZohoSessionResult(false, sessionId, accessKey, body,
                         "Zoho response missing payments_session_id or access_key.");
                 }

                 return new ZohoSessionResult(true, sessionId, accessKey, body, null);
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Unexpected error while creating Zoho payment session.");
                 return new ZohoSessionResult(false, null, null, null, ex.Message);
             }
         }*/
        public async Task<ZohoSessionResult> CreatePaymentSessionAsync(decimal amount,string currency,string email,string mobile,string description,string customerName = "",string phoneCountryCode = "IN",string udf1 = "",string udf2 = "",string udf3 = "",string udf4 = "",string udf5 = "",CancellationToken cancellationToken = default)
        {
            if (amount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));

            try
            {
                var settings = await GetSettingsAsync(cancellationToken);
                var accessToken = await GetAccessTokenAsync(cancellationToken);

                var payload = new
                {
                    amount, // number, NOT string — official spec ke hisaab se fix
                    currency,
                    description,
                    configurations = new
                    {
                        allowed_payment_methods = new[] { "upi", "card" }, // apni zarurat ke hisaab se list adjust karo
                        hosted_checkout_parameters = new
                        {
                            phone_country_code = phoneCountryCode,
                            phone = mobile,
                            name = customerName,
                            email,
                            description,
                            success_url = settings.SuccessUrl,
                            failure_url = settings.FailureUrl,
                            udf1,
                            udf2,
                            udf3,
                            udf4
                        }
                    }
                };

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
                var accessKey = session.TryGetProperty("access_key", out var ak) ? ak.GetString() : null;

                if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(accessKey))
                {
                    return new ZohoSessionResult(false, sessionId, accessKey, body,
                        "Zoho response missing payments_session_id or access_key.");
                }

                return new ZohoSessionResult(true, sessionId, accessKey, body, null);
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
                    return new ZohoSessionStatusResult(false, null, null, null, null, body,
                        $"Zoho retrieve session API returned HTTP {(int)response.StatusCode}.");
                }

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                if (!root.TryGetProperty("payments_session", out var session))
                {
                    return new ZohoSessionStatusResult(false, null, null, null, null, body,
                        "Zoho response did not contain payments_session.");
                }

                // NOTE: real API call karke exact field name confirm karna — docs
                // yahan consistent nahi hain.
                var sessionStatus = session.TryGetProperty("status", out var st) ? st.GetString() : null;
                var paymentId = session.TryGetProperty("payment_id", out var pid) ? pid.GetString() : null;
                var paymentStatus = session.TryGetProperty("payment_status", out var ps) ? ps.GetString() : null;

                decimal? amount = null;
                if (session.TryGetProperty("amount", out var amt) &&
                    decimal.TryParse(amt.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedAmt))
                {
                    amount = parsedAmt;
                }

                return new ZohoSessionStatusResult(true, sessionStatus, paymentId, paymentStatus, amount, body, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving Zoho payment session. SessionId={SessionId}", paymentsSessionId);
                return new ZohoSessionStatusResult(false, null, null, null, null, null, ex.Message);
            }
        }

        // =========================================================
        // Verify Redirect Signature (success_url / failure_url)
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
                Encoding.UTF8.GetBytes(signature.Trim().ToLowerInvariant()));
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
    }
}