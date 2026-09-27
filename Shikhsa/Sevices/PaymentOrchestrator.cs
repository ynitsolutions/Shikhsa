//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Options;
//using Shikhsa.Data;
//using Shikhsa.DataBase.Repositry;
//using Shikhsa.Models;
//using Shikhsa.Models.Payment;
//using Shikhsa.Repository;
//using Shikhsa.Services;
//using Shikhsa.ViewModels;
//using System.Text.Json;

//namespace Shikhsa.Services.Payment
//{
//    /// <summary>
//    /// Orchestrates the complete payment lifecycle: initiation, callback handling,
//    /// and webhook processing. Contains all business logic — controllers stay thin.
//    /// </summary>
//    public sealed class PaymentOrchestrator 
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly ZohoApiClient _zohoApi;
//        private readonly ZohoSignatureService _signatureService;
//        private readonly FeeHeadingRepository _repository;
//        private readonly ILogger<PaymentOrchestrator> _logger;
//        private readonly ZohoOptions _zohoOptions;

//        public PaymentOrchestrator(
//            ApplicationDbContext context,
//            ZohoApiClient zohoApi,
//            ZohoSignatureService signatureService,
//            FeeHeadingRepository repository,
//            ILogger<PaymentOrchestrator> logger,
//            IOptions<ZohoOptions> zohoOptions)
//        {
//            _context = context;
//            _zohoApi = zohoApi;
//            //_signatureService = signatureService;
//            _repository = repository;
//            _logger = logger;
//            _zohoOptions = zohoOptions.Value;
//        }

//        // =========================================================
//        // INITIATE: Create pending transaction + payment session
//        // =========================================================

//        public async Task<PaymentInitiationResult> InitiateAsync(
//            StudentFeeReceiptVM vm, string gatewayName, CancellationToken ct = default)
//        {
//            try
//            {
//                // Idempotency guard: check for recent pending transaction
//                var recentPending = await _context.PaymentTransactionDetail
//                    .Where(x => x.StudentId == vm.StudentId
//                                && x.TransactionStatus == "Pending"
//                                && x.GatewayName == gatewayName
//                                && x.TxnDate > DateTime.Now.AddMinutes(-15))
//                    .OrderByDescending(x => x.TxnDate)
//                    .FirstOrDefaultAsync(ct);

//                if (recentPending != null && !string.IsNullOrWhiteSpace(recentPending.GatewayOrderId))
//                {
//                    _logger.LogInformation(
//                        "Reusing recent pending transaction {TransactionId} for Student {StudentId}",
//                        recentPending.TransactionId, vm.StudentId);

//                    var checkoutUrl = $"{_zohoOptions.CheckoutBaseUrl}/hostedcheckout/{recentPending.GatewayOrderId}";
//                    return new PaymentInitiationResult(true, checkoutUrl, null, recentPending.PaymentTransactionId);
//                }

//                // Create new pending transaction inside a DB transaction
//                await using var dbTransaction = await _context.Database.BeginTransactionAsync(ct);

//                var transaction = await CreatePendingTransactionAsync(vm, gatewayName, ct);
//                await _context.SaveChangesAsync(ct);

//                var student = await _context.Tbl_Students
//                    .AsNoTracking()
//                    .FirstOrDefaultAsync(x => x.StudentId == vm.StudentId, ct);

//                if (student == null)
//                    return new PaymentInitiationResult(false, null, "Student not found.", null);

//                var email = student.Email ?? string.Empty;
//                var mobile = student.ContactNo ?? string.Empty;

//                var session = await _zohoApi.CreatePaymentSessionAsync(
//                    transaction.Amount, _zohoOptions.Currency, email, mobile,
//                    $"Fee payment for {student.ApplicationNo}",
//                    transaction.StudentId.ToString(),
//                    transaction.PaymentTransactionId.ToString(),
//                    vm.ClassId.ToString(), vm.BatchId.ToString(), "",
//                    ct);

