using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Shikhsa.Data;
using Shikhsa.Models;
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
    }
}
