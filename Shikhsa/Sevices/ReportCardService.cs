//using Shikhsa.Models;
//using Shikhsa.Repository;
//using Shikhsa.ViewModels;

//namespace Shikhsa.Sevices
//{
//    public class ReportCardService
//    {
//        private readonly ReportCardRepository _repository;

//        public ReportCardService(
//            ReportCardRepository repository)
//        {
//            _repository = repository;
//        }
//        public async Task<ReportCardVM?> GetAsync(
//            long studentId,
//            int batchId,
//            int examCategoryId)
//        {
//            var student =
//                await _repository.GetStudentHeaderAsync(studentId,batchId);

//            if (student == null)
//                return null;
//            var classId = student.AdmitClassId;
//            var rawMarks = await _repository.GetScholasticAsync(studentId, batchId, examCategoryId);
//            var coScholastic = await _repository.GetCoScholasticAsync(studentId, batchId, examCategoryId);


//            var attendance =await _repository.GetAttendanceSummaryAsync(
//                    studentId,
//                    batchId);


//            var summary =
//                await _repository.GetSummaryAsync(
//                    studentId,
//                    batchId,
//                    examCategoryId);


//            var gradeRanges =
//                await _repository.GetGradeRangesAsync(
//                    batchId,
//                    classId,
//                    examCategoryId);


//            var marks =
//                BuildScholastic(rawMarks, gradeRanges);


//            var overallMaximum =
//                marks.Sum(x => x.TotalMaximumMarks);

//            var overallObtained =
//                marks.Sum(x => x.TotalObtainedMarks);

//            var overallPercentage =
//                overallMaximum == 0
//                    ? 0
//                    : Math.Round(
//                        overallObtained * 100M / overallMaximum,
//                        2);


//            return new ReportCardVM
//            {
//                Student = student,

//                ScholasticMarks = marks,

//                CoScholasticMarks = coScholastic,

//                Attendance = attendance,

//                GradeRanges = gradeRanges,

//                Summary = new ReportCardSummaryVM
//                {
//                    TotalMaximumMarks = overallMaximum,

//                    TotalObtainedMarks = overallObtained,

//                    Percentage = overallPercentage,

//                    Grade = GetGrade(overallPercentage, gradeRanges),

//                    Rank = summary?.RankInClass,

//                    TeacherRemark = summary?.Remarks
//                }
//            };
//        }
//        public async Task<List<ReportCardVM>> GetBulkAsync(
//            List<long> studentIds,
//            int batchId,
//            int examCategoryId)
//        {
//            var results = new List<ReportCardVM>();

//            if (studentIds == null || studentIds.Count == 0)
//                return results;

//            foreach (var studentId in studentIds)
//            {
//                var vm = await GetAsync(
//                    studentId,
//                    batchId,
//                    examCategoryId);

//                if (vm != null)
//                    results.Add(vm);
//            }

//            return results;
//        }
//        public async Task<List<ReportCardStudentRowVM>> GetStudentListAsync(
//            int batchId,
//            int classId,
//            int sectionId)
//        {
//            return await _repository.GetStudentListAsync(
//                batchId,
//                classId,
//                sectionId);
//        }
//        private List<ScholasticMarkVM> BuildScholastic(
//            List<dynamic> rows,
//            List<GradeRangeVM> gradeRanges)
//        {
//            var result = new List<ScholasticMarkVM>();

//            var subjects = rows
//                .GroupBy(x => new
//                {
//                    SubjectId = (int)x.SubjectId,
//                    SubjectName = (string)x.SubjectName
//                });


//            foreach (var subject in subjects)
//            {
//                var vm = new ScholasticMarkVM
//                {
//                    SubjectId = subject.Key.SubjectId,

//                    SubjectName = subject.Key.SubjectName
//                };


//                foreach (var row in subject)
//                {
//                    vm.ExamMarks.Add(
//                        new ExamTypeMarkVM
//                        {
//                            ExamTypeId =
//                                row.ExamTypeId == null
//                                    ? 0
//                                    : (int)row.ExamTypeId,

//                            ExamTypeName =
//                                row.ExamTypeName
//                                ?? "Exam",

