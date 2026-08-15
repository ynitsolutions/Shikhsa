//using Dapper;
//using Microsoft.Data.SqlClient;
//using Shikhsa.Models;
//using Shikhsa.ViewModels;
//using System.Data;

//namespace Shikhsa.Repository
//{
//    public class ReportCardRepository
//    {
//        private readonly IConfiguration _configuration;

//        public ReportCardRepository(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        private SqlConnection Connection()
//        {
//            return new SqlConnection(
//                _configuration.GetConnectionString("DefaultConnection"));
//        }


//        // =========================================================
//        // STUDENT LIST
//        // =========================================================

//        public async Task<List<ReportCardStudentRowVM>> GetStudentListAsync(
//            int batchId,
//            int classId,
//            int sectionId)
//        {
//            using var con = Connection();

//            var result = await con.QueryAsync<ReportCardStudentRowVM>(
//                "USP_GetReportCardStudentList",
//                new
//                {
//                    BatchId = batchId,
//                    ClassId = classId,
//                    SectionId = sectionId
//                },
//                commandType: CommandType.StoredProcedure
//            );

//            return result.ToList();
//        }


//        // =========================================================
//        // STUDENT IDS
//        // (used for class-wise bulk report card download)
//        // =========================================================

//        public async Task<List<long>> GetStudentIdsAsync(
//            int batchId,
//            int classId,
//            int sectionId)
//        {
//            const string sql = @"
//SELECT S.StudentId
//FROM Tbl_Students S
//WHERE S.IsActive = 1
//  AND S.AdmitBatchId = @batchId
//  AND S.AdmitClassId = @classId
//  AND S.AdmitSectionId = @sectionId
//ORDER BY
//    TRY_CONVERT(INT, S.ScholarNumber),
//    S.FirstName,
//    S.MiddleName,
//    S.LastName;
//";

//            using var con = Connection();

//            var result = await con.QueryAsync<long>(
//                sql,
//                new
//                {
//                    batchId,
//                    classId,
//                    sectionId
//                });

//            return result.ToList();
//        }


//        // =========================================================
//        // STUDENT HEADER
//        // =========================================================

//        public async Task<StudentHeaderVM?> GetStudentHeaderAsync(
//            long studentId,
//            int batchId)
//        {
//            const string sql = @"
//SELECT TOP 1

//    S.StudentId,

//    CONCAT(
//        S.FirstName,
//        CASE
//            WHEN NULLIF(S.MiddleName,'') IS NOT NULL
//                THEN ' ' + S.MiddleName
//            ELSE ''
//        END,
//        CASE
//            WHEN NULLIF(S.LastName,'') IS NOT NULL
//                THEN ' ' + S.LastName
//            ELSE ''
//        END
//    ) AS StudentName,

//    CONCAT(
//        ISNULL(P.FatherFirstName,''),
//        CASE
//            WHEN NULLIF(P.FatherMiddleName,'') IS NOT NULL
//                THEN ' ' + P.FatherMiddleName
//            ELSE ''
//        END,
//        CASE
//            WHEN NULLIF(P.FatherLastName,'') IS NOT NULL
//                THEN ' ' + P.FatherLastName
//            ELSE ''
//        END
//    ) AS FatherName,

//    CONCAT(
//        ISNULL(P.MotherFirstName,''),
//        CASE
//            WHEN NULLIF(P.MotherMiddleName,'') IS NOT NULL
//                THEN ' ' + P.MotherMiddleName
//            ELSE ''
//        END,
//        CASE
//            WHEN NULLIF(P.MotherLastName,'') IS NOT NULL
//                THEN ' ' + P.MotherLastName
//            ELSE ''
//        END
//    ) AS MotherName,

//    ISNULL(S.ScholarNumber,'') AS ScholarNumber,

//    ISNULL(S.ApplicationNo,'') AS ApplicationNo,

//    S.DOB,

//    ISNULL(C.DataListItemText,'') AS ClassName,

//    ISNULL(SEC.DataListItemText,'') AS SectionName,

//    B.AcademicYear,

//    SM.SchoolName,
//    ISNULL(SM.SchoolMotto,'') AS SchoolMotto,
//    SM.SchoolAddress,
//    SM.MobileContactNo,
//    SM.Email,
//    SM.Website,
//    SM.Board,
//    SM.LogoPath,
//    SM.AffiliationNo,
//    SM.UDISECode,