//                transaction.TransactionStatus = session.Success ? "Pending" : "Failed";
//                transaction.TransactionError = session.Success ? null : session.ErrorMessage;
//                transaction.GatewayResponse = session.RawResponse;
//                transaction.GatewayOrderId = session.PaymentsSessionId;
//                transaction.TransactionId = session.PaymentsSessionId;

//                await _context.SaveChangesAsync(ct);
//                await dbTransaction.CommitAsync(ct);

//                if (!session.Success || string.IsNullOrWhiteSpace(session.AccessKey))
//                {
//                    return new PaymentInitiationResult(false, null,
//                        session.ErrorMessage ?? "Unable to initiate online payment.", transaction.PaymentTransactionId);
//                }

//                var redirectUrl = $"{_zohoOptions.CheckoutBaseUrl}/hostedcheckout/{session.AccessKey}";
//                return new PaymentInitiationResult(true, redirectUrl, null, transaction.PaymentTransactionId);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error initiating payment for StudentId {StudentId}", vm.StudentId);
//                return new PaymentInitiationResult(false, null,
//                    "Unable to initiate payment. Please try again.", null);
//            }
//        }

//        // =========================================================
//        // CALLBACK: Handle redirect from Zoho (success/failure)
//        // =========================================================

//        public async Task<PaymentCallbackResult> HandleRedirectAsync(
//            string paymentsSessionId, string paymentSessionStatus, string? paymentId,
//            string? paymentStatus, string? amount, string? mandateId,
//            string? udf1, string? udf2, string? udf3, string? udf4, string? udf5,
//            string signature, CancellationToken ct = default)
//        {
//            var signatureValid = _signatureService.VerifyRedirectSignature(
//                paymentsSessionId, paymentSessionStatus, paymentId, paymentStatus,
//                amount, mandateId, udf1, udf2, udf3, udf4, udf5, signature);

//            if (!signatureValid)
//            {
//                _logger.LogWarning("Invalid Zoho redirect signature. SessionId={SessionId}", paymentsSessionId);
//                return new PaymentCallbackResult(false, "Invalid redirect signature.", null, null, 0, null);
//            }

//            var transaction = await _context.PaymentTransactionDetail
//                .Include(x => x.FeeDetails)
//                .Include(x => x.Receipts)
//                .FirstOrDefaultAsync(x => x.GatewayOrderId == paymentsSessionId, ct);

//            if (transaction == null)
//                return new PaymentCallbackResult(false, "Transaction not found.", null, null, 0, null);

//            if (transaction.TransactionStatus == "Success")
//            {
//                return new PaymentCallbackResult(true, "Payment already processed.",
//                    transaction.PaymentTransactionId, transaction.ReferenceNo, transaction.Amount, null);
//            }

//            if (paymentSessionStatus == "in_progress")
//            {
//                transaction.TransactionStatus = "Pending";
//                transaction.TransactionError = "Bank transfer in progress.";
//                await _context.SaveChangesAsync(ct);

//                return new PaymentCallbackResult(false,
//                    "Payment is being processed. You will be notified once confirmed.",
//                    transaction.PaymentTransactionId, null, transaction.Amount, null);
//            }

//            if (paymentSessionStatus != "succeeded" || paymentStatus != "succeeded")
//            {
//                transaction.TransactionStatus = "Failed";
//                transaction.TransactionError = "Payment failed at gateway.";
//                transaction.GatewayPaymentId = paymentId;
//                transaction.ReferenceNo = paymentId;
//                await _context.SaveChangesAsync(ct);

//                return new PaymentCallbackResult(false, transaction.TransactionError,
//                    transaction.PaymentTransactionId, paymentId, transaction.Amount, null);
//            }

//            if (decimal.TryParse(amount, out var redirectAmount) &&
//                Math.Abs(transaction.Amount - redirectAmount) > 0.01m)
//            {
//                transaction.TransactionStatus = "Failed";
//                transaction.TransactionError = "Gateway amount mismatch.";
//                await _context.SaveChangesAsync(ct);

