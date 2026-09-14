//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Shikhsa.Attributes;
//using Shikhsa.Data;
//using Shikhsa.DataBase.Repositry;
//using Shikhsa.Models;
//using Shikhsa.Models.Payment;
//using Shikhsa.Repository;
//using Shikhsa.Services;
//using Shikhsa.ViewModels;
//using System.Text.Json;

//namespace Shikhsa.Controllers
//{
//    public class PaymentController : BaseController
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly FeeHeadingRepository _repository;
//        private readonly AtomPaymentService _atomPaymentService;

//        private const string SessionKey = "PendingOnlineFeeReceipt";

//        public PaymentController(
//            ApplicationDbContext context,
//            FeeHeadingRepository repository,
//            AtomPaymentService atomPaymentService,
//            UserManager<ApplicationUser> userManager,
//            PermissionService permissionService,
//            EmailService email,
//            LookupService lookup)
//            : base(userManager, permissionService, context, email, lookup)
//        {
//            _context = context;
//            _repository = repository;
//            _atomPaymentService = atomPaymentService;
//        }

//        #region Initiate

//        // FeeHeadingController.SaveFeeReceipt yahan par redirect karta hai
//        // jab PaymentMode == "Online". Pending receipt data already
//        // Session["PendingOnlineFeeReceipt"] me JSON ke form me pada hai.
//        [HttpGet]
//        [SkipPermission]
//        public async Task<IActionResult> InitiateFromSession()
//        {
//            var json = HttpContext.Session.GetString(SessionKey);

//            if (string.IsNullOrWhiteSpace(json))
//            {
//                ErrorMessage("Payment session expire ho gaya. Dobara try karo.");
//                return RedirectToAction("FeeReceipt", "FeeHeading");
//            }

//            StudentFeeReceiptVM? vm;
//            try
//            {
//                vm = JsonSerializer.Deserialize<StudentFeeReceiptVM>(json);
//                vm.TotalAmount = vm.ReceiptAmount;
//            }
//            catch
//            {
//                vm = null;
//            }

//            if (vm == null || vm.TotalAmount <= 0)
//            {
//                ErrorMessage("Payment data invalid hai. Dobara try karo.");
//                return RedirectToAction("FeeReceipt", "FeeHeading");
//            }

//            var student = await _context.Tbl_Students
//                .FirstOrDefaultAsync(x => x.StudentId == vm.StudentId);

//            // ============================================
//            // PENDING TRANSACTION ROW BANAO
//            // ============================================
//            var transaction = new PaymentTransactionDetail
//            {
//                StudentId = vm.StudentId,
//                ClassId = vm.ClassId,
//                BatchId = vm.BatchId,
//                StudentName = vm.StudentName ?? $"{student?.FirstName} {student?.MiddleName} {student?.LastName}".Replace("  ", " ").Trim(),
//                Amount = vm.TotalAmount,
//                TxnDate = DateTime.Now,
//                TransactionStatus = "Pending",
//                GatewayName = "ATOM",
//                IsActive = true
//            };

//            _context.PaymentTransactionDetail.Add(transaction);
//            await _context.SaveChangesAsync();

//            // Gateway ke liye unique merchant txn id — apna internal ID prefix kar dete hain
//            string merchTxnId = $"SHK{transaction.PaymentTransactionId}{DateTime.Now:yyMMddHHmmss}";

//            transaction.TransactionId = merchTxnId;
//            transaction.GatewayOrderId = merchTxnId;
//            await _context.SaveChangesAsync();

//            string returnUrl = Url.Action(
//                "Callback",
//                "Payment",
//                null,
//                Request.Scheme) ?? string.Empty;

//            var authResult = await _atomPaymentService.InitiateAsync(
//                merchTxnId,
//                vm.TotalAmount,
//                student?.Email,
//                student?.ContactNo,
//                returnUrl);

//            if (!authResult.Success)
//            {
//                transaction.TransactionStatus = "Failed";
//                transaction.TransactionError = authResult.ErrorMessage ?? "Gateway token generate nahi ho saka.";
//                await _context.SaveChangesAsync();

//                ErrorMessage(transaction.TransactionError);
//                return RedirectToAction("FeeReceipt", "FeeHeading");
//            }

//            var setting = await _atomPaymentService.GetActiveSettingAsync();

//            var checkoutVm = new AtomCheckoutVM
//            {
//                MerchantTransactionId = merchTxnId,
//                Amount = vm.TotalAmount,
//                AtomTokenId = authResult.AtomTokenId ?? string.Empty,
//                MerchId = setting.MerchId,
//                CustomerEmail = student?.Email ?? string.Empty,
//                CustomerMobile = student?.ContactNo ?? string.Empty,
//                ReturnUrl = returnUrl,
//                CheckoutCdn = setting.CheckoutCdn,
//                Environment = setting.CheckoutEnvironment
//            };

//            return View("Checkout", checkoutVm);
//        }

//        #endregion

//        #region Callback

