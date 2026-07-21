using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models;
using Shikhsa.Models.Common;
using Shikhsa.ViewModels;
using System.Security.Claims;

namespace Shikhsa.DataBase.Repositry
{
    public class ExamRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly RoleManager<ApplicationRole> _roleManager;
        public ExamRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;

            _userManager = userManager;

            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;

        }
        public ScholasticExamVM GetViewModel()
        {
            ScholasticExamVM vm = new();

            // Dropdowns
            vm.Subjects = _context.SubjectMasters
                .OrderBy(x => x.SubjectName)
                .ToList();

            vm.ExamCategories = _context.ExamCategories
                .OrderBy(x => x.ExamCategoryName)
                .ToList();

            vm.Batches = _context.Batches
                .Where(x => x.IsActive && x.ActiveForAdmission)
                .OrderByDescending(x => x.BatchId)
                .ToList();

            // Master Dictionaries
            var subjectDict = _context.SubjectMasters
                .ToDictionary(x => x.SubjectId, x => x.SubjectName);

            var classDict = _context.DataListItems
                .ToDictionary(x => x.DataListItemId, x => x.DataListItemText);

            var examTypeDict = _context.DataListItems
                .ToDictionary(x => x.DataListItemId, x => x.DataListItemText);

            var batchDict = _context.Batches
                .ToDictionary(x => x.BatchId, x => x.AcademicYear);

            var examCategoryDict = _context.ExamCategories
                .ToDictionary(x => x.ExamCategoryId, x => x.ExamCategoryName);

            // Active Exams
            var exams = _context.scholasticExams
                .Where(x => x.IsActive)
                .OrderBy(x => x.ExamName)
                .ToList();

            // Group Same Exam
            vm.List = exams
                .GroupBy(x => new
                {
                    x.BatchId,
                    x.ClassId,
                    x.ExamCategoryId,
                    x.ExamType,
                    x.ExamName
                })
                .Select(g =>
                {
                    var first = g.First();

                    first.SubjectNames = string.Join(", ",
                        g.OrderBy(x => subjectDict.ContainsKey(x.SubjectId) ? subjectDict[x.SubjectId] : "")
                         .Select(x => subjectDict.ContainsKey(x.SubjectId)
                                ? subjectDict[x.SubjectId]
                                : ""));

                    first.ClassName = classDict.TryGetValue(first.ClassId, out var className)
                        ? className
                        : "";

                    first.ExamTypeName = examTypeDict.TryGetValue(first.ExamType, out var examType)
                        ? examType
                        : "";

                    first.BatchName = batchDict.TryGetValue(first.BatchId, out var batch)
                        ? batch
                        : "";

                    first.ExamCategoryName = examCategoryDict.TryGetValue(first.ExamCategoryId, out var category)
                        ? category
                        : "";

                    return first;
                })
                .OrderBy(x => x.ExamName)
                .ThenBy(x => x.ClassName)
                .ToList();

            return vm;
        }

        public int Save(ScholasticExamVM vm)
        {
            try
            {
                if (vm.SelectedSubjects == null || !vm.SelectedSubjects.Any())
                    return 0;

                // Edit Case
                if (vm.Exam.Id > 0)
                {
                    // Existing Active Records
                    var oldRecords = _context.scholasticExams
                        .Where(x => x.BatchId == vm.Exam.BatchId
                                 && x.ClassId == vm.Exam.ClassId
                                 && x.ExamName == vm.Exam.ExamName
                                 && x.ExamCategoryId == vm.Exam.ExamCategoryId
                                 && x.ExamType == vm.Exam.ExamType
                                 && x.IsActive)
                        .ToList();

                    // Old Subjects
                    var oldSubjectIds = oldRecords.Select(x => x.SubjectId).ToHashSet();

                    // New Subjects
                    var newSubjectIds = vm.SelectedSubjects.ToHashSet();

                    // 1. Deactivate Removed Subjects
                    foreach (var item in oldRecords.Where(x => !newSubjectIds.Contains(x.SubjectId)))
                    {
                        item.IsActive = false;
                        item.UpdatedBy = vm.Exam.UpdatedBy;
                        item.UpdatedDate = DateTime.Now;
                    }

                    // 2. Insert Only Newly Added Subjects
                    var subjectsToInsert = newSubjectIds.Except(oldSubjectIds).ToList();

                    if (subjectsToInsert.Any())
                    {
                        var newRecords = subjectsToInsert.Select(subjectId => new ScholasticExam
                        {
                            ExamName = vm.Exam.ExamName,
                            SubjectId = subjectId,
                            ClassId = vm.Exam.ClassId,
                            ExamType = vm.Exam.ExamType,
                            ExamCategoryId = vm.Exam.ExamCategoryId,
                            MinMarks = vm.Exam.MinMarks,
                            MaxMarks = vm.Exam.MaxMarks,
                            BatchId = vm.Exam.BatchId,
                            AddedBy = vm.Exam.UpdatedBy,
                            AddedDate = DateTime.Now,
                            IsActive = true
                        }).ToList();

                        _context.scholasticExams.AddRange(newRecords);
                    }

                    _context.SaveChanges();

                    return vm.Exam.Id;
                }

                // Add Case
                var exams = vm.SelectedSubjects.Select(subjectId => new ScholasticExam
                {
                    ExamName = vm.Exam.ExamName,
                    SubjectId = subjectId,
                    ClassId = vm.Exam.ClassId,
                    ExamType = vm.Exam.ExamType,
                    ExamCategoryId = vm.Exam.ExamCategoryId,
                    MinMarks = vm.Exam.MinMarks,
                    MaxMarks = vm.Exam.MaxMarks,
                    BatchId = vm.Exam.BatchId,
                    AddedBy = vm.Exam.AddedBy,
                    AddedDate = DateTime.Now,
                    IsActive = true
                }).ToList();

                _context.scholasticExams.AddRange(exams);
                _context.SaveChanges();

                return exams.First().Id;
            }
            catch
            {
                return 0;
            }
        }


        public ScholasticExamVM Edit(int id)
        {
            var vm = GetViewModel();

            // Selected Record
            var exam = _context.scholasticExams
                               .FirstOrDefault(x => x.Id == id && x.IsActive);

            if (exam == null)
                return null;

            // Same Exam ke saare active records
            var exams = _context.scholasticExams
                .Where(x => x.BatchId == exam.BatchId
                         && x.ClassId == exam.ClassId
                         && x.ExamCategoryId == exam.ExamCategoryId
                         && x.ExamType == exam.ExamType
                         && x.ExamName == exam.ExamName
                         && x.IsActive)
                .OrderBy(x => x.SubjectId)
                .ToList();

            // Form me common values
            vm.Exam = new ScholasticExam
            {
                Id = exam.Id,                    // First record Id
                ExamName = exam.ExamName,
                ClassId = exam.ClassId,
                ExamCategoryId = exam.ExamCategoryId,
                ExamType = exam.ExamType,
                MinMarks = exam.MinMarks,
                MaxMarks = exam.MaxMarks,
                BatchId = exam.BatchId
            };

            // Multi Select Subjects
            vm.SelectedSubjects = exams
                .Select(x => x.SubjectId)
                .Distinct()
                .ToList();

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
        #region Fill Marks Entry Form
        public ExamMarksEntryVM GetFillMarksViewModel()
        {
            ExamMarksEntryVM vm = new();

            vm.Batches = _context.Batches
                .Where(x => x.IsActive && x.ActiveForAdmission)
                .OrderByDescending(x => x.BatchId)
                .ToList();

            vm.Classes = new List<DataListItem>();

            vm.Sections = new List<DataListItem>();

            vm.Staffs = GetStaffList();

            return vm;
        }
        private StaffMaster? GetLoginStaff()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            return _context.StaffMasters
                .FirstOrDefault(x => x.UserId == userId && x.IsActive);
        }
        private List<StaffMaster> GetStaffList()
        {
            if (IsAdminUser())
            {
                return _context.StaffMasters
                    .Where(x => x.IsActive)
                     .OrderBy(x => x.FirstName)
                        .ThenBy(x => x.MiddleName)
                        .ThenBy(x => x.LastName)//.OrderBy(x => x.FullName)
                    .ToList();
            }

            var staff = GetLoginStaff();

            if (staff == null)
                return new List<StaffMaster>();

            return new List<StaffMaster>
    {
        staff
    };
        }
        private bool IsAdminUser()
        {
            var user = _httpContextAccessor.HttpContext.User;

            return user.IsInRole("Admin")
                || user.IsInRole("Principal")
                || user.IsInRole("YN IT Solutions");
        }

        public List<DataListItem> GetClasses(int batchId, long staffId)
        {
            var classIds = _context.ClassTeacherSubjectAssignments
                .Where(x => x.BatchId == batchId
                         && x.StaffId == staffId
                         && x.IsActive)
                .Select(x => x.ClassId)
                .Distinct()
                .ToList();

            return _context.DataListItems
                .Where(x => classIds.Contains(x.DataListItemId))
                .OrderBy(x => x.DataListItemText)
                .ToList();
        }
        public List<DataListItem> GetSections(int batchId, long staffId, int classId)
        {
            var sectionIds = _context.ClassTeacherSubjectAssignments
                .Where(x => x.BatchId == batchId
                         && x.StaffId == staffId
                         && x.ClassId == classId
                         && x.IsActive)
                .Select(x => x.SectionId)
                .Distinct()
                .ToList();

            return _context.DataListItems
                .Where(x => sectionIds.Contains(x.DataListItemId))
                .OrderBy(x => x.DataListItemText)
                .ToList();
        }
        public bool IsClassTeacher(int batchId, int classId, int sectionId, long staffId)
        {
            return _context.ClassTeachers.Any(x =>

                x.BatchId == batchId &&
                x.ClassId == classId &&
                x.SectionId == sectionId &&
                x.StaffId == staffId &&

                x.IsActive);
        }
        public List<int> GetAssignedSubjects(int batchId, int classId, int sectionId, long staffId)
        {
            return _context.ClassTeacherSubjectAssignments
                .Where(x => x.BatchId == batchId
                       && x.ClassId == classId
                       && x.SectionId == sectionId
                       && x.StaffId == staffId
                       && x.IsActive)
                .Select(x => x.SubjectId)
                .Distinct()
                .ToList();
        }
        //public ExamMarksEntryVM LoadStudents(ExamMarksEntryVM vm)
        //{
        //    // Admin / Principal / YN IT Solutions
        //    bool isAdmin = _httpContextAccessor.HttpContext.User.IsInRole("Admin")
        //                || _httpContextAccessor.HttpContext.User.IsInRole("Principal")
        //                || _httpContextAccessor.HttpContext.User.IsInRole("YN IT Solutions");

        //    if (!isAdmin)
        //    {
        //        string userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        //        vm.StaffId = _context.StaffMasters
        //            .Where(x => x.UserId == userId && x.IsActive)
        //            .Select(x => x.StaffId)
        //            .FirstOrDefault();
        //    }

        //    // Reload dropdowns
        //    vm =vm;

        //    //-----------------------------------------
        //    // Check Class Teacher
        //    //-----------------------------------------

        //    bool isClassTeacher = _context.ClassTeachers.Any(x =>
        //            x.BatchId == vm.BatchId &&
        //            x.ClassId == vm.ClassId &&
        //            x.SectionId == vm.SectionId &&
        //            x.StaffId == vm.StaffId &&
        //            x.IsActive);

        //    //-----------------------------------------
        //    // Subject List
        //    //-----------------------------------------

        //    List<int> subjectIds;

        //    if (isClassTeacher)
        //    {
        //        subjectIds = _context.scholasticExams
        //            .Where(x => x.BatchId == vm.BatchId
        //                     && x.ClassId == vm.ClassId
        //                     && x.IsActive)
        //            .Select(x => x.SubjectId)
        //            .Distinct()
        //            .ToList();
        //    }
        //    else
        //    {
        //        subjectIds = _context.ClassTeacherSubjectAssignments
        //            .Where(x => x.BatchId == vm.BatchId
        //                     && x.ClassId == vm.ClassId
        //                     && x.SectionId == vm.SectionId
        //                     && x.StaffId == vm.StaffId
        //                     && x.IsActive)
        //            .Select(x => x.SubjectId)
        //            .Distinct()
        //            .ToList();
        //    }

        //    //-----------------------------------------
        //    // Exam Columns
        //    //-----------------------------------------

        //    vm.Columns = _context.scholasticExams
        //        .Where(x => x.BatchId == vm.BatchId
        //                 && x.ClassId == vm.ClassId
        //                 && subjectIds.Contains(x.SubjectId)
        //                 && x.IsActive)
        //        .OrderBy(x => x.Subject.SubjectName)
        //        .ThenBy(x => x.ExamTypes.DataListItemText)
        //        .Select(x => new ExamMarkColumnVM
        //        {
        //            ExamId = x.Id,
        //            SubjectId = x.SubjectId,
        //            SubjectName = x.Subject.SubjectName+"_"+x.ExamTypes.DataListItemText,
        //            ExamName = x.ExamName,
        //            MaxMarks = x.MaxMarks,
        //            MinMarks = x.MinMarks,

        //        })
        //        .ToList();

        //    //-----------------------------------------
        //    // Students
        //    //-----------------------------------------

        //    var students = _context.Tbl_Students
        //        .Where(x => x.AdmitBatchId == vm.BatchId
        //                 && x.AdmitClassId == vm.ClassId
        //                 && x.AdmitSectionId == vm.SectionId
        //                 && x.IsActive)
        //        .OrderBy(x => x.FirstName)
        //        .ToList();

        //    //-----------------------------------------
        //    // Existing Marks
        //    //-----------------------------------------

        //    var marks = _context.ExamObtainedMarks
        //        .Where(x => x.BatchId == vm.BatchId
        //                 && x.ClassId == vm.ClassId
        //                 && x.SectionId == vm.SectionId)
        //        .ToList();

        //    var markDict = marks.ToDictionary(
        //        x => $"{x.StudentId}_{x.ExamId}",
        //        x => x);

        //    //-----------------------------------------
        //    // Grid
        //    //-----------------------------------------

        //    vm.Students = new List<ExamMarksRowVM>();

        //    foreach (var student in students)
        //    {
        //        ExamMarksRowVM row = new();

        //        row.StudentId = student.StudentId;
        //        row.AdmissionNo = student.ApplicationNo;
        //        row.RollNo = student.ScholarNumber;
        //        row.StudentName = string.Join(" ",
        //            student.FirstName,
        //            student.MiddleName,
        //            student.LastName);

        //        foreach (var col in vm.Columns)
        //        {
        //            string key = $"{student.StudentId}_{col.ExamId}";

        //            ExamMarkVM mark = new();

        //            if (markDict.ContainsKey(key))
        //            {
        //                var db = markDict[key];

        //                mark.ExamObtainedMarkId = db.ExamObtainedMarkId;
        //                mark.ExamId = db.ExamId;
        //                mark.SubjectId = db.SubjectId;
        //                mark.Marks = db.ObtainedMarks;
        //                mark.IsAbsent = db.IsAbsent;
        //                mark.IsFreeze = db.IsFreeze;
        //               // mark.Remarks = db.Remarks;
        //            }
        //            else
        //            {
        //                mark.ExamId = col.ExamId;
        //                mark.SubjectId = col.SubjectId;
        //            }

        //            row.Marks.Add(mark);
        //        }

        //        vm.Students.Add(row);
        //    }

        //    return vm;
        //}

        public ExamMarksEntryVM LoadStudents(ExamMarksEntryVM vm)
        {
            //----------------------------------------------------
            // Logged In User
            //----------------------------------------------------

            bool isAdmin =
                _httpContextAccessor.HttpContext.User.IsInRole("Admin") ||
                _httpContextAccessor.HttpContext.User.IsInRole("Principal") ||
                _httpContextAccessor.HttpContext.User.IsInRole("YN IT Solutions");

            if (!isAdmin)
            {
                string userId = _httpContextAccessor.HttpContext.User
                    .FindFirstValue(ClaimTypes.NameIdentifier);

                vm.StaffId = _context.StaffMasters
                    .Where(x => x.UserId == userId && x.IsActive)
                    .Select(x => x.StaffId)
                    .FirstOrDefault();
            }

            //----------------------------------------------------
            // Class Teacher
            //----------------------------------------------------

            bool isClassTeacher = _context.ClassTeachers.Any(x =>
                x.BatchId == vm.BatchId &&
                x.ClassId == vm.ClassId &&
                x.SectionId == vm.SectionId &&
                x.StaffId == vm.StaffId &&
                x.IsActive);

            vm.IsClassTeacher = isClassTeacher;

            //----------------------------------------------------
            // Allowed Subjects
            //----------------------------------------------------

            List<int> subjectIds;

            if (isClassTeacher)
            {
                subjectIds = _context.scholasticExams
                    .Where(x =>
                        x.BatchId == vm.BatchId &&
                        x.ClassId == vm.ClassId &&
                        x.ExamCategoryId == vm.ExamCategoryId &&
                        x.IsActive)
                    .Select(x => x.SubjectId)
                    .Distinct()
                    .ToList();
            }
            else
            {
                subjectIds = _context.ClassTeacherSubjectAssignments
                    .Where(x =>
                        x.BatchId == vm.BatchId &&
                        x.ClassId == vm.ClassId &&
                        x.SectionId == vm.SectionId &&
                        x.StaffId == vm.StaffId &&
                        x.IsActive)
                    .Select(x => x.SubjectId)
                    .Distinct()
                    .ToList();
            }

            //----------------------------------------------------
            // Exam Columns
            //----------------------------------------------------

            vm.Columns = _context.scholasticExams
                .Where(x =>
                    x.BatchId == vm.BatchId &&
                    x.ClassId == vm.ClassId &&
                    x.ExamCategoryId == vm.ExamCategoryId &&
                    subjectIds.Contains(x.SubjectId) &&
                    x.IsActive)
                .OrderBy(x => x.Subject.SubjectName)
                .ThenBy(x => x.ExamTypes.DataListItemText)
                .Select(x => new ExamMarkColumnVM
                {
                    ExamId = x.Id,
                    SubjectId = x.SubjectId,
                    SubjectName = x.Subject.SubjectName + "_" + x.ExamTypes.DataListItemText,
                    ExamName = x.ExamName,
                    MaxMarks = x.MaxMarks,
                    MinMarks = x.MinMarks,
                    // DisplayOrder = x.Subject.DisplayOrder
                })
                .ToList();

            //----------------------------------------------------
            // Students
            //----------------------------------------------------

            var students = _context.Tbl_Students
                .Where(x =>
                    x.AdmitBatchId == vm.BatchId &&
                    x.AdmitClassId == vm.ClassId &&
                    x.AdmitSectionId == vm.SectionId &&
                    x.IsActive)
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.MiddleName)
                .ThenBy(x => x.LastName)
                .ToList();

            //----------------------------------------------------
            // Existing Marks
            //----------------------------------------------------

            var markDictionary = _context.ExamObtainedMarks
                .Where(x =>
                    x.BatchId == vm.BatchId &&
                    x.ClassId == vm.ClassId &&
                    x.SectionId == vm.SectionId)
                .ToDictionary(
                    x => (x.StudentId, x.ExamId),
                    x => x);

            //----------------------------------------------------
            // Summary (Remark + Rank)
            //----------------------------------------------------

            var summaryDictionary = _context.StudentExamSummaries
                .Where(x =>
                    x.BatchId == vm.BatchId &&
                    x.ClassId == vm.ClassId &&
                    x.SectionId == vm.SectionId &&
                    x.ExamCategoryId == vm.ExamCategoryId)
                .ToDictionary(x => x.StudentId);

            //----------------------------------------------------
            // Grid
            //----------------------------------------------------

            vm.Students = new List<ExamMarksRowVM>();

            foreach (var student in students)
            {
                var row = new ExamMarksRowVM
                {
                    StudentId = student.StudentId,
                    AdmissionNo = student.ApplicationNo,
                    RollNo = student.ScholarNumber,
                    StudentName = string.Join(" ",
                        new[]
                        {
                    student.FirstName,
                    student.MiddleName,
                    student.LastName
                        }.Where(x => !string.IsNullOrWhiteSpace(x)))
                };

                //--------------------------------------
                // Summary
                //--------------------------------------

                if (summaryDictionary.TryGetValue(student.StudentId, out var summary))
                {
                    row.Remarks = summary.Remarks;
                    row.RankInClass = summary.RankInClass ?? 0;
                    row.IsFreeze = summary?.IsFreeze ?? false;
                }

                //--------------------------------------
                // Marks
                //--------------------------------------

                foreach (var column in vm.Columns)
                {
                    var mark = new ExamMarkVM
                    {
                        ExamId = column.ExamId,
                        SubjectId = column.SubjectId
                    };

                    if (markDictionary.TryGetValue((student.StudentId, column.ExamId), out var db))
                    {
                        mark.ExamObtainedMarkId = db.ExamObtainedMarkId;
                        mark.Marks = db.ObtainedMarks;
                        mark.IsAbsent = db.IsAbsent;
                        mark.IsFreeze = db.IsFreeze;
                    }

                    row.Marks.Add(mark);
                }

                vm.Students.Add(row);
            }
            vm.Batches = _context.Batches
               .Where(x => x.IsActive && x.ActiveForAdmission)
               .OrderByDescending(x => x.BatchId)
               .ToList();

            vm.Staffs = GetStaffList();
            vm.Classes = GetClasses(vm.BatchId, vm.StaffId);
            vm.Sections = GetSections(vm.BatchId, vm.StaffId, vm.ClassId);
            vm.ExamCategories = _context.ExamCategories.Where(x => x.IsActive).ToList();
            return vm;
        }
        //public int Save(ExamMarksEntryVM vm, string userId, bool isAdmin)
        //{
        //    using var transaction = _context.Database.BeginTransaction();

        //    try
        //    {
        //        // Logged in Staff
        //        long staffId = vm.StaffId;

        //        if (!isAdmin)
        //        {
        //            staffId = _context.StaffMasters
        //                .Where(x => x.UserId == userId && x.IsActive)
        //                .Select(x => x.StaffId)
        //                .FirstOrDefault();

        //            vm.StaffId = staffId;
        //        }

        //        // Class Teacher Check
        //        bool isClassTeacher = _context.ClassTeachers.Any(x =>
        //            x.BatchId == vm.BatchId &&
        //            x.ClassId == vm.ClassId &&
        //            x.SectionId == vm.SectionId &&
        //            x.StaffId == staffId &&
        //            x.IsActive);

        //        // Allowed Subjects
        //        List<int> allowedSubjects;

        //        if (isClassTeacher)
        //        {
        //            allowedSubjects = _context.scholasticExams
        //                .Where(x => x.BatchId == vm.BatchId
        //                         && x.ClassId == vm.ClassId
        //                         && x.ExamCategoryId == vm.ExamCategoryId
        //                         && x.ExamType == vm.ExamType
        //                         && x.IsActive)
        //                .Select(x => x.SubjectId)
        //                .Distinct()
        //                .ToList();
        //        }
        //        else
        //        {
        //            allowedSubjects = _context.ClassTeacherSubjectAssignments
        //                .Where(x => x.BatchId == vm.BatchId
        //                         && x.ClassId == vm.ClassId
        //                         && x.SectionId == vm.SectionId
        //                         && x.StaffId == staffId
        //                         && x.IsActive)
        //                .Select(x => x.SubjectId)
        //                .Distinct()
        //                .ToList();
        //        }

        //        // Exams
        //        var exams = _context.scholasticExams
        //            .Where(x => x.BatchId == vm.BatchId
        //                     && x.ClassId == vm.ClassId
        //                     && x.ExamCategoryId == vm.ExamCategoryId
        //                     && x.ExamType == vm.ExamType
        //                     && x.IsActive)
        //            .ToDictionary(x => x.Id);

        //        // Existing Marks
        //        var existing = _context.ExamObtainedMarks
        //            .Where(x => x.BatchId == vm.BatchId
        //                     && x.ClassId == vm.ClassId
        //                     && x.SectionId == vm.SectionId)
        //            .ToDictionary(
        //                x => (x.StudentId, x.ExamId),
        //                x => x);

        //        List<Tbl_ExamObtainedMarks> insertList = new();

        //        foreach (var student in vm.Students)
        //        {
        //            foreach (var mark in student.Marks)
        //            {
        //                if (!allowedSubjects.Contains(mark.SubjectId))
        //                    continue;

        //                if (!exams.TryGetValue(mark.ExamId, out var exam))
        //                    continue;

        //                if (mark.Marks.HasValue)
        //                {
        //                    if (mark.Marks < 0)
        //                        mark.Marks = 0;

        //                    if (mark.Marks > exam.MaxMarks)
        //                        mark.Marks = exam.MaxMarks;
        //                }

        //                if (existing.TryGetValue((student.StudentId, mark.ExamId), out var db))
        //                {
        //                    if (db.IsFreeze)
        //                        continue;

        //                    db.ObtainedMarks = mark.Marks;
        //                    db.IsAbsent = mark.IsAbsent;
        //                    //db.Remarks = mark.Remarks;
        //                    db.UpdatedDate = DateTime.Now;
        //                    db.UpdatedBy = userId;
        //                }
        //                else
        //                {
        //                    insertList.Add(new Tbl_ExamObtainedMarks
        //                    {
        //                        BatchId = vm.BatchId,
        //                        ClassId = vm.ClassId,
        //                        SectionId = vm.SectionId,
        //                        StaffId = staffId,
        //                        StudentId = student.StudentId,
        //                        ExamId = mark.ExamId,
        //                        SubjectId = mark.SubjectId,
        //                        ObtainedMarks = mark.Marks,
        //                        IsAbsent = mark.IsAbsent,
        //                       // Remarks = mark.Remarks,
        //                        IsFreeze = false,
        //                        AddedDate = DateTime.Now,
        //                        AddedBy = userId,
        //                        IsActive = true
        //                    });
        //                }
        //            }
        //        }

        //        if (insertList.Any())
        //            _context.ExamObtainedMarks.AddRange(insertList);

        //        _context.SaveChanges();

        //        transaction.Commit();

        //        return 1;
        //    }
        //    catch
        //    {
        //        transaction.Rollback();
        //        return 0;
        //    }
        //}
        public int Save(ExamMarksEntryVM vm, string userId, bool isAdmin)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                //--------------------------------------------------
                // Logged In Staff
                //--------------------------------------------------

                long staffId = vm.StaffId;

                if (!isAdmin)
                {
                    staffId = _context.StaffMasters
                        .Where(x => x.UserId == userId && x.IsActive)
                        .Select(x => x.StaffId)
                        .FirstOrDefault();

                    vm.StaffId = staffId;
                }

                //--------------------------------------------------
                // Check Class Teacher
                //--------------------------------------------------

                bool isClassTeacher = _context.ClassTeachers.Any(x =>
                    x.BatchId == vm.BatchId &&
                    x.ClassId == vm.ClassId &&
                    x.SectionId == vm.SectionId &&
                    x.StaffId == staffId &&
                    x.IsActive);

                //--------------------------------------------------
                // Allowed Subjects
                //--------------------------------------------------

                List<int> allowedSubjects;

                if (isClassTeacher)
                {
                    allowedSubjects = _context.scholasticExams
                        .Where(x =>
                            x.BatchId == vm.BatchId &&
                            x.ClassId == vm.ClassId &&
                            x.ExamCategoryId == vm.ExamCategoryId &&
                            x.IsActive)
                        .Select(x => x.SubjectId)
                        .Distinct()
                        .ToList();
                }
                else
                {
                    allowedSubjects = _context.ClassTeacherSubjectAssignments
                        .Where(x =>
                            x.BatchId == vm.BatchId &&
                            x.ClassId == vm.ClassId &&
                            x.SectionId == vm.SectionId &&
                            x.StaffId == staffId &&
                            x.IsActive)
                        .Select(x => x.SubjectId)
                        .Distinct()
                        .ToList();
                }

                //--------------------------------------------------
                // Exams
                //--------------------------------------------------

                var exams = _context.scholasticExams
                    .Where(x =>
                        x.BatchId == vm.BatchId &&
                        x.ClassId == vm.ClassId &&
                        x.ExamCategoryId == vm.ExamCategoryId &&
                        x.IsActive)
                    .ToDictionary(x => x.Id);

                //--------------------------------------------------
                // Existing Marks
                //--------------------------------------------------

                var existingMarks = _context.ExamObtainedMarks
                    .Where(x =>
                        x.BatchId == vm.BatchId &&
                        x.ClassId == vm.ClassId &&
                        x.SectionId == vm.SectionId)
                    .ToDictionary(x => (x.StudentId, x.ExamId));

                //--------------------------------------------------
                // Existing Student Summary
                //--------------------------------------------------

                var summaries = _context.StudentExamSummaries
                    .Where(x =>
                        x.BatchId == vm.BatchId &&
                        x.ClassId == vm.ClassId &&
                        x.SectionId == vm.SectionId &&
                        x.ExamCategoryId == vm.ExamCategoryId)
                    .ToDictionary(x => x.StudentId);

                //--------------------------------------------------
                // New Insert Lists
                //--------------------------------------------------

                List<Tbl_ExamObtainedMarks> newMarks = new();

                List<StudentExamSummary> newSummaries = new();

                //--------------------------------------------------
                // Student Loop Starts
                //--------------------------------------------------

                foreach (var student in vm.Students)
                {
                    //--------------------------------------------------
                    // Student Summary
                    //--------------------------------------------------

                    if (summaries.TryGetValue(student.StudentId, out var summary))
                    {
                        // Remarks sab update kar sakte hain
                        summary.Remarks = student.Remarks;

                        // Sirf Class Teacher
                        if (isClassTeacher)
                        {
                            summary.RankInClass = student.RankInClass;
                            summary.IsFreeze = student.IsFreeze;
                        }

                        summary.UpdatedDate = DateTime.Now;
                        summary.UpdatedBy = userId;
                    }
                    else
                    {
                        summary = new StudentExamSummary
                        {
                            BatchId = vm.BatchId,
                            ClassId = vm.ClassId,
                            SectionId = vm.SectionId,
                            ExamCategoryId = vm.ExamCategoryId,
                            StudentId = student.StudentId,
                            Remarks = student.Remarks,
                            RankInClass = isClassTeacher ? student.RankInClass : null,
                            IsFreeze = isClassTeacher ? student.IsFreeze : false,
                            AddedDate = DateTime.Now,
                            AddedBy = userId,
                            IsActive = true
                        };

                        newSummaries.Add(summary);
                    }

                    //--------------------------------------------------
                    // Student Freeze
                    //--------------------------------------------------

                    bool studentFreeze = summary.IsFreeze;

           
                    foreach (var mark in student.Marks)
                    {
                        //--------------------------------------------------
                        // Subject Permission
                        //--------------------------------------------------

                        if (!allowedSubjects.Contains(mark.SubjectId))
                            continue;

                        //--------------------------------------------------
                        // Exam Exists
                        //--------------------------------------------------

                        if (!exams.TryGetValue(mark.ExamId, out var exam))
                            continue;

                        //--------------------------------------------------
                        // Validate Marks
                        //--------------------------------------------------

                        if (mark.Marks.HasValue)
                        {
                            if (mark.Marks < 0)
                                mark.Marks = 0;

                            if (mark.Marks > exam.MaxMarks)
                                mark.Marks = exam.MaxMarks;
                        }

                        //--------------------------------------------------
                        // Existing Record
                        //--------------------------------------------------

                        if (existingMarks.TryGetValue((student.StudentId, mark.ExamId), out var db))
                        {
                            db.ObtainedMarks = mark.IsAbsent
                                ? null
                                : mark.Marks;

                            db.IsAbsent = mark.IsAbsent;

                            db.StaffId = staffId;

                            db.UpdatedDate = DateTime.Now;
                            db.UpdatedBy = userId;
                        }
                        else
                        {
                            newMarks.Add(new Tbl_ExamObtainedMarks
                            {
                                BatchId = vm.BatchId,
                                ClassId = vm.ClassId,
                                SectionId = vm.SectionId,
                                StaffId = staffId,
                                StudentId = student.StudentId,
                                ExamId = mark.ExamId,
                                SubjectId = mark.SubjectId,
                                ObtainedMarks = mark.IsAbsent
                                    ? null
                                    : mark.Marks,
                                IsAbsent = mark.IsAbsent,
                                Remarks = null,
                                IsFreeze = false,
                                AddedDate = DateTime.Now,
                                AddedBy = userId,
                                IsActive = true
                            });
                        }
                    }
                    //--------------------------------------------------
             
                } // Student Loop End

                //--------------------------------------------------
                // Bulk Insert
                //--------------------------------------------------

                if (newMarks.Any())
                {
                    _context.ExamObtainedMarks.AddRange(newMarks);
                }

                if (newSummaries.Any())
                {
                    _context.StudentExamSummaries.AddRange(newSummaries);
                }

                //--------------------------------------------------
                // Save Changes
                //--------------------------------------------------

                _context.SaveChanges();

                //--------------------------------------------------
                // Commit
                //--------------------------------------------------

                transaction.Commit();

                return 1;
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                // Optional Logging
                //_logger.LogError(ex, "Error while saving exam marks.");

                return 0;
            }
        }
        #endregion
    }
}