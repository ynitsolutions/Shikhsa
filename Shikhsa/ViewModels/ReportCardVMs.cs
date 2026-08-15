//using Shikhsa.Models;
//using Shikhsa.ViewModels.DataFilter;
//namespace Shikhsa.ViewModels
//{
//    // =========================================================
//    // FILTER
//    // =========================================================

//    public class ReportCardFilterVM : BaseFilterVM
//    {
//        public int? BatchId { get; set; }
//        public int? ClassId { get; set; }
//        public int? SectionId { get; set; }
//        public int? ExamCategoryId { get; set; }

//        public string BoardType { get; set; } = "CBSE";

//        public List<Batches> Batches { get; set; } = new();
//        public List<DataListItem> Classes { get; set; } = new();
//        public List<DataListItem> Sections { get; set; } = new();
//        public List<ExamCategory> ExamCategories { get; set; } = new();

//        public List<ReportCardStudentRowVM> Students { get; set; } = new();
//    }

//    // =========================================================
//    // STUDENT LIST
//    // =========================================================

//    public class ReportCardStudentRowVM
//    {
//        public long StudentId { get; set; }

//        public string? ScholarNumber { get; set; }

//        public string StudentName { get; set; } = "";

//        public string? ClassName { get; set; }

//        public string? SectionName { get; set; }

//        public bool IsReportCardReady { get; set; }
//    }

//    // =========================================================
//    // STUDENT HEADER
//    // =========================================================

//    public class StudentHeaderVM
//    {
//        public long StudentId { get; set; }

//        public string StudentName { get; set; } = "";

//        public string FatherName { get; set; } = "";

//        public string MotherName { get; set; } = "";

//        public string ScholarNumber { get; set; } = "";

//        public string ApplicationNo { get; set; } = "";

//        public DateTime DOB { get; set; }

//        public string ClassName { get; set; } = "";

//        public string SectionName { get; set; } = "";

//        public string AcademicYear { get; set; } = "";

//        public string SchoolName { get; set; } = "";

//        public string SchoolMotto { get; set; } = "";

//        public string SchoolAddress { get; set; } = "";

//        public string MobileContactNo { get; set; } = "";

//        public string Email { get; set; } = "";

//        public string Website { get; set; } = "";

//        public string Board { get; set; } = "";

//        public string? SchoolLogo { get; set; }

//        public string? LogoPath { get; set; }

//        public string? StudentPhoto { get; set; }

//        public string? AffiliationNo { get; set; }

//        public string? UDISECode { get; set; }

//        public string? PENNumber { get; set; }

//        public string? APAARId { get; set; }

//        public int AdmitBatchId { get; set; }

//        public int AdmitClassId { get; set; }
//    }

//    // =========================================================
//    // SCHOLASTIC SUBJECT
//    // =========================================================

//    public class ScholasticMarkVM
//    {
//        public int SubjectId { get; set; }

//        public string SubjectName { get; set; } = "";

//        public decimal MaximumMarks { get; set; }

//        public decimal PassingMarks { get; set; }

//        public decimal ObtainedMarks { get; set; }

//        public decimal Percentage { get; set; }

//        public string Grade { get; set; } = "";

//        public string Result { get; set; } = "";

//        public bool IsAbsent { get; set; }

//        public string? TeacherRemark { get; set; }

//        // Total obtained across all exam types
//        public decimal TotalObtainedMarks { get; set; }

//        // Total maximum across all exam types
//        // (Added: was missing, referenced by ReportCardService and ReportCardPdfService)
//        public decimal TotalMaximumMarks { get; set; }

//        // Exam type wise marks
//        public List<ExamTypeMarkVM> ExamMarks { get; set; } = new();
//    }

//    // =========================================================
//    // EXAM TYPE MARK
//    // Theory / Practical / Viva / Project / etc.
//    // =========================================================

//    public class ExamTypeMarkVM
//    {
//        public int ExamTypeId { get; set; }

//        public string ExamTypeName { get; set; } = "";