//                            DisplayOrder =
//                                row.DisplayOrder == null
//                                    ? 999
//                                    : (int)row.DisplayOrder,

//                            MaximumMarks =
//                                row.MaximumMarks == null
//                                    ? 0
//                                    : (decimal)row.MaximumMarks,

//                            PassingMarks =
//                                row.PassingMarks == null
//                                    ? 0
//                                    : (decimal)row.PassingMarks,

//                            ObtainedMarks =
//                                row.ObtainedMarks == null
//                                    ? 0
//                                    : (decimal)row.ObtainedMarks,

//                            IsAbsent =
//                                row.IsAbsent != null &&
//                                (bool)row.IsAbsent,

//                            Remarks = row.Remarks
//                        });
//                }


//                vm.TotalMaximumMarks =
//                    vm.ExamMarks.Sum(x => x.MaximumMarks);


//                vm.TotalObtainedMarks =
//                    vm.ExamMarks.Sum(x => x.ObtainedMarks);


//                vm.MaximumMarks =
//                    vm.TotalMaximumMarks;


//                vm.ObtainedMarks =
//                    vm.TotalObtainedMarks;


//                vm.PassingMarks =
//                    vm.ExamMarks.Sum(x => x.PassingMarks);


//                vm.IsAbsent =
//                    vm.ExamMarks.Count > 0 &&
//                    vm.ExamMarks.All(x => x.IsAbsent);


//                vm.Percentage =
//                    vm.TotalMaximumMarks == 0
//                        ? 0
//                        : Math.Round(
//                            vm.TotalObtainedMarks * 100M /
//                            vm.TotalMaximumMarks,
//                            2);


//                vm.Result =
//                    vm.IsAbsent
//                        ? "Absent"
//                        : vm.TotalObtainedMarks >= vm.PassingMarks
//                            ? "Pass"
//                            : "Fail";


//                vm.Grade =
//                    vm.IsAbsent
//                        ? ""
//                        : GetGrade(vm.Percentage, gradeRanges);


//                result.Add(vm);
//            }


//            return result;
//        }
//        private string GetGrade(
//            decimal percentage,
//            List<GradeRangeVM> ranges)
//        {
//            if (ranges == null || ranges.Count == 0)
//                return "";

//            var match = ranges.FirstOrDefault(r =>
//                percentage >= r.MinPercentage &&
//                percentage <= r.MaxPercentage);

//            return match?.Grade ?? "";
//        }
//    }
//}
using Shikhsa.Models;
using Shikhsa.Repository;
using Shikhsa.ViewModels;

namespace Shikhsa.Sevices
{
    public class ReportCardService
    {
        private readonly ReportCardRepository _repository;

        public ReportCardService(
            ReportCardRepository repository)
        {
            _repository = repository;
        }


        // =========================================================
        // BUILD SINGLE REPORT CARD
        // Works for both a normal exam category AND the "Final"
        // consolidated report card - condition is auto-detected from
        // the category's name, no separate method/param is needed.
        // =========================================================

