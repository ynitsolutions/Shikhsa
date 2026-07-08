using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Attributes;
using Shikhsa.Data;
using Shikhsa.DataBase.Repositry;
using Shikhsa.Models;
using Shikhsa.Services;
using Shikhsa.ViewModels;

namespace Shikhsa.Controllers
{
    public class AttendanceController : BaseController
    {
        private readonly StaffAttendanceRepository _repository;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public AttendanceController(StaffAttendanceRepository repository, UserManager<ApplicationUser> userManager, EmailService email, ApplicationDbContext context, PermissionService permissionService) : base(userManager, permissionService, context, email)
        {
            _repository = repository;
            _context = context;
        }

        private string CurrentUser =>
            User.Identity?.Name ?? "System";
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
    }

}