//        public decimal MaximumMarks { get; set; }

//        public decimal PassingMarks { get; set; }

//        public decimal ObtainedMarks { get; set; }

//        public bool IsAbsent { get; set; }

//        public string? Remarks { get; set; }
//        public int DisplayOrder { get; set; } = 0;
//    }

//    // =========================================================
//    // EXAM TYPE MASTER
//    // =========================================================

//    public class ExamTypeVM
//    {
//        public int ExamTypeId { get; set; }

//        public string ExamTypeName { get; set; } = "";

//        public int DisplayOrder { get; set; }
//    }

//    // =========================================================
//    // CO-SCHOLASTIC
//    // =========================================================

//    public class CoScholasticVM
//    {
//        public long AreaId { get; set; }

//        public string Title { get; set; } = "";

//        public string Grade { get; set; } = "";

//        public string? TeacherRemark { get; set; }
//    }

//    // =========================================================
//    // ATTENDANCE
//    // =========================================================

//    public class AttendanceSummaryVM
//    {
//        public int WorkingDays { get; set; }

//        public int PresentDays { get; set; }

//        public int AbsentDays { get; set; }

//        public int LeaveDays { get; set; }

//        public int HalfDays { get; set; }

//        public decimal AttendancePercentage { get; set; }
//    }

//    // =========================================================
//    // FINAL SUMMARY
//    // =========================================================

//    public class ReportCardSummaryVM
//    {
//        public decimal TotalMaximumMarks { get; set; }

//        public decimal TotalObtainedMarks { get; set; }

//        public decimal Percentage { get; set; }

//        public string Grade { get; set; } = "";

//        public string Result { get; set; } = "";

//        public int? Rank { get; set; }

//        public string? TeacherRemark { get; set; }

//        public string? PrincipalRemark { get; set; }
//    }

//    // =========================================================
//    // GRADING RANGE
//    // =========================================================

//    public class GradeRangeVM
//    {
//        public int BatchId { get; set; }

//        public int ClassId { get; set; }

//        public int TermId { get; set; }

//        public decimal MinPercentage { get; set; }

//        public decimal MaxPercentage { get; set; }

//        public string Grade { get; set; } = "";

//        public string? Description { get; set; }
//    }

//    public class ReportCardVM
//    {
//        // =========================
//        // Student / School Header
//        // =========================

//        public StudentHeaderVM Student { get; set; } = new();

//        // =========================
//        // Exam Types
//        // Theory / Practical / Viva / Project etc.
//        // =========================

//        public List<ExamTypeVM> ExamTypes { get; set; } = new();

//        // =========================
//        // Scholastic Subjects
//        // =========================

//        public List<ScholasticMarkVM> ScholasticMarks { get; set; } = new();

//        // =========================
//        // Co-Scholastic
//        // =========================

//        public List<CoScholasticVM> CoScholasticMarks { get; set; } = new();

//        // =========================
//        // Attendance
//        // =========================

//        public AttendanceSummaryVM Attendance { get; set; } = new();

//        // =========================
//        // Grade Configuration
//        // =========================

//        public List<GradeRangeVM> GradeRanges { get; set; } = new();

//        // =========================
//        // Final Summary
//        // =========================

//        public ReportCardSummaryVM Summary { get; set; } = new();

//        // =========================
//        // Report Card Settings
//        // =========================

//        public ReportCardSetting Settings { get; set; } = new();

//        // =========================
//        // Remarks
//        // =========================

//        public string? TeacherRemark { get; set; }

//        public string? PrincipalRemark { get; set; }

//        // =========================
//        // Signatures
//        // =========================

//        public string? ClassTeacherSignaturePath { get; set; }

//        public string? PrincipalSignaturePath { get; set; }

//        public string? ParentSignaturePath { get; set; }

//        // =========================
//        // Other Report Card Data
//        // =========================

//        public string? QRCodeText { get; set; }

//        public string? SchoolSeal { get; set; }

//        public string? PrincipalSignature { get; set; }

