//using System.Globalization;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;
//using Microsoft.Extensions.Options;
//using Shikhsa.Models.Payment;

//namespace Shikhsa.Services
//{
//    public sealed record PaymentInitiationResult(
//        bool Success,
//        string? RedirectUrl,
//        string? ErrorMessage,
//        long? PaymentTransactionId);

//    public sealed record PaymentCallbackResult(
//        bool Success,
//        string Message,
//        long? PaymentTransactionId,
//        string? ReferenceNo,
//        decimal Amount,
//        long? ReceiptId);
//}
//public sealed record ZohoAccessToken(string AccessToken, DateTime ExpiresOnUtc);

//    public sealed record ZohoSessionResult(
//        bool Success,
//        string? PaymentsSessionId,
//        string? AccessKey,
//        string? RawResponse,
//        string? ErrorMessage);

//    public sealed record ZohoSessionStatusResult(
//        bool Success,
//        string? PaymentSessionStatus,
//        string? PaymentId,
//        string? PaymentStatus,
//        decimal? Amount,
//        string? RawResponse,
//        string? ErrorMessage);

//    public sealed record ZohoTokenExchangeResult(
//        string AccessToken,
//        string RefreshToken,
//        int ExpiresIn);
//    /// <summary>
//    /// Low-level HTTP client for Zoho Payments API.
//    /// Uses IHttpClientFactory for socket/DNS management.
//    /// </summary>
//    public sealed class ZohoApiClient 
//    {
//        private readonly HttpClient _http;
//        private readonly ZohoOptions _options;
//        private readonly ILogger<ZohoApiClient> _logger;

//        private static ZohoAccessToken? _cachedToken;
//        private static readonly SemaphoreSlim TokenLock = new(1, 1);

//        public ZohoApiClient(
//            HttpClient http,
//            IOptions<ZohoOptions> options,
//            ILogger<ZohoApiClient> logger)
//        {
//            _http = http;
//            _options = options.Value;
//            _logger = logger;
//            _http.BaseAddress = new Uri(_options.AccountsBaseUrl);
//        }

//        // =========================================================
//        // OAuth: Exchange Authorization Code (one-time setup)
//        // =========================================================

//        public async Task<ZohoTokenExchangeResult> ExchangeAuthorizationCodeAsync(
//            string authorizationCode,
//            string redirectUri,
//            CancellationToken ct = default)
//        {
//            if (string.IsNullOrWhiteSpace(authorizationCode))
//                throw new ArgumentException("Authorization code is required.", nameof(authorizationCode));

//            if (string.IsNullOrWhiteSpace(redirectUri))
//                throw new ArgumentException("Redirect URI is required.", nameof(redirectUri));

//            var url =
//                $"{_options.AccountsBaseUrl}/oauth/v2/token" +
//                $"?code={Uri.EscapeDataString(authorizationCode)}" +
//                $"&client_id={Uri.EscapeDataString(_options.ClientId)}" +
//                $"&client_secret={Uri.EscapeDataString(_options.ClientSecret)}" +
//                $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
//                $"&grant_type=authorization_code";

//            using var response = await _http.PostAsync(url, null, ct);
//            var body = await response.Content.ReadAsStringAsync(ct);

//            if (!response.IsSuccessStatusCode)
//            {
//                _logger.LogError(
//                    "Zoho authorization_code exchange failed. HTTP={StatusCode}, Response={Response}",
//                    (int)response.StatusCode, body);

//                throw new InvalidOperationException(
//                    $"Unable to exchange Zoho authorization code. Gateway returned HTTP {(int)response.StatusCode}. " +
//                    "Common cause: code expired (valid ~3 min) or already used — generate a fresh code and retry.");
//            }

//            using var doc = JsonDocument.Parse(body);
//            var root = doc.RootElement;

