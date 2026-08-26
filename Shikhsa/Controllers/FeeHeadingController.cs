using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shikhsa.Attributes;
using Shikhsa.Data;
using Shikhsa.DataBase.Repositry;
using Shikhsa.Helpers;
using Shikhsa.Models;
using Shikhsa.Models.Common;
using Shikhsa.Repository;
using Shikhsa.Services;
using Shikhsa.ViewModels;
using System.Text;

namespace Shikhsa.Controllers
{
    public class FeeHeadingController : BaseController
    {
        private readonly FeeHeadingRepository  _repository;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly EmailService _emailService;
        private readonly NotificationService _notificationService;
        private readonly LookupService _lookup;
        private readonly IRazorViewToStringRenderer _viewRenderer;
        private readonly PdfGeneratorService _pdfGenerator;
        private readonly FeeReceiptRecordRepository _feeReceiptRecordRepository;
        public FeeHeadingController(FeeHeadingRepository repository, RoleManager<ApplicationRole> roleManager, ApplicationDbContext context,
     UserManager<ApplicationUser> userManager,
     PermissionService permissionService, IWebHostEnvironment env, EmailService email, LookupService lookup,NotificationService notificationService, IRazorViewToStringRenderer viewRenderer,      // 👈 constructor param
        PdfGeneratorService pdfGenerator, FeeReceiptRecordRepository feeReceiptRecordRepository) : base(userManager, permissionService, context, email, lookup)
        {
            _repository = repository;
            _context = context;
            _lookup = lookup;
            _notificationService = notificationService;
            _viewRenderer = viewRenderer;                  // 👈 assign
            _pdfGenerator = pdfGenerator;
            _feeReceiptRecordRepository = feeReceiptRecordRepository;
        }
        #region Frequency
        public async Task<IActionResult> FeeFrequency(int id = 0)
        {
            FeeFrequencyPageVM model = new();

            model.List = await _repository.GetAllFrequencyAsync();

            if (id > 0)
            {
                model.Form = await _repository.GetFrequencyByIdAsync(id) ?? new FeeFrequency();
            }

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFeeFrequency(FeeFrequencyPageVM model)
        {
            model.List = await _repository.GetAllFrequencyAsync();

            if (!ModelState.IsValid)
                return View("Index", model);

            if (await _repository.IsDuplicateFrequencyAsync(model.Form.Value, model.Form.FrequencyId))
            {
                ModelState.AddModelError("Form.Value", "Frequency already exists.");

                return View("FeeFrequency", model);
            }

            ResponseModel response;
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";

            if (model.Form.FrequencyId == 0)
            {
                model.Form.AddedDate = DateTime.Now;
                model.Form.AddedBy = userName;
                response = await _repository.FrequencySaveAsync(model.Form);
            }
            else
            {
                model.Form.UpdatedDate = DateTime.Now;
                model.Form.UpdatedBy = userName;
                response = await _repository.FrequencyUpdateAsync(model.Form);
            }

            TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;

            return RedirectToAction(nameof(FeeFrequency));
        }
        public async Task<IActionResult> DeleteFeeFrequency(int id)
        {
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
            var response = await _repository.DeleteFrequencyAsync(id,userName);

            TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;

            return RedirectToAction(nameof(FeeFrequency));
        }
        #endregion
        #region FeeHeading
        public async Task<IActionResult> FeeHeadings(long id = 0)
        {
            FeeHeadingPageVM model = new();

            await BindFrequency(model);

            model.List = await _repository.GetAllFeeHeadingAsync();

            if (id > 0)
            {
                model.Form = await _repository.GetFeeHeadingByIdAsync(id) ?? new FeeHeading();
            }

            return View(model);
        }
        private async Task BindFrequency(FeeHeadingPageVM model)
        {
            var frequency = await _repository.GetActiveFrequencyAsync();

            model.FrequencyList = frequency.Select(x => new SelectListItem
            {
                Value = x.FrequencyId.ToString(),
                Text = x.Text
            }).ToList();
        }
        private async Task TaskBindFeePlansDropdown()
        {
            ViewBag.Classes = GetDataListItems("Class");
            ViewBag.Hostels = GetDataListItems("Hostel List");
            ViewBag.Transports = GetDataListItems("Transport");
            ViewBag.RoomType = GetDataListItems("Room Type");
            ViewBag.MealType = GetDataListItems("Meal Type");

            ViewBag.Batches = await _context.Batches
                .Where(x => x.IsActive && x.ActiveForAdmission)
                .ToListAsync();
            ViewBag.FeeHeadingList = await _repository.GetAllFeeHeadingAsync();

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFeeHeadings(FeeHeadingPageVM model)
        {
            await BindFrequency(model);

            model.List = await _repository.GetAllFeeHeadingAsync();

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Model Error: {error.ErrorMessage}");
                   
                    }
                }
                return View("FeeHeadings", model);

            }
            if (await _repository.IsDuplicateFeeHeadingAsync(model.Form.FeeHeadingName, model.Form.FeeHeadingId))
            {
                ModelState.AddModelError("Form.FeeHeadingName", "Fee Heading already exists.");

                return View("FeeHeadings", model);
            }

