using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models;
using Shikhsa.Models.Common;
using Shikhsa.ViewModels;

namespace Shikhsa.DataBase.Repositry
{
    public class ExamRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<ApplicationRole> _roleManager;
        public ExamRepository(ApplicationDbContext context,UserManager<ApplicationUser> userManager,RoleManager<ApplicationRole> roleManager)
        {
            _context = context;

            _userManager = userManager;

            _roleManager = roleManager;

        }
        public ScholasticExamVM GetViewModel()
        {
            ScholasticExamVM vm = new();

            vm.Subjects = _context.SubjectMasters
                    .Select(x => new SubjectMasters
                    {
                        SubjectId = x.SubjectId,
                        SubjectName = x.SubjectName
                    }).ToList();

          

            vm.ExamCategories = _context.ExamCategories
                    .Select(x => new ExamCategory
                    {
                        ExamCategoryId = x.ExamCategoryId,
                        ExamCategoryName = x.ExamCategoryName
                    }).ToList();

            vm.Batches = _context.Batches.Where(s=>s.IsActive==true && s.ActiveForAdmission==true)
                    .Select(x => new Batches
                    {
                        BatchId = x.BatchId,
                        AcademicYear = x.AcademicYear
                    }).ToList();

            vm.List = _context.scholasticExams.Where(x => x.IsActive).OrderBy(x => x.ExamName).ToList();
            foreach (var item in vm.List)
            {
                item.ClassName = _context.DataListItems
                    .Where(x => x.DataListItemId == item.ClassId)
                    .Select(x => x.DataListItemText)
                    .FirstOrDefault();
                item.ExamTypeName = _context.DataListItems
                   .Where(x => x.DataListItemId == item.ExamType)
                   .Select(x => x.DataListItemText)
                   .FirstOrDefault();

                item.BatchName = _context.Batches
                    .Where(x => x.BatchId == item.BatchId)
                    .Select(x => x.AcademicYear)
                    .FirstOrDefault();

                item.ExamCategoryName = _context.ExamCategories
                    .Where(x => x.ExamCategoryId == item.ExamCategoryId)
                    .Select(x => x.ExamCategoryName)
                    .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(item.SubjectIds))
                {
                    var subjectIds = item.SubjectIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();

                    item.SubjectNames = string.Join(", ",
                        _context.SubjectMasters
                            .Where(x => subjectIds.Contains(x.SubjectId))
                            .Select(x => x.SubjectName)
                            .ToList());
                }
            }