//                _logger.LogWarning(
//                    "Zoho amount mismatch. Local={LocalAmount}, Gateway={GatewayAmount}, SessionId={SessionId}",
//                    transaction.Amount, redirectAmount, paymentsSessionId);

//                return new PaymentCallbackResult(false, "Payment amount verification failed.",
//                    transaction.PaymentTransactionId, paymentId, transaction.Amount, null);
//            }

//            transaction.TransactionStatus = "Success";
//            transaction.GatewayPaymentId = paymentId;
//            transaction.ReferenceNo = paymentId;
//            transaction.PaymentCompletedOn = DateTime.Now;

//            await _context.SaveChangesAsync(ct);

//            var receiptId = await FinalizeReceiptAsync(transaction, ct);

//            return new PaymentCallbackResult(true, "Payment completed successfully.",
//                transaction.PaymentTransactionId, paymentId, transaction.Amount, receiptId);
//        }

//        // =========================================================
//        // WEBHOOK: Authoritative source of truth
//        // =========================================================

//        public async Task<bool> HandleWebhookAsync(string rawPayload, CancellationToken ct = default)
//        {
//            try
//            {
//                using var doc = JsonDocument.Parse(rawPayload);
//                var root = doc.RootElement;

//                var eventType = root.TryGetProperty("event_type", out var et) ? et.GetString() : null;

//                JsonElement payload;
//                if (!root.TryGetProperty("payload", out payload) &&
//                    !root.TryGetProperty("data", out payload))
//                {
//                    payload = root;
//                }

//                var sessionId = payload.TryGetProperty("payments_session_id", out var sid) ? sid.GetString() : null;

//                if (string.IsNullOrWhiteSpace(sessionId))
//                {
//                    _logger.LogWarning("Zoho webhook missing payments_session_id.");
//                    return true;
//                }

//                var transaction = await _context.PaymentTransactionDetail
//                    .Include(x => x.FeeDetails)
//                    .Include(x => x.Receipts)
//                    .FirstOrDefaultAsync(x => x.GatewayOrderId == sessionId, ct);

//                if (transaction == null)
//                {
//                    _logger.LogWarning("Zoho webhook: transaction not found. SessionId={SessionId}", sessionId);
//                    return true;
//                }

//                if (transaction.TransactionStatus == "Success")
//                    return true;

//                var paymentId = payload.TryGetProperty("payment_id", out var pid) ? pid.GetString() : null;

//                decimal? webhookAmount = null;
//                if (payload.TryGetProperty("amount", out var amtEl) &&
//                    decimal.TryParse(amtEl.ToString(), out var parsedAmt))
//                {
//                    webhookAmount = parsedAmt;
//                }

//                if (webhookAmount.HasValue &&
//                    Math.Abs(transaction.Amount - webhookAmount.Value) > 0.01m)
//                {
//                    _logger.LogWarning(
//                        "Webhook amount mismatch. Local={Local}, Gateway={Gateway}, SessionId={SessionId}",
//                        transaction.Amount, webhookAmount.Value, sessionId);

//                    transaction.TransactionStatus = "Failed";
//                    transaction.TransactionError = "Webhook amount mismatch.";
//                    await _context.SaveChangesAsync(ct);
//                    return true;
//                }

//                transaction.GatewayResponse = rawPayload;

//                if (eventType == "payment.success")
//                {
//                    transaction.TransactionStatus = "Success";
//                    transaction.GatewayPaymentId = paymentId;
//                    transaction.ReferenceNo = paymentId;
//                    transaction.PaymentCompletedOn = DateTime.Now;

//                    await _context.SaveChangesAsync(ct);
//                    await FinalizeReceiptAsync(transaction, ct);
//                }
//                else if (eventType == "payment.failed")
//                {
//                    transaction.TransactionStatus = "Failed";
//                    transaction.TransactionError = "Payment failed (via webhook).";
//                    await _context.SaveChangesAsync(ct);
//                }

//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Zoho webhook processing failed.");
//                return false;
//            }
//        }

