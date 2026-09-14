using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shikhsa.Attributes;
using Shikhsa.Data;
using Shikhsa.DataBase.Repositry;
using Shikhsa.Helpers;
using Shikhsa.Models;
using Shikhsa.Services;
using Shikhsa.ViewModels;
using Shikhsa.ViewModels.DataFilter;
using Microsoft.Net.Http.Headers;

namespace Shikhsa.Controllers
{
    public class StudentsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly StudentReportRepository _repo;
        private readonly UserManager<ApplicationUser> _userManager;
        public readonly EmailService _emailService;
        public readonly NotificationService _notificationService;
        private readonly LookupService _lookup;
        private readonly PdfGeneratorService _idCardService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CertificateTemplateRepository _TemplateRepository;
        public StudentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
     PermissionService permissionService, IWebHostEnvironment env,  StudentReportRepository repo, EmailService emailService,NotificationService notificationService,LookupService lookup,PdfGeneratorService idCardService, IServiceScopeFactory scopeFactory, CertificateTemplateRepository certificateTemplate) : base(userManager, permissionService, context,emailService, lookup)
        {
            _context = context;
            _env = env;
            _repo = repo;
            _userManager = userManager;
            _notificationService = notificationService;
            _lookup = lookup;
            _idCardService = idCardService;
            _scopeFactory = scopeFactory;
            _TemplateRepository = certificateTemplate;

        }
        #region Registration
        public async Task<IActionResult> StudentRegistrations()
        {
            ViewBag.BatchList = _context.Batches.Where(x => x.ActiveForAdmission || x.ActiveForRegistration).ToList();
            ViewBag.CategoryList = GetDataListItems("Category");
            ViewBag.ReligionList = GetDataListItems("Religion");
            ViewBag.BoardList = GetDataListItems("Board");
            ViewBag.ClassList = GetDataListItems("Class");
            ViewBag.TranspotList = GetDataListItems("Transport");
            ViewBag.HostelList = GetDataListItems("Hostel");
            ViewBag.GenderList = GetDataListItems("Gender");
            ViewBag.StatusList = GetDataListItems("Status");
            ViewBag.SectionList = GetDataListItems("Section");
            var model = new StudentReportPageVM();

            model.Filter = new StudentListFilterVM();

            model.Filter.SelectedColumns =
            [
                "ApplicationNo","StudentName","FatherName", "FatherMobile"
            ]; 
            model.Students = await _repo.GetStudentReport(model.Filter);

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> StudentRegistrations(StudentReportPageVM model)
        {
            ViewBag.BatchList = _context.Batches.Where(x => x.ActiveForAdmission || x.ActiveForRegistration).ToList();
            ViewBag.CategoryList = GetDataListItems("Category");
            ViewBag.ReligionList = GetDataListItems("Religion");
            ViewBag.BoardList = GetDataListItems("Board");
            ViewBag.ClassList = GetDataListItems("Class");
            ViewBag.GenderList = GetDataListItems("Gender");
            ViewBag.StatusList = GetDataListItems("Status");
            ViewBag.SectionList = GetDataListItems("Section");
            ViewBag.TranspotList = GetDataListItems("Transport");
            ViewBag.HostelList = GetDataListItems("Hostel");
            model.Students =  await _repo.GetStudentReport(model.Filter);
            
           
            return View(model);
        }



        //[HttpGet]
        //public async Task<IActionResult> SaveStudentRegistrations(long? id,string? ApplicationNo)
        //{
        //    ViewBag.BatchList = _context.Batches
        //        .Where(x => x.ActiveForAdmission || x.ActiveForRegistration)
        //        .ToList();

        //    ViewBag.CategoryList = GetDataListItems("Category");
        //    ViewBag.ReligionList = GetDataListItems("Religion");
        //    ViewBag.BoardList = GetDataListItems("Board");
        //    ViewBag.ClassList = GetDataListItems("Class");
        //    ViewBag.GenderList = GetDataListItems("Gender");
        //    ViewBag.StatusList = GetDataListItems("Status");
        //    ViewBag.SectionList = GetDataListItems("Section");

        //    ViewBag.TranspotList = GetDataListItems("Transport");
        //    ViewBag.HostelList = GetDataListItems("Hostel List");
        //    ViewBag.InitialClassId = (await GetSchoolInfo())?.InitialClassId;


        //    // =========================================================
        //    // Existing Student
        //    // ID OR ApplicationNo
        //    // =========================================================

        //    Tbl_StudentsRegistrations? student = null;

        //    // ---------------------------------------------------------
        //    // 1. Search by ID
        //    // ---------------------------------------------------------

        //    if (id.HasValue && id.Value > 0)
        //    {
        //        student = await _context.Tbl_StudentsRegistrations
        //            .Include(x => x.Parent)
        //            .Include(x => x.PreviousSchoolRecord)
        //            .FirstOrDefaultAsync(x => x.StudentId == id.Value);
        //    }

        //    // ---------------------------------------------------------
        //    // 2. If ID not supplied/found, search by ApplicationNo
        //    // ---------------------------------------------------------

        //    if (student == null && !string.IsNullOrWhiteSpace(ApplicationNo))
        //    {
        //        student = await _context.Tbl_StudentsRegistrations
        //            .Include(x => x.Parent)
        //            .Include(x => x.PreviousSchoolRecord)
        //            .FirstOrDefaultAsync(x =>
        //                x.ApplicationNo == ApplicationNo);
        //    }


        //    // =========================================================
        //    // Existing Record Found
        //    // =========================================================

        //    if (student != null)
        //    {
        //        ViewBag.Documents = await _context.Tbl_StudentDocument
        //            .Where(x => x.StudentId == student.StudentId)
        //            .ToListAsync();

        //        return View(student);
        //    }


        //    // =========================================================
        //    // ID/ApplicationNo diya tha but record nahi mila
        //    // =========================================================

        //    if ((id.HasValue && id.Value > 0) ||
        //        !string.IsNullOrWhiteSpace(ApplicationNo))
        //    {
        //        return NotFound();
        //    }


        //    // =========================================================
        //    // New Registration
        //    // =========================================================

        //    ViewBag.Documents = new List<Tbl_StudentDocument>();

        //    return View(new Tbl_StudentsRegistrations());
        //}
        [HttpGet]
        public async Task<IActionResult> SaveStudentRegistrations(long? id,string? ApplicationNo)
        {
            await PopulateViewBags();
            Tbl_StudentsRegistrations? student = null;

            if (id.HasValue && id.Value > 0)
            {
                student = await _context.Tbl_StudentsRegistrations
                    .Include(x => x.Parent)
                    .Include(x => x.PreviousSchoolRecord)
                    .FirstOrDefaultAsync(x => x.StudentId == id.Value);
            }

            if (student == null && !string.IsNullOrWhiteSpace(ApplicationNo))
            {                                               
                student = await _context.Tbl_StudentsRegistrations
                    .Include(x => x.Parent)
                    .Include(x => x.PreviousSchoolRecord)
                    .FirstOrDefaultAsync(x => x.ApplicationNo == ApplicationNo);
            }

            if (student != null)
            {
                // 🔥 Ensure navigation properties are not null
                student.Parent ??= new Tbl_Parents();
                student.PreviousSchoolRecord ??= new Tbl_PreviousSchoolRecord();

                ViewBag.Documents = await _context.Tbl_StudentDocument
                    .Where(x => x.StudentId == student.StudentId)
                    .ToListAsync();
                return View(student);
            }

            if ((id.HasValue && id.Value > 0) || !string.IsNullOrWhiteSpace(ApplicationNo))
                return NotFound();

            // New registration
            var newStudent = new Tbl_StudentsRegistrations
            {
                Parent = new Tbl_Parents(),
                PreviousSchoolRecord = new Tbl_PreviousSchoolRecord()
            };
            ViewBag.Documents = new List<Tbl_StudentDocument>();
            return View(newStudent);
        }
        private async Task PopulateViewBags()
        {
            ViewBag.BatchList = _context.Batches
                .Where(x => x.ActiveForAdmission || x.ActiveForRegistration)
                .ToList();
            ViewBag.CategoryList = GetDataListItems("Category");
            ViewBag.ReligionList = GetDataListItems("Religion");
            ViewBag.BoardList = GetDataListItems("Board");
            ViewBag.ClassList = GetDataListItems("Class");
            ViewBag.GenderList = GetDataListItems("Gender");
            ViewBag.StatusList = GetDataListItems("Status");
            ViewBag.SectionList = GetDataListItems("Section");
            ViewBag.TranspotList = GetDataListItems("Transport");
            ViewBag.HostelList = GetDataListItems("Hostel");
            ViewBag.InitialClassId = (await GetSchoolInfo())?.InitialClassId;
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
       
        public async Task<IActionResult> SaveStudentRegistrations([FromForm] Tbl_StudentsRegistrations model,[FromForm] Tbl_Parents parent,[FromForm] Tbl_PreviousSchoolRecord previousSchool,IFormFile? AadhaarFile,IFormFile? PhotoFile,IFormFile? TCFile,IFormFile? MarksheetFile)
        {
            // सिर्फ यह check करें कि action hit हुआ
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
            // ---------------------- FIX 1: Ensure nested objects exist ----------------------
            parent ??= new Tbl_Parents();
            previousSchool ??= new Tbl_PreviousSchoolRecord();
            model.Parent = parent;
            model.PreviousSchoolRecord = previousSchool;

            // ---------------------- FIX 2: Remove bogus "Student" errors (if any) ----------
            // Sometimes ModelState gets errors for navigation properties that aren't bound.
            ModelState.Remove("Student");    // if present
            ModelState.Remove("Students");   // if present
            ModelState.Remove("PreviousSchoolRecordId");
            ModelState.Remove("PreviousSchoolRecord.Student");    // if present
            ModelState.Remove("Parent.Student");   // if present
           //ModelState.Remove("PreviousSchoolRecordId");
            // ---------------------- FIX 3: Validate ----------------------------------------
            if (!ModelState.IsValid)
            {
                // Log all errors to debug output
                System.Diagnostics.Debug.WriteLine("========== MODELSTATE ERRORS ==========");
                foreach (var key in ModelState.Keys)
                {
                    var entry = ModelState[key];
                    if (entry.Errors.Any())
                    {
                        foreach (var error in entry.Errors)
                        {
                            var msg = error.ErrorMessage ?? error.Exception?.Message ?? "Unknown";
                            System.Diagnostics.Debug.WriteLine($"Key: {key} | Error: {msg}");
                        }
                    }
                }

                // Store errors in TempData to show in view
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage ?? e.Exception?.Message)
                    .Where(m => !string.IsNullOrEmpty(m))
                    .ToList();

                TempData["ValidationErrors"] = string.Join("<br/>", errors);

                await PopulateViewBags();
                return View(model);
            }

            IDbContextTransaction? transaction = null;
            try
            {
                transaction = await _context.Database.BeginTransactionAsync();

                // 3. Ensure nested objects are not null
                model.Parent ??= new Tbl_Parents();
                model.PreviousSchoolRecord ??= new Tbl_PreviousSchoolRecord();

                // ---------- Parent Save/Update ----------
                if (parent.ParentId == 0)
                {
                    parent.AddedBy = userName;
                    parent.AddedDate = DateTime.Now;
                    _context.Tbl_Parents.Add(parent);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var dbParent = await _context.Tbl_Parents
                        .FirstOrDefaultAsync(x => x.ParentId == parent.ParentId)
                        ?? throw new Exception("Parent record not found.");

                    dbParent.FatherFirstName = parent.FatherFirstName;
                    dbParent.FatherMiddleName = parent.FatherMiddleName;
                    dbParent.FatherLastName = parent.FatherLastName;
                    dbParent.FatherContactNo = parent.FatherContactNo;
                    dbParent.FatherEmail = parent.FatherEmail;
                    dbParent.FatherAddress = parent.FatherAddress;

                    dbParent.MotherFirstName = parent.MotherFirstName;
                    dbParent.MotherMiddleName = parent.MotherMiddleName;
                    dbParent.MotherLastName = parent.MotherLastName;
                    dbParent.MotherContactNo = parent.MotherContactNo;
                    dbParent.MotherEmail = parent.MotherEmail;
                    dbParent.MotherAddress = parent.MotherAddress;

                    dbParent.GuardianFirstName = parent.GuardianFirstName;
                    dbParent.GuardianMiddleName = parent.GuardianMiddleName;
                    dbParent.GuardianLastName = parent.GuardianLastName;
                    dbParent.GuardianContactNo = parent.GuardianContactNo;
                    dbParent.GuardianEmail = parent.GuardianEmail;
                    dbParent.GuardianAddress = parent.GuardianAddress;

                    dbParent.UpdatedBy = userName;
                    dbParent.UpdatedDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                // Set ParentId on the student model
                model.ParentId = parent.ParentId;

                // ---------- Student Registration Save/Update ----------
                bool isNewStudent = model.StudentId == 0;
                Tbl_StudentsRegistrations dbStudentReg;

                if (isNewStudent)
                {
                    model.ApplicationNo = GenerateApplicationNo();
                    model.AddedBy = userName;
                    model.AddedDate = DateTime.Now;
                    model.Status = 26;   // default status

                    // Clear navigation properties to avoid EF tracking conflicts
                    model.Parent = null;
                    model.PreviousSchoolRecord = null;

                    _context.Tbl_StudentsRegistrations.Add(model);
                    await _context.SaveChangesAsync();
                    dbStudentReg = model;
                }
                else
                {
                    dbStudentReg = await _context.Tbl_StudentsRegistrations
                        .FirstOrDefaultAsync(x => x.StudentId == model.StudentId)
                        ?? throw new Exception("Student registration not found.");

                    // Copy all scalar properties
                    dbStudentReg.FirstName = model.FirstName;
                    dbStudentReg.MiddleName = model.MiddleName;
                    dbStudentReg.LastName = model.LastName;
                    dbStudentReg.DOB = model.DOB;
                    dbStudentReg.Email = model.Email;
                    dbStudentReg.ContactNo = model.ContactNo;
                    dbStudentReg.LastClass = model.LastClass;
                    dbStudentReg.AadhaarNumber = model.AadhaarNumber;
                    dbStudentReg.APAARId = model.APAARId;
                    dbStudentReg.PENNumber = model.PENNumber;
                    dbStudentReg.LocalAddress = model.LocalAddress;
                    dbStudentReg.PermanentAddress = model.PermanentAddress;
                    dbStudentReg.CategoryId = model.CategoryId;
                    dbStudentReg.GenderId = model.GenderId;
                    dbStudentReg.ReligionId = model.ReligionId;
                    dbStudentReg.IsHandicap = model.IsHandicap;
                    dbStudentReg.HandicapDetails = model.HandicapDetails;
                    dbStudentReg.IdentificationMark = model.IdentificationMark;
                    dbStudentReg.AdmissionBatchId = model.AdmissionBatchId;
                    dbStudentReg.IsInitialClassAdmission = model.IsInitialClassAdmission;
                    dbStudentReg.RegClassId = model.RegClassId;
                    dbStudentReg.ParentId = parent.ParentId;
                    dbStudentReg.IsTranspot = model.IsTranspot;
                    dbStudentReg.TranspotId = model.TranspotId;
                    dbStudentReg.IsHostel = model.IsHostel;
                    dbStudentReg.HostelId = model.HostelId;
                    dbStudentReg.UpdatedBy = userName;
                    dbStudentReg.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                }

                // ---------- Sync Main Student Table ----------
                if (!string.IsNullOrWhiteSpace(dbStudentReg.ApplicationNo))
                {
                    var mainStudent = await _context.Tbl_Students
                        .FirstOrDefaultAsync(x => x.ApplicationNo == dbStudentReg.ApplicationNo);

                    if (mainStudent != null)
                    {
                        mainStudent.FirstName = model.FirstName;
                        mainStudent.MiddleName = model.MiddleName;
                        mainStudent.LastName = model.LastName;
                        mainStudent.DOB = model.DOB;
                        mainStudent.Email = model.Email;
                        mainStudent.ContactNo = model.ContactNo;
                        mainStudent.LastClass = model.LastClass;
                        mainStudent.AadhaarNumber = model.AadhaarNumber;
                        mainStudent.APAARId = model.APAARId;
                        mainStudent.PENNumber = model.PENNumber;
                        mainStudent.LocalAddress = model.LocalAddress;
                        mainStudent.PermanentAddress = model.PermanentAddress;
                        mainStudent.CategoryId = model.CategoryId;
                        mainStudent.GenderId = model.GenderId;
                        mainStudent.ReligionId = model.ReligionId;
                        mainStudent.IsHandicap = model.IsHandicap;
                        mainStudent.HandicapDetails = model.HandicapDetails;
                        mainStudent.IdentificationMark = model.IdentificationMark;
                        mainStudent.AdmissionBatchId = model.AdmissionBatchId;
                        mainStudent.ParentId = parent.ParentId;
                        mainStudent.IsTranspot = model.IsTranspot;
                        mainStudent.TranspotId = model.TranspotId;
                        mainStudent.IsHostel = model.IsHostel;
                        mainStudent.HostelId = model.HostelId;
                        mainStudent.UpdatedBy = userName;
                        mainStudent.UpdatedDate = DateTime.Now;

                        await _context.SaveChangesAsync();
                    }
                }

                // ---------- Previous School ----------
                if (previousSchool != null)
                {
                    var dbPrev = await _context.Tbl_PreviousSchoolRecord
                        .FirstOrDefaultAsync(x => x.StudentId == dbStudentReg.StudentId); // Use StudentId

                    if (dbPrev == null)
                    {
                        previousSchool.StudentId = dbStudentReg.StudentId;
                        previousSchool.ApplicationNo = dbStudentReg.ApplicationNo;
                        previousSchool.AddedBy = userName;
                        previousSchool.AddedDate = DateTime.Now;
                        _context.Tbl_PreviousSchoolRecord.Add(previousSchool);
                    }
                    else
                    {
                        dbPrev.LastSchoolName = previousSchool.LastSchoolName;
                        dbPrev.LastSchoolClass = previousSchool.LastSchoolClass;
                        dbPrev.LastSchoolAddress = previousSchool.LastSchoolAddress;
                        dbPrev.LastSchoolBoard = previousSchool.LastSchoolBoard;
                        dbPrev.LastSchoolCode = previousSchool.LastSchoolCode;
                        dbPrev.LastSchoolUDISECode = previousSchool.LastSchoolUDISECode;
                        dbPrev.ReasonForChange = previousSchool.ReasonForChange;
                        dbPrev.UpdatedBy = userName;
                        dbPrev.UpdatedDate = DateTime.Now;
                    }
                    await _context.SaveChangesAsync();
                }

                // ---------- Documents ----------
                await SaveOrUpdateDocument(dbStudentReg.StudentId, dbStudentReg.ApplicationNo, "AADHAAR", AadhaarFile);
                await SaveOrUpdateDocument(dbStudentReg.StudentId, dbStudentReg.ApplicationNo, "PHOTO", PhotoFile);
                await SaveOrUpdateDocument(dbStudentReg.StudentId, dbStudentReg.ApplicationNo, "TC", TCFile);
                await SaveOrUpdateDocument(dbStudentReg.StudentId, dbStudentReg.ApplicationNo, "MARKSHEET", MarksheetFile);

                await transaction.CommitAsync();

                SuccessMessage(isNewStudent
                    ? "Student Registered Successfully"
                    : "Student Updated Successfully");

                if (isNewStudent && !string.IsNullOrWhiteSpace(model.Email))
                {
                    await _lookup.PopulateAsync(model);
                    await _notificationService.SendAsync(
                        "STUDENT_REGISTRATION",
                        model.Email,
                        model.StudentId,
                        model,
                        model.Parent,
                        model.PreviousSchoolRecord);
                }

                return RedirectToAction(nameof(SaveStudentRegistrations));
            }
            catch (Exception)
            {
                if (transaction != null)
                    await transaction.RollbackAsync();
                throw;
            }
            return Json(new { success = true, message = "POST Action called successfully!" });
        }

        [SkipPermission]
        private async Task SaveOrUpdateDocument(long studentId, string ApplicationNo, string documentType, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return;

            string folder = Path.Combine(_env.WebRootPath, "Uploads", "Students", studentId.ToString());

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

            string filePath = Path.Combine(folder, fileName);

            using (var stream =
                new FileStream(
                    filePath,
                    FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            byte[] bytes;

            using (var ms = new MemoryStream())
            {
                using var input = file.OpenReadStream();
                await input.CopyToAsync(ms);
                bytes = ms.ToArray();
            }

            var document = new Tbl_StudentDocument
            {
                StudentId = studentId,
                ApplicationNo=ApplicationNo,
                DocumentType = documentType,
                FileName = file.FileName,
                FilePath = filePath,
                FileData = bytes,
                UploadDate = DateTime.Now
            };

            _context.Tbl_StudentDocument.Add(document);

            await _context.SaveChangesAsync();

        }
      

[SkipPermission]
    public IActionResult GetDocumentByType(long studentId, string documentType)
    {
        var doc = _context.Tbl_StudentDocument
            .FirstOrDefault(x => x.StudentId == studentId && x.DocumentType == documentType);
        if (doc == null || string.IsNullOrEmpty(doc.FilePath))
            return NotFound();

        var physicalPath = Path.Combine(_env.WebRootPath, doc.FilePath);
        if (!System.IO.File.Exists(physicalPath))
            return NotFound();

        var contentType = GetContentType(doc.FileName);
        var fileStream = System.IO.File.OpenRead(physicalPath);

        // 🔥 Set Content-Disposition to "inline" to display in browser
        var cd = new ContentDispositionHeaderValue("inline")
        {
            FileName = doc.FileName
        };
        Response.Headers.Add("Content-Disposition", cd.ToString());

        return File(fileStream, contentType);
    }

    private string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".pdf" => "application/pdf",
                ".doc" or ".docx" => "application/msword",
                ".xls" or ".xlsx" => "application/vnd.ms-excel",
                _ => "application/octet-stream"
            };
        }
        [SkipPermission]
        private string GenerateApplicationNo()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssffffff");
        }
        [SkipPermission]
        public IActionResult GetParent(string mobile)
        {
            var parent = _context.Tbl_Parents.FirstOrDefault(x => x.FatherContactNo == mobile ||x.MotherContactNo == mobile || x.GuardianContactNo == mobile);
            if (parent == null)
                return Json(null);
            return Json(parent);
        }
        [SkipPermission]
        [HttpPost]
        public async Task<IActionResult> ExportExcel(StudentReportPageVM model)
        {
            var students = await _repo
                .GetStudentReport(model.Filter);

            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Students");

            int col = 1;

            var cols = model.Filter.SelectedColumns ?? new List<string>();

            if (cols.Contains("ApplicationNo"))
                worksheet.Cell(1, col++).Value = "Application No";

            if (cols.Contains("StudentName"))
                worksheet.Cell(1, col++).Value = "Student Name";

            if (cols.Contains("FatherName"))
                worksheet.Cell(1, col++).Value = "Father Name";

            if (cols.Contains("FatherMobile"))
                worksheet.Cell(1, col++).Value = "Father Mobile";

            if (cols.Contains("DOB"))
                worksheet.Cell(1, col++).Value = "DOB";

            if (cols.Contains("AadhaarNumber"))
                worksheet.Cell(1, col++).Value = "Aadhaar No";

            int row = 2;

            foreach (var item in students)
            {
                col = 1;

                if (cols.Contains("ApplicationNo"))
                    worksheet.Cell(row, col++).Value = item.ApplicationNo;

                if (cols.Contains("StudentName"))
                    worksheet.Cell(row, col++).Value = item.StudentName;

                if (cols.Contains("FatherName"))
                    worksheet.Cell(row, col++).Value = item.FatherName;

                if (cols.Contains("FatherMobile"))
                    worksheet.Cell(row, col++).Value = item.FatherMobile;

                if (cols.Contains("DOB"))
                    worksheet.Cell(row, col++).Value = item.DOB.ToString("dd/MM/yyyy");

                if (cols.Contains("AadhaarNumber"))
                    worksheet.Cell(row, col++).Value = item.AadhaarNumber;

                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"StudentReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudentRegistrations(long id)
        {
            var student = await _context.Tbl_StudentsRegistrations
                .FirstOrDefaultAsync(x => x.StudentId == id);

            if (student == null)
            {
                ErrorMessage("Student not found.");
                return RedirectToAction(nameof(StudentRegistrations));
            }

            student.IsActive = !student.IsActive;

            student.UpdatedDate = DateTime.Now;
            student.UpdatedBy = User.Identity?.Name;

            _context.Tbl_StudentsRegistrations.Update(student);

            await _context.SaveChangesAsync();

            SuccessMessage(
                student.IsActive
                ? "Student activated successfully."
                : "Student deactivated successfully.");

            return RedirectToAction(nameof(StudentRegistrations));
        }
        #region old
        //[SkipPermission]
        //[HttpPost]
        //public async Task<IActionResult> UpdateStudentStatus(StudentStatusUpdateVM model)
        //{
        //    using var transaction =
        //        await _context.Database.BeginTransactionAsync();
        //    var admittedStudents = new List<Tbl_Students>();
        //    try
        //    {
        //        var currentUser = HttpContext.Session.GetCurrentUser();
        //        string LuserName = currentUser?.UserName ?? User.Identity?.Name ?? "";
        //        var ids = model.StudentIds
        //            .Split(',')
        //            .Select(long.Parse)
        //            .ToList();

        //        var registrations =
        //            await _context.Tbl_StudentsRegistrations
        //            .Include(x => x.Parent)
        //            .Where(x => ids.Contains(x.StudentId))
        //            .ToListAsync();
        //        List<DataListItem> dataListItems = new List<DataListItem>();
        //        dataListItems = GetDataListItems("Status");
        //        var rejected = dataListItems.Where(x => x.DataListItemValue.Contains("Rejected")).Select(x => x.DataListItemId).FirstOrDefault();
        //        var tcIssued= dataListItems.Where(x => x.DataListItemValue.Contains("TC")).Select(x => x.DataListItemId).FirstOrDefault();
        //        var admitted = dataListItems.Where(x => x.DataListItemValue.Contains("Admitted")).Select(x => x.DataListItemId).FirstOrDefault();
        //        foreach (var registration in registrations)
        //        {
        //            // Rejected / TC Issued
        //            if (model.StatusId == rejected || model.StatusId == tcIssued)
        //            {
        //                registration.Status = model.StatusId;
        //                registration.UpdatedDate = DateTime.Now;
        //                registration.UpdatedBy = LuserName;
        //                continue;
        //            }

        //            // Admitted
        //            if (model.StatusId == admitted)
        //            {
        //                var alreadyExists =
        //                    await _context.Tbl_Students
        //                    .AnyAsync(x =>
        //                        x.StudentRegisterId ==
        //                        registration.StudentId);

        //                if (alreadyExists)
        //                {

        //                    continue;
        //                }

        //                registration.Status = admitted;
        //                registration.RegClassId = model.ClassId;
        //                registration.AdmissionBatchId = model.BatchId;
        //                registration.UpdatedDate = DateTime.Now;
        //                registration.UpdatedBy = LuserName;

        //                var fullName =
        //                    $"{registration.FirstName} " +
        //                    $"{registration.MiddleName} " +
        //                    $"{registration.LastName}"
        //                    .Replace("  ", " ")
        //                    .Trim();

        //                var userName =
        //                    fullName.Replace(" ", "");

        //                var existingUser =
        //                    await _userManager
        //                    .FindByNameAsync(userName);

        //                if (existingUser != null)
        //                {
        //                    userName =
        //                        $"{userName}{registration.StudentId}";
        //                }

        //                string password =
        //                    registration.DOB
        //                    .ToString("dd/MM/yyyy");

        //                var user = new ApplicationUser
        //                {
        //                    UserName = userName,
        //                    FullName = fullName,
        //                    Email = registration.Email,
        //                    IsActive = true,
        //                    NormalPassword = password
        //                };

        //                var result =
        //                    await _userManager
        //                    .CreateAsync(user, password);

        //                if (!result.Succeeded)
        //                {
        //                    throw new Exception(
        //                        string.Join(",",
        //                        result.Errors
        //                        .Select(x => x.Description)));
        //                }

        //                await _userManager
        //                    .AddToRoleAsync(user, "Students");

        //                var student =
        //                    new Tbl_Students
        //                    {
        //                        StudentRegisterId =
        //                            registration.StudentId,

        //                        UserId = user.Id,

        //                        AdmitClassId =
        //                            model.ClassId ?? 0,

        //                        AdmitSectionId =
        //                            model.SectionId ?? 0,

        //                        AdmitBatchId =
        //                            model.BatchId ?? 0,
        //                        AdmissionBatchId=registration.AdmissionBatchId,

        //                        ApplicationNo =
        //                            registration.ApplicationNo,

        //                        FirstName =
        //                            registration.FirstName,

        //                        MiddleName =
        //                            registration.MiddleName,

        //                        LastName =
        //                            registration.LastName,

        //                        DOB =
        //                            registration.DOB,

        //                        Email =
        //                            registration.Email,

        //                        ContactNo =
        //                            registration.ContactNo,

        //                        LastClass =
        //                            registration.LastClass,

        //                        AadhaarNumber =
        //                            registration.AadhaarNumber,

        //                        APAARId =
        //                            registration.APAARId,

        //                        PENNumber =
        //                            registration.PENNumber,

        //                        LocalAddress =
        //                            registration.LocalAddress,

        //                        PermanentAddress =
        //                            registration.PermanentAddress,

        //                        CategoryId =
        //                            registration.CategoryId,
        //                        GenderId =
        //                            registration.GenderId,
        //                        ReligionId =
        //                            registration.ReligionId,
        //                        ParentId =
        //                            registration.ParentId,
        //                        IsHandicap =
        //                            registration.IsHandicap,
        //                        HandicapDetails =
        //                            registration.HandicapDetails,
        //                        IdentificationMark =
        //                            registration.IdentificationMark,
        //                        IsInitialClassAdmission=registration.IsInitialClassAdmission,
        //                        IsHostel=registration.IsHostel,
        //                        HostelId=registration.HostelId,
        //                        IsTranspot=registration.IsTranspot,
        //                        TranspotId=registration.TranspotId,
        //                        Status= admitted,
        //                        AddedBy =LuserName,

        //                        IsActive = true
        //                    };

        //                _context.Tbl_Students.Add(student);


        //            }
        //        }

        //        await _context.SaveChangesAsync();

        //        await transaction.CommitAsync();

        //        await _lookup.PopulateAsync(student);
        //        ApplicationUser? existingUserss = null;
        //        existingUserss = await _userManager.FindByIdAsync(student.UserId);
        //        await _notificationService.SendAsync(
        //    "STUDENT_ADMISSION_CONFIRMED",
        //    student.Email,
        //    student.StudentId,
        //   student,
        //   student.Parent,
        //    existingUserss);
        //        return Json(new
        //        {
        //            success = true,
        //            message = "Status updated successfully."
        //        });

        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();

        //        return Json(new
        //        {
        //            success = false,
        //            message = ex.Message
        //        });
        //    }
        //}
        #endregion
        [SkipPermission]
        [HttpPost]
       
        public async Task<IActionResult> UpdateStudentStatus(StudentStatusUpdateVM model)
        {
            // ---------- 1) Input validation ----------
            if (string.IsNullOrWhiteSpace(model?.StudentIds))
                return Json(new { success = false, message = "No students selected." });

            var ids = model.StudentIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => long.TryParse(s, out var v) ? v : (long?)null)
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
                return Json(new { success = false, message = "Invalid student ids." });

            // ---------- 2) Resolve statuses once ----------
            var dataListItems = GetDataListItems("Status");

           int? rejected = dataListItems.FirstOrDefault(x =>
                x.DataListItemValue.Contains("Rejected", StringComparison.OrdinalIgnoreCase))?.DataListItemId;

            int? tcIssued = dataListItems.FirstOrDefault(x =>
                x.DataListItemValue.Contains("TC", StringComparison.OrdinalIgnoreCase))?.DataListItemId;

            int? admitted = dataListItems.FirstOrDefault(x =>
                x.DataListItemValue.Contains("Admitted", StringComparison.OrdinalIgnoreCase))?.DataListItemId;

            if (admitted == null)
                return Json(new { success = false, message = "Admitted status is not configured." });

            var currentUser = HttpContext.Session.GetCurrentUser();
            string luserName = currentUser?.UserName ?? User.Identity?.Name ?? "";

            // ---------- 3) Transaction ----------
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var admittedStudents = new List<Tbl_Students>();

            try
            {
                var registrations = await _context.Tbl_StudentsRegistrations
                    .Include(x => x.Parent)
                    .Where(x => ids.Contains(x.StudentId))
                    .ToListAsync();

                foreach (var registration in registrations)
                {
                    // ---- Rejected / TC ----
                    if (model.StatusId == rejected || model.StatusId == tcIssued)
                    {
                        registration.Status = model.StatusId;
                        registration.UpdatedDate = DateTime.Now;
                        registration.UpdatedBy = luserName;
                        continue;
                    }

                    // ---- Admitted ----
                    if (model.StatusId == admitted)
                    {
                        var alreadyExists = await _context.Tbl_Students
                            .AnyAsync(x => x.StudentRegisterId == registration.StudentId);

                        if (alreadyExists)
                        {
                            // Still keep the registration in sync
                            registration.Status = admitted;
                            registration.UpdatedDate = DateTime.Now;
                            registration.UpdatedBy = luserName;
                            continue;
                        }

                        registration.Status = admitted;
                        registration.RegClassId = model.ClassId;
                        registration.AdmissionBatchId = model.BatchId;
                        registration.UpdatedDate = DateTime.Now;
                        registration.UpdatedBy = luserName;

                        var fullName = string.Join(" ",
                                new[] { registration.FirstName, registration.MiddleName, registration.LastName }
                                    .Where(s => !string.IsNullOrWhiteSpace(s)))
                            .Trim();

                        if (string.IsNullOrWhiteSpace(fullName))
                            fullName = $"Student {registration.StudentId}";

                        var userName = fullName.Replace(" ", "");
                        if (string.IsNullOrWhiteSpace(userName))
                            userName = $"student{registration.StudentId}";

                        if (await _userManager.FindByNameAsync(userName) != null)
                            userName = $"{userName}{registration.StudentId}";

                        string password = registration.DOB.ToString("dd/MM/yyyy");

                        var user = new ApplicationUser
                        {
                            UserName = userName,
                            FullName = fullName,
                            Email = registration.Email,
                            IsActive = true,
                            NormalPassword = password
                        };

                        var result = await _userManager.CreateAsync(user, password);
                        if (!result.Succeeded)
                        {
                            throw new Exception(string.Join(", ",
                                result.Errors.Select(x => x.Description)));
                        }

                        await _userManager.AddToRoleAsync(user, "Students");

                        var student = new Tbl_Students
                        {
                            StudentRegisterId = registration.StudentId,
                            UserId = user.Id,
                            AdmitClassId = model.ClassId ?? 0,
                            AdmitSectionId = model.SectionId ?? 0,
                            AdmitBatchId = model.BatchId ?? 0,
                            AdmissionBatchId = registration.AdmissionBatchId,
                            ApplicationNo = registration.ApplicationNo,
                            FirstName = registration.FirstName,
                            MiddleName = registration.MiddleName,
                            LastName = registration.LastName,
                            DOB = registration.DOB,
                            Email = registration.Email,
                            ContactNo = registration.ContactNo,
                            LastClass = registration.LastClass,
                            AadhaarNumber = registration.AadhaarNumber,
                            APAARId = registration.APAARId,
                            PENNumber = registration.PENNumber,
                            LocalAddress = registration.LocalAddress,
                            PermanentAddress = registration.PermanentAddress,
                            CategoryId = registration.CategoryId,
                            GenderId = registration.GenderId,
                            ReligionId = registration.ReligionId,
                            ParentId = registration.ParentId,
                            IsHandicap = registration.IsHandicap,
                            HandicapDetails = registration.HandicapDetails,
                            IdentificationMark = registration.IdentificationMark,
                            IsInitialClassAdmission = registration.IsInitialClassAdmission,
                            IsHostel = registration.IsHostel,
                            HostelId = registration.HostelId,
                            IsTranspot = registration.IsTranspot,
                            TranspotId = registration.TranspotId,
                            Status = admitted,
                            AddedBy = luserName,
                            IsActive = true
                        };

                        _context.Tbl_Students.Add(student);
                        admittedStudents.Add(student);       // ✅ collect for post-commit
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }

            // ---------- 4) Post-commit work (NOT rolled back) ----------
            foreach (var student in admittedStudents)
            {
                try
                {
                    await _lookup.PopulateAsync(student);

                    var user = await _userManager.FindByIdAsync(student.UserId);

                    await _notificationService.SendAsync(
                        "STUDENT_ADMISSION_CONFIRMED",
                        student.Email,
                        student.StudentId,
                        student,
                        student.Parent,
                        user);
                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        success = true,
                        message = ex.Message
                    });
                }
            }

            return Json(new
            {
                success = true,
                message = admittedStudents.Count > 0
                    ? $"{admittedStudents.Count} student(s) admitted successfully."
                    : "Status updated successfully."
            });
        }
        #endregion
        #region Promotion
        public async Task<IActionResult> StudentPromotions()
        {
            ViewBag.BatchList = _context.Batches.Where(x => x.ActiveForAdmission || x.ActiveForRegistration).ToList();
            ViewBag.CategoryList = GetDataListItems("Category");
            ViewBag.ReligionList = GetDataListItems("Religion");
            ViewBag.BoardList = GetDataListItems("Board");
            ViewBag.ClassList = GetDataListItems("Class");
            ViewBag.GenderList = GetDataListItems("Gender");
            ViewBag.StatusList = GetDataListItems("Status");
            ViewBag.SectionList = GetDataListItems("Section");
            var model = new StudentReportPageVM();

            model.Filter = new StudentListFilterVM();

            model.Filter.SelectedColumns =
            [
                "ApplicationNo","StudentName","FatherName", "MotherName","Class","Section","Batch"
            ];
            model.Students = await _repo.GetAdmittedStudentsList(model.Filter);

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> PromoteStudents(StudentStatusUpdateVM model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var currentUser = HttpContext.Session.GetCurrentUser();
                string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";

                var ids = model.StudentIds
                               .Split(',', StringSplitOptions.RemoveEmptyEntries)
                               .Select(long.Parse)
                               .ToList();

                var students = await _context.Tbl_Students
                                             .Where(x => ids.Contains(x.StudentRegisterId))
                                             .ToListAsync();

                if (!students.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No students found."
                    });
                }

                foreach (var student in students)
                {
                    if (model.ClassId.HasValue)
                        student.AdmitClassId = model.ClassId.Value;

                    if (model.SectionId.HasValue)
                        student.AdmitSectionId = model.SectionId.Value;

                    if (model.BatchId.HasValue)
                        student.AdmitBatchId = model.BatchId.Value;

                    student.UpdatedBy = userName;
                    student.UpdatedDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = "Class, Section and Batch updated successfully."
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        #endregion Promotion
        #region profile
        // GET: /Students/Profile/101
        [HttpGet]
        [SkipPermission]
        [Route("Students/Profile/{studentId:long}")]
        public async Task<IActionResult> Profile(long studentId)
        {
            var profile = await _repo.GetStudentProfileAsync(studentId);
            if (profile?.Student == null) return NotFound();

            return View(profile);
        }

        // GET (AJAX): /Students/OldBatchSummary?studentId=101&batchId=3
        [HttpGet]
        public async Task<IActionResult> OldBatchSummary(long studentId, int batchId)
        {
            var result = await _repo.GetBatchSummaryAsync(studentId, batchId);

            return Json(new
            {
                batchName = result.BatchInfo?.AcademicYear,
                startDate = result.BatchInfo?.StartDate.ToString("dd-MMM-yyyy"),
                endDate = result.BatchInfo?.EndDate.ToString("dd-MMM-yyyy"),
                totalFee = System.Linq.Enumerable.Sum(result.Fees, f => f.FeeAmount),
                totalPaid = System.Linq.Enumerable.Sum(result.Fees, f => f.PaidAmount),
                totalBalance = System.Linq.Enumerable.Sum(result.Fees, f => f.BalanceAmount),
                attendance = result.Attendance
            });
        }

        // GET: /Students/DownloadIdCard?studentId=101
        [HttpGet]
        public async Task<IActionResult> DownloadIdCard(long studentId)
        {
            var profile = await _repo.GetStudentProfileAsync(studentId);
            if (profile?.Student == null) return NotFound();

            byte[] pdfBytes = await _idCardService.GenerateIdCardAsync(profile);
            return File(pdfBytes, "application/pdf", $"IDCard_{profile.Student.ApplicationNo}.pdf");
        }
        #endregion
        #region Student Report
        public async Task<IActionResult> StudentsReport()
        {
            ViewBag.BatchList = _context.Batches.Where(x => x.ActiveForAdmission || x.ActiveForRegistration).ToList();
            ViewBag.ClassList = GetDataListItems("Class");
            ViewBag.StatusList = GetDataListItems("Status");
            ViewBag.SectionList = GetDataListItems("Section");
            var model = new StudentReportPageVM();

            model.Filter = new StudentListFilterVM();

            model.Filter.SelectedColumns =
            [
                "ApplicationNo","StudentName","FatherName", "MotherName"
            ];
            if (model.Filter.StatusId == null || model.Filter.StatusId == 0)
            {
                var statusList = ViewBag.StatusList as List<DataListItem>;
                model.Filter.StatusId = statusList?.FirstOrDefault(x => x.DataListItemValue== "Admitted")?.DataListItemId;
            }
            model.Students = await _repo.GetStudentReportStatusWise(model.Filter);

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> StudentsReport(StudentReportPageVM model)
        {
            ViewBag.BatchList = _context.Batches.Where(x => x.ActiveForAdmission || x.ActiveForRegistration).ToList();
           
            ViewBag.ClassList = GetDataListItems("Class");
           
            ViewBag.StatusList = GetDataListItems("Status");
            ViewBag.SectionList = GetDataListItems("Section");
            model.Students = await _repo.GetStudentReportStatusWise(model.Filter);


            return View(model);
        }
        public async Task<IActionResult> StudentsTCReport()
        {
            ViewBag.BatchList = _context.Batches.Where(x => x.ActiveForAdmission || x.ActiveForRegistration).ToList();
            ViewBag.ClassList = GetDataListItems("Class");
            ViewBag.SectionList = GetDataListItems("Section");
            ViewBag.StudentCertificateTemplates =await _TemplateRepository.GetStudentCertificateTemplatesAsync("Student");
            var model = new StudentReportPageVM();
            model.Filter = new StudentListFilterVM();

            model.Filter.SelectedColumns =
            [
                "ApplicationNo","StudentName","FatherName", "MotherName","Class","Section"
            ];
            if (model.Filter.StatusId == null || model.Filter.StatusId == 0)
            {
                var statusList = ViewBag.StatusList as List<DataListItem>;
                model.Filter.StatusId = statusList?.FirstOrDefault(x => x.DataListItemValue == "TCIssued")?.DataListItemId;
            }
            model.Students = await _repo.GetStudentReportStatusWise(model.Filter);

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> StudentsTCReport(StudentReportPageVM model)
        {
            ViewBag.BatchList = _context.Batches.Where(x => x.ActiveForAdmission || x.ActiveForRegistration).ToList();
            ViewBag.StudentCertificateTemplates = await _TemplateRepository.GetStudentCertificateTemplatesAsync("Student");
            ViewBag.ClassList = GetDataListItems("Class");
            ViewBag.SectionList = GetDataListItems("Section");
            model.Students = await _repo.GetStudentReportStatusWise(model.Filter);


            return View(model);
        }
        #endregion
    }
}
