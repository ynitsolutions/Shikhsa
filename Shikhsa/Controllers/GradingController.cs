using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Attributes;
using Shikhsa.Data;
using Shikhsa.DataBase.Repositry;
using Shikhsa.Models;
using Shikhsa.Models.Common;
using Shikhsa.Services;
using Shikhsa.ViewModels;

namespace Shikhsa.Controllers
{
    public class GradingController : BaseController
    {
        private readonly GradingRepository _repo;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly LookupService _lookup;

        public GradingController(ApplicationDbContext context,RoleManager<ApplicationRole> roleManager,UserManager<ApplicationUser> userManager, PermissionService permissionService, IWebHostEnvironment env, EmailService email, GradingRepository repo,LookupService lookup) : base(userManager, permissionService, context, email, lookup)
        {
            _repo = repo;
            _context = context;
            _roleManager = roleManager;
            _lookup = lookup;

        }

        public IActionResult GradingCriteria()
        {
            var vm = new GradingCriteriaVM();

            vm.Terms = _context.ExamCategories
                .Select(x => new SelectListItem
                {
                    Value = x.ExamCategoryId.ToString(),
                    Text = x.ExamCategoryName
                }).ToList();

            vm.Classes = GetDataListItems("Class");


            vm.Batches = _context.Batches.Where(s => s.IsActive == true && s.ActiveForAdmission == true)
                    .Select(x => new Batches
                    {
                        BatchId = x.BatchId,
                        AcademicYear = x.AcademicYear
                    }).ToList();

            vm.GradingList = _context.GradingCriteria.Include(x => x.Term)
    .Include(x => x.Class)
    .Include(x => x.Batch).Where(x=>x.IsActive)
                .OrderBy(x => x.MinPercentage)
                .ToList();

            return View(vm);
        }
        //public async Task<IActionResult> GradingCriteria()
        //{
        //    return View(await _repo.GetAll());
        //}

        [HttpPost]
        public async Task<IActionResult> SaveGradingCriteria(GradingCriteriaVM model)
        {
            if (model.Criteria.GradingCriteriaId== 0)
                await _repo.Save(model.Criteria);
            else
                await _repo.Update(model.Criteria);

            return RedirectToAction("GradingCriteria");
        }

        public async Task<IActionResult> SaveGradingCriteria(int id)
        {
            return Json(await _repo.Get(id));
        }

        public async Task<IActionResult> DeleteGradingCriteria(int id)
        {
            await _repo.Delete(id);

            return RedirectToAction(nameof(Index));
        }
        [SkipPermission]
        [HttpPost]
        public async Task<IActionResult> BulkCreate(GradingCriteriaVM vm)
        {
            ResponseModel response = new ResponseModel();
            string userId = CurrentUserName; // Fixed: remove parentheses, use as property
            response = await _repo.SaveBulkGradingCriteria(vm, userId);

            if (response.Status == 1)
            {
                SuccessMessage(response.Message);
            }
            else
            {
                ErrorMessage(response.Message);
            }

            return RedirectToAction("GradingCriteria");
        }
        //[SkipPermission]
        //public IActionResult BulkCreate()
        //{
        //    BulkGradingCriteriaVM vm = new();

        //    vm.Classes = GetDataListItems("Class");

        //    vm.Terms = _context.ExamCategories
        //        .Select(x => new SelectListItem
        //        {
        //            Value = x.ExamCategoryId.ToString(),
        //            Text = x.ExamCategoryName
        //        }).ToList();

        //    vm.Batches = _context.Batches
        //        .Where(x => x.IsActive)
        //        .ToList();

        //    return PartialView("_BulkCreate", vm);
        //}
        //[SkipPermission]
        //[HttpPost]
        //public async Task<IActionResult> BulkCreate(BulkGradingCriteriaVM vm)
        //{
        //    if (vm.Ranges == null || !vm.Ranges.Any())
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = "Please enter grading ranges."
        //        });
        //    }

        //    foreach (var item in vm.Ranges)
        //    {
        //        _context.GradingCriteria.Add(new GradingCriteria
        //        {
        //            BatchId = (int)vm.BatchId,
        //            ClassId = (int)vm.ClassId,
        //            TermId = (int)vm.TermId,

        //            MinPercentage = item.MinPercentage,
        //            MaxPercentage = item.MaxPercentage,

        //            Grade = item.Grade,
        //            Description = item.Description,

        //            IsActive = true
        //        });
        //    }

        //    await _context.SaveChangesAsync();

        //    return Json(new
        //    {
        //        success = true,
        //        message = "Grading Criteria saved successfully."
        //    });
        //}
    }
}