            return vm;
        }
        public int Save(ScholasticExamVM vm)
        {
            try
            {
                vm.Exam.SubjectIds = vm.SelectedSubjects != null && vm.SelectedSubjects.Any()
                    ? string.Join(",", vm.SelectedSubjects)
                    : "";

                if (vm.Exam.Id == 0)
                {
                    _context.scholasticExams.Add(vm.Exam);
                }
                else
                {
                    var exam = _context.scholasticExams.FirstOrDefault(x => x.Id == vm.Exam.Id);

                    if (exam == null)
                        return 0;

                    exam.ExamName = vm.Exam.ExamName;
                    exam.SubjectIds = vm.Exam.SubjectIds;
                    exam.ClassId = vm.Exam.ClassId;
                    exam.ExamCategoryId = vm.Exam.ExamCategoryId;
                    exam.MinMarks = vm.Exam.MinMarks;
                    exam.MaxMarks = vm.Exam.MaxMarks;
                    exam.BatchId = vm.Exam.BatchId;

                    // Update() ki zarurat nahi hai kyunki entity already tracked hai.
                }

                _context.SaveChanges();

                return vm.Exam.Id == 0 ? vm.Exam.Id : vm.Exam.Id;
            }
            catch (Exception ex)
            {
                // Exception ko log karna better rahega.
                // _logger.LogError(ex, "Error while saving Scholastic Exam");

                return 0;
            }
        }
        public ScholasticExamVM Edit(int id)
        {
            var vm = GetViewModel();

            vm.Exam = _context.scholasticExams
                              .FirstOrDefault(x => x.Id == id);

            if (vm.Exam == null)
                return null;

            if (!string.IsNullOrWhiteSpace(vm.Exam.SubjectIds))
            {
                vm.SelectedSubjects = vm.Exam.SubjectIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();
            }

            return vm;
        }
        public List<SubjectMasters> GetSubjects(int classId, int batchId)
        {
            return (from h in _context.ClassBatchSubjectHeaders
                    join d in _context.ClassBatchSubjectDetails
                        on h.HeaderId equals d.HeaderId
                    join s in _context.SubjectMasters
                        on d.SubjectId equals s.SubjectId
                    where h.ClassId == classId
                          && h.BatchId == batchId
                    orderby s.SubjectName
                    select new SubjectMasters
                    {
                        SubjectId = s.SubjectId,
                        SubjectName = s.SubjectName
                    })
                    .Distinct()
                    .ToList();
        }
        public bool Delete(long id)
        {
            try
            {
                var exam = _context.scholasticExams.FirstOrDefault(x => x.Id == id);

                if (exam == null)
                    return false;

                exam.IsActive = false;

                _context.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }
        #region CoScholasticArea

        public async Task<CoScholasticAreaVM> GetDropdownsAsync()
        {
            CoScholasticAreaVM vm = new();

            vm.CoScholasticList = await _context.CoScholastics
                .Where(x => x.IsActive)
                .OrderBy(x => x.Title)
                .Select(x => new SelectListItem
                {
                    Value = x.CoScholasticId.ToString(),
                    Text = x.Title
                }).ToListAsync();

           
            return vm;
        }
        public async Task<ResponseModel> SaveCoScholasticAreaAsync(CoScholasticArea model, string userId)
        {
            ResponseModel response = new();

            try
            {
                bool exists = await _context.CoScholasticAreas.AnyAsync(x =>
                    x.ClassId == model.ClassId
                    && x.CoScholasticId == model.CoScholasticId
                    && x.CoScholasticAreaId != model.CoScholasticAreaId
                    && x.IsActive);

                if (exists)
                {
                    response.Status = 0;
                    response.Message = "This Co-Scholastic is already assigned to the selected class.";
                    return response;
                }

                if (model.CoScholasticAreaId == 0)
                {
                    model.AddedBy = userId;
                    model.AddedDate = DateTime.Now;
                    model.IsActive = true;

                    await _context.CoScholasticAreas.AddAsync(model);

                    response.Message = "Record saved successfully.";
                }
                else
                {
                    var entity = await _context.CoScholasticAreas
                        .FirstOrDefaultAsync(x => x.CoScholasticAreaId == model.CoScholasticAreaId);

                    if (entity == null)
                    {
                        response.Status = 0;
                        response.Message = "Record not found.";
                        return response;
                    }

                    entity.ClassId = model.ClassId;
                    entity.CoScholasticId = model.CoScholasticId;

                    entity.UpdatedBy = userId;
                    entity.UpdatedDate = DateTime.Now;

                    response.Message = "Record updated successfully.";
                }

                await _context.SaveChangesAsync();

                response.Status = 1;
            }
            catch (Exception ex)
            {
                response.Status = 0;
                response.Message = ex.Message;
            }

            return response;
        }
        public async Task<CoScholasticArea?> GetCoScholasticAreaByIdAsync(long id)
        {
            return await _context.CoScholasticAreas
                .FirstOrDefaultAsync(x => x.CoScholasticAreaId == id);
        }

        public async Task<ResponseModel> DeleteCoScholasticAreaAsync(long id, string userId)
        {
            ResponseModel response = new();

            var entity = await _context.CoScholasticAreas.FindAsync(id);

            if (entity == null)
            {
                response.Status = 0;
                response.Message = "Record not found.";
                return response;
            }

            entity.IsActive = !entity.IsActive;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            response.Status = 1;
            response.Message = entity.IsActive
                ? "Record activated successfully."
                : "Record deactivated successfully.";

            return response;
        }
        public async Task<object> GetCoScholasticAreaListAsync(long? coScholasticId, int? classId)
        {
            var query = from area in _context.CoScholasticAreas
                        join co in _context.CoScholastics
                            on area.CoScholasticId equals co.CoScholasticId
                        join cls in _context.DataListItems
                            on area.ClassId equals cls.DataListItemId
                        select new
                        {
                            area.CoScholasticAreaId,
                            area.CoScholasticId,
                            area.ClassId,
                            area.IsActive,

                            CoScholastic = co.Title + " : " + co.SubjectNameInLanguage,

                            ClassName = cls.DataListItemText
                        };

            if (coScholasticId.HasValue && coScholasticId > 0)
                query = query.Where(x => x.CoScholasticId == coScholasticId);

            if (classId.HasValue && classId > 0)
                query = query.Where(x => x.ClassId == classId);

            return await query
                .OrderBy(x => x.ClassName)
                .ThenBy(x => x.CoScholastic)
                .ToListAsync();
        }
        #endregion
    }
}
