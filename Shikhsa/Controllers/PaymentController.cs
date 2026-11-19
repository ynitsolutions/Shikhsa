using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Attributes;
using Shikhsa.Data;
using Shikhsa.Models;
using Shikhsa.Models.Payment;
using Shikhsa.Repository;
using Shikhsa.Services;
using Shikhsa.ViewModels;
using System.Text.Json;

namespace Shikhsa.Controllers
{
    public class PaymentController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly FeeHeadingRepository _repository;
        private readonly AtomPaymentService _atomPaymentService;
        private readonly ZohoPaymentService _zohoPaymentService;
        private readonly ILogger<PaymentController> _logger;
        private readonly IConfiguration _configuration;

        // Sandbox testing me false (log me "signature valid=true" dikhne ke baad true kar dein).
        // Waise bhi asli faisla CompleteAsync me Zoho se server-side confirm hone par hota hai.
        private const bool EnforceRedirectSignature = false;
        private const string SessionKey = "PendingOnlineFeeReceipt";

        public PaymentController(
            ApplicationDbContext context,
            FeeHeadingRepository repository,
            AtomPaymentService atomPaymentService,
            ZohoPaymentService zohoPaymentService,
            ILogger<PaymentController> logger,
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            PermissionService permissionService,
            EmailService email,
            LookupService lookup)
            : base(userManager, permissionService, context, email, lookup)
        {
            _context = context;
            _repository = repository;
            _atomPaymentService = atomPaymentService;
            _zohoPaymentService = zohoPaymentService;
            _logger = logger;
            _configuration = configuration;
        }

        private string GetDefaultGatewayName()
        {
            var configuredGateway = _configuration["PaymentGateway:DefaultGateway"];

            if (string.IsNullOrWhiteSpace(configuredGateway))
            {
                _logger.LogWarning(
                    "PaymentGateway:DefaultGateway appsettings.json me set nahi hai. ATOM ko fallback use kiya jaa raha hai.");
                return "ATOM";
            }

            return configuredGateway.Trim().ToUpperInvariant();
        }

        #region Initiate (single entry point — session based)

        [HttpGet]
        [SkipPermission]
        public async Task<IActionResult> InitiateFromSession(CancellationToken cancellationToken)
        {
            _logger.LogWarning(
                    "InitiateFromSession CALLED | TraceId={TraceId} | Method={Method} | Path={Path} | Referer={Referer} | Time={Time}",
                    HttpContext.TraceIdentifier,
                    Request.Method,
                    Request.Path,
                    Request.Headers.Referer.ToString(),
                    DateTime.Now
                );
            var json = HttpContext.Session.GetString(SessionKey);

            if (string.IsNullOrWhiteSpace(json))
            {
                ErrorMessage("Payment session expire ho gaya. Dobara try karo.");
                return RedirectToAction("FeeReceipt", "FeeHeading");
            }

            StudentFeeReceiptVM? vm;
            try
            {
                vm = JsonSerializer.Deserialize<StudentFeeReceiptVM>(json);
                if (vm != null) vm.TotalAmount = vm.ReceiptAmount;
            }
            catch
            {
                vm = null;
            }

            if (vm == null || vm.TotalAmount <= 0)
            {
                ErrorMessage("Payment data is invalid. Please try again.");
                return RedirectToAction("FeeReceipt", "FeeHeading");
            }

            var student = await _context.Tbl_Students
                .FirstOrDefaultAsync(x => x.StudentId == vm.StudentId, cancellationToken);

            var gatewayName = GetDefaultGatewayName();

            // ============================================
            // EK HI JAGAH transaction row banti hai (chahe
            // ATOM ho ya ZOHO) — duplicate row wala bug fix
            // ============================================

            var transaction = new PaymentTransactionDetail
            {
                StudentId = vm.StudentId,
                ClassId = vm.ClassId,
                BatchId = vm.BatchId,
                StudentName = vm.StudentName ?? $"{student?.FirstName} {student?.MiddleName} {student?.LastName}".Replace("  ", " ").Trim(),
                Amount = vm.TotalAmount,
                TxnDate = DateTime.Now,
                TransactionStatus = "Pending",
                GatewayName = gatewayName,
                IsActive = true,
                ApplicationNo=student?.ApplicationNo,
                PaymentModeId=vm.PaymentModeId,
                FatherName= $"{student?.Parent?.FatherFirstName} {student?.Parent?.FatherMiddleName} {student?.Parent?.FatherLastName}".Replace("  ", " ").Trim(),
                // ZOHO: fee snapshot DB me (redirect/webhook ke time session par nirbhar nahi rehna).
                // Column NVARCHAR(MAX) hona chahiye. ATOM rows ko chhua nahi gaya.
                GatewaySignature = gatewayName == "ZOHO" ? json : null
            };

            _context.PaymentTransactionDetail.Add(transaction);
            await _context.SaveChangesAsync(cancellationToken);

            return gatewayName switch
            {
                "ZOHO" => await InitiateZohoAsync(transaction, vm, student, cancellationToken),
                _ => await InitiateAtomAsync(transaction, vm, student, cancellationToken)
            };
        }

