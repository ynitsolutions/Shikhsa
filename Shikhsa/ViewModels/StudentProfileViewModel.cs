using Shikhsa.Models;
using Shikhsa.Models.Payment;
using System.Collections.Generic;

namespace Shikhsa.ViewModels
{
    
    public class StudentProfileViewModel
    {
        public Tbl_Students Student { get; set; }
        public Tbl_Parents Parent { get; set; }
        public StudentExtraInfo ExtraInfo { get; set; } = new();
        public List<Tbl_StudentDocument> Documents { get; set; } = new();
        public Tbl_PreviousSchoolRecord PreviousSchool { get; set; }

        public List<StudentFee> CurrentFees { get; set; } = new();
        public List<FeeReceipt> RecentReceipts { get; set; } = new();

        public List<AttendanceMonthSummary> CurrentAttendance { get; set; } = new();
        public List<Batches> BatchHistory { get; set; } = new();
        public List<ScholasticMarkRow> ScholasticMarks { get; set; } = new();
        public List<ExamSummaryRow> ExamSummaries { get; set; } = new();
        public List<CoScholasticGradeRow> CoScholasticGrades { get; set; } = new();
        public decimal TotalFeeAmount => System.Linq.Enumerable.Sum(CurrentFees, f => f.FeeAmount);
        public decimal TotalPaidAmount => System.Linq.Enumerable.Sum(CurrentFees, f => f.PaidAmount);
        public decimal TotalBalanceAmount => System.Linq.Enumerable.Sum(CurrentFees, f => f.BalanceAmount);

        // Photo document ko dhoondhne ka helper (DocumentType = "Photo" convention)
        public Tbl_StudentDocument PhotoDocument =>
            Documents.Find(d => d.DocumentType == "Photo");
    }

    // Attendance ka pivoted monthly row — StudentAttendance table se hi SP me aggregate hota hai
    public class AttendanceMonthSummary
    {
        public string Month { get; set; }
        public int TotalDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }

        public double AttendancePercent =>
            TotalDays == 0 ? 0 : System.Math.Round((PresentDays * 100.0) / TotalDays, 1);
    }
    public class ScholasticMarkRow
    {
        public long ExamObtainedMarkId { get; set; }
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public decimal? ObtainedMarks { get; set; }
        public bool IsAbsent { get; set; }
        public string Remarks { get; set; }
    }

    public class ExamSummaryRow
    {
        public int ExamCategoryId { get; set; }
        public string ExamCategoryName { get; set; }
        public int? RankInClass { get; set; }
        public string Remarks { get; set; }
    }

    public class CoScholasticGradeRow
    {
        public int ExamCategoryId { get; set; }
        public string ExamCategoryName { get; set; }
        public long CoScholasticAreaId { get; set; }
        public string ActivityTitle { get; set; }
        public string Grade { get; set; }
    }
    public class StudentExtraInfo
    {
        public string CategoryName { get; set; }
        public string GenderName { get; set; }
        public string ReligionName { get; set; }
        public string CurrentBatchName { get; set; }
    }
    // Old batch tab ke AJAX response ke liye
    public class BatchSummaryResult
    {
        public Batches BatchInfo { get; set; }
        public List<StudentFee> Fees { get; set; } = new();
        public List<AttendanceMonthSummary> Attendance { get; set; } = new();

        public List<ScholasticMarkRow> ScholasticMarks { get; set; } = new();
        public List<ExamSummaryRow> ExamSummaries { get; set; } = new();
        public List<CoScholasticGradeRow> CoScholasticGrades { get; set; } = new();
    }
}