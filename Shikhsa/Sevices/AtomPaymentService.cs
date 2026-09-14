//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;
//using Microsoft.EntityFrameworkCore;
//using Shikhsa.Data;
//using Shikhsa.Models.Payment;

//namespace Shikhsa.Services
//{
//    public sealed class AtomPaymentService
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IHttpClientFactory _httpClientFactory;
//        private readonly ILogger<AtomPaymentService> _logger;

//        public AtomPaymentService(
//            ApplicationDbContext context,
//            IHttpClientFactory httpClientFactory,
//            ILogger<AtomPaymentService> logger)
//        {
//            _context = context;
//            _httpClientFactory = httpClientFactory;
//            _logger = logger;
//        }

//        public async Task<PaymentGatewaySetting> GetActiveSettingAsync(
//            CancellationToken cancellationToken = default)
//        {
//            var setting = await _context.PaymentGatewaySettings
//                .Where(x => x.GatewayName == "ATOM" && x.IsActive)
//                .OrderByDescending(x => x.PaymentGatewaySettingId)
//                .FirstOrDefaultAsync(cancellationToken);

//            if (setting == null)
//                throw new InvalidOperationException(
//                    "Koi active ATOM PaymentGatewaySetting nahi mila. Pehle usse configure/DB me add karo.");

//            return setting;
//        }


//        public async Task<AtomAuthResult> InitiateAsync(
//            string merchTxnId,
//            decimal amount,
//            string? custEmail,
//            string? custMobile,
//            string returnUrl,
//            CancellationToken cancellationToken = default)
//        {
//            var setting = await GetActiveSettingAsync(cancellationToken);

//            try
//            {
//                var payload = new
//                {
//                    payInstrument = new
//                    {
//                        headDetails = new
//                        {
//                            version = "OTSv1.1",
//                            api = "AUTH",
//                            platform = "WEB"
//                        },
//                        merchDetails = new
//                        {
//                            merchId = setting.MerchId,
//                            password = setting.MerchPassword,
//                            merchTxnId,
//                            merchTxnDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
//                            returnUrl
//                        },
//                        payDetails = new
//                        {
//                            amount = amount.ToString("F2"),
//                            product = setting.ProductId,
//                            txnCurrency = "INR"
//                        },
//                        custDetails = new
//                        {
//                            custEmail = custEmail ?? string.Empty,
//                            custMobile = custMobile ?? string.Empty
//                        }
//                    }
//                };

//                string requestJson = JsonSerializer.Serialize(payload);

//                // ATOM ka auth call encrypted body expect karta hai
//                string encryptedBody = EncrypDecrpt.Encrypt(
//                    requestJson,
//                    setting.RequestEncryptKey,
//                    setting.RequestSalt);

//                var client = _httpClientFactory.CreateClient(nameof(AtomPaymentService));

//                using var content = new StringContent(
//                    encryptedBody,
//                    Encoding.UTF8,
//                    "text/plain");

//                using var response = await client.PostAsync(
//                    setting.AuthUrl,
//                    content,
//                    cancellationToken);

//                string rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

//                if (!response.IsSuccessStatusCode)
//                {
//                    _logger.LogWarning(
//                        "ATOM auth call failed. MerchTxnId={MerchTxnId}, StatusCode={StatusCode}, Body={Body}",
//                        merchTxnId, response.StatusCode, rawResponse);

//                    return new AtomAuthResult
//                    {
//                        Success = false,
//                        ErrorMessage = $"ATOM auth call failed with HTTP {(int)response.StatusCode}.",
//                        RawResponse = rawResponse
//                    };
//                }

//                // Response bhi encrypted aata hai — decrypt karke parse karo
//                string decrypted;
//                try
//                {
//                    decrypted = EncrypDecrpt.Decrypt(
//                        rawResponse,
//                        setting.ResponseDecryptKey,
//                        setting.ResponseSalt);
//                }
//                catch
//                {
//                    // Kabhi-kabhi gateway plain JSON error bhi bhej deta hai (encrypt se pehle hi fail)
//                    decrypted = rawResponse;
//                }