            ResponseModel response;
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
            if (model.Form.FeeHeadingId == 0)
            {
                model.Form.AddedDate = DateTime.Now;
                model.Form.AddedBy = userName;
                response = await _repository.SaveFeeHeadingAsync(model.Form);
            }
            else
            {
                model.Form.UpdatedDate = DateTime.Now;
                model.Form.UpdatedBy = userName;
                response = await _repository.UpdateFeeHeadingAsync(model.Form);
            }

            TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;

            return RedirectToAction(nameof(FeeHeadings));
        }

        public async Task<IActionResult> DeleteFeeHeadings(long id)
        {
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
            var response = await _repository.DeleteFeeHeadingAsync(id,userName);

            TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;

            return RedirectToAction(nameof(FeeHeadings));
        }
        #endregion
        #region FeePlan
            #region TuitionFeePlan
            public async Task<IActionResult> FeePlans()
            {
            await TaskBindFeePlansDropdown();


            var vm = new FeePlanIndexViewModel
                    { 
                        TuitionPlans = await _repository.GetAllTuitionFeePlanAsync(),
                        TransportPlans = await _repository.GetAllTransportFeePlanAsync(),
                        HostelPlans = await _repository.GetAllHostelFeePlanAsync()
                    };
                    return View(vm);
            }
     

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTuitionFee(FeePlanIndexViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please fill all required fields.";
                    return RedirectToAction(nameof(FeePlans));
                }

                var currentUser = HttpContext.Session.GetCurrentUser();
                string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";

                ResponseModel response = await _repository.SaveOrUpdateTuitionFeePlanAsync(vm.NewTuition, userName);

                TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;