        #endregion

        #region ATOM flow

        private async Task<IActionResult> InitiateAtomAsync(
            PaymentTransactionDetail transaction, StudentFeeReceiptVM vm, Tbl_Students? student,
            CancellationToken cancellationToken)
        {
            string merchTxnId = $"SHK{transaction.PaymentTransactionId}{DateTime.Now:yyMMddHHmmss}";

            transaction.TransactionId = merchTxnId;
            transaction.GatewayOrderId = merchTxnId;
            await _context.SaveChangesAsync(cancellationToken);

            string returnUrl = Url.Action("Callback", "Payment", null, Request.Scheme) ?? string.Empty;

            var setting = await _atomPaymentService.GetActiveSettingAsync();

            var checkoutVm = new AtomCheckoutVM
            {
                MerchantTransactionId = merchTxnId,
                Amount = vm.TotalAmount,
                MerchId = setting.MerchId,
                CustomerEmail = student?.Email ?? string.Empty,
                CustomerMobile = student?.ContactNo ?? string.Empty,
                ReturnUrl = returnUrl,
                CheckoutCdn = setting.CheckoutCdn,
                Environment = setting.CheckoutEnvironment,
                Password = setting.MerchPassword,
                ProductId = setting.ProductId,
                RequestEncryptKey = setting.RequestEncryptKey,
                RequestSalt = setting.RequestSalt
            };

            var authResult = await _atomPaymentService.InitiateAsync(
                merchTxnId, vm.TotalAmount, student?.Email, student?.ContactNo, returnUrl);

            if (!authResult.Success)
            {
                transaction.TransactionStatus = "Failed";
                transaction.TransactionError = authResult.ErrorMessage ?? "Gateway token generate nahi ho saka.";
                await _context.SaveChangesAsync(cancellationToken);

                ErrorMessage(transaction.TransactionError);
                return RedirectToAction("FeeReceipt", "FeeHeading");
            }

            checkoutVm.AtomTokenId = authResult.AtomTokenId ?? string.Empty;
            await PopulateCheckoutDetailsAsync(checkoutVm, transaction, student, cancellationToken);
            return View("Checkout", checkoutVm);
        }

        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            AtomCallbackResponse callback;

            if (Request.Method == HttpMethods.Post && Request.HasFormContentType)
            {
                callback = _atomPaymentService.ParseCallback(Request.Form);
            }
            else
            {
                callback = new AtomCallbackResponse
                {
                    MerchTxnId = Request.Query["merchTxnId"],
                    AtomTxnId = Request.Query["atomTxnId"],
                    StatusCode = Request.Query["statusCode"],
                    StatusDescription = Request.Query["statusDescription"],
                    RawResponse = Request.QueryString.Value
                };
            }

            if (string.IsNullOrWhiteSpace(callback.MerchTxnId))
            {
                return View("Result", new AtomPaymentResultVM
                {
                    Success = false,
                    Message = "Payment gateway se invalid response mila."
                });
            }

            var transaction = await _context.PaymentTransactionDetail
                .FirstOrDefaultAsync(x => x.TransactionId == callback.MerchTxnId);

            if (transaction == null)
            {
                return View("Result", new AtomPaymentResultVM
                {
                    Success = false,
                    Message = "Transaction record nahi mila."
                });
            }

            transaction.GatewayResponse = callback.RawResponse;
            transaction.GatewayPaymentId = callback.AtomTxnId;
            transaction.ReferenceNo = callback.AtomTxnId;
            transaction.PaymentId = callback.StatusCode;

