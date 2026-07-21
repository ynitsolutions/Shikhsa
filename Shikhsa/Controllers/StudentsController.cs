using ClosedXML.Excel;
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
        public StudentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
     PermissionService permissionService, IWebHostEnvironment env,  StudentReportRepository repo, EmailService emailService,NotificationService notificationService) : base(userManager, permissionService, context,emailService)
        {
            _context = context;
            _env = env;
            _repo = repo;
            _userManager = userManager;
            _notificationService = notificationService;
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
            ViewBag.HostelList = GetDataListItems("Hostel List");
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
            ViewBag.HostelList = GetDataListItems("Hostel List");
            model.Students =  await _repo.GetStudentReport(model.Filter);
            
           
            return View(model);
        }
       
        [HttpGet]
        public async Task<IActionResult> SaveStudentRegistrations(long? id)
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
            ViewBag.HostelList = GetDataListItems("Hostel List");
            ViewBag.InitialClassId = (await GetSchoolInfo())?.InitialClassId;
            if (id > 0)
            {
                var student = await _context.Tbl_StudentsRegistrations.Include(x => x.Parent).Include(x => x.PreviousSchoolRecord)
                    .FirstOrDefaultAsync(x => x.StudentId == id);
                ViewBag.Documents = await _context.Tbl_StudentDocument.Where(x => x.StudentId == id).ToListAsync();
                if (student == null)
                    return NotFound();

                return View(student);
            }
            else
            {
                return View(new Tbl_StudentsRegistrations());
            }


        }
        #region Old
        //    [HttpPost]
        //    public async Task<IActionResult> SaveStudentRegistrations(Tbl_StudentsRegistrations model,Tbl_Parents parent,Tbl_PreviousSchoolRecord previousSchool,IFormFile AadhaarFile,IFormFile PhotoFile,IFormFile TCFile,
        //        IFormFile MarksheetFile)
        //    {
        //        var currentUser = HttpContext.Session.GetCurrentUser();
        //        string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";

        //        try
        //        {
        //            using var transaction = await _context.Database.BeginTransactionAsync();

        //            #region Parent Save

        //            if (parent.ParentId == 0)
        //            {
        //                parent.AddedBy = userName;
        //                Console.WriteLine(parent.ParentId);

        //                foreach (var entry in _context.ChangeTracker.Entries<Tbl_Parents>())
        //                {
        //                    Console.WriteLine($"Tracked ParentId = {entry.Entity.ParentId}");
        //                }
        //                _context.Tbl_Parents.Add(parent);
        //                await _context.SaveChangesAsync();

        //                model.ParentId = parent.ParentId;
        //            }
        //            else
        //            {
        //                var dbParent = await _context.Tbl_Parents
        //                    .FirstOrDefaultAsync(x => x.ParentId == parent.ParentId);

        //                if (dbParent == null)
        //                    throw new Exception("Parent record not found.");
        //                Console.WriteLine($"parent object       : {parent?.ParentId}");
        //                Console.WriteLine($"model.Parent object : {model.Parent?.ParentId}");
        //                Console.WriteLine(Object.ReferenceEquals(parent, model.Parent));
        //                dbParent.FatherFirstName = parent.FatherFirstName;
        //                dbParent.FatherMiddleName = parent.FatherMiddleName;
        //                dbParent.FatherLastName = parent.FatherLastName;
        //                dbParent.FatherContactNo = parent.FatherContactNo;
        //                dbParent.FatherEmail = parent.FatherEmail;
        //                dbParent.FatherAddress = parent.FatherAddress;

        //                dbParent.MotherFirstName = parent.MotherFirstName;
        //                dbParent.MotherMiddleName = parent.MotherMiddleName;
        //                dbParent.MotherLastName = parent.MotherLastName;
        //                dbParent.MotherContactNo = parent.MotherContactNo;
        //                dbParent.MotherEmail = parent.MotherEmail;
        //                dbParent.MotherAddress = parent.MotherAddress;

        //                dbParent.UpdatedBy = userName;
        //                dbParent.UpdatedDate = DateTime.Now;

        //                await _context.SaveChangesAsync();

        //                model.ParentId = parent.ParentId;
        //            }

        //            #endregion

        //            #region Student Save

        //            bool isNewStudent = model.StudentId == 0;

        //            if (isNewStudent)
        //            {
        //                model.ApplicationNo = GenerateApplicationNo();
        //                model.AddedDate = DateTime.Now;
        //                model.AddedBy = userName;
        //                model.Status = 26;
        //                _context.Tbl_StudentsRegistrations.Add(model);
        //                await _context.SaveChangesAsync();
        //            }
        //            else
        //            {
        //                var dbStudent = await _context.Tbl_StudentsRegistrations
        //                    .FirstOrDefaultAsync(x => x.StudentId == model.StudentId);

        //                if (dbStudent == null)
        //                    throw new Exception("Student record not found.");

        //                dbStudent.FirstName = model.FirstName;
        //                dbStudent.MiddleName = model.MiddleName;
        //                dbStudent.LastName = model.LastName;
        //                dbStudent.DOB = model.DOB;
        //                dbStudent.Email = model.Email;
        //                dbStudent.ContactNo = model.ContactNo;
        //                dbStudent.AadhaarNumber = model.AadhaarNumber;
        //                dbStudent.APAARId = model.APAARId;
        //                dbStudent.PENNumber = model.PENNumber;
        //                dbStudent.LocalAddress = model.LocalAddress;
        //                dbStudent.PermanentAddress = model.PermanentAddress;
        //                dbStudent.CategoryId = model.CategoryId;
        //                dbStudent.ReligionId = model.ReligionId;
        //                dbStudent.IsHandicap = model.IsHandicap;
        //                dbStudent.HandicapDetails = model.HandicapDetails;
        //                dbStudent.IdentificationMark = model.IdentificationMark;
        //                dbStudent.AdmissionBatchId = model.AdmissionBatchId;
        //                dbStudent.ParentId = model.ParentId;
        //                dbStudent.Status = 26;
        //                dbStudent.UpdatedBy = userName;
        //                dbStudent.UpdatedDate = DateTime.Now;

        //                await _context.SaveChangesAsync();
        //            }

        //            #endregion

        //            #region Previous School

        //            if (previousSchool != null)
        //            {
        //                var dbPrevious = await _context.Tbl_PreviousSchoolRecord
        //                    .FirstOrDefaultAsync(x => x.StudentId == model.StudentId);

        //                if (dbPrevious == null)
        //                {
        //                    previousSchool.StudentId = model.StudentId;
        //                    previousSchool.AddedBy = userName;

        //                    _context.Tbl_PreviousSchoolRecord.Add(previousSchool);
        //                }
        //                else
        //                {
        //                    dbPrevious.LastSchoolName = previousSchool.LastSchoolName;
        //                    dbPrevious.LastSchoolClass = previousSchool.LastSchoolClass;
        //                    dbPrevious.LastSchoolAddress = previousSchool.LastSchoolAddress;
        //                    dbPrevious.LastSchoolBoard = previousSchool.LastSchoolBoard;
        //                    dbPrevious.LastSchoolCode = previousSchool.LastSchoolCode;
        //                    dbPrevious.LastSchoolUDISECode = previousSchool.LastSchoolUDISECode;
        //                    dbPrevious.ReasonForChange = previousSchool.ReasonForChange;

        //                    dbPrevious.UpdatedBy = userName;
        //                    dbPrevious.UpdatedDate = DateTime.Now;
        //                }

        //                await _context.SaveChangesAsync();
        //            }

        //            #endregion

        //            #region Documents

        //            await SaveDocument(model.StudentId, "AADHAAR", AadhaarFile);
        //            await SaveDocument(model.StudentId, "PHOTO", PhotoFile);
        //            await SaveDocument(model.StudentId, "TC", TCFile);
        //            await SaveDocument(model.StudentId, "MARKSHEET", MarksheetFile);

        //            #endregion

        //            await transaction.CommitAsync();

        //            SuccessMessage(isNewStudent
        //                ? "Student Registered Successfully"
        //                : "Student Updated Successfully");
        //        }
        //        catch (Exception ex)
        //        {
        //            // Log Error Here
        //            // _logger.LogError(ex, "Student Registration Failed");

        //            ErrorMessage(ex.Message);

        //            ViewBag.BatchList = _context.Batches
        //                .Where(x => x.ActiveForAdmission || x.ActiveForRegistration)
        //                .ToList();

        //            ViewBag.CategoryList = GetDataListItems("Category");
        //            ViewBag.ReligionList = GetDataListItems("Religion");
        //            ViewBag.BoardList = GetDataListItems("Board");
        //            ViewBag.ClassList = GetDataListItems("Class");
        //            ViewBag.GenderList = GetDataListItems("Gender");
        //            ViewBag.StatusList = GetDataListItems("Status");
        //            ViewBag.SectionList = GetDataListItems("Section");
        //            ViewBag.InitialClassId = (await GetSchoolInfo())?.InitialClassId;

        //            return View("SaveStudentRegistrations", model);
        //        }

        //        // Send Email AFTER transaction is committed
        //       // try
        //        {
        //            string fullName = string.Join(" ",
        //                new[]
        //                {
        //            model.FirstName,
        //            model.MiddleName,
        //            model.LastName
        //                }.Where(x => !string.IsNullOrWhiteSpace(x)));

        //            string guardianName = string.Join(" ",
        //                new[]
        //                {
        //            parent.GuardianFirstName,
        //            parent.GuardianMiddleName,
        //            parent.GuardianLastName
        //                }.Where(x => !string.IsNullOrWhiteSpace(x)));
        //            await _notificationService.SendAsync(
        //"STUDENT_REGISTRATION",
        //model.Email!,
        //model.StudentId,
        //model,
        //parent,
        //previousSchool);

        //        }
        //        //catch (Exception ex)
        //        //{


        //        //}

        //        return RedirectToAction(nameof(StudentRegistrations));
        //    }
        #endregion old
        [HttpPost]
        public async Task<IActionResult> SaveStudentRegistrations(
    Tbl_StudentsRegistrations model,
    Tbl_Parents parent,
    Tbl_PreviousSchoolRecord previousSchool,
    IFormFile AadhaarFile,
    IFormFile PhotoFile,
    IFormFile TCFile,
    IFormFile MarksheetFile)
        {
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
            IDbContextTransaction? transaction = null;
            try
            {
                transaction = await _context.Database.BeginTransactionAsync();

                // ******** IMPORTANT ********
                // Remove duplicate Parent instance tracked by MVC Model Binder
                model.Parent = null;

                #region Parent Save

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
                        .FirstOrDefaultAsync(x => x.ParentId == parent.ParentId);

                    if (dbParent == null)
                        throw new Exception("Parent record not found.");

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

                model.ParentId = parent.ParentId;
                model.Parent = null;

                #endregion

                #region Student Save

                bool isNewStudent = model.StudentId == 0;

                if (isNewStudent)
                {
                    model.ApplicationNo = GenerateApplicationNo();
                    model.AddedBy = userName;
                    model.AddedDate = DateTime.Now;
                    model.Status = 26;

                    _context.Tbl_StudentsRegistrations.Add(model);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var dbStudent = await _context.Tbl_StudentsRegistrations
                        .FirstOrDefaultAsync(x => x.StudentId == model.StudentId);

                    if (dbStudent == null)
                        throw new Exception("Student record not found.");

                    dbStudent.FirstName = model.FirstName;
                    dbStudent.MiddleName = model.MiddleName;
                    dbStudent.LastName = model.LastName;
                    dbStudent.DOB = model.DOB;
                    dbStudent.Email = model.Email;
                    dbStudent.ContactNo = model.ContactNo;
                    dbStudent.AadhaarNumber = model.AadhaarNumber;
                    dbStudent.APAARId = model.APAARId;
                    dbStudent.PENNumber = model.PENNumber;
                    dbStudent.LocalAddress = model.LocalAddress;
                    dbStudent.PermanentAddress = model.PermanentAddress;
                    dbStudent.CategoryId = model.CategoryId;
                    dbStudent.ReligionId = model.ReligionId;
                    dbStudent.GenderId = model.GenderId;
                    dbStudent.IsHandicap = model.IsHandicap;
                    dbStudent.HandicapDetails = model.HandicapDetails;
                    dbStudent.IsTranspot = model.IsTranspot;
                    dbStudent.TranspotId = model.TranspotId;
                    dbStudent.HostelId = model.HostelId;
                    dbStudent.IsHostel = model.IsHostel;
                    dbStudent.IdentificationMark = model.IdentificationMark;
                    dbStudent.AdmissionBatchId = model.AdmissionBatchId;
                    dbStudent.ParentId = parent.ParentId;
                    dbStudent.Status = 26;
                    dbStudent.UpdatedBy = userName;
                    dbStudent.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                }

                #endregion

                #region Previous School

                if (previousSchool != null)
                {
                    var dbPrevious = await _context.Tbl_PreviousSchoolRecord
                        .FirstOrDefaultAsync(x => x.StudentId == model.StudentId);

                    if (dbPrevious == null)
                    {
                        previousSchool.StudentId = model.StudentId;
                        previousSchool.AddedBy = userName;
                        previousSchool.AddedDate = DateTime.Now;

                        _context.Tbl_PreviousSchoolRecord.Add(previousSchool);
                    }
                    else
                    {
                        dbPrevious.LastSchoolName = previousSchool.LastSchoolName;
                        dbPrevious.LastSchoolClass = previousSchool.LastSchoolClass;
                        dbPrevious.LastSchoolAddress = previousSchool.LastSchoolAddress;
                        dbPrevious.LastSchoolBoard = previousSchool.LastSchoolBoard;
                        dbPrevious.LastSchoolCode = previousSchool.LastSchoolCode;
                        dbPrevious.LastSchoolUDISECode = previousSchool.LastSchoolUDISECode;
                        dbPrevious.ReasonForChange = previousSchool.ReasonForChange;
                        dbPrevious.UpdatedBy = userName;
                        dbPrevious.UpdatedDate = DateTime.Now;
                    }

                    await _context.SaveChangesAsync();
                }

                #endregion

                #region Documents

                await SaveDocument(model.StudentId, "AADHAAR", AadhaarFile);
                await SaveDocument(model.StudentId, "PHOTO", PhotoFile);
                await SaveDocument(model.StudentId, "TC", TCFile);
                await SaveDocument(model.StudentId, "MARKSHEET", MarksheetFile);

                #endregion

                await transaction.CommitAsync();

                SuccessMessage(isNewStudent
                    ? "Student Registered Successfully"
                    : "Student Updated Successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }

            await _notificationService.SendAsync(
                "STUDENT_REGISTRATION",
                model.Email!,
                model.StudentId,
                model,
                parent,
                previousSchool);

            return RedirectToAction(nameof(StudentRegistrations));
        }

        [SkipPermission]
        private async Task SaveDocument(long studentId, string documentType, IFormFile file)
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
        [SkipPermission]
        [HttpPost]
        public async Task<IActionResult> UpdateStudentStatus(StudentStatusUpdateVM model)
        {
            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var currentUser = HttpContext.Session.GetCurrentUser();
                string LuserName = currentUser?.UserName ?? User.Identity?.Name ?? "";
                var ids = model.StudentIds
                    .Split(',')
                    .Select(long.Parse)
                    .ToList();

                var registrations =
                    await _context.Tbl_StudentsRegistrations
                    .Include(x => x.Parent)
                    .Where(x => ids.Contains(x.StudentId))
                    .ToListAsync();

                foreach (var registration in registrations)
                {
                    // Rejected / TC Issued
                    if (model.StatusId == 28 || model.StatusId == 29)
                    {
                        registration.Status = model.StatusId;
                        registration.UpdatedDate = DateTime.Now;
                        registration.UpdatedBy = LuserName;
                        continue;
                    }

                    // Admitted
                    if (model.StatusId == 27)
                    {
                        var alreadyExists =
                            await _context.Tbl_Students
                            .AnyAsync(x =>
                                x.StudentRegisterId ==
                                registration.StudentId);

                        if (alreadyExists)
                            continue;

                        registration.Status = 27;
                        registration.RegClassId = model.ClassId;
                        registration.AdmissionBatchId = model.BatchId;
                        registration.UpdatedDate = DateTime.Now;
                        registration.UpdatedBy = LuserName;

                        var fullName =
                            $"{registration.FirstName} " +
                            $"{registration.MiddleName} " +
                            $"{registration.LastName}"
                            .Replace("  ", " ")
                            .Trim();

                        var userName =
                            fullName.Replace(" ", "");

                        var existingUser =
                            await _userManager
                            .FindByNameAsync(userName);

                        if (existingUser != null)
                        {
                            userName =
                                $"{userName}{registration.StudentId}";
                        }

                        string password =
                            registration.DOB
                            .ToString("dd/MM/yyyy");

                        var user = new ApplicationUser
                        {
                            UserName = userName,
                            FullName = fullName,
                            Email = registration.Email,
                            IsActive = true,
                            NormalPassword = password
                        };

                        var result =
                            await _userManager
                            .CreateAsync(user, password);

                        if (!result.Succeeded)
                        {
                            throw new Exception(
                                string.Join(",",
                                result.Errors
                                .Select(x => x.Description)));
                        }

                        await _userManager
                            .AddToRoleAsync(user, "Students");

                        var student =
                            new Tbl_Students
                            {
                                StudentRegisterId =
                                    registration.StudentId,

                                UserId = user.Id,

                                AdmitClassId =
                                    model.ClassId ?? 0,

                                AdmitSectionId =
                                    model.SectionId ?? 0,

                                AdmitBatchId =
                                    model.BatchId ?? 0,

                                ApplicationNo =
                                    registration.ApplicationNo,

                                FirstName =
                                    registration.FirstName,

                                MiddleName =
                                    registration.MiddleName,

                                LastName =
                                    registration.LastName,

                                DOB =
                                    registration.DOB,

                                Email =
                                    registration.Email,

                                ContactNo =
                                    registration.ContactNo,

                                LastClass =
                                    registration.LastClass,

                                AadhaarNumber =
                                    registration.AadhaarNumber,

                                APAARId =
                                    registration.APAARId,

                                PENNumber =
                                    registration.PENNumber,

                                LocalAddress =
                                    registration.LocalAddress,

                                PermanentAddress =
                                    registration.PermanentAddress,

                                CategoryId =
                                    registration.CategoryId,

                                GenderId =
                                    registration.GenderId,

                                ReligionId =
                                    registration.ReligionId,

                                ParentId =
                                    registration.ParentId,

                                IsHandicap =
                                    registration.IsHandicap,

                                HandicapDetails =
                                    registration.HandicapDetails,

                                IdentificationMark =
                                    registration.IdentificationMark,
                                AddedBy=LuserName,
                                IsActive = true
                            };

                        _context.Tbl_Students.Add(student);
                    }
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = "Status updated successfully."
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
    }

}
