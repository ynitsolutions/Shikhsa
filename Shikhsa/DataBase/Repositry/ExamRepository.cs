using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models;
using Shikhsa.Models.Common;
using Shikhsa.ViewModels;
using System.ComponentModel;
using System.Security.Claims;
using System.IO;



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

        public T FillStudentFilters<T>(T vm) where T : StudentFilterVM
        {
            vm.Batches = _context.Batches
                .Where(x => x.IsActive && x.ActiveForAdmission)
                .OrderByDescending(x => x.BatchId)
                .ToList();
            vm.Staffs = GetStaffList();

            if (vm.BatchId > 0 && vm.StaffId > 0)
                vm.Classes = GetClasses(vm.BatchId, vm.StaffId);

            if (vm.ClassId > 0)
                vm.Sections = GetSections(vm.BatchId,
                                          vm.StaffId,
                                          vm.ClassId);

            return vm;
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
            int Admitted = _context.DataListItems.Where(x => x.DataListItemValue == "Admitted" && x.IsActive).Select(x => x.DataListItemId).FirstOrDefault();
            var students = _context.Tbl_Students
                .Where(x =>
                    x.AdmitBatchId == vm.BatchId &&
                    x.AdmitClassId == vm.ClassId &&
                    x.AdmitSectionId == vm.SectionId &&
                    x.IsActive && x.Status == Admitted)
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
        #region
        public CoScholasticGradeEntryVM LoadStudents(CoScholasticGradeEntryVM vm)
        {
            // Dynamic Columns
            bool isClassTeacher = _context.ClassTeachers.Any(x =>
                x.BatchId == vm.BatchId &&
                x.ClassId == vm.ClassId &&
                x.SectionId == vm.SectionId &&
                x.StaffId == vm.StaffId &&
                x.IsActive);

            vm.IsClassTeacher = isClassTeacher;
            vm.Columns = _context.CoScholasticAreas
                .Where(x => x.ClassId == vm.ClassId && x.IsActive)
                .OrderBy(x => x.CoScholastic.Title)
                .Select(x => new CoScholasticColumnVM
                {
                    CoScholasticAreaId = x.CoScholasticAreaId,
                    CoScholasticId = x.CoScholasticId,
                    Title = x.CoScholastic.Title,
                    SubjectNameInLanguage = x.CoScholastic.SubjectNameInLanguage
                })
                .ToList();
            int Admitted = _context.DataListItems.Where(x => x.DataListItemValue == "Admitted" && x.IsActive).Select(x => x.DataListItemId).FirstOrDefault();
            var students = _context.Tbl_Students
                .Where(x =>
                    x.AdmitBatchId == vm.BatchId &&
                    x.AdmitClassId == vm.ClassId &&
                    x.AdmitSectionId == vm.SectionId &&
                    x.IsActive && x.Status == Admitted)
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.MiddleName)
                .ThenBy(x => x.LastName)
                .ToList();

            var grades = _context.CoScholasticGrades
                .Where(x => x.BatchId == vm.BatchId
                         && x.ClassId == vm.ClassId
                         && x.SectionId == vm.SectionId
                         && x.ExamCategoryId == vm.ExamCategoryId)
                .ToList();

            vm.Students = students.Select(student =>
            {
                var row = new CoScholasticStudentVM
                {
                    StudentId = student.StudentId,
                    AdmissionNo = student.ApplicationNo,
                    StudentName = student.FirstName + " " + student.MiddleName + " " + student.LastName,
                    IsFreeze = grades.Any(x =>
                          x.StudentId == student.StudentId &&
                          x.IsFreeze)
                };

                foreach (var column in vm.Columns)
                {
                    var grade = grades.FirstOrDefault(x =>
                        x.StudentId == student.StudentId &&
                        x.CoScholasticAreaId == column.CoScholasticAreaId);

                    row.Grades.Add(new StudentCoScholasticGradeVM
                    {
                        GradeEntryId = grade?.GradeEntryId ?? 0,
                        StudentId = student.StudentId,
                        CoScholasticAreaId = column.CoScholasticAreaId,
                        ExamCategoryId = vm.ExamCategoryId,
                        Grade = grade?.Grade ?? "",

                    });
                }

                return row;
            }).ToList();

            return vm;
        }
        public CoScholasticGradeEntryVM GetcoscholasticMarksViewModel()
        {
            CoScholasticGradeEntryVM vm = new();

            vm.Batches = _context.Batches
                .Where(x => x.IsActive && x.ActiveForAdmission)
                .OrderByDescending(x => x.BatchId)
                .ToList();

            vm.Classes = new List<DataListItem>();

            vm.Sections = new List<DataListItem>();

            vm.Staffs = GetStaffList();
            vm.Students = new List<CoScholasticStudentVM>();
            return vm;
        }
        public int SaveCoscholastic(CoScholasticGradeEntryVM vm)
        {
            try
            {
                foreach (var student in vm.Students)
                {
                    foreach (var grade in student.Grades)
                    {
                        var dbGrade = _context.CoScholasticGrades
                            .FirstOrDefault(x =>
                                x.BatchId == vm.BatchId &&
                                x.ClassId == vm.ClassId &&
                                x.SectionId == vm.SectionId &&
                                x.ExamCategoryId == vm.ExamCategoryId &&
                                x.StudentId == student.StudentId &&
                                x.CoScholasticAreaId == grade.CoScholasticAreaId);

                        if (dbGrade == null)
                        {
                            dbGrade = new CoScholasticGrade
                            {
                                BatchId = vm.BatchId,
                                ClassId = vm.ClassId,
                                SectionId = vm.SectionId,
                                ExamCategoryId = vm.ExamCategoryId,
                                StudentId = student.StudentId,
                                CoScholasticAreaId = grade.CoScholasticAreaId,
                                Grade = grade.Grade,
                                AddedDate = DateTime.Now,
                                AddedBy = GetUserName(),
                                IsActive = true,
                                IsFreeze = student.IsFreeze
                            };

                            _context.CoScholasticGrades.Add(dbGrade);
                        }
                        else
                        {
                            dbGrade.Grade = grade.Grade;
                            dbGrade.IsFreeze = student.IsFreeze;
                            dbGrade.UpdatedDate = DateTime.Now;
                            dbGrade.UpdatedBy = GetUserName();
                        }
                    }
                }

                return _context.SaveChanges();
            }
            catch
            {
                return 0;
            }
        }
        #endregion
        private string GetUserName()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
        }

        public byte[] ExportCoScholasticExcel(CoScholasticGradeEntryVM vm)
        {
            // Dynamic Columns
            var columns = _context.CoScholasticAreas
                .Where(x => x.ClassId == vm.ClassId && x.IsActive)
                .OrderBy(x => x.CoScholastic.Title)
                .Select(x => new
                {
                    x.CoScholasticAreaId,
                    x.CoScholastic.Title,
                    x.CoScholastic.SubjectNameInLanguage
                })
                .ToList();

            int admitted = _context.DataListItems
                .Where(x => x.DataListItemValue == "Admitted" && x.IsActive)
                .Select(x => x.DataListItemId)
                .FirstOrDefault();

            var students = _context.Tbl_Students
                .Where(x =>
                    x.AdmitBatchId == vm.BatchId &&
                    x.AdmitClassId == vm.ClassId &&
                    x.AdmitSectionId == vm.SectionId &&
                    x.Status == admitted &&
                    x.IsActive)
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.MiddleName)
                .ThenBy(x => x.LastName)
                .ToList();

            var grades = _context.CoScholasticGrades
                .Where(x =>
                    x.BatchId == vm.BatchId &&
                    x.ClassId == vm.ClassId &&
                    x.SectionId == vm.SectionId &&
                    x.ExamCategoryId == vm.ExamCategoryId)
                .ToList();

            using var workbook = new XLWorkbook();

            var ws = workbook.Worksheets.Add("Co-Scholastic Grades");

            int row = 1;
            int col = 1;

            // Header
            ws.Cell(row, col++).Value = "Sr.";
            ws.Cell(row, col++).Value = "Admission No";
            ws.Cell(row, col++).Value = "Student Name";

            foreach (var area in columns)
            {
                ws.Cell(row, col).Value =
                    $"{area.Title}\n{area.SubjectNameInLanguage}";

                ws.Cell(row, col).Style.Alignment.WrapText = true;

                col++;
            }

            ws.Cell(row, col).Value = "Freeze";

            // Header Style
            var header = ws.Range(1, 1, 1, col);

            header.Style.Font.Bold = true;
            header.Style.Font.FontColor = XLColor.White;
            header.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            row++;
            //=====================================
            // Student Data
            //=====================================

            int sr = 1;

            foreach (var student in students)
            {
                col = 1;

                ws.Cell(row, col++).Value = sr++;

                ws.Cell(row, col++).Value = student.ApplicationNo;

                ws.Cell(row, col++).Value =
                    $"{student.FirstName} {student.MiddleName} {student.LastName}".Replace("  ", " ").Trim();

                foreach (var area in columns)
                {
                    var grade = grades.FirstOrDefault(x =>
                        x.StudentId == student.StudentId &&
                        x.CoScholasticAreaId == area.CoScholasticAreaId);

                    // Selected Grade (A/B/C/D)
                    ws.Cell(row, col++).Value = grade?.Grade ?? "";
                }

                // Freeze Status
                bool isFreeze = grades.Any(x =>
                    x.StudentId == student.StudentId &&
                    x.IsFreeze);

                ws.Cell(row, col).Value = isFreeze ? "Yes" : "No";

                row++;
            }

            //=====================================
            // Formatting
            //=====================================

            var usedRange = ws.RangeUsed();

            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            ws.Columns().AdjustToContents();
            ws.Rows().AdjustToContents();

            ws.SheetView.FreezeRows(1);

            //=====================================
            // Return Excel
            //=====================================

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }
        public byte[] ExportExamMarksExcel(ExamMarksEntryVM vm)
        {
            #region Load Subject Columns

            var columns = _context.scholasticExams
                .Where(x => x.BatchId == vm.BatchId
                         && x.ClassId == vm.ClassId
                         && x.ExamCategoryId == vm.ExamCategoryId
                         && x.IsActive)
                .OrderBy(x => x.Subject.SubjectName)
                .ThenBy(x => x.ExamType)
                .Select(x => new ExamMarkColumnVM
                {
                    ExamId = x.Id,
                    SubjectId = x.SubjectId,
                    SubjectName = x.Subject.SubjectName,
                    ExamName = x.ExamCategory.ExamCategoryName,

                    // Apni navigation property ke hisab se change karna
                    ExamTypeName = x.ExamTypes.DataListItemText,

                    MaxMarks = x.MaxMarks
                })
                .ToList();

            #endregion

            #region Load Students

            int admitted = _context.DataListItems
                .Where(x => x.DataListItemValue == "Admitted" && x.IsActive)
                .Select(x => x.DataListItemId)
                .FirstOrDefault();

            var students = _context.Tbl_Students
                .Where(x =>
                    x.AdmitBatchId == vm.BatchId &&
                    x.AdmitClassId == vm.ClassId &&
                    x.AdmitSectionId == vm.SectionId &&
                    x.Status == admitted &&
                    x.IsActive)
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.MiddleName)
                .ThenBy(x => x.LastName)
                .ToList();

            #endregion

            #region Load Marks

            var examIds = columns
                .Select(x => x.ExamId)
                .ToList();

            var marks = _context.ExamObtainedMarks
                .Where(x =>
                    x.BatchId == vm.BatchId &&
                    x.ClassId == vm.ClassId &&
                    x.SectionId == vm.SectionId &&
                    examIds.Contains(x.ExamId))
                .ToList();

            #endregion

            #region Load Grading Criteria

            var grading = _context.GradingCriteria
                .Where(x =>
                    x.BatchId == vm.BatchId &&
                    x.ClassId == vm.ClassId &&
                    x.TermId == vm.ExamCategoryId &&
                    x.IsActive)
                .OrderByDescending(x => x.MinPercentage)
                .ToList();

            #endregion

            #region Workbook

            using var workbook = new XLWorkbook();

            var ws = workbook.Worksheets.Add("Exam Marks Report");

            int row = 1;
            int col = 1;

            #endregion

            #region School Title

            int lastColumn = columns.Count + 8;

            ws.Range(row, 1, row, lastColumn).Merge();

            ws.Cell(row, 1).Value = "STUDENT EXAM MARKS REPORT";

            ws.Range(row, 1, row, lastColumn).Style.Font.Bold = true;
            ws.Range(row, 1, row, lastColumn).Style.Font.FontSize = 16;
            ws.Range(row, 1, row, lastColumn).Style.Font.FontColor = XLColor.White;

            ws.Range(row, 1, row, lastColumn).Style.Fill.BackgroundColor =
                XLColor.FromHtml("#1F4E78");

            ws.Range(row, 1, row, lastColumn)
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            row++;

            #endregion

            #region Report Information

            ws.Cell(row, 1).Value = "Batch";
            ws.Cell(row, 2).Value = vm.Batches
                .FirstOrDefault(x => x.BatchId == vm.BatchId)?.AcademicYear;

            ws.Cell(row, 3).Value = "Class";
            ws.Cell(row, 4).Value = vm.Classes
                .FirstOrDefault(x => x.DataListItemId == vm.ClassId)?.DataListItemText;

            ws.Cell(row, 5).Value = "Section";
            ws.Cell(row, 6).Value = vm.Sections
                .FirstOrDefault(x => x.DataListItemId == vm.SectionId)?.DataListItemText;

            ws.Cell(row, 7).Value = "Term";
            ws.Cell(row, 8).Value = vm.ExamCategories
                .FirstOrDefault(x => x.ExamCategoryId == vm.ExamCategoryId)?.ExamCategoryName;

            ws.Range(row, 1, row, 8).Style.Font.Bold = true;

            row += 2;

            #endregion
            #region Header

            col = 1;

            ws.Cell(row, col++).Value = "Sr.";
            ws.Cell(row, col++).Value = "Admission No";
            ws.Cell(row, col++).Value = "Student Name";

            foreach (var subject in columns)
            {
                string header = $"{subject.ExamName}_{subject.SubjectName}";

                if (!string.IsNullOrWhiteSpace(subject.ExamTypeName))
                {
                    header += $"_{subject.ExamTypeName}";
                }

                header += $" ({subject.MaxMarks})";

                ws.Cell(row, col).Value = header;

                ws.Cell(row, col).Style.Alignment.WrapText = true;
                ws.Cell(row, col).Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;

                ws.Cell(row, col).Style.Alignment.Vertical =
                    XLAlignmentVerticalValues.Center;

                ws.Cell(row, col).Style.Font.Bold = true;

                col++;
            }

            ws.Cell(row, col++).Value = "Total";
            ws.Cell(row, col++).Value = "%";
            ws.Cell(row, col++).Value = "Grade";
            ws.Cell(row, col++).Value = "Rank";
            ws.Cell(row, col).Value = "Freeze";

            var headerRange = ws.Range(row, 1, row, col);

            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E78");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            row++;

            #endregion

            #region Student Data

            int sr = 1;

            foreach (var student in students)
            {
                col = 1;

                ws.Cell(row, col++).Value = sr++;

                ws.Cell(row, col++).Value = student.ApplicationNo;

                string studentName =
                    $"{student.FirstName} {student.MiddleName} {student.LastName}"
                    .Replace("  ", " ")
                    .Trim();

                ws.Cell(row, col++).Value = studentName;

                decimal totalObtained = 0;
                decimal totalMaxMarks = 0;

                bool isFreeze = false;

                foreach (var subject in columns)
                {
                    var mark = marks.FirstOrDefault(x =>
                        x.StudentId == student.StudentId &&
                        x.ExamId == subject.ExamId);

                    if (mark != null)
                    {
                        decimal obtained = mark.ObtainedMarks ?? 0;

                        decimal percent = 0;

                        if (subject.MaxMarks > 0)
                        {
                            percent = Math.Round(
                                (obtained * 100M) / subject.MaxMarks,
                                2);
                        }

                        string grade = grading
                            .FirstOrDefault(x =>
                                percent >= x.MinPercentage &&
                                percent <= x.MaxPercentage)
                            ?.Grade ?? "";

                        ws.Cell(row, col).Value =
                            $"{obtained:0.##} - {percent:0.00}% - {grade}";

                        totalObtained += obtained;
                        totalMaxMarks += subject.MaxMarks;

                        if (mark.IsFreeze)
                            isFreeze = true;
                    }
                    else
                    {
                        ws.Cell(row, col).Value = "";

                        totalMaxMarks += subject.MaxMarks;
                    }

                    col++;
                }

                // Total

                ws.Cell(row, col++).Value = totalObtained;

                // Percentage

                decimal overallPercent = 0;

                if (totalMaxMarks > 0)
                {
                    overallPercent = Math.Round(
                        (totalObtained * 100M) / totalMaxMarks,
                        2);
                }

                ws.Cell(row, col++).Value = overallPercent;

                // Overall Grade

                string overallGrade = grading
                    .FirstOrDefault(x =>
                        overallPercent >= x.MinPercentage &&
                        overallPercent <= x.MaxPercentage)
                    ?.Grade ?? "";

                ws.Cell(row, col++).Value = overallGrade;

                // Rank (Part-3 me fill hoga)

                ws.Cell(row, col++).Value = "";

                // Freeze

                ws.Cell(row, col).Value =
                    isFreeze ? "Yes" : "No";

                row++;
            }

            #endregion
            #region Rank Calculation

            int totalColumn = columns.Count + 4;   // Total Column
            int rankColumn = columns.Count + 7;    // Rank Column

            var rankList = new List<(int RowNo, decimal Total)>();

            // Student Data starts after:
            // Row1 = Title
            // Row2 = Batch/Class/Section
            // Row3 = Blank
            // Row4 = Header
            // Row5 = Student Data

            int dataStartRow = 5;

            for (int r = dataStartRow; r < row; r++)
            {
                decimal total = ws.Cell(r, totalColumn).GetValue<decimal>();

                rankList.Add((r, total));
            }

            rankList = rankList
                .OrderByDescending(x => x.Total)
                .ToList();

            int rank = 1;

            for (int i = 0; i < rankList.Count; i++)
            {
                if (i > 0 &&
                    rankList[i].Total < rankList[i - 1].Total)
                {
                    rank = i + 1;
                }

                ws.Cell(rankList[i].RowNo, rankColumn).Value = rank;
            }

            #endregion

            #region Formatting

            int lastColumns = columns.Count + 8;

            var usedRange = ws.Range(1, 1, row - 1, lastColumns);

            // Borders

            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Alignment

            usedRange.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            usedRange.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            // Alternate Row Color

            for (int r = dataStartRow; r < row; r++)
            {
                if ((r - dataStartRow) % 2 == 0)
                {
                    ws.Range(r, 1, r, lastColumns)
                      .Style.Fill.BackgroundColor =
                      XLColor.FromHtml("#F8F9FA");
                }
            }

            // Total Column

            ws.Column(totalColumn).Style.Font.Bold = true;

            // Percentage Column

            ws.Column(totalColumn + 1).Style.NumberFormat.Format = "0.00";

            // Auto Fit

            ws.Columns().AdjustToContents();

            // Minimum Width

            ws.Column(2).Width = 18;
            ws.Column(3).Width = 30;

            for (int i = 4; i <= columns.Count + 3; i++)
            {
                if (ws.Column(i).Width < 25)
                    ws.Column(i).Width = 25;
            }

            // Freeze Header

            ws.SheetView.FreezeRows(4);

            // Auto Filter

            ws.Range(4, 1, 4, lastColumns).SetAutoFilter();

            #endregion

            #region Return File

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();

            #endregion
        }
    }
   
}