//    S.PENNumber,
//    S.APAARId,

//    S.AdmitBatchId,
//    S.AdmitClassId

//FROM Tbl_Students S

//LEFT JOIN Tbl_Parents P
//    ON P.ParentId = S.ParentId

//LEFT JOIN DataListItems C
//    ON C.DataListItemId = S.AdmitClassId

//LEFT JOIN DataListItems SEC
//    ON SEC.DataListItemId = S.AdmitSectionId

//INNER JOIN Batches B
//    ON B.BatchId = @batchId

//OUTER APPLY
//(
//    SELECT TOP 1 *
//    FROM SchoolMasters
//    WHERE IsActive = 1
//    ORDER BY SchoolId
//) SM

//WHERE S.StudentId = @studentId
//  AND S.AdmitBatchId = @batchId
//  AND S.IsActive = 1;
//";

//            using var con = Connection();

//            return await con.QueryFirstOrDefaultAsync<StudentHeaderVM>(
//                sql,
//                new
//                {
//                    studentId,
//                    batchId
//                });
//        }


//        // =========================================================
//        // SCHOLASTIC
//        // =========================================================

//        public async Task<List<dynamic>> GetScholasticAsync(
//            long studentId,
//            int batchId,
//            int examCategoryId)
//        {
//            const string sql = @"
//SELECT
//    M.ExamObtainedMarkId,
//    M.StudentId,
//    M.BatchId,
//    M.ClassId,
//    M.SectionId,

//    M.SubjectId,
//    SUB.SubjectName,

//    M.ExamId,

//    E.ExamName,
//    E.ExamType,

//    D.DataListItemId AS ExamTypeId,
//    D.DataListItemText AS ExamTypeName,
//    D.DisplayOrder,

//    E.MinMarks AS PassingMarks,
//    E.MaxMarks AS MaximumMarks,

//    ISNULL(M.ObtainedMarks,0) AS ObtainedMarks,

//    M.IsAbsent,
//    M.Remarks

//FROM ExamObtainedMarks M

//INNER JOIN ScholasticExams E
//    ON E.Id = M.ExamId

//INNER JOIN SubjectMasters SUB
//    ON SUB.SubjectId = M.SubjectId

//LEFT JOIN DataListItems D
//    ON D.DataListItemId = E.ExamType

//LEFT JOIN DataLists DL
//    ON DL.DataListId = D.DataListId
//   AND DL.Description = 'Exam Type'

//WHERE M.StudentId = @studentId
//  AND M.BatchId = @batchId
//  AND E.ExamCategoryId = @examCategoryId
//  AND M.IsActive = 1
//  AND E.IsActive = 1

//ORDER BY
//    SUB.SubjectName,
//    ISNULL(D.DisplayOrder,999),
//    D.DataListItemText;
//";

//            using var con = Connection();

//            var result = await con.QueryAsync(
//                sql,
//                new
//                {
//                    studentId,
//                    batchId,
//                    examCategoryId
//                });

//            return result.ToList();
//        }


//        // =========================================================
//        // CO-SCHOLASTIC
//        // =========================================================

//        public async Task<List<CoScholasticVM>> GetCoScholasticAsync(
//            long studentId,
//            int batchId,
//            int examCategoryId)
//        {
//            const string sql = @"
//SELECT

//    CSG.CoScholasticAreaId AS AreaId,

//    CS.Title,

//    CSG.Grade,

//    NULL AS TeacherRemark

//FROM CoScholasticGrades CSG

//INNER JOIN CoScholasticAreas CSA
//    ON CSA.CoScholasticAreaId = CSG.CoScholasticAreaId

//INNER JOIN CoScholastics CS
//    ON CS.CoScholasticId = CSA.CoScholasticId

//WHERE CSG.StudentId = @studentId
//  AND CSG.BatchId = @batchId
//  AND CSG.ExamCategoryId = @examCategoryId
//  AND CSG.IsActive = 1

//ORDER BY
//    CS.Title;
//";

//            using var con = Connection();

//            var result = await con.QueryAsync<CoScholasticVM>(
//                sql,
//                new
//                {
//                    studentId,
//                    batchId,
//                    examCategoryId
//                });

//            return result.ToList();
//        }


