using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models;
using Shikhsa.Models.Common;
using Shikhsa.ViewModels;

namespace Shikhsa.DataBase.Repositry
{
    public class ClassTeacherRepository
    {
        private readonly ApplicationDbContext _context;

        public ClassTeacherRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<SelectListItem>> GetBatchList()
        {
            return await _context.Batches

                .Where(x => x.IsActive)

                .OrderByDescending(x => x.BatchId)

                .Select(x => new SelectListItem
                {
                    Value = x.BatchId.ToString(),
                    Text = x.AcademicYear
                })

                .ToListAsync();
        }
        public async Task<List<SelectListItem>> GetClassList()
        {
            return await _context.DataListItems

                .Include(x => x.DataList)

                .Where(x =>

                    x.DataList.DataListName == "Class"

                    && x.IsActive)

                .OrderBy(x => x.DisplayOrder)

                .Select(x => new SelectListItem
                {
                    Value = x.DataListItemId.ToString(),

                    Text = x.DataListItemText
                })

                .ToListAsync();
        }
        public async Task<List<SelectListItem>> GetSectionList()
        {
            return await _context.DataListItems

                .Include(x => x.DataList)

                .Where(x =>

                    x.DataList.DataListName == "Section"

                    && x.IsActive)

                .OrderBy(x => x.DisplayOrder)

                .Select(x => new SelectListItem
                {
                    Value = x.DataListItemId.ToString(),

                    Text = x.DataListItemText
                })

                .ToListAsync();
        }
        public async Task<List<SelectListItem>> GetTeacherList()
        {
            return await _context.StaffMasters

                .Where(x => x.IsActive)

                .OrderBy(x => x.FirstName)

                .Select(x => new SelectListItem
                {
                    Value = x.StaffId.ToString(),

                    Text = x.FullName
                })

                .ToListAsync();
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
        public async Task<ClassTeacherAssignmentVM> GetPageData()
        {
            var vm = new ClassTeacherAssignmentVM();

            vm.Batches = await GetBatchList();

            vm.CopyBatches = await GetBatchList();

            vm.Sections = await GetSectionList();

            return vm;
        }
        public async Task<List<ClassAccordionVM>> LoadAssignments(int batchId, int sectionId)
        {
            var classes = await _context.DataListItems
                .AsNoTracking()
                .Include(x => x.DataList)
                .Where(x => x.DataList.DataListName == "Class"
                            && x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            var assignments = await _context.ClassTeacherSubjectAssignments
                .AsNoTracking()
                .Where(x => x.BatchId == batchId
                         && x.SectionId == sectionId
                         && x.IsActive)
                .ToListAsync();

            var classTeachers = await _context.ClassTeachers
                .AsNoTracking()
                .Where(x => x.BatchId == batchId
                         && x.SectionId == sectionId
                         && x.IsActive)
                .ToListAsync();

            var staffIds = assignments
                .Select(x => x.StaffId)
                .Distinct()
                .ToList();

            var staffs = await _context.StaffMasters
                .AsNoTracking()
                .Where(x => staffIds.Contains(x.StaffId))
                .ToListAsync();

            List<ClassAccordionVM> list = new();

            foreach (var cls in classes)
            {
                ClassAccordionVM panel = new();

                panel.ClassId = cls.DataListItemId;
                panel.ClassName = cls.DataListItemText;

                var classAssignments = assignments
                    .Where(x => x.ClassId == cls.DataListItemId)
                    .GroupBy(x => x.StaffId)
                    .ToList();

                foreach (var grp in classAssignments)
                {
                    var staff = staffs.FirstOrDefault(x => x.StaffId == grp.Key);

                    if (staff == null)
                        continue;

                    TeacherAssignmentVM teacher = new();

                    teacher.StaffId = staff.StaffId;

                    teacher.StaffName = staff.FullName;

                    teacher.IsClassTeacher = classTeachers.Any(x =>
                            x.ClassId == cls.DataListItemId
                         && x.StaffId == staff.StaffId);

                    teacher.SubjectIds = grp
                        .Select(x => (int)x.SubjectId)
                        .ToList();

                    // Subject dropdown isi class ke hisab se
                    teacher.Subjects = GetSubjects(cls.DataListItemId, batchId)
                        .Select(x => new SelectListItem
                        {
                            Value = x.SubjectId.ToString(),
                            Text = x.SubjectName
                        }).ToList();

                    panel.Teachers.Add(teacher);
                }

                list.Add(panel);
            }

            return list;
        }
        //public async Task<ResponseModel> SaveAssignments( SaveClassTeacherAssignmentVM vm,string userId)
        //{
        //    ResponseModel rs = new ResponseModel();
        //    using var transaction = await _context.Database.BeginTransactionAsync();

        //    try
        //    {
        //        // Delete old data

        //        var oldTeachers = await _context.ClassTeachers
        //            .Where(x => x.BatchId == vm.BatchId &&
        //                        x.SectionId == vm.SectionId)
        //            .ToListAsync();

        //        if (oldTeachers.Any())
        //            _context.ClassTeachers.RemoveRange(oldTeachers);


        //        var oldSubjects = await _context.ClassTeacherSubjectAssignments
        //            .Where(x => x.BatchId == vm.BatchId &&
        //                        x.SectionId == vm.SectionId)
        //            .ToListAsync();

        //        if (oldSubjects.Any())
        //            _context.ClassTeacherSubjectAssignments.RemoveRange(oldSubjects);

        //        await _context.SaveChangesAsync();


        //        //----------------------------------------------------
        //        // Insert New Data
        //        //----------------------------------------------------

        //        foreach (var cls in vm.Classes)
        //        {
        //            if (cls.Teachers == null)
        //                continue;

        //            //------------------------------------------------
        //            // Validation
        //            //------------------------------------------------

        //            if (cls.Teachers.Count(x => x.IsClassTeacher) > 1)
        //                throw new Exception($"Only one class teacher allowed for ClassId {cls.ClassId}.");

        //            //------------------------------------------------

        //            foreach (var teacher in cls.Teachers)
        //            {
        //                if (teacher.StaffId == 0)
        //                    continue;

        //                //--------------------------------------------
        //                // Class Teacher
        //                //--------------------------------------------

        //                if (teacher.IsClassTeacher)
        //                {
        //                    _context.ClassTeachers.Add(new ClassTeacher
        //                    {
        //                        BatchId = vm.BatchId,

        //                        ClassId = cls.ClassId,

        //                        SectionId = vm.SectionId,

        //                        StaffId = teacher.StaffId,

        //                        AddedBy = userId,

        //                        AddedDate = DateTime.Now,

        //                        IsActive = true
        //                    });
        //                }

        //                //--------------------------------------------
        //                // Subject Assignment
        //                //--------------------------------------------

        //                if (teacher.SubjectIds == null)
        //                    continue;

        //                foreach (var subjectId in teacher.SubjectIds.Distinct())
        //                {
        //                    _context.ClassTeacherSubjectAssignments.Add(
        //                        new ClassTeacherSubjectAssignment
        //                        {
        //                            BatchId = vm.BatchId,

        //                            SectionId = vm.SectionId,

        //                            ClassId = cls.ClassId,

        //                            StaffId = teacher.StaffId,

        //                            SubjectId = subjectId,

        //                            AddedBy = userId,

        //                            AddedDate = DateTime.Now,

        //                            IsActive = true
        //                        });
        //                }
        //            }
        //        }

        //        await _context.SaveChangesAsync();

        //        await transaction.CommitAsync();
        //        rs.Status = 1;
        //        rs.Message = "Data saved successfully.";
        //        return rs;
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        rs.Status = 0;
        //        rs.Message = ex.Message;
        //        return rs;
        //    }
        //}
        public async Task<ResponseModel> SaveAssignments(SaveClassTeacherAssignmentVM vm, string userId)
        {
            ResponseModel re = new ResponseModel();
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                //-------------------------------------------------------
                // Delete Existing Records
                //-------------------------------------------------------

                _context.ClassTeachers.RemoveRange(
                    _context.ClassTeachers.Where(x =>
                        x.BatchId == vm.BatchId &&
                        x.SectionId == vm.SectionId));

                _context.ClassTeacherSubjectAssignments.RemoveRange(
                    _context.ClassTeacherSubjectAssignments.Where(x =>
                        x.BatchId == vm.BatchId &&
                        x.SectionId == vm.SectionId));

                //-------------------------------------------------------
                // Prepare New Data
                //-------------------------------------------------------

                List<ClassTeacher> classTeachers = new();
                List<ClassTeacherSubjectAssignment> assignments = new();

                foreach (var cls in vm.Classes)
                {
                    if (cls.Teachers == null || !cls.Teachers.Any())
                        continue;

                    // Only one Class Teacher
                    if (cls.Teachers.Count(x => x.IsClassTeacher) > 1)
                        throw new Exception($"Only one Class Teacher is allowed in Class {cls.ClassId}.");

                    // Duplicate Teacher Validation
                    if (cls.Teachers.GroupBy(x => x.StaffId).Any(g => g.Count() > 1))
                        throw new Exception($"Duplicate Teacher found in Class {cls.ClassId}.");

                    foreach (var teacher in cls.Teachers)
                    {
                        if (teacher.StaffId <= 0)
                            continue;

                        //---------------------------------
                        // Class Teacher
                        //---------------------------------

                        if (teacher.IsClassTeacher)
                        {
                            classTeachers.Add(new ClassTeacher
                            {
                                BatchId = vm.BatchId,
                                ClassId = cls.ClassId,
                                SectionId = vm.SectionId,
                                StaffId = teacher.StaffId,
                                AddedBy = userId,
                                AddedDate = DateTime.Now,
                                IsActive = true
                            });
                        }

                        //---------------------------------
                        // Subjects
                        //---------------------------------

                        if (teacher.SubjectIds == null)
                            continue;

                        foreach (var subjectId in teacher.SubjectIds.Distinct())
                        {
                            assignments.Add(new ClassTeacherSubjectAssignment
                            {
                                BatchId = vm.BatchId,
                                SectionId = vm.SectionId,
                                ClassId = cls.ClassId,
                                StaffId = teacher.StaffId,
                                SubjectId = subjectId,
                                AddedBy = userId,
                                AddedDate = DateTime.Now,
                                IsActive = true
                            });
                        }
                    }
                }

                //-------------------------------------------------------
                // Bulk Insert
                //-------------------------------------------------------

                if (classTeachers.Count > 0)
                    _context.ClassTeachers.AddRange(classTeachers);

                if (assignments.Count > 0)
                    _context.ClassTeacherSubjectAssignments.AddRange(assignments);

                //-------------------------------------------------------
                // Single Save
                //-------------------------------------------------------

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                re.Status = 1;
                re.Message = "Data saved successfully.";
                return re;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                re.Status = 0;
                re.Message = ex.Message;
                throw;
            }
        }
        public async Task<bool> CopyPreviousBatch(int oldBatchId,int newBatchId,int sectionId,string userId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                //-------------------------------------------------------
                // Delete Existing
                //-------------------------------------------------------

                _context.ClassTeachers.RemoveRange(
                    _context.ClassTeachers.Where(x =>
                        x.BatchId == newBatchId &&
                        x.SectionId == sectionId));

                _context.ClassTeacherSubjectAssignments.RemoveRange(
                    _context.ClassTeacherSubjectAssignments.Where(x =>
                        x.BatchId == newBatchId &&
                        x.SectionId == sectionId));

                //-------------------------------------------------------
                // Previous Teachers
                //-------------------------------------------------------

                var classTeachers = await _context.ClassTeachers
                    .AsNoTracking()
                    .Where(x => x.BatchId == oldBatchId &&
                                x.SectionId == sectionId)
                    .Select(x => new ClassTeacher
                    {
                        BatchId = newBatchId,
                        ClassId = x.ClassId,
                        SectionId = x.SectionId,
                        StaffId = x.StaffId,
                        AddedBy = userId,
                        AddedDate = DateTime.Now,
                        IsActive = true
                    })
                    .ToListAsync();

                //-------------------------------------------------------
                // Previous Assignments
                //-------------------------------------------------------

                var assignments = await _context.ClassTeacherSubjectAssignments
                    .AsNoTracking()
                    .Where(x => x.BatchId == oldBatchId &&
                                x.SectionId == sectionId)
                    .Select(x => new ClassTeacherSubjectAssignment
                    {
                        BatchId = newBatchId,
                        SectionId = x.SectionId,
                        ClassId = x.ClassId,
                        StaffId = x.StaffId,
                        SubjectId = x.SubjectId,
                        AddedBy = userId,
                        AddedDate = DateTime.Now,
                        IsActive = true
                    })
                    .ToListAsync();

                //-------------------------------------------------------
                // Bulk Insert
                //-------------------------------------------------------

                if (classTeachers.Count > 0)
                    _context.ClassTeachers.AddRange(classTeachers);

                if (assignments.Count > 0)
                    _context.ClassTeacherSubjectAssignments.AddRange(assignments);

                //-------------------------------------------------------
                // Single Save
                //-------------------------------------------------------

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<List<ClassTeacherDashboardVM>> GetDashboard(int batchId)
        {
            //==========================
            // Load Class Teachers
            //==========================

            var teachers = await
            (
                from ct in _context.ClassTeachers.AsNoTracking()

                join cls in _context.DataListItems.AsNoTracking()
                    on ct.ClassId equals cls.DataListItemId

                join sec in _context.DataListItems.AsNoTracking()
                    on ct.SectionId equals sec.DataListItemId

                join st in _context.StaffMasters.AsNoTracking()
                    on ct.StaffId equals st.StaffId

                where ct.BatchId == batchId
                      && ct.IsActive

                orderby cls.DisplayOrder,
                        sec.DisplayOrder,
                        st.FirstName

                select new ClassTeacherDashboardVM
                {
                    ClassId = ct.ClassId,

                    ClassName = cls.DataListItemText,

                    SectionId = ct.SectionId,

                    SectionName = sec.DataListItemText,

                    StaffId = st.StaffId,

                    StaffName = st.FullName,

                    PhotoPath = st.PhotoPath
                }

            ).ToListAsync();



            //==========================
            // Load Subject Assignment
            //==========================

            var subjectLookup = await
            (
                from a in _context.ClassTeacherSubjectAssignments.AsNoTracking()

                join s in _context.SubjectMasters.AsNoTracking()

                    on a.SubjectId equals s.SubjectId

                where a.BatchId == batchId
                      && a.IsActive

                select new
                {
                    a.ClassId,

                    a.SectionId,

                    a.StaffId,

                    s.SubjectName
                }

            ).ToListAsync();



            //==========================
            // Lookup
            //==========================

            var lookup = subjectLookup.ToLookup(x => new
            {
                x.ClassId,
                x.SectionId,
                x.StaffId
            });



            //==========================
            // Bind Subject
            //==========================

            foreach (var teacher in teachers)
            {
                teacher.SubjectText = string.Join(", ",
                  lookup[new
                  {
                      ClassId = (int)teacher.ClassId,
                      SectionId = (int)teacher.SectionId,
                      teacher.StaffId
                  }]
                  .Select(x => x.SubjectName)
                  .Distinct()
                  .OrderBy(x => x));
            }

            return teachers;
        }
        public async Task<List<Select2VM>> SearchTeacher(string term)
        {
            term = term ?? "";

            return await _context.StaffMasters

                .AsNoTracking()

                .Where(x => x.IsActive)

                .Where(x =>

                    x.FirstName.Contains(term)

                    ||

                    x.LastName.Contains(term)

                    ||

                    x.StaffCode.Contains(term)

                    ||

                    x.MobileNo.Contains(term))

                .OrderBy(x => x.FirstName)

                .Take(30)

                .Select(x => new Select2VM
                {
                    id = x.StaffId,

                    text = x.FullName
                })

                .ToListAsync();
        }
        public async Task<List<SubjectCacheVM>> GetSubjectCache(int batchId)
        {
            return await (
                from h in _context.ClassBatchSubjectHeaders.AsNoTracking()
                join d in _context.ClassBatchSubjectDetails.AsNoTracking()
                    on h.HeaderId equals d.HeaderId
                join s in _context.SubjectMasters.AsNoTracking()
                    on d.SubjectId equals s.SubjectId
                where h.BatchId == batchId
                      && h.IsActive
                      && s.IsActive
                orderby h.ClassId, s.SubjectName
                select new SubjectCacheVM
                {
                    ClassId = h.ClassId,
                    SubjectId = s.SubjectId,
                    SubjectName = s.SubjectName
                })
                .Distinct()
                .ToListAsync();
        }
    }
}