//        // ATOM checkout complete hone ke baad user ko isi URL par
//        // wapas bhejta hai (ReturnUrl). Yahan hum result verify karke
//        // receipt finalize karte hain.
//        [HttpPost]
//        [HttpGet]
//        public async Task<IActionResult> Callback()
//        {
//            AtomCallbackResponse callback;

//            if (Request.Method == HttpMethods.Post && Request.HasFormContentType)
//            {
//                callback = _atomPaymentService.ParseCallback(Request.Form);
//            }
//            else
//            {
//                callback = new AtomCallbackResponse
//                {
//                    MerchTxnId = Request.Query["merchTxnId"],
//                    AtomTxnId = Request.Query["atomTxnId"],
//                    StatusCode = Request.Query["statusCode"],
//                    StatusDescription = Request.Query["statusDescription"],
//                    RawResponse = Request.QueryString.Value
//                };
//            }

//            if (string.IsNullOrWhiteSpace(callback.MerchTxnId))
//            {
//                return View("Result", new AtomPaymentResultVM
//                {
//                    Success = false,
//                    Message = "Payment gateway se invalid response mila."
//                });
//            }

//            var transaction = await _context.PaymentTransactionDetail
//                .FirstOrDefaultAsync(x => x.TransactionId == callback.MerchTxnId);

//            if (transaction == null)
//            {
//                return View("Result", new AtomPaymentResultVM
//                {
//                    Success = false,
//                    Message = "Transaction record nahi mila."
//                });
//            }

//            transaction.GatewayResponse = callback.RawResponse;
//            transaction.GatewayPaymentId = callback.AtomTxnId;
//            transaction.ReferenceNo = callback.AtomTxnId;
//            transaction.PaymentId = callback.StatusCode;

//            if (!callback.IsSuccess)
//            {
//                transaction.TransactionStatus = "Failed";
//                transaction.TransactionError = callback.StatusDescription ?? "Payment failed.";
//                await _context.SaveChangesAsync();

//                return View("Result", new AtomPaymentResultVM
//                {
//                    Success = false,
//                    Message = transaction.TransactionError,
//                    Amount = transaction.Amount,
//                    ReferenceNo = callback.AtomTxnId
//                });
//            }

//            // ============================================
//            // SUCCESS → PENDING RECEIPT KO FINALIZE KARO
//            // ============================================
//            var json = HttpContext.Session.GetString(SessionKey);
//            StudentFeeReceiptVM? pendingVm = null;

//            if (!string.IsNullOrWhiteSpace(json))
//            {
//                try
//                {
//                    pendingVm = JsonSerializer.Deserialize<StudentFeeReceiptVM>(json);
//                }
//                catch
//                {
//                    pendingVm = null;
//                }
//            }

//            long? receiptId = null;

//            if (pendingVm != null)
//            {
//                var onlineModeId = GetDataListItems("Payment Mode")
//                    .Where(x => x.DataListItemText != null &&
//                                x.DataListItemText.Trim().Equals("Online", StringComparison.OrdinalIgnoreCase))
//                    .Select(x => (int?)x.DataListItemId)
//                    .FirstOrDefault();

//                pendingVm.PaymentModeId = onlineModeId ?? 0;

//                try
//                {
//                    receiptId = _repository.SaveCashFeeReceipt(pendingVm);
//                    HttpContext.Session.Remove(SessionKey);
//                }
//                catch (Exception ex)
//                {
//                    transaction.TransactionError = "Payment success hua lekin receipt save karte waqt error: " + ex.Message;
//                }
//            }
//            else
//            {
//                transaction.TransactionError = "Payment success hua lekin session expire ho chuki thi — receipt manually verify karo.";
//            }

//            transaction.TransactionStatus = "Success";
//            transaction.PaymentCompletedOn = DateTime.Now;

//            if (receiptId.HasValue)
//            {
//                var receipt = await _context.FeeReceipt
//                    .FirstOrDefaultAsync(x => x.FeeReceiptId == receiptId.Value);

//                if (receipt != null)
//                    receipt.PaymentTransactionId = transaction.PaymentTransactionId;
//            }

//            await _context.SaveChangesAsync();

//            return View("Result", new AtomPaymentResultVM
//            {
//                Success = true,
//                Message = transaction.TransactionError ?? "Payment successful!",
//                Amount = transaction.Amount,
//                ReferenceNo = callback.AtomTxnId,
//                ReceiptId = receiptId
//            });
//        }

//        #endregion
//    }

//    /* ============================================
//       Views/Payment/Checkout.cshtml is VM ko already
//       @model Shikhsa.Controllers.AtomCheckoutVM se reference karta hai.
//       ============================================ */
//    public class AtomCheckoutVM
//    {
//        public string MerchantTransactionId { get; set; } = string.Empty;

//        public decimal Amount { get; set; }

//        public string AtomTokenId { get; set; } = string.Empty;

//        public string MerchId { get; set; } = string.Empty;

//        public string CustomerEmail { get; set; } = string.Empty;

//        public string CustomerMobile { get; set; } = string.Empty;

//        public string ReturnUrl { get; set; } = string.Empty;

//        public string CheckoutCdn { get; set; } = string.Empty;

//        public string Environment { get; set; } = "uat";
//    }