//        // =========================================================
//        // ATTENDANCE
//        // =========================================================

//        public async Task<AttendanceSummaryVM> GetAttendanceSummaryAsync(
//            long studentId,
//            int batchId)
//        {
//            const string sql = @"
//SELECT

//    COUNT(*) AS WorkingDays,

//    SUM(
//        CASE
//            WHEN A.AttendanceTypeId = 1 THEN 1
//            ELSE 0
//        END
//    ) AS PresentDays,

//    SUM(
//        CASE
//            WHEN A.AttendanceTypeId = 2 THEN 1
//            ELSE 0
//        END
//    ) AS AbsentDays,

//    SUM(
//        CASE
//            WHEN A.AttendanceTypeId = 3 THEN 1
//            ELSE 0
//        END
//    ) AS LeaveDays,

//    SUM(
//        CASE
//            WHEN A.AttendanceTypeId = 4 THEN 1
//            ELSE 0
//        END
//    ) AS HalfDays

//FROM StudentAttendances A

//WHERE A.StudentId = @studentId
//  AND A.BatchId = @batchId
//  AND A.IsActive = 1;
//";

//            using var con = Connection();

//            var result =
//                await con.QueryFirstOrDefaultAsync<AttendanceSummaryVM>(
//                    sql,
//                    new
//                    {
//                        studentId,
//                        batchId
//                    });

//            result ??= new AttendanceSummaryVM();

//            if (result.WorkingDays > 0)
//            {
//                result.AttendancePercentage =
//                    Math.Round(
//                        result.PresentDays * 100M /
//                        result.WorkingDays,
//                        2);
//            }

//            return result;
//        }


//        // =========================================================
//        // SUMMARY
//        // =========================================================

//        public async Task<StudentExamSummary?> GetSummaryAsync(
//            long studentId,
//            int batchId,
//            int examCategoryId)
//        {
//            const string sql = @"
//SELECT TOP 1 *

//FROM StudentExamSummaries

//WHERE StudentId = @studentId
//  AND BatchId = @batchId
//  AND ExamCategoryId = @examCategoryId
//  AND IsActive = 1;
//";

//            using var con = Connection();

//            return await con.QueryFirstOrDefaultAsync<StudentExamSummary>(
//                sql,
//                new
//                {
//                    studentId,
//                    batchId,
//                    examCategoryId
//                });
//        }


//        // =========================================================
//        // GRADE RANGE
//        // =========================================================

//        public async Task<List<GradeRangeVM>> GetGradeRangesAsync(
//            int batchId,
//            int classId,
//            int termId)
//        {
//            const string sql = @"
//SELECT

//    BatchId,
//    ClassId,
//    TermId,
//    MinPercentage,
//    MaxPercentage,
//    Grade,
//    Description

//FROM GradingCriteria

//WHERE BatchId = @batchId
//  AND ClassId = @classId
//  AND TermId = @termId
//  AND IsActive = 1

//ORDER BY
//    MinPercentage DESC;
//";

//            using var con = Connection();

//            var result = await con.QueryAsync<GradeRangeVM>(
//                sql,
//                new
//                {
//                    batchId,
//                    classId,
//                    termId
//                });

//            return result.ToList();
//        }
//    }
//}
using Dapper;
using Microsoft.Data.SqlClient;
using Shikhsa.Models;
using Shikhsa.ViewModels;
using System.Data;

namespace Shikhsa.Repository
{
    public class ReportCardRepository
    {
        private readonly IConfiguration _configuration;