        public async Task<ReportCardVM?> GetAsync(
            long studentId,
            int batchId,
            int examCategoryId)
        {
            var student =
                await _repository.GetStudentHeaderAsync(
                    studentId,
                    batchId);

            if (student == null)
                return null;

            // classId is not supplied by callers (controller only has
            // studentId/batchId/examCategoryId) - it's derived from the
            // student's own admission class.
            var classId = student.AdmitClassId;


            // ---------------------------------------------------
            // "Final" is a virtual/consolidated category - it has
            // no marks/attendance/remarks of its own. When the
            // requested category is "Final":
            //   - the Scholastic table shows ALL periodic categories
            //     side by side (built further below).
            //   - attendance, co-scholastic, remarks and the overall
            //     summary are instead pulled from "Annual Exam".
            // For any other category, everything works exactly as
            // before (single flat scholastic table for that category).
            // ---------------------------------------------------

            var examCategory =
                await _repository.GetExamCategoryAsync(
                    examCategoryId);

            var isFinalReport =
                examCategory != null &&
                examCategory.CategoryName
                    .Trim()
                    .Equals("Final", StringComparison.OrdinalIgnoreCase);

            var detailCategoryId = examCategoryId;

            if (isFinalReport)
            {
                var annualCategoryId =
                    await _repository.GetExamCategoryIdByNameAsync(
                        "Annual Exam");

                if (annualCategoryId.HasValue)
                    detailCategoryId = annualCategoryId.Value;
            }


            var rawMarks =
                await _repository.GetScholasticAsync(
                    studentId,
                    batchId,
                    detailCategoryId);


            var coScholastic =
                await _repository.GetCoScholasticAsync(
                    studentId,
                    batchId,
                    detailCategoryId);


            var attendance =
                await _repository.GetAttendanceSummaryAsync(
                    studentId,
                    batchId);


            var summary =
                await _repository.GetSummaryAsync(
                    studentId,
                    batchId,
                    detailCategoryId);


            var gradeRanges =
                await _repository.GetGradeRangesAsync(
                    batchId,
                    classId,
                    detailCategoryId);


            var marks =
                BuildScholastic(rawMarks, gradeRanges);


            var overallMaximum =
                marks.Sum(x => x.TotalMaximumMarks);

            var overallObtained =
                marks.Sum(x => x.TotalObtainedMarks);

            var overallPercentage =
                overallMaximum == 0
                    ? 0
                    : Math.Round(
                        overallObtained * 100M / overallMaximum,
                        2);


            var vm = new ReportCardVM
            {
                Student = student,

                ScholasticMarks = marks,

                CoScholasticMarks = coScholastic,

                Attendance = attendance,

                GradeRanges = gradeRanges,

                IsFinalReport = isFinalReport,

                Summary = new ReportCardSummaryVM
                {
                    TotalMaximumMarks = overallMaximum,

                    TotalObtainedMarks = overallObtained,

                    Percentage = overallPercentage,

                    Grade = GetGrade(overallPercentage, gradeRanges),

                    Rank = summary?.RankInClass,

                    TeacherRemark = summary?.Remarks
                }
            };


            // ---------------------------------------------------
            // Build the consolidated (First Periodic / Second Periodic
            // / Third Periodic / Half Yearly / Annual Exam) matrix -
            // only when this is a "Final" report card.
            // ---------------------------------------------------

            if (isFinalReport)
            {
                var periodicRows =
                    await _repository.GetScholasticAcrossCategoriesAsync(
                        studentId,
                        batchId);

                var distinctCategoryIds = periodicRows
                    .Select(r => (int)r.ExamCategoryId)
                    .Distinct()
                    .ToList();

                var gradeRangesByCategory =
                    new Dictionary<int, List<GradeRangeVM>>();

                foreach (var catId in distinctCategoryIds)
                {
                    gradeRangesByCategory[catId] =
                        await _repository.GetGradeRangesAsync(
                            batchId,
                            classId,
                            catId);
                }

                var (layout, finalRows) =
                    BuildFinalScholastic(
                        periodicRows,
                        gradeRangesByCategory);

                vm.FinalCategoryLayout = layout;
                vm.FinalScholasticMarks = finalRows;
            }


            return vm;
        }


        // =========================================================
        // BUILD BULK REPORT CARDS
        // (used for class-wise / selected-students ZIP download)
        // Works transparently for Final or normal categories since it
        // just calls GetAsync per student.
        // =========================================================

        public async Task<List<ReportCardVM>> GetBulkAsync(
            List<long> studentIds,
            int batchId,
            int examCategoryId)
        {
            var results = new List<ReportCardVM>();

            if (studentIds == null || studentIds.Count == 0)
                return results;

            foreach (var studentId in studentIds)
            {
                var vm = await GetAsync(
                    studentId,
                    batchId,
                    examCategoryId);

                if (vm != null)
                    results.Add(vm);
            }

            return results;
        }


        // =========================================================
        // STUDENT LIST
        // (used by the ReportCard filter/search page)
        // =========================================================

        public async Task<List<ReportCardStudentRowVM>> GetStudentListAsync(
            int batchId,
            int classId,
            int sectionId)
        {
            return await _repository.GetStudentListAsync(
                batchId,
                classId,
                sectionId);
        }


