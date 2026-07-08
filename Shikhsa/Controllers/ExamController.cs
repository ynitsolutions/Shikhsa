using DocumentFormat.OpenXml.Office2010.Excel;
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
    public class ExamController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ExamRepository _repository;
        public ExamController(
     ApplicationDbContext context,
     RoleManager<ApplicationRole> roleManager,
     UserManager<ApplicationUser> userManager,
     PermissionService permissionService, IWebHostEnvironment env, EmailService email,ExamRepository examRepository
 ) :
            base(userManager, permissionService, context, email)
        {
            _context = context;
            _roleManager = roleManager;
            _repository = examRepository;
           
        }
        #region Exam Terms
        public async Task<IActionResult> ExamCategory(int? id)
        {
            ExamCategoryVM model = new();

            if (id.HasValue)
            {
                model.ExamCategory = await _context.ExamCategories
                    .FirstOrDefaultAsync(x => x.ExamCategoryId == id);
            }

            model.ExamCategoryList = await _context.ExamCategories
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> SaveExamCategory(ExamCategoryVM model)
        {
            if (!ModelState.IsValid)
            {
                model.ExamCategoryList = await _context.ExamCategories
                    .OrderBy(x => x.DisplayOrder)
                    .ToListAsync();

                return View(model);
            }

            bool duplicateName = await _context.ExamCategories.AnyAsync(x =>
                x.ExamCategoryName.ToLower() ==
                model.ExamCategory.ExamCategoryName.ToLower()
                &&
                x.ExamCategoryId != model.ExamCategory.ExamCategoryId);

            if (duplicateName)
            {
                ErrorMessage("Exam Category already exists.");

                model.ExamCategoryList = await _context.ExamCategories.ToListAsync();

                return View(model);
            }

            bool duplicateShort = await _context.ExamCategories.AnyAsync(x =>
                x.ShortName.ToLower() ==
                model.ExamCategory.ShortName.ToLower()
                &&
                x.ExamCategoryId != model.ExamCategory.ExamCategoryId);

            if (duplicateShort)
            {
                ErrorMessage("Short Name already exists.");

                model.ExamCategoryList = await _context.ExamCategories.ToListAsync();

                return View(model);
            }

            decimal totalWeightage =
                await _context.ExamCategories
                .Where(x =>
                    x.IncludeInFinalResult &&
                    x.ExamCategoryId != model.ExamCategory.ExamCategoryId)
                .SumAsync(x => x.Weightage);

            totalWeightage += model.ExamCategory.IncludeInFinalResult
                ? model.ExamCategory.Weightage
                : 0;

            if (totalWeightage > 100)
            {
                ErrorMessage("Total Weightage cannot exceed 100.");

                model.ExamCategoryList =
                    await _context.ExamCategories
                    .ToListAsync();

                return View(model);
            }

            if (model.ExamCategory.ExamCategoryId == 0)
            {
                model.ExamCategory.AddedBy = User.Identity.Name;

                _context.ExamCategories.Add(model.ExamCategory);

                SuccessMessage( "Saved Successfully.");
            }
            else
            {
                var db = await _context.ExamCategories
                    .FirstAsync(x =>
                        x.ExamCategoryId ==
                        model.ExamCategory.ExamCategoryId);

                db.ExamCategoryName = model.ExamCategory.ExamCategoryName;

                db.ShortName = model.ExamCategory.ShortName;

                db.DisplayOrder = model.ExamCategory.DisplayOrder;

                db.Weightage = model.ExamCategory.Weightage;

                db.IncludeInFinalResult =
                    model.ExamCategory.IncludeInFinalResult;

                db.IsMarksEntryAllowed =
                    model.ExamCategory.IsMarksEntryAllowed;

                db.UpdatedDate = DateTime.Now;
                db.UpdatedBy= User.Identity.Name;
                SuccessMessage("Updated Successfully.");
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("ExamCategory");
        }
        #endregion
        #region Create Exams
        public IActionResult ScholasticExams()
        {
            ScholasticExamVM vm = new ScholasticExamVM();
           vm = _repository.GetViewModel();
            vm.Classes = GetDataListItems("Class");
            vm.ExamType = GetDataListItems("Exam Type");
           
            return View(vm);
        }
        [HttpPost]
        public IActionResult SaveScholasticExams(ScholasticExamVM vm)
        {
            if (!ModelState.IsValid)
            {
                ModelState.Values.SelectMany(v => v.Errors);
                vm = _repository.GetViewModel();
                vm.Classes = GetDataListItems("Class");
                vm.ExamType = GetDataListItems("Exam Type");
                return View("ScholasticExams", vm);
            }

           int result = _repository.Save(vm);

            if (result > 0)
                SuccessMessage("Record saved successfully.");
            else
                ErrorMessage("Unable to save record.");

            return RedirectToAction("ScholasticExams");
        }
        [HttpGet]
        public IActionResult SaveScholasticExams(int id)
        {
            ScholasticExamVM vm = new ScholasticExamVM();
             vm = _repository.Edit(id);

            if (vm == null)
                return NotFound();
            vm.SelectedSubjects = vm.Exam.SubjectIds
                   .Split(',')
                   .Select(int.Parse)
                   .ToList();
            vm.Classes = GetDataListItems("Class");
            vm.ExamType = GetDataListItems("Exam Type");
            return View("ScholasticExams", vm);
        }
        [SkipPermission]
        [HttpGet]
        public JsonResult GetSubjects(int classId, int batchId)
        {
            var data = _repository.GetSubjects(classId, batchId);

            return Json(data.Select(x => new
            {
                id = x.SubjectId,
                text = x.SubjectName
            }));
        }
        [HttpPost]
        public IActionResult DeleteScholasticExams(long id)
        {
            try
            {
                bool result = _repository.Delete(id);

                if (result)
                {

                    SuccessMessage("Record deleted successfully.");

                }
                else
                    ErrorMessage("Record not found.");
               
            }
            catch (Exception ex)
            {
                ErrorMessage(ex.Message);
               
            }
            return RedirectToAction("ScholasticExams");
        }
        #endregion
    }
}
