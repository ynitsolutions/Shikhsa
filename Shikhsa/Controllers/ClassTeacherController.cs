using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shikhsa.Attributes;
using Shikhsa.Data;
using Shikhsa.DataBase.Repositry;
using Shikhsa.Helpers;
using Shikhsa.Models;
using Shikhsa.Models.Common;
using Shikhsa.Services;
using Shikhsa.ViewModels;
using System.Security.Claims;

namespace Shikhsa.Controllers
{

    public class ClassTeacherController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ClassTeacherRepository _repository;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LookupService _lookup;

        public ClassTeacherController(ClassTeacherRepository repository, RoleManager<ApplicationRole> roleManager,
     UserManager<ApplicationUser> userManager, EmailService email, ApplicationDbContext context, PermissionService permissionService, LookupService lookup  
     ) :base(userManager, permissionService, context, email,lookup)
        {
            _repository = repository;
            _lookup=lookup;
        }

        #region Index

        public async Task<IActionResult> TeacherAllocation()
        {
            var vm = await _repository.GetPageData();

            return View(vm);
        }


        #endregion
        [SkipPermission]
        [HttpGet]
        public async Task<IActionResult> LoadAssignments(
        int batchId,
        int sectionId)
        {
            var model = await _repository.LoadAssignments(batchId, sectionId);

            ViewBag.BatchId = batchId;

            return PartialView("_AssignmentGrid", model);
        }
        [HttpPost]
        public async Task<IActionResult> SaveTeacherAllocation([FromBody] SaveClassTeacherAssignmentVM vm)
        {
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid data."
                });

            ResponseModel result = await _repository.SaveAssignments(vm,userName);

            return Json(new
            {
                success = result.Status,
                message = result.Message
            });
        }
        [SkipPermission]
        [HttpPost]
        public async Task<JsonResult> CopyPreviousBatch(
        int oldBatchId,
        int newBatchId,
        int sectionId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                await _repository.CopyPreviousBatch(
                            oldBatchId,
                            newBatchId,
                            sectionId,
                            userId);

                return Json(new
                {
                    success = true,

                    message = "Data Copied Successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,

                    message = ex.Message
                });
            }
        }
        [SkipPermission]
        [HttpGet]
        public async Task<IActionResult> SearchTeacher(string term)
        {
            var data = await _repository.SearchTeacher(term);

            return Json(data);
        }
        [SkipPermission]
        [HttpGet]
        public async Task<IActionResult> GetSubjects(int batchId)
        {
            var data = await _repository.GetSubjectCache(batchId);

            return Json(data);
        }
        [SkipPermission]
        public async Task<IActionResult> Dashboard(int batchId)
        {
            var model = await _repository.GetDashboard(batchId);

            return PartialView("_Dashboard", model);
        }

    }
}