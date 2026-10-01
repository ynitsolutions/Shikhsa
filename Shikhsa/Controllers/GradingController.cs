using ClosedXML.Excel;
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


            vm.Batches = _context.Batches.Where(s => (s.IsActive == true && s.ActiveForAdmission == true) && s.IsActive == true)
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

            return RedirectToAction(nameof(GradingCriteria));
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

        [SkipPermission]
        [HttpGet]
        public async Task<IActionResult> ExportGradingCriteriaExcel()
                {
                    try
                    {
                        var gradingList = await _context.GradingCriteria
                            .Include(x => x.Term)
                            .Include(x => x.Class)
                            .Include(x => x.Batch)
                            .OrderBy(x => x.Batch.AcademicYear)
                            .ThenBy(x => x.Class.DataListItemText)
                            .ThenBy(x => x.MinPercentage)
                            .ToListAsync();

                        using var workbook = new XLWorkbook();

                        var worksheet = workbook.Worksheets.Add("Grading Criteria");

                        // Title
                        worksheet.Cell(1, 1).Value = "Grading Criteria";
                        worksheet.Range(1, 1, 1, 9).Merge();

                        worksheet.Cell(1, 1).Style.Font.Bold = true;
                        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                        worksheet.Cell(1, 1).Style.Alignment.Horizontal =
                            XLAlignmentHorizontalValues.Center;

                        // Headers
                        string[] headers =
                        {
                    "#",
                    "Term",
                    "Class",
                    "Batch",
                    "Min %",
                    "Max %",
                    "Grade",
                    "Description",
                    "Status"
                };

                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cell(3, i + 1).Value = headers[i];
                        }

                        var headerRange = worksheet.Range(3, 1, 3, headers.Length);

                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Alignment.Horizontal =
                            XLAlignmentHorizontalValues.Center;

                        // Data
                        int row = 4;
                        int sr = 1;

                        foreach (var item in gradingList)
                        {
                            worksheet.Cell(row, 1).Value = sr;
                            worksheet.Cell(row, 2).Value =
                                item.Term?.ExamCategoryName ?? "";

                            worksheet.Cell(row, 3).Value =
                                item.Class?.DataListItemText ?? "";

                            worksheet.Cell(row, 4).Value =
                                item.Batch?.AcademicYear ?? "";

                            worksheet.Cell(row, 5).Value = item.MinPercentage;
                            worksheet.Cell(row, 6).Value = item.MaxPercentage;

                            worksheet.Cell(row, 7).Value =
                                item.Grade ?? "";

                            worksheet.Cell(row, 8).Value =
                                item.Description ?? "";

                            worksheet.Cell(row, 9).Value =
                                item.IsActive ? "Active" : "Inactive";

                            row++;
                            sr++;
                        }

                        // Formatting
                        var usedRange = worksheet.Range(
                            3,
                            1,
                            Math.Max(row - 1, 3),
                            headers.Length);

                        usedRange.Style.Border.OutsideBorder =
                            XLBorderStyleValues.Thin;

                        usedRange.Style.Border.InsideBorder =
                            XLBorderStyleValues.Thin;

                        worksheet.Columns().AdjustToContents();

                        // Keep description readable
                        worksheet.Column(8).Width = 35;
                        worksheet.Column(8).Style.Alignment.WrapText = true;

                        worksheet.SheetView.FreezeRows(3);

                        using var stream = new MemoryStream();

                        workbook.SaveAs(stream);

                        var fileName =
                            $"Grading_Criteria_{DateTime.Now:yyyy-MM-dd}.xlsx";

                        return File(
                            stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            fileName);
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage(ex.Message);

                        return RedirectToAction(nameof(GradingCriteria));
                    }
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