//                using var doc = JsonDocument.Parse(decrypted);
//                var root = doc.RootElement;

//                string? atomTokenId = TryGetString(root, "atomTokenId");
//                string? statusCode = TryGetString(root, "statusCode");
//                string? statusDescription = TryGetString(root, "statusDescription");

//                bool success = !string.IsNullOrWhiteSpace(atomTokenId);

//                return new AtomAuthResult
//                {
//                    Success = success,
//                    AtomTokenId = atomTokenId,
//                    MerchId = setting.MerchId,
//                    MerchTxnId = merchTxnId,
//                    ErrorCode = success ? null : statusCode,
//                    ErrorMessage = success ? null : (statusDescription ?? "ATOM token generate nahi ho saka."),
//                    RawResponse = decrypted
//                };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(
//                    ex,
//                    "ATOM InitiateAsync me exception. MerchTxnId={MerchTxnId}",
//                    merchTxnId);

//                return new AtomAuthResult
//                {
//                    Success = false,
//                    ErrorMessage = "Payment gateway se connect karte waqt error aaya. Thodi der baad try karo."
//                };
//            }
//        }

//        public AtomCallbackResponse ParseCallback(IFormCollection form)
//        {
//            string raw = string.Join(
//                "&",
//                form.Select(x => $"{x.Key}={x.Value}"));

//            return new AtomCallbackResponse
//            {
//                MerchId = form["merchId"],
//                MerchTxnId = form["merchTxnId"],
//                AtomTxnId = form["atomTxnId"],
//                BankTxnId = form["bankTxnId"],
//                Amount = decimal.TryParse(form["amount"], out var amt) ? amt : 0,
//                StatusCode = form["statusCode"],
//                StatusDescription = form["statusDescription"],
//                PaymentMode = form["paymentMode"],
//                BankName = form["bankName"],
//                SignatureOrHash = form["signature"],
//                RawResponse = raw
//            };
//        }
//        public async Task<AtomStatusCheckResult> CheckTransactionStatusAsync(
//            string merchTxnId,
//            DateTime txnDate,
//            CancellationToken cancellationToken = default)
//        {
//            var setting = await GetActiveSettingAsync(cancellationToken);

//            if (string.IsNullOrWhiteSpace(setting.StatusCheckUrl))
//            {
//                return new AtomStatusCheckResult
//                {
//                    Success = false,
//                    ErrorMessage = "StatusCheckUrl configure nahi hai PaymentGatewaySetting me."
//                };
//            }

//            try
//            {
//                var payload = new
//                {
//                    payInstrument = new
//                    {
//                        merchDetails = new
//                        {
//                            merchId = setting.MerchId,
//                            password = setting.MerchPassword,
//                            merchTxnId,
//                            merchTxnDate = txnDate.ToString("yyyy-MM-dd HH:mm:ss")
//                        }
//                    }
//                };

//                string requestJson = JsonSerializer.Serialize(payload);

//                string encryptedBody = EncrypDecrpt.Encrypt(
//                    requestJson,
//                    setting.RequestEncryptKey,
//                    setting.RequestSalt);

//                var client = _httpClientFactory.CreateClient(nameof(AtomPaymentService));

//                using var content = new StringContent(
//                    encryptedBody,
//                    Encoding.UTF8,
//                    "text/plain");

//                using var response = await client.PostAsync(
//                    setting.StatusCheckUrl,
//                    content,
//                    cancellationToken);

//                string rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

//                if (!response.IsSuccessStatusCode)
//                {
//                    return new AtomStatusCheckResult
//                    {
//                        Success = false,
//                        ErrorMessage = $"Status check HTTP {(int)response.StatusCode}.",
//                        RawResponse = rawResponse
//                    };
//                }

//                string decrypted;
//                try
//                {
//                    decrypted = EncrypDecrpt.Decrypt(
//                        rawResponse,
//                        setting.ResponseDecryptKey,
//                        setting.ResponseSalt);
//                }
//                catch
//                {
//                    decrypted = rawResponse;
//                }

