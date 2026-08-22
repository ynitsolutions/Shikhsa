using Microsoft.AspNetCore.Mvc.Rendering;
using Shikhsa.Models;

namespace Shikhsa.ViewModels
{
    public class StudentFeeGenerateVM
    {
        public long StudentId { get; set; }

        public int ClassId { get; set; }

        public int BatchId { get; set; }

        public decimal DueFeeBatch { get; set; }
    }
    public class StudentFeePageVM
    {
        // Filters
        public int? SelectedClassId { get; set; }

        public int? SelectedBatchId { get; set; }

        public long? SelectedStudentId { get; set; }

        public decimal DueFeeBatch { get; set; }

        // Dropdowns
        // Existing BaseController methods se bind honge
        public List<DataListItem> Classes { get; set; } = new();

        public List<DataListItem> Sections { get; set; } = new();
        public List<DataListItem> PaymentMode { get; set; } = new();
        public List<Batches> Batches { get; set; } = new();


        // Students for selected class/batch
        public List<StudentFeeStudentVM> Students { get; set; } = new();

        // Selected Student Details
        public StudentFeeStudentVM? SelectedStudent { get; set; }
        public StudentFeeReceiptVM? FeeReceipt { get; set; }
    }
    public class StudentFeeStudentVM
    {
        public long StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string FatherName { get; set; } = string.Empty;

        public string ContactNo { get; set; } = string.Empty;

        public string RollNo { get; set; } = string.Empty;

        public int? ClassId { get; set; }

        public string ClassName { get; set; } = string.Empty;

        public int? SectionId { get; set; }

        public string SectionName { get; set; } = string.Empty;

        public int? BatchId { get; set; }

        public string BatchName { get; set; } = string.Empty;

        public decimal DueBalance { get; set; }
    }
    public class StudentFeeReceiptVM
    {
        public long StudentId { get; set; }

        public int ClassId { get; set; }

        public int BatchId { get; set; }


        // Student Information
        public string? StudentName { get; set; }

        public string? FatherName { get; set; }

        public string? ContactNo { get; set; }

        public string? RollNo { get; set; }


        // Total Due
        public decimal DueBalance { get; set; }


        // Fee Details
        public List<StudentFeeReceiptItemVM> TuitionFees { get; set; }
            = new();

        public List<StudentFeeReceiptItemVM> TransportFees { get; set; }
            = new();

        public List<StudentFeeReceiptItemVM> HostelFees { get; set; }
            = new();


        // Receipt Calculation
        public decimal TotalAmount { get; set; }

        public decimal TotalFees { get; set; }

        public decimal LateFee { get; set; }

        public string Concession { get; set; }

        public decimal ConcessionAmount { get; set; }

        public decimal NetFee { get; set; }

        public decimal ReceiptAmount { get; set; }

        public decimal BalanceAmount { get; set; }


        // Payment
        public int? PaymentModeId { get; set; }

        public string PaymentMode { get; set; } = "Cash";

        public string? Remark { get; set; }


        public DateTime ReceiptDate { get; set; }
            = DateTime.Now;


        public bool SelectAll { get; set; }
    }
    public class StudentFeeReceiptItemVM
    {
        public long FeeId { get; set; }

        public long FeePlanId { get; set; }

        public string FeeType { get; set; } = string.Empty;

        public string FeeHeadingName { get; set; } = string.Empty;

        public int Month { get; set; }

        public int Year { get; set; }

        public decimal Amount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal Balance { get; set; }

        public decimal CollectAmount { get; set; }

        public bool IsSelected { get; set; }

        public string PaymentStatus { get; set; } = string.Empty;

        public string FeeDescription { get; set; } = string.Empty;
    }
}