//            if (root.TryGetProperty("error", out var errorEl))
//            {
//                var error = errorEl.GetString();
//                _logger.LogError("Zoho authorization_code exchange error. Error={Error}", error);
//                throw new InvalidOperationException($"Zoho returned an OAuth error: {error}.");
//            }

//            var accessToken = root.TryGetProperty("access_token", out var at) ? at.GetString() : null;
//            var refreshToken = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
//            var expiresIn = root.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 3600;

//            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
//            {
//                throw new InvalidOperationException(
//                    "Zoho response did not include access_token/refresh_token. " +
//                    "Ensure the authorization URL included access_type=offline.");
//            }

//            _cachedToken = new ZohoAccessToken(accessToken, DateTime.UtcNow.AddSeconds(expiresIn));

//            return new ZohoTokenExchangeResult(accessToken, refreshToken, expiresIn);
//        }

//        // =========================================================
//        // OAuth: Get Access Token (cached, auto-refresh)
//        // =========================================================

//        public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
//        {
//            if (_cachedToken != null && _cachedToken.ExpiresOnUtc > DateTime.UtcNow.AddMinutes(2))
//                return _cachedToken.AccessToken;

//            await TokenLock.WaitAsync(ct);
//            try
//            {
//                if (_cachedToken != null && _cachedToken.ExpiresOnUtc > DateTime.UtcNow.AddMinutes(2))
//                    return _cachedToken.AccessToken;
//            string url;
            
//                url =
//                    $"{_options.AccountsBaseUrl}/oauth/v2/token" +
//                    $"?refresh_token={Uri.EscapeDataString(_options.RefreshToken)}" +
//                    $"&client_id={Uri.EscapeDataString(_options.ClientId)}" +
//                    $"&client_secret={Uri.EscapeDataString(_options.ClientSecret)}" +
//                    $"&grant_type=refresh_token";
            
           
//                using var response = await _http.PostAsync(url, null, ct);
//                var body = await response.Content.ReadAsStringAsync(ct);

//                if (!response.IsSuccessStatusCode)
//                {
//                    _logger.LogError(
//                        "Zoho OAuth token refresh failed. HTTP={StatusCode}, Response={Response}",
//                        (int)response.StatusCode, body);

//                    throw new InvalidOperationException("Unable to refresh Zoho access token.");
//                }

//                using var doc = JsonDocument.Parse(body);
//                var root = doc.RootElement;

//                var accessToken = root.GetProperty("access_token").GetString()
//                    ?? throw new InvalidOperationException("Zoho token response missing access_token.");

//                var expiresIn = root.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 3600;

//                _cachedToken = new ZohoAccessToken(accessToken, DateTime.UtcNow.AddSeconds(expiresIn));

//                return accessToken;
//            }
//            finally
//            {
//                TokenLock.Release();
//            }
//        }

//        // =========================================================
//        // Create Payment Session (Hosted Checkout)
//        // =========================================================

//        public async Task<ZohoSessionResult> CreatePaymentSessionAsync(
//            decimal amount, string currency, string email, string mobile,
//            string description, string udf1 = "", string udf2 = "", string udf3 = "",
//            string udf4 = "", string udf5 = "", CancellationToken ct = default)
//        {
//            if (amount <= 0)
//                throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));

//            try
//            {
//                var accessToken = await GetAccessTokenAsync(ct);

//                var payload = new
//                {
//                    amount = amount.ToString("0.00", CultureInfo.InvariantCulture),
//                    currency,
//                    configurations = new
//                    {
//                        hosted_checkout_parameters = new
//                        {
//                            phone = mobile,
//                            description,
//                            email,
//                            success_url = _options.SuccessUrl,
//                            failure_url = _options.FailureUrl,
//                            udf1,
//                            udf2,
//                            udf3,
//                            udf4,
//                            udf5
//                        }
//                    }
//                };

//                var json = JsonSerializer.Serialize(payload);

//                // account_id is a query parameter, NOT a header
//                var url = $"{_options.ApiBaseUrl}/paymentsessions" +
//                          $"?account_id={Uri.EscapeDataString(_options.AccountId)}";