        public ReportCardRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection Connection()
        {
            return new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));
        }


        // =========================================================
        // STUDENT LIST
        // =========================================================

        public async Task<List<ReportCardStudentRowVM>> GetStudentListAsync(
            int batchId,
            int classId,
            int sectionId)
        {
            using var con = Connection();

            var result = await con.QueryAsync<ReportCardStudentRowVM>(
                "USP_GetReportCardStudentList",
                new
                {
                    BatchId = batchId,
                    ClassId = classId,
                    SectionId = sectionId
                },
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }


        // =========================================================
        // STUDENT IDS
        // (used for class-wise bulk report card download)
        // =========================================================

        public async Task<List<long>> GetStudentIdsAsync(
            int batchId,
            int classId,
            int sectionId)
        {
            const string sql = @"
SELECT S.StudentId
FROM Tbl_Students S
WHERE S.IsActive = 1
  AND S.AdmitBatchId = @batchId
  AND S.AdmitClassId = @classId
  AND S.AdmitSectionId = @sectionId
ORDER BY
    TRY_CONVERT(INT, S.ScholarNumber),
    S.FirstName,
    S.MiddleName,
    S.LastName;
";

            using var con = Connection();

            var result = await con.QueryAsync<long>(
                sql,
                new
                {
                    batchId,
                    classId,
                    sectionId
                });

            return result.ToList();
        }


        // =========================================================
        // EXAM CATEGORY LOOKUP
        // NOTE: assumes a table "ExamCategories" with columns
        // ExamCategoryId, CategoryName, DisplayOrder, IsActive.
        // Adjust names below if your schema differs.
        // =========================================================

        public async Task<ExamCategoryLookupVM?> GetExamCategoryAsync(
            int examCategoryId)
        {
            const string sql = @"
SELECT
    ExamCategoryId,
    ExamCategoryName AS CategoryName

FROM ExamCategories

WHERE ExamCategoryId = @examCategoryId
  AND IsActive = 1;
";

            using var con = Connection();

            return await con.QueryFirstOrDefaultAsync<ExamCategoryLookupVM>(
                sql,
                new { examCategoryId });
        }


        public async Task<int?> GetExamCategoryIdByNameAsync(
            string categoryName)
        {
            const string sql = @"
SELECT TOP 1 ExamCategoryId

FROM ExamCategories

WHERE ExamCategoryName = @categoryName
  AND IsActive = 1;
";

            using var con = Connection();

            return await con.QueryFirstOrDefaultAsync<int?>(
                sql,
                new { categoryName });
        }


        // =========================================================
        // SCHOLASTIC MARKS ACROSS ALL EXAM CATEGORIES
        // Used to build the consolidated "Final" report card table
        // (First Periodic / Second Periodic / ... / Annual Exam).
        // "Final" itself is excluded since it has no marks of its own.
        // =========================================================

        public async Task<List<dynamic>> GetScholasticAcrossCategoriesAsync(
            long studentId,
            int batchId)
        {
            const string sql = @"
SELECT
    EC.ExamCategoryId,
    EC.ExamCategoryName AS CategoryName,
    EC.DisplayOrder AS CategoryDisplayOrder,

    M.SubjectId,
    SUB.SubjectName,

    D.DataListItemId AS ExamTypeId,
    D.DataListItemText AS ExamTypeName,
    D.DisplayOrder AS ExamTypeDisplayOrder,

    E.MaxMarks AS MaximumMarks,
    ISNULL(M.ObtainedMarks,0) AS ObtainedMarks,

    M.IsAbsent

FROM ExamObtainedMarks M

INNER JOIN ScholasticExams E
    ON E.Id = M.ExamId

INNER JOIN SubjectMasters SUB
    ON SUB.SubjectId = M.SubjectId

INNER JOIN ExamCategories EC
    ON EC.ExamCategoryId = E.ExamCategoryId

LEFT JOIN DataListItems D
    ON D.DataListItemId = E.ExamType

WHERE M.StudentId = @studentId
  AND M.BatchId = @batchId
  AND M.IsActive = 1
  AND E.IsActive = 1
  AND EC.IsActive = 1
  AND EC.ExamCategoryName <> 'Final'

ORDER BY
    EC.DisplayOrder,
    SUB.SubjectName,
    ISNULL(D.DisplayOrder,999);
";

            using var con = Connection();

            var result = await con.QueryAsync(
                sql,
                new
                {
                    studentId,
                    batchId
                });

            return result.ToList();
        }


        // =========================================================
        // STUDENT HEADER
        // =========================================================

        public async Task<StudentHeaderVM?> GetStudentHeaderAsync(
            long studentId,
            int batchId)
        {
            const string sql = @"
SELECT TOP 1

    S.StudentId,

    CONCAT(
        S.FirstName,
        CASE
            WHEN NULLIF(S.MiddleName,'') IS NOT NULL
                THEN ' ' + S.MiddleName
            ELSE ''
        END,
        CASE
            WHEN NULLIF(S.LastName,'') IS NOT NULL
                THEN ' ' + S.LastName
            ELSE ''
        END
    ) AS StudentName,

    CONCAT(
        ISNULL(P.FatherFirstName,''),
        CASE
            WHEN NULLIF(P.FatherMiddleName,'') IS NOT NULL
                THEN ' ' + P.FatherMiddleName
            ELSE ''
        END,
        CASE
            WHEN NULLIF(P.FatherLastName,'') IS NOT NULL
                THEN ' ' + P.FatherLastName
            ELSE ''
        END
    ) AS FatherName,

    CONCAT(
        ISNULL(P.MotherFirstName,''),
        CASE
            WHEN NULLIF(P.MotherMiddleName,'') IS NOT NULL
                THEN ' ' + P.MotherMiddleName
            ELSE ''
        END,
        CASE
            WHEN NULLIF(P.MotherLastName,'') IS NOT NULL
                THEN ' ' + P.MotherLastName
            ELSE ''
        END
    ) AS MotherName,

    ISNULL(S.ScholarNumber,'') AS ScholarNumber,

    ISNULL(S.ApplicationNo,'') AS ApplicationNo,

    S.DOB,

    ISNULL(C.DataListItemText,'') AS ClassName,

    ISNULL(SEC.DataListItemText,'') AS SectionName,

    B.AcademicYear,

    SM.SchoolName,
    ISNULL(SM.SchoolMotto,'') AS SchoolMotto,
    SM.SchoolAddress,
    SM.MobileContactNo,
    SM.Email,
    SM.Website,
    SM.Board,
    SM.LogoPath,
    SM.AffiliationNo,
    SM.UDISECode,

    S.PENNumber,
    S.APAARId,

    S.AdmitBatchId,
    S.AdmitClassId

FROM Tbl_Students S

LEFT JOIN Tbl_Parents P
    ON P.ParentId = S.ParentId

LEFT JOIN DataListItems C
    ON C.DataListItemId = S.AdmitClassId

LEFT JOIN DataListItems SEC
    ON SEC.DataListItemId = S.AdmitSectionId

INNER JOIN Batches B
    ON B.BatchId = @batchId

OUTER APPLY
(
    SELECT TOP 1 *
    FROM SchoolMasters
    WHERE IsActive = 1
    ORDER BY SchoolId
) SM

WHERE S.StudentId = @studentId
  AND S.AdmitBatchId = @batchId
  AND S.IsActive = 1;
";

            using var con = Connection();

            return await con.QueryFirstOrDefaultAsync<StudentHeaderVM>(
                sql,
                new
                {
                    studentId,
                    batchId
                });
        }


        // =========================================================
        // SCHOLASTIC
        // =========================================================

        public async Task<List<dynamic>> GetScholasticAsync(
            long studentId,
            int batchId,
            int examCategoryId)
        {
            const string sql = @"
SELECT
    M.ExamObtainedMarkId,
    M.StudentId,
    M.BatchId,
    M.ClassId,
    M.SectionId,

    M.SubjectId,
    SUB.SubjectName,

    M.ExamId,

    E.ExamName,
    E.ExamType,

    D.DataListItemId AS ExamTypeId,
    D.DataListItemText AS ExamTypeName,
    D.DisplayOrder,

    E.MinMarks AS PassingMarks,
    E.MaxMarks AS MaximumMarks,

    ISNULL(M.ObtainedMarks,0) AS ObtainedMarks,

    M.IsAbsent,
    M.Remarks

FROM ExamObtainedMarks M

INNER JOIN ScholasticExams E
    ON E.Id = M.ExamId

INNER JOIN SubjectMasters SUB
    ON SUB.SubjectId = M.SubjectId

LEFT JOIN DataListItems D
    ON D.DataListItemId = E.ExamType

LEFT JOIN DataLists DL
    ON DL.DataListId = D.DataListId
   AND DL.Description = 'Exam Type'

WHERE M.StudentId = @studentId
  AND M.BatchId = @batchId
  AND E.ExamCategoryId = @examCategoryId
  AND M.IsActive = 1
  AND E.IsActive = 1

ORDER BY
    SUB.SubjectName,
    ISNULL(D.DisplayOrder,999),
    D.DataListItemText;
";

            using var con = Connection();

            var result = await con.QueryAsync(
                sql,
                new
                {
                    studentId,
                    batchId,
                    examCategoryId
                });

            return result.ToList();
        }


        // =========================================================
        // CO-SCHOLASTIC
        // =========================================================

        public async Task<List<CoScholasticVM>> GetCoScholasticAsync(
            long studentId,
            int batchId,
            int examCategoryId)
        {
            const string sql = @"
SELECT

    CSG.CoScholasticAreaId AS AreaId,

    CS.Title,

    CSG.Grade,

    NULL AS TeacherRemark

FROM CoScholasticGrades CSG

INNER JOIN CoScholasticAreas CSA
    ON CSA.CoScholasticAreaId = CSG.CoScholasticAreaId

INNER JOIN CoScholastics CS
    ON CS.CoScholasticId = CSA.CoScholasticId

WHERE CSG.StudentId = @studentId
  AND CSG.BatchId = @batchId
  AND CSG.ExamCategoryId = @examCategoryId
  AND CSG.IsActive = 1

ORDER BY
    CS.Title;
";

            using var con = Connection();

            var result = await con.QueryAsync<CoScholasticVM>(
                sql,
                new
                {
                    studentId,
                    batchId,
                    examCategoryId
                });

            return result.ToList();
        }


        // =========================================================
        // ATTENDANCE
        // =========================================================

        public async Task<AttendanceSummaryVM> GetAttendanceSummaryAsync(
            long studentId,
            int batchId)
        {
            const string sql = @"
SELECT

    COUNT(*) AS WorkingDays,

    SUM(
        CASE
            WHEN A.AttendanceTypeId = 1 THEN 1
            ELSE 0
        END
    ) AS PresentDays,

    SUM(
        CASE
            WHEN A.AttendanceTypeId = 2 THEN 1
            ELSE 0
        END
    ) AS AbsentDays,

    SUM(
        CASE
            WHEN A.AttendanceTypeId = 3 THEN 1
            ELSE 0
        END
    ) AS LeaveDays,

    SUM(
        CASE
            WHEN A.AttendanceTypeId = 4 THEN 1
            ELSE 0
        END
    ) AS HalfDays

FROM StudentAttendances A

WHERE A.StudentId = @studentId
  AND A.BatchId = @batchId
  AND A.IsActive = 1;
";

            using var con = Connection();

            var result =
                await con.QueryFirstOrDefaultAsync<AttendanceSummaryVM>(
                    sql,
                    new
                    {
                        studentId,
                        batchId
                    });

            result ??= new AttendanceSummaryVM();

            if (result.WorkingDays > 0)
            {
                result.AttendancePercentage =
                    Math.Round(
                        result.PresentDays * 100M /
                        result.WorkingDays,
                        2);
            }

            return result;
        }


        // =========================================================
        // SUMMARY
        // =========================================================

        public async Task<StudentExamSummary?> GetSummaryAsync(
            long studentId,
            int batchId,
            int examCategoryId)
        {
            const string sql = @"
SELECT TOP 1 *

FROM StudentExamSummaries

WHERE StudentId = @studentId
  AND BatchId = @batchId
  AND ExamCategoryId = @examCategoryId
  AND IsActive = 1;
";

            using var con = Connection();

            return await con.QueryFirstOrDefaultAsync<StudentExamSummary>(
                sql,
                new
                {
                    studentId,
                    batchId,
                    examCategoryId
                });
        }


        // =========================================================
        // GRADE RANGE
        // =========================================================

        public async Task<List<GradeRangeVM>> GetGradeRangesAsync(
            int batchId,
            int classId,
            int termId)
        {
            const string sql = @"
SELECT

    BatchId,
    ClassId,
    TermId,
    MinPercentage,
    MaxPercentage,
    Grade,
    Description

FROM GradingCriteria

WHERE BatchId = @batchId
  AND ClassId = @classId
  AND TermId = @termId
  AND IsActive = 1

ORDER BY
    MinPercentage DESC;
";

            using var con = Connection();

            var result = await con.QueryAsync<GradeRangeVM>(
                sql,
                new
                {
                    batchId,
                    classId,
                    termId
                });

            return result.ToList();
        }
    }


    // =========================================================
    // EXAM CATEGORY LOOKUP
    // =========================================================

    public class ExamCategoryLookupVM
    {
        public int ExamCategoryId { get; set; }

        public string CategoryName { get; set; } = "";
    }
}

