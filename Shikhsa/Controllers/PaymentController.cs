
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
                ErrorMessage("Payment data invalid hai. Dobara try karo.");
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
                IsActive = true
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

        private async Task<IActionResult> InitiateZohoAsync(
            PaymentTransactionDetail transaction, StudentFeeReceiptVM vm, Tbl_Students? student,
            CancellationToken cancellationToken)
        {
            try
            {
                var email = student?.Email ?? string.Empty;
                var mobile = student?.ContactNo ?? string.Empty;

                //var session = await _zohoPaymentService.CreatePaymentSessionAsync(
                //    transaction.Amount, "INR", email, mobile,
                //    $"Fee payment for {student?.ApplicationNo}",
                //    transaction.StudentId.ToString(),
                //    transaction.PaymentTransactionId.ToString(),
                //    vm.ClassId.ToString(), vm.BatchId.ToString(), "",
                //    cancellationToken);
                var session = await _zohoPaymentService.CreatePaymentSessionAsync(
                                transaction.Amount,
                                "INR",
                                email,
                                mobile,
                                $"Fee payment for {student?.ApplicationNo}",
                                student != null ? $"{student.FirstName} {student.LastName}".Trim() : "",
                                "IN",
                                transaction.StudentId.ToString(),
                                transaction.PaymentTransactionId.ToString(),
                                vm.ClassId.ToString(),
                                vm.BatchId.ToString(),
                                "",
                                cancellationToken);
                transaction.TransactionStatus = session.Success ? "Pending" : "Failed";
                transaction.TransactionError = session.Success ? null : session.ErrorMessage;
                transaction.GatewayResponse = session.RawResponse;
                transaction.GatewayOrderId = session.PaymentsSessionId;
                transaction.TransactionId = session.PaymentsSessionId;

                await _context.SaveChangesAsync(cancellationToken);

                if (!session.Success || string.IsNullOrWhiteSpace(session.AccessKey))
                {
                    ErrorMessage(session.ErrorMessage ?? "Unable to initiate online payment.");
                    return RedirectToAction("FeeReceipt", "FeeHeading");
                }

                var settings = await _zohoPaymentService.GetSettingsAsync(cancellationToken);

                // NOTE: /hostedpages/ verified Zoho docs se — apne account par
                // ek test payment karke confirm kar lena
                return Redirect($"{settings.CheckoutBaseUrl}/hostedpages/{session.AccessKey}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while initiating Zoho fee payment for StudentId {StudentId}", vm.StudentId);

                transaction.TransactionStatus = "Failed";
                transaction.TransactionError = "Unable to initiate payment. Please try again.";
                await _context.SaveChangesAsync(cancellationToken);

                ErrorMessage(transaction.TransactionError);
                return RedirectToAction("FeeReceipt", "FeeHeading");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Success(
            string payments_session_id, string payment_session_status, string? payment_id,
            string? payment_status, string? amount, string? mandate_id,
            string? udf1, string? udf2, string? udf3, string? udf4, string? udf5,
            string signature, CancellationToken cancellationToken)
        {
            return await HandleZohoRedirectAsync(
                payments_session_id, payment_session_status, payment_id, payment_status,
                amount, mandate_id, udf1, udf2, udf3, udf4, udf5, signature, cancellationToken);
        }

        [HttpGet]
        public async Task<IActionResult> Failure(
            string payments_session_id, string payment_session_status, string? payment_id,
            string? payment_status, string? amount, string? mandate_id,
            string? udf1, string? udf2, string? udf3, string? udf4, string? udf5,
            string signature, CancellationToken cancellationToken)
        {
            return await HandleZohoRedirectAsync(
                payments_session_id, payment_session_status, payment_id, payment_status,
                amount, mandate_id, udf1, udf2, udf3, udf4, udf5, signature, cancellationToken);
        }

        private async Task<IActionResult> HandleZohoRedirectAsync(
            string paymentsSessionId, string paymentSessionStatus, string? paymentId,
            string? paymentStatus, string? amount, string? mandateId,
            string? udf1, string? udf2, string? udf3, string? udf4, string? udf5,
            string signature, CancellationToken cancellationToken)
        {
            var signatureValid = await _zohoPaymentService.VerifyRedirectSignatureAsync(
                paymentsSessionId, paymentSessionStatus, paymentId, paymentStatus,
                amount, mandateId, udf1, udf2, udf3, udf4, udf5, signature, cancellationToken);

            if (!signatureValid)
            {
                _logger.LogWarning("Invalid Zoho redirect signature. SessionId={SessionId}", paymentsSessionId);
                return View("Result", AtomPaymentResultVM.Failed("Invalid redirect signature."));
            }

            var transaction = await _context.PaymentTransactionDetail
                .Include(x => x.FeeDetails)
                .Include(x => x.Receipts)
                .FirstOrDefaultAsync(x => x.GatewayOrderId == paymentsSessionId, cancellationToken);

            if (transaction == null)
                return View("Result", AtomPaymentResultVM.Failed("Transaction not found."));

            if (transaction.TransactionStatus == "Success")
            {
                return View("Result", new AtomPaymentResultVM(
                    true, "Payment already processed.",
                    transaction.PaymentTransactionId, transaction.ReferenceNo, transaction.Amount));
            }

            if (paymentSessionStatus == "in_progress")
            {
                transaction.TransactionStatus = "Pending";
                transaction.TransactionError = "Bank transfer in progress.";
                await _context.SaveChangesAsync(cancellationToken);

                return View("Result", AtomPaymentResultVM.Failed(
                    "Payment is being processed. You will be notified once confirmed."));
            }

            if (paymentSessionStatus != "succeeded" || paymentStatus != "succeeded")
            {
                transaction.TransactionStatus = "Failed";
                transaction.TransactionError = "Payment failed at gateway.";
                transaction.GatewayPaymentId = paymentId;
                transaction.ReferenceNo = paymentId;
                await _context.SaveChangesAsync(cancellationToken);

                return View("Result", new AtomPaymentResultVM(
                    false, transaction.TransactionError, transaction.PaymentTransactionId,
                    paymentId, transaction.Amount));
            }

            if (decimal.TryParse(amount, out var redirectAmount) &&
                Math.Abs(transaction.Amount - redirectAmount) > 0.01m)
            {
                transaction.TransactionStatus = "Failed";
                transaction.TransactionError = "Gateway amount mismatch.";
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(
                    "Zoho amount mismatch. Local={LocalAmount}, Gateway={GatewayAmount}, SessionId={SessionId}",
                    transaction.Amount, redirectAmount, paymentsSessionId);

                return View("Result", AtomPaymentResultVM.Failed("Payment amount verification failed."));
            }

            transaction.TransactionStatus = "Success";
            transaction.GatewayPaymentId = paymentId;
            transaction.ReferenceNo = paymentId;
            transaction.PaymentCompletedOn = DateTime.Now;

            await _context.SaveChangesAsync(cancellationToken);

            var receiptId = await FinalizeReceiptAsync(transaction, cancellationToken);

            return View("Result", new AtomPaymentResultVM(
                true, "Payment completed successfully.",
                transaction.PaymentTransactionId, paymentId, transaction.Amount, receiptId));
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        [SkipPermission]
        public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
        {
            Request.EnableBuffering();

            using var reader = new StreamReader(Request.Body, leaveOpen: true);
            var rawPayload = await reader.ReadToEndAsync(cancellationToken);
            Request.Body.Position = 0;

            if (!Request.Headers.TryGetValue("X-Zoho-Webhook-Signature", out var signatureHeader))
            {
                _logger.LogWarning("Zoho webhook missing signature header.");
                return BadRequest();
            }

            var valid = await _zohoPaymentService.VerifyWebhookSignatureAsync(
                signatureHeader.ToString(), rawPayload, cancellationToken);

            if (!valid)
            {
                _logger.LogWarning("Invalid Zoho webhook signature.");
                return Unauthorized();
            }

            try
            {
                using var doc = JsonDocument.Parse(rawPayload);
                var root = doc.RootElement;

                var eventType = root.TryGetProperty("event_type", out var et) ? et.GetString() : null;

                if (!root.TryGetProperty("payload", out var payload) &&
                    !root.TryGetProperty("data", out payload))
                {
                    payload = root;
                }

                var sessionId = payload.TryGetProperty("payments_session_id", out var sid) ? sid.GetString() : null;

                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    _logger.LogWarning("Zoho webhook missing payments_session_id.");
                    return Ok();
                }

                var transaction = await _context.PaymentTransactionDetail
                    .Include(x => x.FeeDetails)
                    .Include(x => x.Receipts)
                    .FirstOrDefaultAsync(x => x.GatewayOrderId == sessionId, cancellationToken);

                if (transaction == null)
                {
                    _logger.LogWarning("Zoho webhook: transaction not found. SessionId={SessionId}", sessionId);
                    return Ok();
                }

                if (transaction.TransactionStatus == "Success")
                    return Ok();

                var paymentId = payload.TryGetProperty("payment_id", out var pid) ? pid.GetString() : null;

                transaction.GatewayResponse = rawPayload;

                if (eventType == "payment.success")
                {
                    transaction.TransactionStatus = "Success";
                    transaction.GatewayPaymentId = paymentId;
                    transaction.ReferenceNo = paymentId;
                    transaction.PaymentCompletedOn = DateTime.Now;

                    await _context.SaveChangesAsync(cancellationToken);
                    await FinalizeReceiptAsync(transaction, cancellationToken);
                }
                else if (eventType == "payment.failed")
                {
                    transaction.TransactionStatus = "Failed";
                    transaction.TransactionError = "Payment failed (via webhook).";
                    await _context.SaveChangesAsync(cancellationToken);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Zoho webhook processing failed.");
                return Ok();
            }
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

                var authUrl =
                    $"{settings.AccountsBaseUrl}/oauth/v2/auth" +
                    $"?scope=ZohoPay.payments.CREATE,ZohoPay.payments.READ,ZohoPay.payments.UPDATE" +
                    $"&client_id={Uri.EscapeDataString(settings.ClientId)}" +
                    $"&soid=zohopay.{Uri.EscapeDataString(settings.AccountId)}" +
                    $"&state=zoho-oauth-setup" +
                    $"&response_type=code" +
                    $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                    $"&access_type=offline";

                return Redirect(authUrl);
            }

            try
            {
                var redirectUri = Url.Action(nameof(ZohoOAuthSetup), "Payment", null, Request.Scheme) ?? "";

                var result = await _zohoPaymentService.ExchangeAuthorizationCodeAsync(code, redirectUri, cancellationToken);

                var setting = await _context.PaymentGatewaySettings
                    .FirstOrDefaultAsync(x => x.GatewayName == "ZOHO", cancellationToken);

                if (setting == null)
                {
                    TempData["Error"] = "ZOHO gateway row DB me nahi mili — pehle wo create karo.";
                    return RedirectToAction("Index", "Settings");
                }

                setting.ZohoRefreshToken = result.RefreshToken;
                setting.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

                TempData["Success"] = "Zoho refresh token successfully generate aur save ho gaya.";
                return RedirectToAction("Index", "Settings");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Settings");
            }
        }

        #endregion

        #region Shared: Receipt finalization (dono gateways use karte hain)

        private async Task<long> FinalizeReceiptAsync(PaymentTransactionDetail transaction, CancellationToken cancellationToken)
        {
            var existingReceipt = await _context.FeeReceipt
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PaymentTransactionId == transaction.PaymentTransactionId, cancellationToken);

            if (existingReceipt != null)
                return existingReceipt.FeeReceiptId;

            if (transaction.Receipts.Any())
                return transaction.Receipts.First().FeeReceiptId;

            decimal lateFee = 0m, concessionAmount = 0m;
            string? concession = null, remark = null;

            if (!string.IsNullOrWhiteSpace(transaction.GatewaySignature))
            {
                try
                {
                    using var meta = JsonDocument.Parse(transaction.GatewaySignature);
                    var root = meta.RootElement;

                    lateFee = root.TryGetProperty("LateFee", out var late) ? late.GetDecimal() : 0m;
                    concessionAmount = root.TryGetProperty("ConcessionAmount", out var ca) ? ca.GetDecimal() : 0m;
                    concession = root.TryGetProperty("Concession", out var c) ? c.GetString() : null;
                    remark = root.TryGetProperty("Remark", out var r) ? r.GetString() : null;
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex,
                        "Unable to read payment metadata for PaymentTransactionId {Id}", transaction.PaymentTransactionId);
                }
            }

            var items = transaction.FeeDetails
                .Select(x => new StudentFeeReceiptItemVM
                {
                    FeeId = x.FeeId,
                    FeePlanId = x.StudentFeeId,
                    FeeType = x.FeeType,
                    FeeDescription = x.FeeType,
                    Month = x.Month,
                    Year = x.Year,
                    Amount = x.FeeAmount,
                    CollectAmount = x.PaidAmount,
                    IsSelected = true
                }).ToList();

            var vm = new StudentFeeReceiptVM
            {
                StudentId = transaction.StudentId,
                PaymentModeId = transaction.PaymentModeId,
                ReceiptDate = DateTime.Now,
                LateFee = lateFee,
                Concession = concession ?? "",
                ConcessionAmount = concessionAmount,
                Remark = remark
            };

            var student = await _context.Tbl_Students
                .AsNoTracking()
                .FirstAsync(x => x.StudentId == transaction.StudentId, cancellationToken);

            vm.ClassId = student.AdmitClassId ?? 0;
            vm.BatchId = student.AdmitBatchId ?? 0;

            vm.TuitionFees = items.Where(x => x.FeeType.Equals("Tuition", StringComparison.OrdinalIgnoreCase)).ToList();
            vm.TransportFees = items.Where(x => x.FeeType.Equals("Transport", StringComparison.OrdinalIgnoreCase)).ToList();
            vm.HostelFees = items.Where(x => x.FeeType.Equals("Hostel", StringComparison.OrdinalIgnoreCase)).ToList();

            long receiptId;
            try
            {
                receiptId = _repository.SaveCashFeeReceipt(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to save receipt for PaymentTransactionId {Id}. Manual reconciliation required.",
                    transaction.PaymentTransactionId);

                transaction.TransactionError = "Payment success but receipt save failed. Admin will reconcile.";
                await _context.SaveChangesAsync(cancellationToken);

                throw;
            }

            var receipt = await _context.FeeReceipt.FirstAsync(x => x.FeeReceiptId == receiptId, cancellationToken);
            receipt.PaymentTransactionId = transaction.PaymentTransactionId;
            await _context.SaveChangesAsync(cancellationToken);

            return receiptId;
        }

        #endregion
    }

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
    }
}