//                using var request = new HttpRequestMessage(HttpMethod.Post, url);
//                request.Headers.Add("Authorization", $"Zoho-oauthtoken {accessToken}");
//                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

//                using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseContentRead, ct);
//                var body = await response.Content.ReadAsStringAsync(ct);

//                if (!response.IsSuccessStatusCode)
//                {
//                    _logger.LogWarning(
//                        "Zoho create payment session failed. HTTP={StatusCode}, Response={Response}",
//                        (int)response.StatusCode, body);

//                    return new ZohoSessionResult(false, null, null, body,
//                        $"Zoho payment session API returned HTTP {(int)response.StatusCode}.");
//                }

//                using var doc = JsonDocument.Parse(body);
//                var root = doc.RootElement;

//                if (!root.TryGetProperty("payments_session", out var session))
//                    return new ZohoSessionResult(false, null, null, body,
//                        "Zoho response did not contain payments_session.");

//                var sessionId = session.TryGetProperty("payments_session_id", out var sid) ? sid.GetString() : null;
//                var accessKey = session.TryGetProperty("access_key", out var ak) ? ak.GetString() : null;

//                if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(accessKey))
//                {
//                    return new ZohoSessionResult(false, sessionId, accessKey, body,
//                        "Zoho response missing payments_session_id or access_key.");
//                }

//                return new ZohoSessionResult(true, sessionId, accessKey, body, null);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Unexpected error while creating Zoho payment session.");
//                return new ZohoSessionResult(false, null, null, null, ex.Message);
//            }
//        }

//        // =========================================================
//        // Retrieve Payment Session (status check / reconciliation)
//        // =========================================================

//        public async Task<ZohoSessionStatusResult> RetrievePaymentSessionAsync(
//            string paymentsSessionId, CancellationToken ct = default)
//        {
//            try
//            {
//                var accessToken = await GetAccessTokenAsync(ct);

//                var url = $"{_options.ApiBaseUrl}/paymentsessions/{Uri.EscapeDataString(paymentsSessionId)}" +
//                          $"?account_id={Uri.EscapeDataString(_options.AccountId)}";

//                using var request = new HttpRequestMessage(HttpMethod.Get, url);
//                request.Headers.Add("Authorization", $"Zoho-oauthtoken {accessToken}");

//                using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseContentRead, ct);
//                var body = await response.Content.ReadAsStringAsync(ct);

//                if (!response.IsSuccessStatusCode)
//                {
//                    return new ZohoSessionStatusResult(false, null, null, null, null, body,
//                        $"Zoho retrieve session API returned HTTP {(int)response.StatusCode}.");
//                }

//                using var doc = JsonDocument.Parse(body);
//                var root = doc.RootElement;

//                if (!root.TryGetProperty("payments_session", out var session))
//                {
//                    return new ZohoSessionStatusResult(false, null, null, null, null, body,
//                        "Zoho response did not contain payments_session.");
//                }

//                // NOTE: Field names below are based on API spec but MUST be confirmed
//                // with a real sandbox call before production use.
//                var sessionStatus = session.TryGetProperty("status", out var st) ? st.GetString() : null;
//                var paymentId = session.TryGetProperty("payment_id", out var pid) ? pid.GetString() : null;
//                var paymentStatus = session.TryGetProperty("payment_status", out var ps) ? ps.GetString() : null;

//                decimal? amount = null;
//                if (session.TryGetProperty("amount", out var amt) &&
//                    decimal.TryParse(amt.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
//                {
//                    amount = parsed;
//                }

//                return new ZohoSessionStatusResult(true, sessionStatus, paymentId, paymentStatus, amount, body, null);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Unexpected error while retrieving Zoho payment session. SessionId={SessionId}",
//                    paymentsSessionId);
//                return new ZohoSessionStatusResult(false, null, null, null, null, null, ex.Message);
//            }
//        }
//    }