//                using var doc = JsonDocument.Parse(decrypted);
//                var root = doc.RootElement;

//                string? statusCode = TryGetString(root, "statusCode");
//                string? statusDescription = TryGetString(root, "statusDescription");
//                string? atomTxnId = TryGetString(root, "atomTxnId");

//                bool success =
//                    !string.IsNullOrWhiteSpace(statusCode) &&
//                    statusCode.Equals("OTS0000", StringComparison.OrdinalIgnoreCase);

//                return new AtomStatusCheckResult
//                {
//                    Success = success,
//                    StatusCode = statusCode,
//                    StatusDescription = statusDescription,
//                    AtomTxnId = atomTxnId,
//                    RawResponse = decrypted
//                };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(
//                    ex,
//                    "ATOM CheckTransactionStatusAsync me exception. MerchTxnId={MerchTxnId}",
//                    merchTxnId);

//                return new AtomStatusCheckResult
//                {
//                    Success = false,
//                    ErrorMessage = ex.Message
//                };
//            }
//        }

//        private static string? TryGetString(JsonElement root, string propertyName)
//        {
//            return root.TryGetProperty(propertyName, out var value)
//                ? value.GetString()
//                : null;
//        }
//    }
//}

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models.Payment;

namespace Shikhsa.Services
{
    public sealed class AtomPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AtomPaymentService> _logger;