            bool parsedSuccess = callback.StatusCode == "OTS0000" || callback.StatusCode == "SUCCESS" || callback.IsSuccess;

            if (!parsedSuccess)
            {
                transaction.TransactionStatus = "Failed";
                transaction.TransactionError = callback.StatusDescription ?? "Payment failed.";
                await _context.SaveChangesAsync();

                return View("Result", new AtomPaymentResultVM
                {
                    Success = false,
                    Message = transaction.TransactionError,
                    Amount = transaction.Amount,
                    ReferenceNo = callback.AtomTxnId
                });
            }

            var json = HttpContext.Session.GetString(SessionKey);
            StudentFeeReceiptVM? pendingVm = null;

            if (!string.IsNullOrWhiteSpace(json))
            {
                try { pendingVm = JsonSerializer.Deserialize<StudentFeeReceiptVM>(json); }
                catch { pendingVm = null; }
            }

            long? receiptId = null;

            if (pendingVm != null)
            {
                var onlineModeId = GetDataListItems("Payment Mode")
                    .Where(x => x.DataListItemText != null &&
                                x.DataListItemText.Trim().Equals("Online", StringComparison.OrdinalIgnoreCase))
                    .Select(x => (int?)x.DataListItemId)
                    .FirstOrDefault();

                pendingVm.PaymentModeId = onlineModeId ?? 0;

                try
                {
                    receiptId = _repository.SaveCashFeeReceipt(pendingVm);
                    HttpContext.Session.Remove(SessionKey);
                }
                catch (Exception ex)
                {
                    transaction.TransactionError = "Payment success hua lekin receipt save karte waqt error: " + ex.Message;
                }
            }
            else
            {
                transaction.TransactionError = "Payment success hua lekin session expire ho chuki thi — receipt manually verify karo.";
            }

            transaction.TransactionStatus = "Success";
            transaction.PaymentCompletedOn = DateTime.Now;

            if (receiptId.HasValue)
            {
                var receipt = await _context.FeeReceipt.FirstOrDefaultAsync(x => x.FeeReceiptId == receiptId.Value);
                if (receipt != null) receipt.PaymentTransactionId = transaction.PaymentTransactionId;
            }

            await _context.SaveChangesAsync();