        // =========================================================
        // BUILD SCHOLASTIC
        // (single/normal exam category flat table)
        // =========================================================

        private List<ScholasticMarkVM> BuildScholastic(
            List<dynamic> rows,
            List<GradeRangeVM> gradeRanges)
        {
            var result = new List<ScholasticMarkVM>();

            var subjects = rows
                .GroupBy(x => new
                {
                    SubjectId = (int)x.SubjectId,
                    SubjectName = (string)x.SubjectName
                });


            foreach (var subject in subjects)
            {
                var vm = new ScholasticMarkVM
                {
                    SubjectId = subject.Key.SubjectId,

                    SubjectName = subject.Key.SubjectName
                };


                foreach (var row in subject)
                {
                    vm.ExamMarks.Add(
                        new ExamTypeMarkVM
                        {
                            ExamTypeId =
                                row.ExamTypeId == null
                                    ? 0
                                    : (int)row.ExamTypeId,

                            ExamTypeName =
                                row.ExamTypeName
                                ?? "Exam",

                            DisplayOrder =
                                row.DisplayOrder == null
                                    ? 999
                                    : (int)row.DisplayOrder,

                            MaximumMarks =
                                row.MaximumMarks == null
                                    ? 0
                                    : (decimal)row.MaximumMarks,

                            PassingMarks =
                                row.PassingMarks == null
                                    ? 0
                                    : (decimal)row.PassingMarks,

                            ObtainedMarks =
                                row.ObtainedMarks == null
                                    ? 0
                                    : (decimal)row.ObtainedMarks,

                            IsAbsent =
                                row.IsAbsent != null &&
                                (bool)row.IsAbsent,

                            Remarks = row.Remarks
                        });
                }


                vm.TotalMaximumMarks =
                    vm.ExamMarks.Sum(x => x.MaximumMarks);


                vm.TotalObtainedMarks =
                    vm.ExamMarks.Sum(x => x.ObtainedMarks);


                vm.MaximumMarks =
                    vm.TotalMaximumMarks;


                vm.ObtainedMarks =
                    vm.TotalObtainedMarks;


                vm.PassingMarks =
                    vm.ExamMarks.Sum(x => x.PassingMarks);


                vm.IsAbsent =
                    vm.ExamMarks.Count > 0 &&
                    vm.ExamMarks.All(x => x.IsAbsent);


                vm.Percentage =
                    vm.TotalMaximumMarks == 0
                        ? 0
                        : Math.Round(
                            vm.TotalObtainedMarks * 100M /
                            vm.TotalMaximumMarks,
                            2);


                vm.Result =
                    vm.IsAbsent
                        ? "Absent"
                        : vm.TotalObtainedMarks >= vm.PassingMarks
                            ? "Pass"
                            : "Fail";


                vm.Grade =
                    vm.IsAbsent
                        ? ""
                        : GetGrade(vm.Percentage, gradeRanges);


                result.Add(vm);
            }


            return result;
        }


        // =========================================================
        // BUILD "FINAL" CONSOLIDATED SCHOLASTIC TABLE
        // Groups cross-category rows into a fixed column layout
        // (one group per exam category) plus a per-subject matrix
        // that follows that same layout.
        // =========================================================