                return RedirectToAction(nameof(FeePlans));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(FeePlans));
            }
        }
        public async Task<IActionResult> SaveTuitionFee(int id)
            {
                await TaskBindFeePlansDropdown();
                var vm = new FeePlanIndexViewModel
                {
                    NewTuition = await _repository.GetTuitionFeePlanByIdAsync(id) ?? new TuitionFeePlan(),
                    TuitionPlans = await _repository.GetAllTuitionFeePlanAsync(),
                    TransportPlans = await _repository.GetAllTransportFeePlanAsync(),
                    HostelPlans = await _repository.GetAllHostelFeePlanAsync()
                };
                return View("FeePLans", vm);
            }
            [HttpPost]
            public async Task<IActionResult> DeleteTuitionFee(long id)
            {
                var currentUser = HttpContext.Session.GetCurrentUser();
                string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
                ResponseModel response = await _repository.DeleteTuitionFeePlanAsync(id, userName);
                TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;
                return RedirectToAction(nameof(FeePlans));
            }
        #endregion
        #region TransportFeePlan
        public async Task<IActionResult> SaveTransportFee(int id)
        {
            await TaskBindFeePlansDropdown();
            var vm = new FeePlanIndexViewModel
            {
                NewTransport= await _repository.GetTransportFeePlanByIdAsync(id) ?? new TransportFeePlan(),
                TuitionPlans = await _repository.GetAllTuitionFeePlanAsync(),
                TransportPlans = await _repository.GetAllTransportFeePlanAsync(),
                HostelPlans = await _repository.GetAllHostelFeePlanAsync()
            };
            return View("FeePLans", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTransportFee(FeePlanIndexViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please fill all required fields.";
                    return RedirectToAction(nameof(FeePlans));
                }

                var currentUser = HttpContext.Session.GetCurrentUser();
                string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";

                ResponseModel response = await _repository.SaveUpdateTransportFeePlanAsync(vm.NewTransport, userName);

                TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;

                return RedirectToAction(nameof(FeePlans));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(FeePlans));
            }
        }
    
        [HttpPost]
        public async Task<IActionResult> DeleteTransportFee(int id)
        {
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
            ResponseModel response = await _repository.DeleteTransportFeePlanAsync(id, userName);
            TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;
            return RedirectToAction(nameof(FeePlans));
        }
        #endregion
        #region HostelFeePlan
        public async Task<IActionResult> SaveHostelFee(int id)
        {
            await TaskBindFeePlansDropdown();
            var vm = new FeePlanIndexViewModel
            {
                NewHostel = await _repository.GetHostelFeePlanByIdAsync(id) ?? new HostelFeePlan(),
                TuitionPlans = await _repository.GetAllTuitionFeePlanAsync(),
                TransportPlans = await _repository.GetAllTransportFeePlanAsync(),
                HostelPlans = await _repository.GetAllHostelFeePlanAsync()
            };
            return View("FeePLans", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveHostelFee(FeePlanIndexViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please fill all required fields.";
                    return RedirectToAction(nameof(FeePlans));
                }

                var currentUser = HttpContext.Session.GetCurrentUser();
                string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";

                ResponseModel response = await _repository.SaveUpdateHostelFeePlanAsync(vm.NewHostel,userName);

                TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;

                return RedirectToAction(nameof(FeePlans));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(FeePlans));
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteHostelFee(int id)
        {
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
            ResponseModel response = await _repository.DeleteHostelFeePlanAsync(id, userName);
            TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;
            return RedirectToAction(nameof(FeePlans));
        }
        #endregion
        #endregion
        #region Fee Receipt
        [HttpGet]
        public async Task<IActionResult> FeeReceipt(int? classId = null,int? batchId = null,long? studentId = null)
        {
            var vm = new StudentFeePageVM
            {
                SelectedClassId = classId,
              
                SelectedBatchId = batchId,
                SelectedStudentId = studentId,

                // BaseController ka existing function
                Classes = GetDataListItems("Class"),

                // BaseController ka existing function
                Sections = GetDataListItems("Section"),

                // BaseController ka existing function
                Batches = GetBatchList()
            };


            var currentUser = HttpContext.Session.GetCurrentUser();


            if (currentUser != null && currentUser.RoleName == "Student")
            {
                // ============================================
                // STUDENT LOGIN → SIRF USKA APNA RECORD
                // ============================================

                var ownStudent = _context.Tbl_Students
                    .AsNoTracking()
                    .FirstOrDefault(x => x.UserId == currentUser.Id && x.IsActive);

                if (ownStudent != null)
                {
                    vm.Students = new List<StudentFeeStudentVM>
            {
                _repository.GetStudentDetails(ownStudent.StudentId)
            }.Where(x => x != null).ToList();

                    // Class/Batch bhi auto-select kar do, dropdown disable jaisa UX ke liye
                    vm.SelectedClassId = ownStudent.AdmitClassId;
                    vm.SelectedBatchId = ownStudent.AdmitBatchId;

                    // Agar studentId explicitly nahi diya to auto-select kar do
                    if (!studentId.HasValue)
                    {
                        vm.SelectedStudentId = ownStudent.StudentId;
                        studentId = ownStudent.StudentId;
                    }
                }

                ViewBag.IsStudentUser = true;   // View me dropdown ko readonly/hidden karne ke liye
            }
        
            else
            {
                // ============================================
                // ADMIN/STAFF LOGIN → EXISTING CLASS+BATCH FILTER LOGIC
                // ============================================

                if (classId.HasValue && batchId.HasValue)
                {
                    vm.Students = _repository.GetStudents(classId.Value, batchId.Value);
                }
            }


            // ============================================
            // SELECTED STUDENT KA FEE DETAIL LOAD KARO
            // ============================================

            if (studentId.HasValue)
            {
                vm.SelectedStudent = _repository.GetStudentDetails(studentId.Value);

                if (vm.SelectedClassId.HasValue && vm.SelectedBatchId.HasValue)
                {
                    vm.FeeReceipt = _repository.GetUnpaidFees(
                        studentId.Value,
                        vm.SelectedClassId.Value,
                        vm.SelectedBatchId.Value);
                }

                vm.PaymentMode = GetDataListItems("Payment Mode");
            }

            return View(vm);
        }
        #endregion
        #region Save Fee Receipt

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFeeReceipt(StudentFeePageVM vm)
        {
            if (vm.FeeReceipt == null ||
                !vm.SelectedStudentId.HasValue ||
                !vm.SelectedClassId.HasValue ||
                !vm.SelectedBatchId.HasValue)
            {
                TempData["Error"] = "Invalid data. Please try again.";

                return RedirectToAction("FeeReceipt", "FeeHeading", new
                {
                    classId = vm.SelectedClassId,
                    batchId = vm.SelectedBatchId,
                    studentId = vm.SelectedStudentId
                });
            }
            var currentUser = HttpContext.Session.GetCurrentUser();

            if (currentUser != null && currentUser.RoleName == "Student")
            {
                bool isOwnRecord = _context.Tbl_Students
                    .Any(x => x.StudentId == vm.SelectedStudentId.Value && x.UserId == currentUser.Id);

                if (!isOwnRecord)
                {
                    TempData["Error"] = "You are not authorized to pay fee for this student.";
                    return RedirectToAction("FeeReceipt", "FeeHeading");
                }
            }
            // Fill required IDs into FeeReceipt VM
            vm.FeeReceipt.StudentId = vm.SelectedStudentId.Value;
            vm.FeeReceipt.ClassId = vm.SelectedClassId.Value;
            vm.FeeReceipt.BatchId = vm.SelectedBatchId.Value;


            // ============================================
            // PAYMENT MODE TEXT NIKALO
            // ============================================

            var paymentModeText = GetDataListItems("Payment Mode")
                .Where(x => x.DataListItemId == vm.FeeReceipt.PaymentModeId)
                .Select(x => x.DataListItemText)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(paymentModeText))
            {
                TempData["Error"] = "Please select a Payment Mode.";

                return RedirectToAction("FeeReceipt", "FeeHeading", new
                {
                    classId = vm.SelectedClassId,
                    batchId = vm.SelectedBatchId,
                    studentId = vm.SelectedStudentId
                });
            }


            // ============================================
            // CASH → SAVE + PRINT (A5, 2 copies)
            // ============================================

            if (paymentModeText.Trim().Equals("Cash", StringComparison.OrdinalIgnoreCase))
            {
                long receiptId = _repository.SaveCashFeeReceipt(vm.FeeReceipt);
                try
                {
                    await SendFeeReceiptEmailAsync(receiptId, "FEE_RECEIPT");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fee receipt email failed: " + ex.Message);
                }
                return RedirectToAction("PrintReceipt", new { id = receiptId });
            }


            // ============================================
            // ONLINE → ABHI KE LIYE SKIP (next part me karenge)
            // ============================================

            if (paymentModeText.Trim().Equals("Online", StringComparison.OrdinalIgnoreCase))
            {

                HttpContext.Session.SetString("PendingOnlineFeeReceipt",System.Text.Json.JsonSerializer.Serialize(vm.FeeReceipt));

                return RedirectToAction("InitiateFromSession", "Payment");
                //TempData["Info"] = "Online payment integration next part me implement hoga.";

                //return RedirectToAction("FeeReceipt", "FeeHeading", new
                //{
                //    classId = vm.SelectedClassId,
                //    batchId = vm.SelectedBatchId,
                //    studentId = vm.SelectedStudentId
                //});
            }


            // ============================================
            // OTHER PAYMENT MODE (Cheque, Card etc.) — abhi block
            // ============================================

            TempData["Error"] = $"'{paymentModeText}' payment mode abhi supported nahi hai.";

            return RedirectToAction("FeeReceipt", "FeeHeading", new
            {
                classId = vm.SelectedClassId,
                batchId = vm.SelectedBatchId,
                studentId = vm.SelectedStudentId
            });
        }


        // =========================================================
        // PRINT RECEIPT
        // =========================================================

        [HttpGet]
        [SkipPermission]
        public async Task<IActionResult> PrintReceipt(long id)
        {
            var receipt = _repository.GetReceiptForPrint(id);
            if (receipt == null)
                return NotFound();

            var schoolDetails = await _context.SchoolMasters.FirstOrDefaultAsync();
            ViewBag.School = schoolDetails;
            var studentWithParent = await _context.Tbl_Students
        .Include(x => x.Parent)
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.StudentId == receipt.StudentId);

            ViewBag.FatherName = studentWithParent?.Parent == null
                ? string.Empty
                : $"{studentWithParent.Parent.FatherFirstName} {studentWithParent.Parent.FatherMiddleName} {studentWithParent.Parent.FatherLastName}".Trim();

            return View(receipt);
        }


    //    [HttpPost]
    //    public async Task<IActionResult> SendFeeReceiptEmail(long feeReceiptId, string templateCode)
    //    {
    //        var receipt = await _context.FeeReceipt
    //            .Include(x => x.Student)
    //                .ThenInclude(s => s.Parent)
    //            .Include(x => x.PaymentMode)
    //            .Include(x => x.Details)
    //            .FirstOrDefaultAsync(x => x.FeeReceiptId == feeReceiptId);

    //        if (receipt == null)
    //            return NotFound();

    //        // ============================================
    //        // BATCH NAME (join se aayega)
    //        // ============================================

    //        var batchName = await _context.Batches
    //            .Where(x => x.BatchId == receipt.BatchId)
    //            .Select(x => x.AcademicYear)
    //            .FirstOrDefaultAsync();

    //        var firstDetail = receipt.Details.FirstOrDefault();


    //        // ============================================
    //        // EXTRA [NotMapped] FIELDS YAHIN POPULATE KARO
    //        // ============================================

    //        receipt.BatchName = batchName ?? "";
    //        //receipt.TotalFee = receipt.Details.Sum(x => x.Amount).ToString("N2");
    //        receipt.AlreadyPaidAmount = receipt.Details.Sum(x => x.PreviousPaidAmount).ToString("N2");
    //        receipt.FeeType = firstDetail?.FeeType ?? "";
    //        receipt.FeeDescription = firstDetail?.FeeDescription ?? "";
    //        receipt.FeeMonth = firstDetail != null
    //                                        ? new DateTime(2000, firstDetail.Month, 1).ToString("MMMM")
    //                                        : "";
    //        receipt.FeeYear = firstDetail?.Year.ToString() ?? "";


    //        // ============================================
    //        // PDF ATTACHMENT (agar attach karna hai)
    //        // ============================================

    //        string receiptHtml = await _viewRenderer.RenderViewToStringAsync(
    //            "~/Views/FeeHeading/PrintReceipt.cshtml",
    //            receipt,
    //            new Dictionary<string, object> { { "School", await _notificationService.GetSchoolInfo() } });

    //        byte[] pdfBytes = await _pdfGenerator.GeneratePdfFromHtmlAsync(receiptHtml);

    //        var attachments = new List<(string, byte[], string)>
    //{
    //    ($"Receipt-{receipt.ReceiptNumber}.pdf", pdfBytes, "application/pdf")
    //};


    //        // ============================================
    //        // NOTIFICATION SEND — Entity DIRECTLY pass ho raha hai
    //        // ============================================

    //        var result = await _notificationService.SendAsync(
    //            templateCode,
    //            receipt.Student.Email,
    //            receipt.FeeReceiptId,
    //            attachments,
    //            receipt.Student,          // → {{Student.XXX}}
    //            receipt.Student.Parent,   // → {{Parent.XXX}}
    //            receipt                   // → {{Fee.XXX}}
    //        );

    //        return Json(result);
    //    }
        [SkipPermission]
        private async Task SendFeeReceiptEmailAsync(long feeReceiptId, string templateCode)
        {
            var receipt = await _context.FeeReceipt
                .Include(x => x.Student)
                    .ThenInclude(s => s.Parent)
                .Include(x => x.PaymentMode)
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.FeeReceiptId == feeReceiptId);

            if (receipt == null)
                return;

            if (string.IsNullOrWhiteSpace(receipt.Student?.Email))
                return;

            var batchName = await _context.Batches
                .Where(x => x.BatchId == receipt.BatchId)
                .Select(x => x.AcademicYear)
                .FirstOrDefaultAsync();

            var firstDetail = receipt.Details.FirstOrDefault();

            receipt.BatchName = batchName ?? "";
            receipt.AlreadyPaidAmount = receipt.Details.Sum(x => x.PreviousPaidAmount).ToString("N2");
            receipt.FeeType = firstDetail?.FeeType ?? "";
            receipt.FeeDescription = firstDetail?.FeeDescription ?? "";
            receipt.FeeMonth = firstDetail != null
                                            ? new DateTime(2000, firstDetail.Month, 1).ToString("MMMM")
                                            : "";
            receipt.FeeYear = firstDetail?.Year.ToString() ?? "";

            var school = await _notificationService.GetSchoolInfo();
            string? logoBase64Uri = GetLogoAsBase64(school?.LogoPath);
            string receiptHtml = await _viewRenderer.RenderViewToStringAsync(
                "~/Views/FeeHeading/PrintReceipt.cshtml",
                receipt,
                new Dictionary<string, object> { { "School", school }, { "LogoBase64", logoBase64Uri } });

            byte[] pdfBytes = await _pdfGenerator.GeneratePdfFromHtmlAsync(receiptHtml);

            var attachments = new List<(string FileName, byte[] Content, string ContentType)>
    {
        ($"Receipt-{receipt.ReceiptNumber}.pdf", pdfBytes, "application/pdf")
    };

            // ✅ NAYA METHOD — attachments alag, entities alag, koi ambiguity nahi
            await _notificationService.SendWithAttachmentAsync(
                templateCode,
                receipt.Student.Email,
                receipt.FeeReceiptId,
                attachments,
                receipt.Student,          // {{Student.XXX}}
                receipt.Student.Parent,   // {{Parent.XXX}}
                receipt                   // {{Fee.XXX}}
            );
        }
        private string? GetLogoAsBase64(string? logoRelativePath)
        {
            if (string.IsNullOrWhiteSpace(logoRelativePath))
                return null;

            try
            {
                // logoRelativePath jaisa hai "/Uploads/School/logo.png"
                string trimmedPath = logoRelativePath.TrimStart('/', '\\');

                string physicalPath = Path.Combine(_env.WebRootPath, trimmedPath);

                if (!System.IO.File.Exists(physicalPath))
                    return null;

                byte[] imageBytes = System.IO.File.ReadAllBytes(physicalPath);

                string extension = Path.GetExtension(physicalPath).TrimStart('.').ToLower();
                string mimeType = extension switch
                {
                    "png" => "image/png",
                    "jpg" or "jpeg" => "image/jpeg",
                    "gif" => "image/gif",
                    "svg" => "image/svg+xml",
                    "webp" => "image/webp",
                    _ => "image/png"
                };

                string base64 = Convert.ToBase64String(imageBytes);

                return $"data:{mimeType};base64,{base64}";
            }
            catch
            {
                return null;
            }
        }

        #endregion
        #region Fee Receipt Records

        [HttpGet]
        public IActionResult FeeReceiptReports()
        {
            var model = new FeeReceiptRecordVM();

            /*
                Class
            */
            model.Classes =
                GetDataListItems("Class")
                .Select(x => new SelectListItem
                {
                    Value = x.DataListItemId.ToString(),
                    Text = x.DataListItemText
                })
                .ToList();


            /*
                Batch
            */
            model.Batches =
                GetBatchList()
                .Select(x => new SelectListItem
                {
                    Value = x.BatchId.ToString(),
                    Text = x.AcademicYear
                })
                .ToList();


            /*
                Payment Mode
            */
            model.PaymentModes =
                GetDataListItems("Payment Mode")
                .Select(x => new SelectListItem
                {
                    Value = x.DataListItemId.ToString(),
                    Text = x.DataListItemText
                })
                .ToList();


            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FeeReceiptReports([FromForm] FeeReceiptDataTableRequest request)
        {
            try
            {
                /*
                    DataTables search
                */

                var search =
                    Request.Form["search[value]"]
                        .FirstOrDefault();


                request.Search = search;


                /*
                    DataTables order
                */

                var orderColumnIndex =
                    Request.Form["order[0][column]"]
                        .FirstOrDefault();


                var orderDirection =
                    Request.Form["order[0][dir]"]
                        .FirstOrDefault();


                request.OrderDirection =
                    string.Equals(
                        orderDirection,
                        "asc",
                        StringComparison.OrdinalIgnoreCase)
                        ? "ASC"
                        : "DESC";


                /*
                    DataTables column index
                */

                request.OrderColumn =
                    orderColumnIndex switch
                    {
                        "0" => "ReceiptId",
                        "1" => "StudentName",
                        "2" => "ClassName",
                        "3" => "AcademicYear",
                        "4" => "FeeHeadings",
                        "5" => "PaymentMode",
                        "6" => "TotalFee",
                        "7" => "PaidAmount",
                        "8" => "ReceiptDate",

                        _ => "ReceiptDate"
                    };


                var result =
                    await _feeReceiptRecordRepository
                        .GetReceiptRecordsAsync(request);


                return Json(new
                {
                    draw = result.Draw,

                    recordsTotal =
                        result.RecordsTotal,

                    recordsFiltered =
                        result.RecordsFiltered,

                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = request.Draw,

                    recordsTotal = 0,

                    recordsFiltered = 0,

                    data = Array.Empty<object>(),

                    error = ex.Message
                });
            }
        }
        [HttpGet]
        public async Task<IActionResult> ExportFeeReceiptReports(DateTime? dateFrom = null,DateTime? dateTo = null,string? search = null,int? classId = null,int? batchId = null,int? paymentModeId = null)
        {
            var request = new FeeReceiptDataTableRequest
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                Search = search,
                ClassId = classId,
                BatchId = batchId,
                PaymentModeId = paymentModeId,

                Start = 0,
                Length = -1,

                OrderColumn = "ReceiptDate",
                OrderDirection = "DESC"
            };


            var records =
                await _feeReceiptRecordRepository
                    .GetReceiptRecordsForExportAsync(request);


            var csv = new StringBuilder();


            /*
                CSV Header
            */

            csv.AppendLine(
                "Receipt ID,Student,Class,Batch,Fee Headings,Payment Mode,Total Fee,Paid Amount,Date");


            foreach (var item in records)
            {
                csv.AppendLine(
                    string.Join(",",
                        CsvEscape(item.FeeReceiptId.ToString()),

                        CsvEscape(item.StudentName),

                        CsvEscape(item.ClassName),

                        CsvEscape(item.AcademicYear),

                        CsvEscape(item.FeeHeadings),

                        CsvEscape(item.PaymentMode),

                        CsvEscape(
                            item.TotalFee.ToString("0.00")),

                        CsvEscape(
                            item.PaidAmount.ToString("0.00")),

                        CsvEscape(
                            item.ReceiptDate
                                .ToString("dd/MM/yyyy hh:mm tt"))
                    ));
            }


            var bytes =
                Encoding.UTF8.GetBytes(
                    csv.ToString());


            return File(
                bytes,
                "text/csv",
                $"FeeReceiptRecords_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }


        private static string CsvEscape(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";

            return "\"" +
                   value.Replace("\"", "\"\"") +
                   "\"";
        }
        #endregion
        #region Fee Payment Report

        [HttpGet]
        public async Task<IActionResult> StudentFeePaymentReport(int? batchId, int? classId, int? sectionId, long? studentId)
        {
            var vm = new FeePaymentReportVM
            {
                SelectedBatchId = batchId,
                SelectedClassId = classId,
                SelectedSectionId = sectionId,
                SelectedStudentId = studentId,

                Classes = GetDataListItems("Class"),
                Sections = GetDataListItems("Section"),
                Batches = GetBatchList()
            };

            if (classId.HasValue && batchId.HasValue)
            {
                vm.Students = _repository.GetStudents(classId.Value, batchId.Value);

                if (sectionId.HasValue)
                {
                    vm.Students = vm.Students.Where(x => x.SectionId == sectionId.Value).ToList();
                }
            }

            if (studentId.HasValue)
            {
                vm.SelectedStudent = _repository.GetStudentDetails(studentId.Value);
            }

            if (batchId.HasValue || studentId.HasValue)
            {
                vm.Rows = _repository.GetFeePaymentReport(batchId, classId, sectionId, studentId);
            }

            return View(vm);
        }


        // =========================================================
        // EXPORT — PDF
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> ExportStudentFeePaymentReportPdf(int? batchId, int? classId, int? sectionId, long? studentId)
        {
            var vm = new FeePaymentReportVM
            {
                SelectedBatchId = batchId,
                SelectedClassId = classId,
                SelectedSectionId = sectionId,
                SelectedStudentId = studentId
            };

            if (studentId.HasValue)
            {
                vm.SelectedStudent = _repository.GetStudentDetails(studentId.Value);
            }

            vm.Rows = _repository.GetFeePaymentReport(batchId, classId, sectionId, studentId);

            var school = await _notificationService.GetSchoolInfo();
            string logoBase64 = GetLogoAsBase64(school?.LogoPath);

            string className = classId.HasValue
                ? GetDataListItems("Class").Where(x => x.DataListItemId == classId).Select(x => x.DataListItemText).FirstOrDefault() ?? ""
                : "";

            string batchName = batchId.HasValue
                ? GetBatchList().Where(x => x.BatchId == batchId).Select(x => x.AcademicYear).FirstOrDefault() ?? ""
                : "";

            string html = await _viewRenderer.RenderViewToStringAsync(
                "~/Views/FeeHeading/FeePaymentReportPdf.cshtml",
                vm,
                new Dictionary<string, object>
                {
            { "School", school },
            { "LogoBase64", logoBase64 },
            { "ClassName", className },
            { "BatchName", batchName }
                });

            byte[] pdfBytes = await _pdfGenerator.GenerateA4PdfFromHtmlAsync(html);

            return File(pdfBytes, "application/pdf", $"Fee-Payment-Report-{DateTime.Now:yyyyMMdd-HHmmss}.pdf");
        }


        // =========================================================
        // EXPORT — EXCEL
        // =========================================================

        [HttpGet]
        public IActionResult ExportStudentFeePaymentReportExcel(int? batchId, int? classId, int? sectionId, long? studentId)
        {
            var rows = _repository.GetFeePaymentReport(batchId, classId, sectionId, studentId);

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var sheet = workbook.Worksheets.Add("Fee Payment Report");

            // ============================================
            // HEADER ROW
            // ============================================

            string[] headers =
            {
        "Receipt No", "Date", "Fee Type", "Fee Head", "Total Fee",
        "Already Paid", "Paid Amount", "Late Fee", "Concession", "Remaining Due"
    };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = sheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            }

            // ============================================
            // DATA ROWS
            // ============================================

            int rowIndex = 2;

            foreach (var r in rows)
            {
                sheet.Cell(rowIndex, 1).Value = r.ReceiptNumber;
                sheet.Cell(rowIndex, 2).Value = r.ReceiptDate.ToString("dd-MMM-yyyy");
                sheet.Cell(rowIndex, 3).Value = r.FeeType;
                sheet.Cell(rowIndex, 4).Value = r.FeeHeadName;
                sheet.Cell(rowIndex, 5).Value = r.TotalFee;
                sheet.Cell(rowIndex, 6).Value = r.AlreadyPaid;
                sheet.Cell(rowIndex, 7).Value = r.PaidAmount;
                sheet.Cell(rowIndex, 8).Value = r.LateFee;
                sheet.Cell(rowIndex, 9).Value = r.Concession;
                sheet.Cell(rowIndex, 10).Value = r.RemainingDue;

                for (int c = 5; c <= 10; c++)
                {
                    sheet.Cell(rowIndex, c).Style.NumberFormat.Format = "#,##0.00";
                }

                rowIndex++;
            }

            // ============================================
            // TOTAL ROW (FOOTER)
            // ============================================

            sheet.Cell(rowIndex, 4).Value = "Total";
            sheet.Cell(rowIndex, 4).Style.Font.Bold = true;

            sheet.Cell(rowIndex, 5).Value = rows.Sum(x => x.TotalFee);
            sheet.Cell(rowIndex, 6).Value = rows.Sum(x => x.AlreadyPaid);
            sheet.Cell(rowIndex, 7).Value = rows.Sum(x => x.PaidAmount);
            sheet.Cell(rowIndex, 8).Value = rows.Sum(x => x.LateFee);
            sheet.Cell(rowIndex, 9).Value = rows.Sum(x => x.Concession);
            sheet.Cell(rowIndex, 10).Value = rows.Sum(x => x.RemainingDue);

            for (int c = 5; c <= 10; c++)
            {
                sheet.Cell(rowIndex, c).Style.Font.Bold = true;
                sheet.Cell(rowIndex, c).Style.NumberFormat.Format = "#,##0.00";
                sheet.Cell(rowIndex, c).Style.Border.TopBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            }

            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Fee-Payment-Report-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx");
        }

        #endregion
    }
}