        public AtomPaymentService(
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory,
            ILogger<AtomPaymentService> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<PaymentGatewaySetting> GetActiveSettingAsync(
            CancellationToken cancellationToken = default)
        {
            var setting = await _context.PaymentGatewaySettings
                .Where(x => x.GatewayName == "ATOM" && x.IsActive)
                .OrderByDescending(x => x.PaymentGatewaySettingId)
                .FirstOrDefaultAsync(cancellationToken);

            if (setting == null)
                throw new InvalidOperationException(
                    "Koi active ATOM PaymentGatewaySetting nahi mila. Pehle usse configure/DB me add karo.");

            return setting;
        }

        public async Task<AtomAuthResult> InitiateAsync(
            string merchTxnId,
            decimal amount,
            string? custEmail,
            string? custMobile,
            string returnUrl,
            CancellationToken cancellationToken = default)
        {
            var setting = await GetActiveSettingAsync(cancellationToken);

            try
            {
                var payload = new
                {
                    payInstrument = new
                    {
                        headDetails = new
                        {
                            version = "OTSv1.1",
                            api = "AUTH",
                            platform = "WEB"
                        },
                        merchDetails = new
                        {
                            merchId = setting.MerchId,
                            password = setting.MerchPassword,
                            merchTxnId,
                            merchTxnDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            returnUrl
                        },
                        payDetails = new
                        {
                            amount = amount.ToString("F2"),
                            product = setting.ProductId,
                            txnCurrency = "INR"
                        },
                        custDetails = new
                        {
                            custEmail = custEmail ?? string.Empty,
                            custMobile = custMobile ?? string.Empty
                        }
                    }
                };

                string requestJson = JsonSerializer.Serialize(payload);

                string encryptedBody = EncrypDecrpt.Encrypt(
                    requestJson,
                    setting.RequestEncryptKey,
                    setting.RequestSalt);

                var client = _httpClientFactory.CreateClient(nameof(AtomPaymentService));

                // The gateway requires standard key-value URL-encoded pairs
                var formParameters = new Dictionary<string, string>
{
    { "encData", encryptedBody },
    { "merchId", setting.MerchId }
};

                // FormUrlEncodedContent automatically configures headers to application/x-www-form-urlencoded
                using var content = new FormUrlEncodedContent(formParameters);

                using var response = await client.PostAsync(
                    setting.AuthUrl,
                    content,
                    cancellationToken);



                string rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "ATOM auth call failed. MerchTxnId={MerchTxnId}, StatusCode={StatusCode}, Body={Body}",
                        merchTxnId, response.StatusCode, rawResponse);

                    return new AtomAuthResult
                    {
                        Success = false,
                        ErrorMessage = $"ATOM auth call failed with HTTP {(int)response.StatusCode}.",
                        RawResponse = rawResponse
                    };
                }

                string decrypted;
                try
                {
                    // Extract the pure hex value if the gateway wraps it in form-encoded format
                    string targetHex = rawResponse;

                    if (rawResponse.Contains("encData="))
                    {
                        // Split the string across parameter keys to find 'encData'
                        var queryParams = System.Web.HttpUtility.ParseQueryString(rawResponse);
                        targetHex = queryParams["encData"] ?? rawResponse;
                    }

                    // Safety fallback: if it's already a plain JSON error block, skip decryption
                    if (targetHex.Trim().StartsWith("{") && targetHex.Trim().EndsWith("}"))
                    {
                        decrypted = targetHex;
                    }
                    else
                    {
                        // Pass ONLY the pure hex string to the decryption engine
                        decrypted = EncrypDecrpt.Decrypt(
                            targetHex.Trim(),
                            setting.ResponseDecryptKey,
                            setting.ResponseSalt);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ATOM response text normalization or decryption failed. Raw payload: {Raw}", rawResponse);

                    return new AtomAuthResult
                    {
                        Success = false,
                        ErrorMessage = "Gateway encryption mismatch error. Please verify the Response Key and Salt parameters.",
                        RawResponse = rawResponse
                    };
                }



                using var doc = JsonDocument.Parse(decrypted);
                var root = doc.RootElement;

                string? atomTokenId = null;
                string? statusCode = null;
                string? statusDescription = null;

                // NTT DATA / Atom OTSv1.1 standard nested payload tree parsing
                if (root.TryGetProperty("payInstrument", out var payInstrument))
                {
                    if (payInstrument.TryGetProperty("payDetails", out var payDetails))
                    {
                        atomTokenId = TryGetString(payDetails, "atomTokenId");
                    }

                    if (payInstrument.TryGetProperty("responseDetails", out var responseDetails))
                    {
                        statusCode = TryGetString(responseDetails, "statusCode");
                        statusDescription = TryGetString(responseDetails, "statusDescription");
                    }
                }

                // Fallback safety layer: check the root level just in case the gateway structure flattens
                atomTokenId ??= TryGetString(root, "atomTokenId");
                statusCode ??= TryGetString(root, "statusCode");
                statusDescription ??= TryGetString(root, "statusDescription");

                // In NTT DATA OTS platform APIs, "OTS0000" explicitly means Success/Token Generated.
                // If atomTokenId is populated, it is also a success indicator.
                bool success = !string.IsNullOrWhiteSpace(atomTokenId) ||
                               (statusCode != null && statusCode.Equals("OTS0000", StringComparison.OrdinalIgnoreCase));

                _logger.LogInformation("ATOM Auth Parse Finished. Success={Success}, Token={Token}, Code={Code}, Message={Msg}",
                    success, atomTokenId, statusCode, statusDescription);

                return new AtomAuthResult
                {
                    Success = success,
                    AtomTokenId = atomTokenId,
                    MerchId = setting.MerchId,
                    MerchTxnId = merchTxnId,
                    ErrorCode = statusCode,
                    ErrorMessage = success ? null : (statusDescription ?? "ATOM token generate nahi ho saka."),
                    RawResponse = decrypted
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ATOM InitiateAsync me exception. MerchTxnId={MerchTxnId}",
                    merchTxnId);

                return new AtomAuthResult
                {
                    Success = false,
                    ErrorMessage = "Payment gateway se connect karte waqt error aaya. Thodi der baad try karo."
                };
            }
        }

