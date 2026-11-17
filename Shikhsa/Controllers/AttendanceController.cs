using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Attributes;
using Shikhsa.Data;
using Shikhsa.DataBase.Repositry;
using Shikhsa.Models;
using Shikhsa.Repositories;
using Shikhsa.Services;
using Shikhsa.ViewModels;

namespace Shikhsa.Controllers
{
    public class AttendanceController : BaseController
    {
        private readonly StaffAttendanceRepository _repository;
        private readonly StudentAttendanceRepository _studentAttendanceRepository;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LookupService _lookup;
        private readonly LeaveService _leaveService;
        public AttendanceController(StaffAttendanceRepository repository, UserManager<ApplicationUser> userManager, EmailService email, ApplicationDbContext context, PermissionService permissionService, StudentAttendanceRepository studentAttendanceRepository, LookupService lookup, LeaveService leaveService) : base(userManager, permissionService, context, email,lookup)
        {
            _repository = repository;
            _context = context;
            _studentAttendanceRepository = studentAttendanceRepository;
            _lookup = lookup;
            _leaveService = leaveService;
        }

        private string CurrentUser =>User.Identity?.Name ?? "System";
        public async Task<IActionResult>StaffAttendance(DateOnly? attendanceDate, string? search)
        {
            var date = attendanceDate ?? DateOnly.FromDateTime(DateTime.Today);

            var vm = new StaffAttendanceVM
            {
                AttendanceDate = date,
                Search = search,
                Staffs = await _repository.GetStaffAttendanceAsync(date, search),
                AttendanceTypes = await _context.AttendanceTypes
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.AttendanceTypeId)
                    .ToListAsync()
            };

            ViewBag.Summary = await _repository.GetSummaryAsync(date);

            return View(vm);
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SaveStaffAttendance(StaffAttendance attendance)
        //{
        //    if (!ModelState.IsValid)
        //        return RedirectToAction(nameof(Index),
        //            new { attendanceDate = attendance.AttendanceDate });

        //    attendance.AddedBy = CurrentUser;
        //    attendance.AddedDate = DateTime.Now;

        //    await _repository.SaveAttendanceAsync(attendance);

        //    TempData["Success"] = "Attendance saved successfully.";

        //    return RedirectToAction(nameof(Index),
        //        new { attendanceDate = attendance.AttendanceDate });
        //}
        //[HttpPost]
        //[HttpPost]
        //public async Task<IActionResult> SaveAll(StaffAttendanceVM vm, bool markAll = false)
        //{
        //    if (markAll)
        //    {
        //        var presentTypeId = await _context.AttendanceTypes
        //            .Where(x => x.Code == "P")
        //            .Select(x => x.AttendanceTypeId)
        //            .FirstOrDefaultAsync();

        //        foreach (var item in vm.Staffs)
        //        {
        //            item.AttendanceTypeId = presentTypeId;
        //        }

        //        vm.AttendanceTypes = await _context.AttendanceTypes
        //            .Where(x => x.IsActive)
        //            .OrderBy(x => x.AttendanceTypeId)
        //            .ToListAsync();

        //        ViewBag.Summary = await _repository.GetSummaryAsync(vm.AttendanceDate);

        //        return View("StaffAttendance", vm);
        //    }

        //    var userName = User.Identity?.Name ?? "System";

        //    await _repository.SaveAllAttendanceAsync(vm, userName);

        //    TempData["Success"] = "Attendance saved successfully.";

        //    return RedirectToAction(nameof(StaffAttendance),
        //        new { attendanceDate = vm.AttendanceDate });
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveStaffAttendance(StaffAttendanceVM vm, string submitType)
        {
            if (submitType == "MarkAll")
            {
                var presentTypeId = await _context.AttendanceTypes
                    .Where(x => x.Code == "P")
                    .Select(x => x.AttendanceTypeId)
                    .FirstOrDefaultAsync();

                foreach (var item in vm.Staffs)
                {
                    item.AttendanceTypeId = presentTypeId;
                }

                vm.AttendanceTypes = await _context.AttendanceTypes
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.AttendanceTypeId)
                    .ToListAsync();

                ViewBag.Summary = await _repository.GetSummaryAsync(vm.AttendanceDate);

                return View("StaffAttendance", vm);
            }

            var userName = CurrentUser;

            await _repository.SaveAllAttendanceAsync(vm, userName);

            TempData["Success"] = "Attendance saved successfully.";