            return View("Result", new AtomPaymentResultVM
            {
                Success = true,
                Message = transaction.TransactionError ?? "Payment successful!",
                Amount = transaction.Amount,
                ReferenceNo = callback.AtomTxnId,
                ReceiptId = receiptId
            });
        }

        #endregion

        #region ZOHO flow

        //private async Task<IActionResult> InitiateZohoAsync(
        //    PaymentTransactionDetail transaction, StudentFeeReceiptVM vm, Tbl_Students? student,
        //    CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        var email = student?.Email ?? string.Empty;
        //        var mobile = student?.ContactNo ?? string.Empty;

        //        // udf1 = StudentId, udf2 = PaymentTransactionId, udf3 = ClassId, udf4 = BatchId
        //        var session = await _zohoPaymentService.CreatePaymentSessionAsync(
        //                        transaction.Amount,
        //                        "INR",
        //                        email,
        //                        mobile,
        //                        $"Fee payment for {student?.ApplicationNo}",
        //                        student != null ? $"{student.FirstName} {student.LastName}".Trim() : "",
        //                        "IN",
        //                        transaction.StudentId.ToString(),
        //                        transaction.PaymentTransactionId.ToString(),
        //                        vm.ClassId.ToString(),
        //                        vm.BatchId.ToString(),
        //                        "",
        //                        cancellationToken);

        //        transaction.TransactionStatus = session.Success ? "Pending" : "Failed";
        //        transaction.TransactionError = session.Success ? null : session.ErrorMessage;
        //        transaction.GatewayResponse = session.RawResponse;
        //        transaction.GatewayOrderId = session.PaymentsSessionId;
        //        transaction.TransactionId = session.PaymentsSessionId;

        //        await _context.SaveChangesAsync(cancellationToken);

        //        if (!session.Success || string.IsNullOrWhiteSpace(session.AccessKey))
        //        {
        //            ErrorMessage(session.ErrorMessage ?? "Unable to initiate online payment.");
        //            return RedirectToAction("FeeReceipt", "FeeHeading");
        //        }

        //        var settings = await _zohoPaymentService.GetSettingsAsync(cancellationToken);

        //        // Zoho hosted checkout docs: https://payments.zoho.in/hostedcheckout/<access_key>
        //        // (sandbox me host paymentssandbox.zoho.in; settings me CheckoutBaseUrl wahi dega)
        //        return View("Checkout", new AtomCheckoutVM
        //        {
        //            MerchantTransactionId = transaction.TransactionId,
        //            Amount = transaction.Amount,

        //            // Atom properties: existing values as they are
        //            AtomTokenId = string.Empty,
        //            MerchId = string.Empty,
        //            CustomerEmail = email,
        //            CustomerMobile = mobile,
        //            ReturnUrl = string.Empty,
        //            CheckoutCdn = string.Empty,
        //            Environment = string.Empty,

        //            // Zoho properties
        //            ZohoAccountId = settings.AccountId,
        //            ZohoApiKey = settings.ApiKey,
        //            ZohoPaymentSessionId = session.PaymentsSessionId,
        //            ZohoConfirmPaymentUrl = Url.Action(
        //                "ConfirmZohoWidgetPayment",
        //                "Payment"
        //            ),
        //            ZohoIsTestMode = true
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error while initiating Zoho fee payment for StudentId {StudentId}", vm.StudentId);

        //        transaction.TransactionStatus = "Failed";
        //        transaction.TransactionError = "Unable to initiate payment. Please try again.";
        //        await _context.SaveChangesAsync(cancellationToken);

        //        ErrorMessage(transaction.TransactionError);
        //        return RedirectToAction("FeeReceipt", "FeeHeading");
        //    }
        //}

        private async Task<IActionResult> InitiateZohoAsync(
            PaymentTransactionDetail transaction,
            StudentFeeReceiptVM vm,
            Tbl_Students? student,
            CancellationToken cancellationToken)
        {
            try
            {
                var email = student?.Email ?? string.Empty;
                var mobile = student?.ContactNo ?? string.Empty;

                // UDF Mapping:
                // udf1 = StudentId
                // udf2 = PaymentTransactionId
                // udf3 = ClassId
                // udf4 = BatchId

                var session = await _zohoPaymentService.CreatePaymentSessionAsync(
                    transaction.Amount,
                    "INR",
                    email,
                    mobile,
                    $"Fee payment for {student?.ApplicationNo}",
                    student != null
                        ? $"{student.FirstName} {student.LastName}".Trim()
                        : string.Empty,
                    "IN",
                    transaction.StudentId.ToString(),
                    transaction.PaymentTransactionId.ToString(),
                    vm.ClassId.ToString(),
                    vm.BatchId.ToString(),
                    "",
                    cancellationToken);

                var isSessionCreated =
                    session.Success &&
                    !string.IsNullOrWhiteSpace(session.PaymentsSessionId);

                // Common Gateway Information
                transaction.GatewayName = "ZOHO";
                transaction.GatewayOrderId = session.PaymentsSessionId;
                transaction.GatewayPaymentId = null;
                transaction.TransactionId = session.PaymentsSessionId;
                transaction.PaymentId = null;

                // Student Information
                transaction.ApplicationNo = student?.ApplicationNo;
                transaction.StudentName = student != null
                    ? $"{student.FirstName} {student.LastName}".Trim()
                    : transaction.StudentName;

                transaction.ClassId = vm.ClassId;
                transaction.BatchId = vm.BatchId;

                // Transaction Information
                transaction.TransactionStatus =
                    isSessionCreated ? "Pending" : "Failed";

                transaction.TransactionError =
                    isSessionCreated
                        ? null
                        : session.ErrorMessage ??
                          "Unable to create Zoho payment session.";

                // Preserve Fee Snapshot:
                // DO NOT clear GatewaySignature.
                // It contains serialized StudentFeeReceiptVM.

                // Payment is not completed yet
                transaction.PaymentCompletedOn = null;
                transaction.ReferenceNo = null;

                // Gateway Response
                transaction.GatewayResponse = session.RawResponse;

                transaction.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync(cancellationToken);

                if (!isSessionCreated)
                {
                    ErrorMessage(transaction.TransactionError);
                    return RedirectToAction("FeeReceipt", "FeeHeading");
                }

                var settings = await _zohoPaymentService.GetSettingsAsync(
                    cancellationToken);
                var checkoutVm = new AtomCheckoutVM
                {
                    MerchantTransactionId = transaction.TransactionId,
                    Amount = transaction.Amount,
                    CustomerEmail = email,
                    CustomerMobile = mobile,
                    ZohoAccountId = settings.AccountId,
                    ZohoApiKey = settings.ApiKey,
                    ZohoPaymentSessionId = session.PaymentsSessionId,
                    ZohoConfirmPaymentUrl = Url.Action("ConfirmZohoWidgetPayment", "Payment"),
                    ZohoIsTestMode = true
                };

                // Yahan call karein
                await PopulateCheckoutDetailsAsync(checkoutVm, transaction, student, cancellationToken);

                return View("Checkout", checkoutVm);
                //return View("Checkout", new AtomCheckoutVM
                //{
                //    MerchantTransactionId = transaction.TransactionId,
                //    Amount = transaction.Amount,

                //    // Atom properties
                //    AtomTokenId = string.Empty,
                //    MerchId = string.Empty,
                //    CustomerEmail = email,
                //    CustomerMobile = mobile,
                //    ReturnUrl = string.Empty,
                //    CheckoutCdn = string.Empty,
                //    Environment = string.Empty,

                //    // Zoho properties
                //    ZohoAccountId = settings.AccountId,
                //    ZohoApiKey = settings.ApiKey,
                //    ZohoPaymentSessionId = session.PaymentsSessionId,
                //    ZohoConfirmPaymentUrl = Url.Action(
                //        "ConfirmZohoWidgetPayment",
                //        "Payment"),
                //    ZohoIsTestMode = true
                //});
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error initiating Zoho fee payment. StudentId: {StudentId}, TransactionId: {TransactionId}",
                    transaction.StudentId,
                    transaction.PaymentTransactionId);

                transaction.TransactionStatus = "Failed";
                transaction.TransactionError = ex.Message;
                transaction.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync(cancellationToken);

                ErrorMessage("Unable to initiate Zoho payment.");
                return RedirectToAction("FeeReceipt", "FeeHeading");
            }
        }



        // Zoho redirect: GET (docs me query string). POST bhi accept karte hain.'
        [AcceptVerbs("GET", "POST")]
        [SkipPermission]
        public Task<IActionResult> Success() => HandleZohoReturnAsync(isSuccessUrl: true);

        [AcceptVerbs("GET", "POST")]
        [SkipPermission]
        public Task<IActionResult> Failure() => HandleZohoReturnAsync(isSuccessUrl: false);

        private async Task<IActionResult> HandleZohoReturnAsync(bool isSuccessUrl)
        {
            var ct = HttpContext.RequestAborted;

            string Get(string key) =>
                Request.HasFormContentType && Request.Form.ContainsKey(key)
                    ? Request.Form[key].ToString()
                    : Request.Query[key].ToString();

            _logger.LogInformation("Zoho {Kind} redirect. Method={Method} Query={Query}",
                isSuccessUrl ? "SUCCESS" : "FAILURE", Request.Method, Request.QueryString);

            string sessionId = Get("payments_session_id");

            // Fallback: udf2 me PaymentTransactionId bheja tha
            if (string.IsNullOrWhiteSpace(sessionId) && long.TryParse(Get("udf2"), out var ptId))
            {
                sessionId = await _context.PaymentTransactionDetail.AsNoTracking()
                    .Where(x => x.GatewayName == "ZOHO" && x.PaymentTransactionId == ptId)
                    .Select(x => x.GatewayOrderId)
                    .FirstOrDefaultAsync(ct) ?? "";
            }

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return View("Result", AtomPaymentResultVM.Failed(
                    "Payment ki details nahi mil payi. Agar paisa kat gaya ho to school office se sampark karein."));
            }

            // Redirect signature: log hota hai; EnforceRedirectSignature true ho to reject
            var signature = Get("signature");
            if (!string.IsNullOrWhiteSpace(signature))
            {
                bool sigOk = await _zohoPaymentService.VerifyRedirectSignatureAsync(
                    sessionId, Get("payment_session_status"), Get("payment_id"), Get("payment_status"),
                    Get("amount"), Get("mandate_id"), Get("udf1"), Get("udf2"), Get("udf3"), Get("udf4"), Get("udf5"),
                    signature, ct);

                _logger.LogInformation("Zoho redirect signature valid={Valid} session={S}", sigOk, sessionId);

                if (!sigOk && EnforceRedirectSignature)
                    return View("Result", AtomPaymentResultVM.Failed("Invalid redirect signature."));
            }

            // Asli faisla: Zoho se server-side confirm + receipt (idempotent)
            var result = await _zohoPaymentService.CompleteAsync(sessionId, ct);

            var row = await _context.PaymentTransactionDetail.AsNoTracking()
                .Where(x => x.GatewayName == "ZOHO" && x.GatewayOrderId == sessionId)
                .Select(x => new { x.PaymentTransactionId, x.Amount, x.GatewayPaymentId })
                .FirstOrDefaultAsync(ct);

            if (result.Success)
            {
                return View("Result", new AtomPaymentResultVM(
                    true, "Payment completed successfully.",
                    row?.PaymentTransactionId, row?.GatewayPaymentId, row?.Amount ?? 0, result.ReceiptId));
            }

            var message = isSuccessUrl
                ? result.Message + " Agar paisa kat gaya hai to thodi der me receipt ban jayegi, ya school office se sampark karein."
                : "Payment complete nahi hui. Aap dobara try kar sakte hain.";

            return View("Result", new AtomPaymentResultVM(
                false, message,
                row?.PaymentTransactionId, row?.GatewayPaymentId, row?.Amount ?? 0));
        }

        // Zoho webhook (server-to-server): signature verify -> session id nikalo -> CompleteAsync.
        // Webhook URL Zoho portal > Settings > Webhooks me: https://<domain>/Payment/Webhook
        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [SkipPermission]
        public async Task<IActionResult> Webhook()
        {
            var ct = HttpContext.RequestAborted;

            string raw;
            using (var reader = new StreamReader(Request.Body))
                raw = await reader.ReadToEndAsync(ct);

            var header = Request.Headers["X-Zoho-Webhook-Signature"].ToString();
            _logger.LogInformation("Zoho webhook received. Header={H} Body={B}", header, raw);

            if (string.IsNullOrWhiteSpace(header) ||
                !await _zohoPaymentService.VerifyWebhookSignatureAsync(header, raw, ct))
            {
                _logger.LogWarning("Zoho webhook signature invalid.");
                return Unauthorized();
            }

            // Replay protection: timestamp 5 minute se purana ho to reject
            var t = header.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Split('=', 2))
                .Where(kv => kv.Length == 2 && kv[0].Trim().Equals("t", StringComparison.OrdinalIgnoreCase))
                .Select(kv => kv[1].Trim())
                .FirstOrDefault();

            if (long.TryParse(t, out var ts))
            {
                // timestamp seconds ya milliseconds ho sakta hai
                var when = ts > 100_000_000_000 ? DateTimeOffset.FromUnixTimeMilliseconds(ts)
                                                : DateTimeOffset.FromUnixTimeSeconds(ts);
                if (Math.Abs((DateTimeOffset.UtcNow - when).TotalMinutes) > 5)
                {
                    _logger.LogWarning("Zoho webhook timestamp too old: {When}", when);
                    return Unauthorized();
                }
            }

            string? sessionId = null;
            try
            {
                using var doc = JsonDocument.Parse(raw);
                sessionId = FindString(doc.RootElement, "payments_session_id");
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Zoho webhook body JSON nahi hai.");
            }

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                // 200 dete hain taaki Zoho baar-baar retry na kare; payload log me hai
                _logger.LogWarning("Zoho webhook me payments_session_id nahi mila.");
                return Ok();
            }

            try
            {
                var result = await _zohoPaymentService.CompleteAsync(sessionId, ct);
                _logger.LogInformation("Zoho webhook processed. Session={S} Success={Ok} Msg={M}",
                    sessionId, result.Success, result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Zoho webhook processing failed. Session={S}", sessionId);
            }

            return Ok();
        }

        // JSON me kahin bhi nested "name" property dhundta hai (webhook payload structure ke liye)
        private static string? FindString(JsonElement el, string name)
        {
            switch (el.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var p in el.EnumerateObject())
                    {
                        if (p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                            p.Value.ValueKind == JsonValueKind.String)
                            return p.Value.GetString();

                        var found = FindString(p.Value, name);
                        if (found != null) return found;
                    }
                    break;
                case JsonValueKind.Array:
                    foreach (var item in el.EnumerateArray())
                    {
                        var found = FindString(item, name);
                        if (found != null) return found;
                    }
                    break;
            }
            return null;
        }

        #endregion

        #region Zoho OAuth one-time setup

        [HttpGet]
        // [Authorize(Roles = "Admin")]  // apni auth policy lagao
        public async Task<IActionResult> ZohoOAuthSetup(string? code, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                var settings = await _zohoPaymentService.GetSettingsAsync(cancellationToken);

                var redirectUri = Url.Action(nameof(ZohoOAuthSetup), "Payment", null, Request.Scheme) ?? "";

                // Sandbox: scope prefix ZohoPaySandbox aur soid zohopaysandbox.<account_id>
                var sandbox = settings.ApiBaseUrl.Contains("sandbox", StringComparison.OrdinalIgnoreCase);
                var scope = sandbox ? "ZohoPaySandbox" : "ZohoPay";
                var soid = sandbox ? "zohopaysandbox" : "zohopay";

                var authUrl =
                    $"{settings.AccountsBaseUrl}/oauth/v2/org/auth" +
                    $"?scope={scope}.payments.CREATE,{scope}.payments.READ,{scope}.payments.UPDATE" +
                    $"&client_id={Uri.EscapeDataString(settings.ClientId)}" +
                    $"&soid={soid}.{Uri.EscapeDataString(settings.AccountId)}" +
                    $"&state=zoho-oauth-setup" +
                    $"&response_type=code" +
                    $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                    $"&access_type=offline";

                return Redirect(authUrl);
            }

            try
            {
                var redirectUri = Url.Action(nameof(ZohoOAuthSetup), "Payment", null, Request.Scheme) ?? "";

                // Refresh token yahin encrypted (key/value "RefreshToken") save ho jata hai
                await _zohoPaymentService.ExchangeAuthorizationCodeAsync(code, redirectUri, cancellationToken);

                TempData["Success"] = "Zoho refresh token successfully generate aur save ho gaya.";
                return RedirectToAction("Index", "Settings");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Settings");
            }
        }

        public class ZohoWidgetConfirmRequest
        {
            public string? PaymentSessionId { get; set; } = string.Empty;
            public string? PaymentId { get; set; } = string.Empty;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SkipPermission]
        public async Task<IActionResult> ConfirmZohoWidgetPayment(
            [FromBody] ZohoWidgetConfirmRequest request)
        {
            var ct = HttpContext.RequestAborted;

            //if (string.IsNullOrWhiteSpace(request.PaymentSessionId) ||
            //    string.IsNullOrWhiteSpace(request.PaymentId))
            //{
            //    return BadRequest(new
            //    {
            //        success = false,
            //        message = "Invalid payment response."
            //    });
            //}
            if (string.IsNullOrWhiteSpace(request.PaymentSessionId))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid payment session."
                });
            }
            var transaction = await _context.PaymentTransactionDetail
                .FirstOrDefaultAsync(x =>
                    x.GatewayName == "ZOHO" &&
                    x.GatewayOrderId == request.PaymentSessionId, ct);

            if (transaction == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Payment transaction not found."
                });
            }

            try
            {
                // Server-side verification through Zoho.
                // Do not trust the browser's success response.
                //var result = await _zohoPaymentService.CompleteAsync(
                //    request.PaymentSessionId, ct);
                var result = await _zohoPaymentService.CompleteAsync(request.PaymentSessionId,request.PaymentId,ct);

                if (!result.Success)
                {
                    return Ok(new
                    {
                        success = false,
                        message = result.Message ??
                                  "Payment could not be verified."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Payment completed successfully.",
                    redirectUrl = Url.Action(nameof(PaymentResult),"Payment",new { id = transaction.PaymentTransactionId })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Zoho widget payment confirmation failed. Session={SessionId}",
                    request.PaymentSessionId);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Payment verification is pending. Please check again."
                });
            }
        }
        [HttpGet]
        [SkipPermission]
        public async Task<IActionResult> PaymentResult(long id)
        {
            var transaction = await _context.PaymentTransactionDetail
                .FirstOrDefaultAsync(x => x.PaymentTransactionId == id);

            if (transaction == null)
                return View("Result",
                    AtomPaymentResultVM.Failed("Transaction not found."));

            return View("Result", new AtomPaymentResultVM
            {
                Success = transaction.TransactionStatus == "Success",
                //Message = transaction.TransactionStatus == "Success"
                //    ? "Payment completed successfully."
                //    : transaction.TransactionError ?? "Payment is being verified.",
                Message = transaction.TransactionStatus switch
                {
                    "Success" => "Payment completed successfully.",
                    "Failed" => "Payment failed. Please try again.",
                    "Canceled" => "Payment was canceled.",
                    "Pending" => "Payment is being verified with Zoho.",
                    _ => transaction.TransactionError ?? "Payment status is unavailable."
                },
                PaymentTransactionId = transaction.PaymentTransactionId,
                ReferenceNo = transaction.GatewayPaymentId,
                Amount = transaction.Amount
            });
        }
        // PaymentController ke andar ye helper method add karein
        private async Task PopulateCheckoutDetailsAsync(
            AtomCheckoutVM checkoutVm,
            PaymentTransactionDetail transaction,
            Tbl_Students? student,
            CancellationToken cancellationToken)
        {
            // 1. Session se School Name & Address nikalna (Apni exact Session Keys check kar lein)
            checkoutVm.SchoolName = HttpContext.Session.GetString("SchoolName") ?? "Shiksha School";
            checkoutVm.SchoolAddress = HttpContext.Session.GetString("SchoolAddress") ?? "";
            checkoutVm.SchoolContactNO = HttpContext.Session.GetString("SchoolMobile") ?? "";

            // 2. Student Basic Details
            checkoutVm.StudentName = transaction.StudentName ?? string.Empty;
            checkoutVm.ApplicationNo = transaction.ApplicationNo ?? student?.ApplicationNo ?? "N/A";
            checkoutVm.FatherName = transaction.FatherName ?? "N/A";
            
            // Optional: Agar Class ka naam dikhana ho (Apni Class table ke hisab se check karein)
             var classObj = await _context.DataListItems.FirstOrDefaultAsync(c => c.DataListItemId == transaction.ClassId, cancellationToken);
            // checkoutVm.ClassName = classObj?.ClassName ?? "";
            checkoutVm.ClassName = classObj.DataListItemText ?? "N/A";
        }

        #endregion
    }

    // Views (Checkout.cshtml / Result.cshtml) in ko Shikhsa.Controllers.AtomCheckoutVM / AtomPaymentResultVM
    // naam se use karte hain, isliye ye PaymentController ke andar nahi, namespace level par hone chahiye.
    public class AtomCheckoutVM
    {
        public string MerchantTransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string AtomTokenId { get; set; } = string.Empty;
        public string MerchId { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerMobile { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string CheckoutCdn { get; set; } = string.Empty;
        public string Environment { get; set; } = "uat";
        public string Password { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string RequestEncryptKey { get; set; } = string.Empty;
        public string RequestSalt { get; set; } = string.Empty;
        public string ZohoAccountId { get; set; } = string.Empty;

        public string ZohoApiKey { get; set; } = string.Empty;

        public string ZohoCreateSessionUrl { get; set; } = string.Empty;

        public string ZohoVerifyPaymentUrl { get; set; } = string.Empty;
        public string? ZohoPaymentSessionId { get; set; }

        public bool ZohoIsTestMode { get; set; }

        public string? ZohoConfirmPaymentUrl { get; set; }

        public string SchoolName { get; set; } = string.Empty;
        public string SchoolAddress { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string ApplicationNo { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string SchoolContactNO { get; set; } = string.Empty;
    }

    public class AtomPaymentResultVM
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public long? PaymentTransactionId { get; set; }
        public decimal Amount { get; set; }
        public string? ReferenceNo { get; set; }
        public long? ReceiptId { get; set; }

        public AtomPaymentResultVM() { }

        public AtomPaymentResultVM(bool success, string message, long? paymentTransactionId,
            string? referenceNo, decimal amount, long? receiptId = null)
        {
            Success = success;
            Message = message;
            PaymentTransactionId = paymentTransactionId;
            ReferenceNo = referenceNo;
            Amount = amount;
            ReceiptId = receiptId;
        }

        public static AtomPaymentResultVM Failed(string message)
            => new AtomPaymentResultVM { Success = false, Message = message };
        public string ZohoPaymentSessionId { get; set; } = string.Empty;
        public bool ZohoIsTestMode { get; set; }

        public string ZohoConfirmPaymentUrl { get; set; } = string.Empty;
    }
}