//        // =========================================================
//        // PRIVATE: Create Pending Transaction (with DB transaction)
//        // =========================================================

//        private async Task<PaymentTransactionDetail> CreatePendingTransactionAsync(
//            StudentFeeReceiptVM vm, string gatewayName, CancellationToken ct)
//        {
//            var selected = vm.TuitionFees
//                .Where(x => x.IsSelected && x.CollectAmount > 0)
//                .Concat(vm.TransportFees.Where(x => x.IsSelected && x.CollectAmount > 0))
//                .Concat(vm.HostelFees.Where(x => x.IsSelected && x.CollectAmount > 0))
//                .ToList();

//            if (selected.Count == 0)
//                throw new InvalidOperationException("No fee selected.");

//            var collectAmount = selected.Sum(x => x.CollectAmount);
//            var paymentAmount = Math.Max(0, collectAmount + vm.LateFee - vm.ConcessionAmount);

//            if (paymentAmount <= 0)
//                throw new InvalidOperationException("Payment amount must be greater than zero.");

//            var student = await _context.Tbl_Students
//                .AsNoTracking()
//                .FirstOrDefaultAsync(x => x.StudentId == vm.StudentId && x.IsActive, ct);

//            if (student == null)
//                throw new InvalidOperationException("Student not found.");

//            var merchantTxnId = $"SHK{DateTime.Now:yyyyMMddHHmmssfff}{Random.Shared.Next(100, 999)}";

//            var transaction = new PaymentTransactionDetail
//            {
//                StudentId = vm.StudentId,
//                ApplicationNo = student.ApplicationNo,
//                ClassId = vm.ClassId,
//                BatchId = vm.BatchId,
//                StudentName = $"{student.FirstName} {student.MiddleName} {student.LastName}"
//                    .Replace("  ", " ").Trim(),
//                PaymentModeId = vm.PaymentModeId,
//                Amount = paymentAmount,
//                TxnDate = DateTime.Now,
//                TransactionStatus = "Pending",
//                TransactionId = merchantTxnId,
//                GatewayName = gatewayName,
//                GatewayOrderId = merchantTxnId,
//                Type = "Fee",
//                IsActive = true,
//                GatewaySignature = JsonSerializer.Serialize(new
//                {
//                    vm.LateFee,
//                    vm.Concession,
//                    vm.ConcessionAmount,
//                    vm.Remark
//                })
//            };

//            foreach (var item in selected)
//            {
//                var studentFee = await _context.StudentFees
//                    .FirstOrDefaultAsync(x => x.StudentId == vm.StudentId
//                                              && x.FeeId == item.FeeId
//                                              && x.Month == item.Month
//                                              && x.Year == item.Year, ct);

//                if (studentFee == null)
//                {
//                    studentFee = new StudentFee
//                    {
//                        StudentId = vm.StudentId,
//                        ApplicationNo = student.ApplicationNo,
//                        FeeId = item.FeeId,
//                        ClassId = vm.ClassId,
//                        BatchId = vm.BatchId,
//                        Month = item.Month,
//                        Year = item.Year,
//                        FeeType = item.FeeType,
//                        FeeAmount = item.Amount,
//                        PaidAmount = 0,
//                        BalanceAmount = item.Amount,
//                        IsFullyPaid = false,
//                        IsActive = true
//                    };

//                    _context.StudentFees.Add(studentFee);
//                    await _context.SaveChangesAsync(ct);
//                }

//                transaction.FeeDetails.Add(new PaymentTransactionFeeDetail
//                {
//                    StudentFeeId = studentFee.StudentFeeId,
//                    FeeId = item.FeeId,
//                    FeeType = item.FeeType,
//                    Month = item.Month,
//                    Year = item.Year,
//                    FeeAmount = item.Amount,
//                    PaidAmount = item.CollectAmount,
//                    AdjustedAmount = 0,
//                    IsActive = true
//                });
//            }

//            _context.PaymentTransactionDetail.Add(transaction);
//            await _context.SaveChangesAsync(ct);