            return RedirectToAction(nameof(StaffAttendance),
                new { attendanceDate = vm.AttendanceDate });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStaffAttendance(long id, DateOnly attendanceDate)
        {
            await _repository.DeleteAttendanceAsync(id);

            TempData["Success"] = "Attendance deleted.";

            return RedirectToAction(nameof(Index),
                new { attendanceDate });
        }
        [SkipPermission]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllPresent(DateOnly attendanceDate)
        {
            await _repository.MarkAllPresentAsync(attendanceDate, CurrentUser);

            TempData["Success"] = "All staff marked Present.";

            return RedirectToAction(nameof(Index),
                new { attendanceDate });
        }
        public async Task<IActionResult> AttendanceType(int id = 0)
        {
            var vm = new AttendanceTypeVM();

            vm.AttendanceTypes = await _repository.GetAllAsync();

            if (id > 0)
            {
                vm.AttendanceType = await _repository.GetByIdAsync(id) ?? new AttendanceType();
            }

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAttendanceType(AttendanceTypeVM model)
        {
            if (!ModelState.IsValid)
            {
                model.AttendanceTypes = await _repository.GetAllAsync();
                return View("AttendanceType", model);
            }

            await _repository.SaveAsync(model.AttendanceType);

            TempData["Success"] = "Record saved successfully.";

            return RedirectToAction(nameof(AttendanceType));
        }
        public async Task<IActionResult> DeleteAttendanceTypee(int id)
        {
            await _repository.DeleteAsync(id);

            TempData["Success"] = "Record deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
        #region Student Attendance
        [HttpGet]
        public async Task<IActionResult> StudentsAttendance()
        {
            StudentAttendanceVM vm = new();
            await _lookup.BindAsync(vm, User);
            vm.Students = new List<StudentAttendanceRowVM>();
            //vm = PrepareFilters(vm);

            vm.AttendanceTypes = _context.AttendanceTypes
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ToList();

            return View(vm);
        }
        public IActionResult Index()
        {
            StudentAttendanceVM vm = new();

           // vm = PrepareFilters(vm);

            vm.AttendanceTypes = _context.AttendanceTypes
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ToList();

            return View(vm);
        }
        //[HttpPost]
        //public IActionResult ClassChanged(StudentAttendanceVM vm)
        //{
        //    vm = PrepareFilters(vm);

        //    return View("Index", vm);
        //}
        //[HttpPost]
        //public IActionResult SectionChanged(StudentAttendanceVM vm)
        //{
        //    vm = PrepareFilters(vm);

        //    return View("Index", vm);
        //}
        [HttpPost]
        [SkipPermission]
        public IActionResult LoadStudents(StudentAttendanceVM vm)
        {
            //vm = PrepareFilters(vm);

            vm.Students = _studentAttendanceRepository.LoadStudents(vm);

            vm.AttendanceTypes = _context.AttendanceTypes
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ToList();
            return PartialView("_StudentsAttendanceList", vm);
            // return View("SaveStudentsAttendance", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StudentsAttendance(StudentAttendanceVM vm)
        {
            if (!ModelState.IsValid)
            {
                foreach (var item in ModelState)
                {
                    var key = item.Key;

                    foreach (var error in item.Value.Errors)
                    {
                        Console.WriteLine($"{key} => {error.ErrorMessage}");
                    }
                }

                return View(vm);
            }

            _studentAttendanceRepository.Save(vm, User.Identity!.Name!);

            TempData["success"] = "Attendance saved successfully.";

            return RedirectToAction(nameof(StudentsAttendance));
        }
        //private StudentAttendanceVM PrepareFilters(StudentAttendanceVM vm)
        //{
        //    var userId = _userManager.GetUserId(User);

        //    bool isAdminUser =
        //        User.IsInRole("Admin") ||
        //        User.IsInRole("Principal") ||
        //        User.IsInRole("Developer") ||
        //        User.IsInRole("YN IT Solutions");

        //    // ---------------- Batch ----------------

        //    vm.Batches = _context.Batches
        //        .Where(x => x.IsActive)
        //        .OrderByDescending(x => x.BatchId)
        //        .ToList();

        //    // ---------------- Staff ----------------

        //    if (isAdminUser)
        //    {
        //        vm.StaffList = _context.StaffMasters
        //            .Where(x => x.IsActive)
        //            .OrderBy(x => x.FullName)

        //            .ToList();
        //    }
        //    else
        //    {
        //        var staff = _context.StaffMasters
        //            .FirstOrDefault(x => x.UserId == userId && x.IsActive);

        //        if (staff != null)
        //        {
        //            vm.StaffId = staff.StaffId;

        //           vm.StaffList = new List<StaffMaster>
        //            {
        //                new StaffMaster
        //                {
        //                    StaffId = staff.StaffId,
        //                   FirstName = staff.FirstName,
        //                   MiddleName = staff.MiddleName,
        //                    LastName = staff.LastName,

        //                }
        //           };


        //            ViewBag.LockStaff = true;
        //        }
        //    }

        //    // ---------------- Class ----------------

        //    if (vm.BatchId > 0)
        //    {
        //        //vm.Classes = _context.DataListItems
        //        //    .Where(x => x.IsActive)
        //        //    .OrderBy(x => x.DisplayOrder)
        //        //    .ToList
        //        //    
        //        vm.Classes = GetDataListItems("Class");
        //    }

        //    // ---------------- Section ----------------

        //    if (vm.ClassId > 0)
        //    {
        //        //vm.Sections = _context.SectionMasters
        //        //    .Where(x => x.IsActive)
        //        //    .OrderBy(x => x.DisplayOrder)
        //        //    .ToList();
        //        vm.Sections = GetDataListItems("Section");
        //    }

        //    // ---------------- Attendance Types ----------------

        //    vm.AttendanceTypes = _context.AttendanceTypes
        //        .Where(x => x.IsActive)
        //        .OrderBy(x => x.DisplayOrder)
        //        .ToList();

        //    // ---------------- Class Teacher ----------------

        //    if (!isAdminUser && vm.StaffId > 0)
        //    {
        //        var assignment = _context.ClassTeachers
        //            .FirstOrDefault(x =>
        //                x.StaffId == vm.StaffId &&

        //                x.IsActive);

        //        if (assignment != null)
        //        {
        //            vm.BatchId = assignment.BatchId;
        //            vm.ClassId = assignment.ClassId;
        //            vm.SectionId = assignment.SectionId;

        //            ViewBag.LockBatch = true;
        //            ViewBag.LockClass = true;
        //            ViewBag.LockSection = true;
        //        }
        //    }

        //    return vm;
        //}
        #endregion
        public async Task<IActionResult> ViewAttendance(StudentAttendanceReportVM vm)
        {
           // await _lookup.BindAsync(vm, User);
           vm.Batches = _context.Batches.Where(x => (x.ActiveForAdmission || x.ActiveForRegistration) && x.IsActive == true).OrderByDescending(x => x.BatchId).ToList();
            vm.Classes = GetDataListItems("Class");
            vm.Sections = GetDataListItems("Section");
            if (vm.BatchId > 0 &&
                vm.ClassId > 0 &&
                vm.SectionId > 0)
            {
                vm = await _studentAttendanceRepository.GetAttendanceReportAsync(vm);

              
               
            }

            return View(vm);
        }
        [HttpPost]
        [SkipPermission]
        public async Task<IActionResult> ExportAttendance(StudentAttendanceReportVM vm)
        {
            vm = await _studentAttendanceRepository.GetAttendanceReportAsync(vm);

            var bytes = _studentAttendanceRepository.ExportAttendanceExcel(vm);

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Attendance_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
        #region Leave Management
        private (int Id, string Role) ResolveLeaveUser()
        {
            var sessionRole = GetCurrentUser()?.RoleName ?? "";

            // Normalise every role into exactly three leave user types
            string role;
            if (sessionRole.Equals("Student", StringComparison.OrdinalIgnoreCase) ||
                sessionRole.Equals("Students", StringComparison.OrdinalIgnoreCase))
                role = "Student";
            else if (sessionRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                     sessionRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                role = "Admin";
            else if (sessionRole.Equals("Teacher", StringComparison.OrdinalIgnoreCase) ||
                sessionRole.Equals("Teachers", StringComparison.OrdinalIgnoreCase))
                role = "Teacher";
            else
                role = "Staff";

            var idText = role == "Student"
                ? HttpContext.Session.GetString("StudentID")
                : HttpContext.Session.GetString("StaffID");

            int.TryParse(idText, out var id);
            return (id, role);
        }
        [HttpGet]
        public async Task<IActionResult> Leaves()
        {
            var (id, role) = ResolveLeaveUser();
            if (id == 0 && role != "Admin")
            {
                ErrorMessage("Your profile is not linked with a Student/Staff record.");
                return RedirectToAction("Index", "Dashboard");
            }

            var vm = await _leaveService.GetLeaveDashboardAsync(id, role);
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveLeaves(LeavePageViewModel model)
        {
            var (id, role) = ResolveLeaveUser();
            if (id == 0)
            {
                ErrorMessage("Your profile is not linked with a Student/Staff record.");
                return RedirectToAction(nameof(Leaves));
            }

            model.CurrentUserId = id;
            model.CurrentRoleName = role;

            // ---- Role specific validation ----
            if (role == "Student")
            {
                if (string.IsNullOrWhiteSpace(model.LeaveSubject))
                    ModelState.AddModelError(nameof(model.LeaveSubject), "Leave subject is required");

                // Student never sends leave type / paid flag
                model.LeaveTypeCode = null;
                model.LeaveTypeName = null;
                model.IsPaidLeave = false;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.LeaveTypeCode))
                {
                    ModelState.AddModelError(nameof(model.LeaveTypeCode), "Please select leave type");
                }
                else
                {
                    // Never trust the posted name: read it from AttendanceTypes
                    var type = await _context.AttendanceTypes.AsNoTracking()
                        .FirstOrDefaultAsync(a => a.Code == model.LeaveTypeCode && a.IsLeave && a.IsActive);

                    if (type == null)
                        ModelState.AddModelError(nameof(model.LeaveTypeCode), "Invalid leave type");
                    else
                        model.LeaveTypeName = type.Name;
                }
                model.LeaveSubject = null;
            }

            // ---- Date validation ----
            model.FromDate = model.FromDate.Date;
            model.ToDate = model.ToDate.Date;

            if (model.ToDate < model.FromDate)
            {
                ModelState.AddModelError(nameof(model.ToDate), "To date cannot be before From date");
            }
            else
            {
                var maxDays = (model.ToDate - model.FromDate).Days + 1;
                if (model.NoOfDays > maxDays)
                    ModelState.AddModelError(nameof(model.NoOfDays), $"No. of days cannot be more than {maxDays} for the selected dates");
            }

            // ---- Overlap check (ignore rejected leaves) ----
            if (ModelState.IsValid)
            {
                bool overlap = await _context.LeaveApplications.AnyAsync(l =>
                    l.ApplicantId == id &&
                    l.ApplicantType == role &&
                    l.Status != LeaveStatus.Rejected &&
                    l.FromDate <= model.ToDate &&
                    l.ToDate >= model.FromDate);

                if (overlap)
                    ModelState.AddModelError("", "You already have a pending/approved leave in these dates.");
            }

            if (!ModelState.IsValid)
            {
                // Rebuild lists, keep what the user typed
                var vm = await _leaveService.GetLeaveDashboardAsync(id, role);
                vm.LeaveTypeCode = model.LeaveTypeCode;
                vm.LeaveTypeName = model.LeaveTypeName;
                vm.IsPaidLeave = model.IsPaidLeave;
                vm.LeaveSubject = model.LeaveSubject;
                vm.NoOfDays = model.NoOfDays;
                vm.FromDate = model.FromDate;
                vm.ToDate = model.ToDate;
                vm.ApplicationBody = model.ApplicationBody;
                return View("Leaves", vm);
            }

            var result = await _leaveService.ApplyLeaveAsync(model);
            if (result.Success) SuccessMessage(result.Message); else ErrorMessage(result.Message);

            return RedirectToAction(nameof(Leaves));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLeaves(int leaveId, bool isApproved, string? remarks)
        {
            var (id, role) = ResolveLeaveUser();
            if ((id == 0 || role == "Student")&& role != "Admin")
            {
                ErrorMessage("Unauthorized action.");
                return RedirectToAction(nameof(Leaves));
            }

            var leave = await _context.LeaveApplications.AsNoTracking()
                .FirstOrDefaultAsync(l => l.LeaveId == leaveId);

            if (leave == null)
            {
                ErrorMessage("Leave application not found.");
                return RedirectToAction(nameof(Leaves));
            }

            if (leave.Status != LeaveStatus.Pending)
            {
                WarningMessage("This leave application is already processed.");
                return RedirectToAction(nameof(Leaves));
            }

            // Class teacher may act only on students of his own classes (Admin: everything)
            if (role != "Admin")
            {
                bool isMyClassStudent = leave.ApplicantType.Equals("Student", StringComparison.OrdinalIgnoreCase)
                    && leave.ClassId.HasValue
                    && await _context.ClassTeachers.AnyAsync(ct =>
                           ct.StaffId == id && ct.ClassId == leave.ClassId.Value);

                if (!isMyClassStudent)
                {
                    ErrorMessage("You are not allowed to process this leave application.");
                    return RedirectToAction(nameof(Leaves));
                }
            }

            if (!isApproved && string.IsNullOrWhiteSpace(remarks))
            {
                ErrorMessage("Please enter remarks while rejecting a leave.");
                return RedirectToAction(nameof(Leaves));
            }

            try
            {
                var result = await _leaveService.ProcessLeaveActionAsync(leaveId, id, role, isApproved, remarks?.Trim());
                if (result.Success) SuccessMessage(result.Message); else ErrorMessage(result.Message);
            }
            catch (Exception ex)
            {
                ErrorMessage(ex.Message);
            }

            return RedirectToAction(nameof(Leaves));
        }
        #endregion
    }

}