//        public string? TeacherSignature { get; set; }

//        // =========================
//        // Convenience Properties
//        // =========================

//        public decimal TotalMaximumMarks
//        {
//            get
//            {
//                return ScholasticMarks?
//                    .Sum(x => x.MaximumMarks) ?? 0;
//            }
//        }

//        public decimal TotalObtainedMarks
//        {
//            get
//            {
//                return ScholasticMarks?
//                    .Sum(x => x.ObtainedMarks) ?? 0;
//            }
//        }

//        public decimal Percentage
//        {
//            get
//            {
//                if (TotalMaximumMarks <= 0)
//                    return 0;

//                return Math.Round(
//                    TotalObtainedMarks * 100M / TotalMaximumMarks,
//                    2);
//            }
//        }

//        public string Result
//        {
//            get
//            {
//                if (ScholasticMarks == null ||
//                    ScholasticMarks.Count == 0)
//                    return "N/A";

//                return ScholasticMarks.All(x =>
//                    x.Result.Equals(
//                        "Pass",
//                        StringComparison.OrdinalIgnoreCase))
//                    ? "PASS"
//                    : "FAIL";
//            }
//        }

//        public bool IsPass =>
//            Result.Equals(
//                "PASS",
//                StringComparison.OrdinalIgnoreCase);
//    }
//}

using Shikhsa.Models;
using Shikhsa.ViewModels.DataFilter;

namespace Shikhsa.ViewModels
{
    // =========================================================
    // FILTER
    // =========================================================

    public class ReportCardFilterVM : BaseFilterVM
    {
        public int? BatchId { get; set; }
        public int? ClassId { get; set; }
        public int? SectionId { get; set; }
        public int? ExamCategoryId { get; set; }

        public string BoardType { get; set; } = "CBSE";

        public List<Batches> Batches { get; set; } = new();
        public List<DataListItem> Classes { get; set; } = new();
        public List<DataListItem> Sections { get; set; } = new();
        public List<ExamCategory> ExamCategories { get; set; } = new();

        public List<ReportCardStudentRowVM> Students { get; set; } = new();
    }

    // =========================================================
    // STUDENT LIST
    // =========================================================

    public class ReportCardStudentRowVM
    {
        public long StudentId { get; set; }

        public string? ScholarNumber { get; set; }

        public string StudentName { get; set; } = "";

        public string? ClassName { get; set; }

        public string? SectionName { get; set; }

        public bool IsReportCardReady { get; set; }
    }

    // =========================================================
    // STUDENT HEADER
    // =========================================================

    public class StudentHeaderVM
    {
        public long StudentId { get; set; }

        public string StudentName { get; set; } = "";

        public string FatherName { get; set; } = "";

        public string MotherName { get; set; } = "";

        public string ScholarNumber { get; set; } = "";

        public string ApplicationNo { get; set; } = "";

        public DateTime DOB { get; set; }

        public string ClassName { get; set; } = "";

        public string SectionName { get; set; } = "";

        public string AcademicYear { get; set; } = "";

        public string SchoolName { get; set; } = "";

        public string SchoolMotto { get; set; } = "";

        public string SchoolAddress { get; set; } = "";

        public string MobileContactNo { get; set; } = "";

        public string Email { get; set; } = "";

        public string Website { get; set; } = "";

        public string Board { get; set; } = "";

        public string? SchoolLogo { get; set; }

        public string? LogoPath { get; set; }

        public string? StudentPhoto { get; set; }

        public string? AffiliationNo { get; set; }

        public string? UDISECode { get; set; }

        public string? PENNumber { get; set; }

        public string? APAARId { get; set; }

        public int AdmitBatchId { get; set; }

        public int AdmitClassId { get; set; }
    }

    // =========================================================
    // SCHOLASTIC SUBJECT
    // =========================================================

    public class ScholasticMarkVM
    {
        public int SubjectId { get; set; }

        public string SubjectName { get; set; } = "";

        public decimal MaximumMarks { get; set; }