        private (List<FinalCategoryLayoutVM> Layout, List<FinalScholasticRowVM> Rows)
            BuildFinalScholastic(
                List<dynamic> rows,
                Dictionary<int, List<GradeRangeVM>> gradeRangesByCategory)
        {
            // ---- Column layout: one entry per exam category, in
            // ---- DisplayOrder, with the union of exam type names
            // ---- seen for that category across all subjects.

            var layout = rows
                .GroupBy(r => new
                {
                    CategoryId = (int)r.ExamCategoryId,
                    CategoryName = (string)r.CategoryName,
                    DisplayOrder = (int)r.CategoryDisplayOrder
                })
                .OrderBy(g => g.Key.DisplayOrder)
                .Select(g =>
                {
                    var typeNames = g
                        .Where(r => r.ExamTypeId != null)
                        .GroupBy(r => new
                        {
                            Name = (string)(r.ExamTypeName ?? "Exam"),
                            Order = r.ExamTypeDisplayOrder == null
                                ? 999
                                : (int)r.ExamTypeDisplayOrder
                        })
                        .OrderBy(x => x.Key.Order)
                        .Select(x => x.Key.Name)
                        .Distinct()
                        .ToList();

                    return new FinalCategoryLayoutVM
                    {
                        ExamCategoryId = g.Key.CategoryId,
                        CategoryName = g.Key.CategoryName,
                        ShowBreakdown = typeNames.Count > 1,
                        ExamTypeNames = typeNames
                    };
                })
                .ToList();


            // ---- Per-subject matrix, following the layout above.

            var subjectRows = rows
                .GroupBy(r => new
                {
                    SubjectId = (int)r.SubjectId,
                    SubjectName = (string)r.SubjectName
                })
                .OrderBy(g => g.Key.SubjectName)
                .Select(subjectGroup =>
                {
                    var row = new FinalScholasticRowVM
                    {
                        SubjectId = subjectGroup.Key.SubjectId,
                        SubjectName = subjectGroup.Key.SubjectName
                    };

                    foreach (var cat in layout)
                    {
                        var categoryRows = subjectGroup
                            .Where(r => (int)r.ExamCategoryId == cat.ExamCategoryId)
                            .ToList();

                        var examTypeMarks = cat.ExamTypeNames
                            .Select(typeName =>
                            {
                                var match = categoryRows.FirstOrDefault(
                                    r => (string)(r.ExamTypeName ?? "Exam") == typeName);

                                return new FinalExamTypeMarkVM
                                {
                                    ExamTypeName = typeName,

                                    ObtainedMarks =
                                        match == null
                                            ? 0
                                            : (decimal)match.ObtainedMarks,

                                    MaximumMarks =
                                        match == null
                                            ? 0
                                            : (decimal)match.MaximumMarks,

                                    IsAbsent =
                                        match != null &&
                                        match.IsAbsent != null &&
                                        (bool)match.IsAbsent
                                };
                            })
                            .ToList();

                        var isAbsent =
                            categoryRows.Count > 0 &&
                            categoryRows.All(r =>
                                r.IsAbsent != null && (bool)r.IsAbsent);

                        var totalObtained =
                            isAbsent
                                ? 0
                                : categoryRows
                                    .Where(r =>
                                        r.IsAbsent == null || !(bool)r.IsAbsent)
                                    .Sum(r => (decimal)r.ObtainedMarks);

                        var totalMaximum =
                            categoryRows.Sum(r => (decimal)r.MaximumMarks);

                        var percentage =
                            totalMaximum == 0
                                ? 0
                                : Math.Round(
                                    totalObtained * 100M / totalMaximum,
                                    2);

                        var ranges =
                            gradeRangesByCategory.TryGetValue(
                                cat.ExamCategoryId,
                                out var catRanges)
                                ? catRanges
                                : new List<GradeRangeVM>();

                        row.Categories.Add(new FinalCategoryMarkVM
                        {
                            ExamCategoryId = cat.ExamCategoryId,
                            CategoryName = cat.CategoryName,
                            ShowBreakdown = cat.ShowBreakdown,
                            ExamTypeMarks = examTypeMarks,
                            TotalObtainedMarks = totalObtained,
                            TotalMaximumMarks = totalMaximum,
                            IsAbsent = isAbsent,
                            Grade = isAbsent ? "" : GetGrade(percentage, ranges)
                        });
                    }

                    return row;
                })
                .ToList();


            return (layout, subjectRows);
        }


        // =========================================================
        // GRADE LOOKUP
        // Finds the grade whose Min/Max percentage band contains the
        // given percentage (both ends inclusive).
        // =========================================================

        private string GetGrade(
            decimal percentage,
            List<GradeRangeVM> ranges)
        {
            if (ranges == null || ranges.Count == 0)
                return "";

            var match = ranges.FirstOrDefault(r =>
                percentage >= r.MinPercentage &&
                percentage <= r.MaxPercentage);

            return match?.Grade ?? "";
        }
    }
}