//            return transaction;
//        }

//        // =========================================================
//        // PRIVATE: Finalize Receipt (safe, idempotent)
//        // =========================================================

//        private async Task<long> FinalizeReceiptAsync(
//            PaymentTransactionDetail transaction, CancellationToken ct)
//        {
//            var existingReceipt = await _context.FeeReceipt
//                .AsNoTracking()
//                .FirstOrDefaultAsync(x => x.PaymentTransactionId == transaction.PaymentTransactionId, ct);

//            if (existingReceipt != null)
//                return existingReceipt.FeeReceiptId;

//            if (transaction.Receipts.Any())
//                return transaction.Receipts.First().FeeReceiptId;

//            decimal lateFee = 0m, concessionAmount = 0m;
//            string? concession = null, remark = null;

//            if (!string.IsNullOrWhiteSpace(transaction.GatewaySignature))
//            {
//                try
//                {
//                    using var meta = JsonDocument.Parse(transaction.GatewaySignature);
//                    var root = meta.RootElement;
//                    lateFee = root.TryGetProperty("LateFee", out var late) ? late.GetDecimal() : 0m;
//                    concessionAmount = root.TryGetProperty("ConcessionAmount", out var ca) ? ca.GetDecimal() : 0m;
//                    concession = root.TryGetProperty("Concession", out var c) ? c.GetString() : null;
//                    remark = root.TryGetProperty("Remark", out var r) ? r.GetString() : null;
//                }
//                catch (JsonException ex)
//                {
//                    _logger.LogWarning(ex,
//                        "Unable to read payment metadata for PaymentTransactionId {Id}",
//                        transaction.PaymentTransactionId);
//                }
//            }

//            var items = transaction.FeeDetails
//                .Select(x => new StudentFeeReceiptItemVM
//                {
//                    FeeId = x.FeeId,
//                    FeePlanId = x.StudentFeeId,
//                    FeeType = x.FeeType,
//                    FeeDescription = x.FeeType,
//                    Month = x.Month,
//                    Year = x.Year,
//                    Amount = x.FeeAmount,
//                    CollectAmount = x.PaidAmount,
//                    IsSelected = true
//                }).ToList();

//            var vm = new StudentFeeReceiptVM
//            {
//                StudentId = transaction.StudentId,
//                PaymentModeId = transaction.PaymentModeId,
//                ReceiptDate = DateTime.Now,
//                LateFee = lateFee,
//                Concession = concession ?? "",
//                ConcessionAmount = concessionAmount,
//                Remark = remark
//            };

//            var student = await _context.Tbl_Students
//                .AsNoTracking()
//                .FirstAsync(x => x.StudentId == transaction.StudentId, ct);

//            vm.ClassId = student.AdmitClassId ?? 0;
//            vm.BatchId = student.AdmitBatchId ?? 0;

//            vm.TuitionFees = items.Where(x => x.FeeType.Equals("Tuition", StringComparison.OrdinalIgnoreCase)).ToList();
//            vm.TransportFees = items.Where(x => x.FeeType.Equals("Transport", StringComparison.OrdinalIgnoreCase)).ToList();
//            vm.HostelFees = items.Where(x => x.FeeType.Equals("Hostel", StringComparison.OrdinalIgnoreCase)).ToList();

//            long receiptId;
//            try
//            {
//                receiptId = _repository.SaveCashFeeReceipt(vm);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex,
//                    "Failed to save receipt for PaymentTransactionId {Id}. Manual reconciliation required.",
//                    transaction.PaymentTransactionId);

//                transaction.TransactionError = "Payment success but receipt save failed. Admin will reconcile.";
//                await _context.SaveChangesAsync(ct);

//                throw;
//            }

//            var receipt = await _context.FeeReceipt.FirstAsync(x => x.FeeReceiptId == receiptId, ct);
//            receipt.PaymentTransactionId = transaction.PaymentTransactionId;
//            await _context.SaveChangesAsync(ct);

//            return receiptId;
//        }
//    }
//}