        public decimal PassingMarks { get; set; }

        public decimal ObtainedMarks { get; set; }

        public decimal Percentage { get; set; }

        public string Grade { get; set; } = "";

        public string Result { get; set; } = "";

        public bool IsAbsent { get; set; }

        public string? TeacherRemark { get; set; }

        // Total obtained across all exam types
        public decimal TotalObtainedMarks { get; set; }

        // Total maximum across all exam types
        // (Added: was missing, referenced by ReportCardService and ReportCardPdfService)
        public decimal TotalMaximumMarks { get; set; }

        // Exam type wise marks
        public List<ExamTypeMarkVM> ExamMarks { get; set; } = new();
    }

    // =========================================================
    // EXAM TYPE MARK
    // Theory / Practical / Viva / Project / etc.
    // =========================================================

    public class ExamTypeMarkVM
    {
        public int ExamTypeId { get; set; }

        public string ExamTypeName { get; set; } = "";

        public decimal MaximumMarks { get; set; }

        public decimal PassingMarks { get; set; }

        public decimal ObtainedMarks { get; set; }

        public bool IsAbsent { get; set; }

        public string? Remarks { get; set; }
        public int DisplayOrder { get; set; } = 0;
    }

    // =========================================================
    // EXAM TYPE MASTER
    // =========================================================

    public class ExamTypeVM
    {
        public int ExamTypeId { get; set; }

        public string ExamTypeName { get; set; } = "";

        public int DisplayOrder { get; set; }
    }

    // =========================================================
    // CO-SCHOLASTIC
    // =========================================================

    public class CoScholasticVM
    {
        public long AreaId { get; set; }

        public string Title { get; set; } = "";

        public string Grade { get; set; } = "";

        public string? TeacherRemark { get; set; }
    }

    // =========================================================
    // ATTENDANCE
    // =========================================================

    public class AttendanceSummaryVM
    {
        public int WorkingDays { get; set; }

        public int PresentDays { get; set; }

        public int AbsentDays { get; set; }

        public int LeaveDays { get; set; }

        public int HalfDays { get; set; }

        public decimal AttendancePercentage { get; set; }
    }

    // =========================================================
    // FINAL SUMMARY
    // =========================================================

    public class ReportCardSummaryVM
    {
        public decimal TotalMaximumMarks { get; set; }

        public decimal TotalObtainedMarks { get; set; }

        public decimal Percentage { get; set; }

        public string Grade { get; set; } = "";

        public string Result { get; set; } = "";

        public int? Rank { get; set; }

        public string? TeacherRemark { get; set; }

        public string? PrincipalRemark { get; set; }
    }

    // =========================================================
    // "FINAL" REPORT CARD - CONSOLIDATED SCHOLASTIC TABLE
    // Used only when the requested Exam Category's name is "Final".
    // Shows one column-group per underlying exam category
    // (First Periodic, Second Periodic, ... , Annual Exam).
    // =========================================================

    // Describes the column layout for one category-group in the
    // header row. Same layout applies to every subject row.
    public class FinalCategoryLayoutVM
    {
        public int ExamCategoryId { get; set; }

        public string CategoryName { get; set; } = "";

        // true => show one column per exam type (e.g. Theory, Practical)
        // plus Total + Grade. false => show just Total + Grade
        // (this is what collapses "Annual Exam" down automatically).
        public bool ShowBreakdown { get; set; }

        public List<string> ExamTypeNames { get; set; } = new();
    }

    public class FinalExamTypeMarkVM
    {
        public string ExamTypeName { get; set; } = "";

        public decimal ObtainedMarks { get; set; }

        public decimal MaximumMarks { get; set; }

        public bool IsAbsent { get; set; }
    }

    public class FinalCategoryMarkVM
    {
        public int ExamCategoryId { get; set; }

        public string CategoryName { get; set; } = "";

        public bool ShowBreakdown { get; set; }

        public List<FinalExamTypeMarkVM> ExamTypeMarks { get; set; } = new();