        public AtomCallbackResponse ParseCallback(IFormCollection form)
        {
            string raw = string.Join("&", form.Select(x => $"{x.Key}={x.Value}"));

            // Handshake properties exposed directly out of NTT DATA payment gateway specifications
            string status = form["statusCode"].ToString();
            bool isSuccessTxn = status == "OTS0000" || form["status"].ToString().Equals("SUCCESS", StringComparison.OrdinalIgnoreCase);

            return new AtomCallbackResponse
            {
                MerchId = form["merchId"],
                MerchTxnId = form["merchTxnId"],
                AtomTxnId = form["atomTxnId"],
                BankTxnId = form["bankTxnId"],
                Amount = decimal.TryParse(form["amount"], out var amt) ? amt : 0,
                StatusCode = status,
                StatusDescription = form["statusDescription"],
                PaymentMode = form["paymentMode"],
                BankName = form["bankName"],
                SignatureOrHash = form["signature"],
                IsActive = isSuccessTxn,
                RawResponse = raw
            };
        }

        public async Task<AtomStatusCheckResult> CheckTransactionStatusAsync(
            string merchTxnId,
            DateTime txnDate,
            CancellationToken cancellationToken = default)
        {
            var setting = await GetActiveSettingAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(setting.StatusCheckUrl))
            {
                return new AtomStatusCheckResult
                {
                    Success = false,
                    ErrorMessage = "StatusCheckUrl configure nahi hai PaymentGatewaySetting me."
                };
            }

            try
            {
                var payload = new
                {
                    payInstrument = new
                    {
                        merchDetails = new
                        {
                            merchId = setting.MerchId,
                            password = setting.MerchPassword,
                            merchTxnId,
                            merchTxnDate = txnDate.ToString("yyyy-MM-dd HH:mm:ss")
                        }
                    }
                };

                string requestJson = JsonSerializer.Serialize(payload);

                string encryptedBody = EncrypDecrpt.Encrypt(
                    requestJson,
                    setting.RequestEncryptKey,
                    setting.RequestSalt);

                var client = _httpClientFactory.CreateClient(nameof(AtomPaymentService));

                using var content = new StringContent(encryptedBody, Encoding.UTF8, "text/plain");
                using var response = await client.PostAsync(setting.StatusCheckUrl, content, cancellationToken); 
                string rawResponse = await response.Content.ReadAsStringAsync(cancellationToken); 
                if (!response.IsSuccessStatusCode)
                {
                    return new AtomStatusCheckResult 
                    {
                        Success = false,
                        ErrorMessage = $"Status check HTTP {(int)response.StatusCode}.", RawResponse = rawResponse
                    }; 
                }
                string decrypted; 
                try
                { 
                    decrypted = EncrypDecrpt.Decrypt(rawResponse, setting.ResponseDecryptKey, setting.ResponseSalt);
                }
                catch
                {
                    decrypted = rawResponse;
                }
                using var doc = JsonDocument.Parse(decrypted); 
                var root = doc.RootElement; 
                string? statusCode = TryGetString(root, "statusCode");
                string? statusDescription = TryGetString(root, "statusDescription"); 
                string? atomTxnId = TryGetString(root, "atomTxnId"); 
                bool success = !string.IsNullOrWhiteSpace(statusCode) && statusCode.Equals("OTS0000", StringComparison.OrdinalIgnoreCase); 
                return new AtomStatusCheckResult 
                { 
                    Success = success, 
                    StatusCode = statusCode, 
                    StatusDescription = statusDescription, 
                    AtomTxnId = atomTxnId, 
                    RawResponse = decrypted 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ATOM CheckTransactionStatusAsync me exception. MerchTxnId={MerchTxnId}", merchTxnId);
                return new AtomStatusCheckResult
                { 
                    Success = false, 
                    ErrorMessage = ex.Message
                };
            }
        }
        private static string? TryGetString(JsonElement root, string propertyName)
        {
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(propertyName, out var value))
            {
                // Return based on what type of data the gateway sent back
                return value.ValueKind switch
                {
                    JsonValueKind.String => value.GetString(),
                    JsonValueKind.Number => value.GetRawText(), // Extract as a string even if it's a numeric status/amount
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.Null => null,
                    _ => value.GetRawText()
                };
            }
            return null;
        }
    }
}