//    /* ============================================
//       Views/Payment/Result.cshtml is VM ko already
//       @model Shikhsa.Controllers.AtomPaymentResultVM se reference karta hai.
//       ============================================ */
//    public class AtomPaymentResultVM
//    {
//        public bool Success { get; set; }

//        public string Message { get; set; } = string.Empty;

//        public decimal Amount { get; set; }

//        public string? ReferenceNo { get; set; }

//        public long? ReceiptId { get; set; }
//    }
//}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Attributes;
using Shikhsa.Data;
using Shikhsa.DataBase.Repositry;
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

        private const string SessionKey = "PendingOnlineFeeReceipt";

        public PaymentController(ApplicationDbContext context, FeeHeadingRepository repository, AtomPaymentService atomPaymentService, UserManager<ApplicationUser> userManager, PermissionService permissionService, EmailService email, LookupService lookup)
            : base(userManager, permissionService, context, email, lookup)
        {
            _context = context;
            _repository = repository;
            _atomPaymentService = atomPaymentService;
        }

        #region Initiate

        [HttpGet]
        [SkipPermission]
        public async Task<IActionResult> InitiateFromSession()
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
                if (vm != null)
                {
                    vm.TotalAmount = vm.ReceiptAmount;
                }
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
                .FirstOrDefaultAsync(x => x.StudentId == vm.StudentId);

            // ============================================
            // 1. CREATE PENDING TRANSACTION ROW IN DATABASE
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
                GatewayName = "ATOM",
                IsActive = true
            };

            _context.PaymentTransactionDetail.Add(transaction);
            await _context.SaveChangesAsync();

            // Generate unique transaction identifier matching internal requirements
            string merchTxnId = $"SHK{transaction.PaymentTransactionId}{DateTime.Now:yyMMddHHmmss}";

            transaction.TransactionId = merchTxnId;
            transaction.GatewayOrderId = merchTxnId;
            await _context.SaveChangesAsync();

            string returnUrl = Url.Action("Callback", "Payment", null, Request.Scheme) ?? string.Empty;

            // Get database parameters configured for NTTDATA Payment Gateway
            var setting = await _atomPaymentService.GetActiveSettingAsync();

            // ============================================
            // 2. ENCRYPT PARAMETERS FOR NTT DATA FORM REDIRECT
            // ============================================
            // We pass the raw fields directly down to the model view payload 
            // for form action submissions standard to NTT DATA ASP.NET setups.
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

            // Complete standard handshake initiation asynchronously to confirm status readiness
            var authResult = await _atomPaymentService.InitiateAsync(
                merchTxnId,
                vm.TotalAmount,
                student?.Email,
                student?.ContactNo,
                returnUrl);

            if (!authResult.Success)
            {
                transaction.TransactionStatus = "Failed";
                transaction.TransactionError = authResult.ErrorMessage ?? "Gateway token generate nahi ho saka.";
                await _context.SaveChangesAsync();

                ErrorMessage(transaction.TransactionError);
                return RedirectToAction("FeeReceipt", "FeeHeading");
            }

            checkoutVm.AtomTokenId = authResult.AtomTokenId ?? string.Empty;

            // Direct mapping into the NTTDATA payment checkout layout page
            return View("Checkout", checkoutVm);
        }

        #endregion

        #region Callback

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

            // Normalize variable status flags matching NTTDATA payment gateway outputs
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

            // ============================================
            // FINALIZE SUCCESSFUL TRANSACTIONS & SAVE FEES
            // ============================================
            var json = HttpContext.Session.GetString(SessionKey);
            StudentFeeReceiptVM? pendingVm = null;

            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    pendingVm = JsonSerializer.Deserialize<StudentFeeReceiptVM>(json);
                }
                catch
                {
                    pendingVm = null;
                }
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
            transaction.TransactionStatus = "Success"; transaction.PaymentCompletedOn = DateTime.Now; if (receiptId.HasValue) { var receipt = await _context.FeeReceipt.FirstOrDefaultAsync(x => x.FeeReceiptId == receiptId.Value); if (receipt != null) receipt.PaymentTransactionId = transaction.PaymentTransactionId; }
            await _context.SaveChangesAsync(); return View("Result", new AtomPaymentResultVM { Success = true, Message = transaction.TransactionError ?? "Payment successful!", Amount = transaction.Amount, ReferenceNo = callback.AtomTxnId, ReceiptId = receiptId });
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
            // Required credentials added for form posts
            public string Password { get; set; } = string.Empty;
            public string ProductId { get; set; } = string.Empty;
            public string RequestEncryptKey { get; set; } = string.Empty;
            public string RequestSalt { get; set; } = string.Empty;
        }

        /* ============================================
           Views/Payment/Result.cshtml is VM ko already
           @model Shikhsa.Controllers.AtomPaymentResultVM se reference karta hai.
           ============================================ */
        public class AtomPaymentResultVM
        {
            public bool Success { get; set; }

            public string Message { get; set; } = string.Empty;

            public decimal Amount { get; set; }

            public string? ReferenceNo { get; set; }

            public long? ReceiptId { get; set; }
        }
    
        
    
}