        public decimal TotalObtainedMarks { get; set; }

        public decimal TotalMaximumMarks { get; set; }

        public string Grade { get; set; } = "";

        public bool IsAbsent { get; set; }
    }

    public class FinalScholasticRowVM
    {
        public int SubjectId { get; set; }

        public string SubjectName { get; set; } = "";

        public List<FinalCategoryMarkVM> Categories { get; set; } = new();
    }

    // =========================================================
    // GRADING RANGE
    // =========================================================

    public class GradeRangeVM
    {
        public int BatchId { get; set; }

        public int ClassId { get; set; }

        public int TermId { get; set; }

        public decimal MinPercentage { get; set; }

        public decimal MaxPercentage { get; set; }

        public string Grade { get; set; } = "";

        public string? Description { get; set; }
    }

    public class ReportCardVM
    {
        // =========================
        // Student / School Header
        // =========================

        public StudentHeaderVM Student { get; set; } = new();

        // =========================
        // Exam Types
        // Theory / Practical / Viva / Project etc.
        // =========================

        public List<ExamTypeVM> ExamTypes { get; set; } = new();

        // =========================
        // Scholastic Subjects
        // (used when the exam category is a normal, single category)
        // =========================

        public List<ScholasticMarkVM> ScholasticMarks { get; set; } = new();

        // =========================
        // "Final" Report Card - consolidated view across all
        // exam categories (only populated when IsFinalReport is true)
        // =========================

        public bool IsFinalReport { get; set; }

        public List<FinalCategoryLayoutVM> FinalCategoryLayout { get; set; } = new();

        public List<FinalScholasticRowVM> FinalScholasticMarks { get; set; } = new();

        // =========================
        // Co-Scholastic
        // =========================

        public List<CoScholasticVM> CoScholasticMarks { get; set; } = new();

        // =========================
        // Attendance
        // =========================

        public AttendanceSummaryVM Attendance { get; set; } = new();

        // =========================
        // Grade Configuration
        // =========================

        public List<GradeRangeVM> GradeRanges { get; set; } = new();

        // =========================
        // Final Summary
        // =========================

        public ReportCardSummaryVM Summary { get; set; } = new();

        // =========================
        // Report Card Settings
        // =========================

        public ReportCardSetting Settings { get; set; } = new();

        // =========================
        // Remarks
        // =========================

        public string? TeacherRemark { get; set; }

        public string? PrincipalRemark { get; set; }

        // =========================
        // Signatures
        // =========================

        public string? ClassTeacherSignaturePath { get; set; }

        public string? PrincipalSignaturePath { get; set; }

        public string? ParentSignaturePath { get; set; }

        // =========================
        // Other Report Card Data
        // =========================

        public string? QRCodeText { get; set; }

        public string? SchoolSeal { get; set; }

        public string? PrincipalSignature { get; set; }

        public string? TeacherSignature { get; set; }

        // =========================
        // Convenience Properties
        // =========================

        public decimal TotalMaximumMarks
        {
            get
            {
                return ScholasticMarks?
                    .Sum(x => x.MaximumMarks) ?? 0;
            }
        }

        public decimal TotalObtainedMarks
        {
            get
            {
                return ScholasticMarks?
                    .Sum(x => x.ObtainedMarks) ?? 0;
            }
        }

        public decimal Percentage
        {
            get
            {
                if (TotalMaximumMarks <= 0)
                    return 0;

                return Math.Round(
                    TotalObtainedMarks * 100M / TotalMaximumMarks,
                    2);
            }
        }

        public string Result
        {
            get
            {
                if (ScholasticMarks == null ||
                    ScholasticMarks.Count == 0)
                    return "N/A";

                return ScholasticMarks.All(x =>
                    x.Result.Equals(
                        "Pass",
                        StringComparison.OrdinalIgnoreCase))
                    ? "PASS"
                    : "FAIL";
            }
        }

        public bool IsPass =>
            Result.Equals(
                "PASS",
                StringComparison.OrdinalIgnoreCase);
    }
}