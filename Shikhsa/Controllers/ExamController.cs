using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Attributes;
using Shikhsa.Data;
using Shikhsa.DataBase.Repositry;
using Shikhsa.Helpers;
using Shikhsa.Models;
using Shikhsa.Services;
using Shikhsa.ViewModels;
using System.Security.Claims;

namespace Shikhsa.Controllers
{
    public class ExamController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ExamRepository _repository;
        public ExamController(ApplicationDbContext context,RoleManager<ApplicationRole> roleManager,UserManager<ApplicationUser> userManager,PermissionService permissionService, IWebHostEnvironment env, EmailService email,ExamRepository examRepository):
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
        //[HttpGet]
        //public IActionResult SaveScholasticExams(int id)
        //{
        //    ScholasticExamVM vm = new ScholasticExamVM();
        //     vm = _repository.Edit(id);

        //    if (vm == null)
        //        return NotFound();
        //    vm.SelectedSubjects = vm.Exam.SubjectIds
        //           .Split(',')
        //           .Select(int.Parse)
        //           .ToList();
        //    vm.Classes = GetDataListItems("Class");
        //    vm.ExamType = GetDataListItems("Exam Type");
        //    return View("ScholasticExams", vm);
        //}
        [HttpGet]
        public IActionResult SaveScholasticExams(int id)
        {
            var vm = _repository.Edit(id);

            if (vm == null)
                return NotFound();

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
        #region CoScholastic
        public async Task<IActionResult> CoScholastic(int id = 0)
        {
            CoScholasticPageVM vm = new();

            vm.CoScholasticList = await _context.CoScholastics
                .OrderBy(x => x.Title)
                .ToListAsync();

            if (id > 0)
            {
                vm.CoScholastic = await _context.CoScholastics.FindAsync(id) ?? new CoScholastic();
            }

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCoScholastic(CoScholasticPageVM vm)
        {
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
            if (!ModelState.IsValid)
            {
                vm.CoScholasticList = await _context.CoScholastics
                    .OrderBy(x => x.Title)
                    .ToListAsync();

                return View("Index", vm);
            }

            if (vm.CoScholastic.CoScholasticId == 0)
            {
                vm.CoScholastic.AddedBy = userName;
                vm.CoScholastic.IsActive = true;

                _context.CoScholastics.Add(vm.CoScholastic);

               SuccessMessage("Co-Scholastic saved successfully.");
            }
            else
            {
                var db = await _context.CoScholastics.FindAsync(vm.CoScholastic.CoScholasticId);

                if (db == null)
                    return NotFound();

                db.Title = vm.CoScholastic.Title;
                db.SubjectNameInLanguage = vm.CoScholastic.SubjectNameInLanguage;
                db.UpdatedDate = DateTime.Now;
                db.UpdatedBy = userName;
               SuccessMessage("Co-Scholastic updated successfully.");
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(CoScholastic));
        }

        public async Task<IActionResult> DeleteCoScholastic(int id)
        {
            var data = await _context.CoScholastics.FindAsync(id);
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
            if (data == null)
                return RedirectToAction(nameof(CoScholastic));

            data.IsActive = !data.IsActive;
            data.UpdatedDate = DateTime.Now;
            data.UpdatedBy = userName;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(CoScholastic));
        }
        #endregion
        #region CoSchalasticArea
        //public async Task<IActionResult> CoScholasticArea()
        //{
        //    CoScholasticAreaVM vm = new();
        //     vm = await _repository.GetDropdownsAsync();
        //    vm.ClassList = GetDataListItems("Class");
        //    return View(vm);
        //}
        public async Task<IActionResult> CoScholasticArea(long? id, long? coScholasticId, int? classId)
        {
            CoScholasticAreaVM vm = new();
                vm = await _repository.GetDropdownsAsync();
               vm.ClassList = GetDataListItems("Class");

            if (id.HasValue)
            {
                vm.CoScholasticArea = await _repository.GetCoScholasticAreaByIdAsync(id.Value);
            }

            ViewBag.List = await _repository.GetCoScholasticAreaListAsync(coScholasticId, classId);

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCoScholasticArea(CoScholasticAreaVM model)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _repository.GetDropdownsAsync();
                model.ClassList = vm.ClassList;
                model.CoScholasticList = vm.CoScholasticList;

                return View("Index", model);
            }
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
            var result = await _repository.SaveCoScholasticAreaAsync(model.CoScholasticArea, userName);

            return RedirectToAction(nameof(CoScholasticArea));
        }
        [HttpGet]
        public async Task<IActionResult> SaveCoScholasticArea(long id)
        {
            var data = await _repository.GetCoScholasticAreaByIdAsync(id);

            if (data == null)
                return Json(null);

            return Json(data);
        }
        [SkipPermission]
        [HttpGet]
        public async Task<IActionResult> GetList(long? coScholasticId, int? classId)
        {
            var data = await _repository.GetCoScholasticAreaListAsync(coScholasticId, classId);

            return Json(new
            {
                data
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCoScholasticArea(long id)
        {
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";

            var result = await _repository.DeleteCoScholasticAreaAsync(id, userName);

            return RedirectToAction(nameof(CoScholasticArea));
        }
        #endregion
        #region Exam Marks Entry

        [HttpGet]
        public IActionResult ExamMarksEntry()
        {
            var vm = _repository.GetFillMarksViewModel();

            return View(vm);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[SkipPermission]
        //public IActionResult BatchChanged(ExamMarksEntryVM vm)
        //{
        //    vm = _repository.GetFillMarksViewModel();

        //    vm.BatchId = Request.Form["BatchId"].ToString() == ""
        //        ? 0
        //        : Convert.ToInt32(Request.Form["BatchId"]);

        //    vm.StaffId = Request.Form["StaffId"].ToString() == ""
        //        ? 0
        //        : Convert.ToInt64(Request.Form["StaffId"]);

        //    vm.Classes = _repository.GetClasses(vm.BatchId, vm.StaffId);

        //    return View("ExamMarksEntry", vm);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SkipPermission]
        public IActionResult BatchChanged(ExamMarksEntryVM vm)
        {
            vm = PrepareFilters(new ExamMarksEntryVM());

            return View("ExamMarksEntry", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SkipPermission]
        public IActionResult ClassChanged(ExamMarksEntryVM vm)
        {
            vm = PrepareFilters(new ExamMarksEntryVM());

            return View("ExamMarksEntry", vm);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[SkipPermission]
        //public IActionResult ClassChanged(ExamMarksEntryVM vm)
        //{
        //    vm = _repository.GetFillMarksViewModel();

        //    vm.BatchId = Convert.ToInt32(Request.Form["BatchId"]);
        //    vm.StaffId = Convert.ToInt64(Request.Form["StaffId"]);
        //    vm.ClassId = Convert.ToInt32(Request.Form["ClassId"]);

        //    vm.Classes = _repository.GetClasses(vm.BatchId, vm.StaffId);

        //    vm.Sections = _repository.GetSections(
        //        vm.BatchId,
        //        vm.StaffId,
        //        vm.ClassId);

        //    return View("ExamMarksEntry", vm);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SkipPermission]
        public IActionResult SectionChanged(ExamMarksEntryVM vm)
        {
            vm = PrepareFilters(new ExamMarksEntryVM());

            vm.ExamCategories = _context.ExamCategories
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ToList();

            return View("ExamMarksEntry", vm);
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[SkipPermission]
        //public IActionResult SectionChanged(ExamMarksEntryVM vm)
        //{
        //    vm = _repository.GetFillMarksViewModel();

        //    vm.BatchId = Convert.ToInt32(Request.Form["BatchId"]);
        //    vm.StaffId = Convert.ToInt64(Request.Form["StaffId"]);
        //    vm.ClassId = Convert.ToInt32(Request.Form["ClassId"]);
        //    vm.SectionId = Convert.ToInt32(Request.Form["SectionId"]);

        //    vm.Classes = _repository.GetClasses(vm.BatchId, vm.StaffId);

        //    vm.Sections = _repository.GetSections(
        //        vm.BatchId,
        //        vm.StaffId,
        //        vm.ClassId);
        //    vm.ExamCategories = _context.ExamCategories.Where(x => x.IsActive).ToList();

        //    return View("ExamMarksEntry", vm);
        //}

        [HttpPost]
        [SkipPermission]
        [ValidateAntiForgeryToken]
        public IActionResult LoadStudents(ExamMarksEntryVM vm)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            bool isAdmin =
                User.IsInRole("Admin") ||
                User.IsInRole("Principal") ||
                User.IsInRole("YN IT Solutions");

            //  vm = _repository.LoadStudents(vm, userId, isAdmin);
            vm = _repository.LoadStudents(vm);
            return View("ExamMarksEntry", vm);
        }

        [HttpPost]
        [SkipPermission]
        [ValidateAntiForgeryToken]
        public IActionResult Save(ExamMarksEntryVM vm)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            bool isAdmin =
                User.IsInRole("Admin") ||
                User.IsInRole("Principal") ||
                User.IsInRole("YN IT Solutions");

            int result = _repository.Save(vm, userId, isAdmin);

            if (result > 0)
            {
                TempData["Success"] = "Marks saved successfully.";
            }
            else
            {
                TempData["Error"] = "Unable to save marks.";
            }

            //vm = _repository.LoadStudents(vm, userId, isAdmin);
            vm = _repository.LoadStudents(vm);

            return View("ExamMarksEntry", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SkipPermission]
        public IActionResult ExportExcel(ExamMarksEntryVM vm)
        {
            var file = _repository.ExportExamMarksExcel(vm);

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ExamMarks_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
        #endregion
        private T PrepareFilters<T>(T vm) where T : StudentFilterVM
        {
            vm.BatchId = string.IsNullOrWhiteSpace(Request.Form["BatchId"])
                            ? 0
                            : Convert.ToInt32(Request.Form["BatchId"]);

            vm.StaffId = string.IsNullOrWhiteSpace(Request.Form["StaffId"])
                            ? 0
                            : Convert.ToInt64(Request.Form["StaffId"]);

            vm.ClassId = string.IsNullOrWhiteSpace(Request.Form["ClassId"])
                            ? 0
                            : Convert.ToInt32(Request.Form["ClassId"]);

            vm.SectionId = string.IsNullOrWhiteSpace(Request.Form["SectionId"])
                            ? 0
                            : Convert.ToInt32(Request.Form["SectionId"]);

            return _repository.FillStudentFilters(vm);
        }
        #region Co-Scholastic Entry
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SkipPermission]
        public IActionResult CoScholasticBatchChanged(CoScholasticGradeEntryVM vm)
        {
            vm = PrepareFilters(new CoScholasticGradeEntryVM());

            return View("CoScholasticMarkEntry", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SkipPermission]
        public IActionResult CoScholasticClassChanged(CoScholasticGradeEntryVM vm)
        {
            vm = PrepareFilters(new CoScholasticGradeEntryVM());

            return View("CoScholasticMarkEntry", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SkipPermission]
        public IActionResult CoScholasticSectionChanged(CoScholasticGradeEntryVM vm)
        {
            vm = PrepareFilters(new CoScholasticGradeEntryVM());

            vm.ExamCategories = _context.ExamCategories
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ToList();

            return View("CoScholasticMarkEntry", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SkipPermission]
        public IActionResult LoadCoScholasticStudents(CoScholasticGradeEntryVM vm)
        {
            vm = PrepareFilters(vm);

            vm.ExamCategories = _context.ExamCategories
                .Where(x => x.IsActive)
                .ToList();

            vm = _repository.LoadStudents(vm);

            return View("CoScholasticMarkEntry", vm);
        }
        [HttpGet]
        public IActionResult CoScholasticMarkEntry()
        {
            var vm = _repository.GetcoscholasticMarksViewModel();

            return View(vm);
        }
        [HttpPost]
        [SkipPermission]
        [ValidateAntiForgeryToken]
        public IActionResult CoScholasticMarkEntry(CoScholasticGradeEntryVM vm)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            bool isAdmin =
                User.IsInRole("Admin") ||
                User.IsInRole("Principal") ||
                User.IsInRole("YN IT Solutions");

            int result = _repository.SaveCoscholastic(vm);

            if (result > 0)
            {
                TempData["Success"] = "Marks saved successfully.";
            }
            else
            {
                TempData["Error"] = "Unable to save marks.";
            }

            //vm = _repository.LoadStudents(vm, userId, isAdmin);
            vm = _repository.LoadStudents(vm);

            return View("CoScholasticMarkEntry", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SkipPermission]
        public IActionResult ExportCoScholasticExcel(CoScholasticGradeEntryVM vm)
        {
            try
            {
                var fileBytes = _repository.ExportCoScholasticExcel(vm);

                string fileName =
                    $"CoScholasticGradeEntry_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction(nameof(CoScholasticMarkEntry));
            }
        }
        #endregion
    }
}
