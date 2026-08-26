using Microsoft.AspNetCore.Mvc.Rendering;
using Shikhsa.Models;

namespace Shikhsa.ViewModels
{
    public class FeeReceiptRecordVM
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public string? Search { get; set; }

        public int? ClassId { get; set; }
        public int? BatchId { get; set; }
        public int? PaymentModeId { get; set; }

        public List<SelectListItem> Classes { get; set; } = new();
        public List<SelectListItem> Batches { get; set; } = new();
        public List<SelectListItem> PaymentModes { get; set; } = new();
    }


    public class FeeReceiptRecordItemVM
    {
        public long FeeReceiptId { get; set; }

        public string? ReceiptNumber { get; set; }

        public string? StudentName { get; set; }

        public string? ClassName { get; set; }

        public string? AcademicYear { get; set; }

        public string? FeeHeadings { get; set; }

        public string? PaymentMode { get; set; }

        public decimal TotalFee { get; set; }

        public decimal PaidAmount { get; set; }

        public DateTime ReceiptDate { get; set; }
    }


    public class FeeReceiptDataTableRequest
    {
        public int Draw { get; set; }

        public int Start { get; set; }

        public int Length { get; set; }

        public string? Search { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public int? ClassId { get; set; }

        public int? BatchId { get; set; }

        public int? PaymentModeId { get; set; }

        public string? OrderColumn { get; set; }

        public string? OrderDirection { get; set; }
    }


    public class FeeReceiptDataTableResponse
    {
        public int Draw { get; set; }

        public int RecordsTotal { get; set; }

        public int RecordsFiltered { get; set; }

        public List<FeeReceiptRecordItemVM> Data { get; set; } = new();
    }
    public class FeePaymentReportVM
    {
        // Filters
        public int? SelectedBatchId { get; set; }
        public int? SelectedClassId { get; set; }
        public int? SelectedSectionId { get; set; }
        public long? SelectedStudentId { get; set; }

        // Dropdowns
        public List<Batches> Batches { get; set; } = new();
        public List<DataListItem> Classes { get; set; } = new();
        public List<DataListItem> Sections { get; set; } = new();
        public List<StudentFeeStudentVM> Students { get; set; } = new();

        // Report Data
        public List<FeePaymentReportRowVM> Rows { get; set; } = new();

        // Selected Student Info (header ke liye)
        public StudentFeeStudentVM? SelectedStudent { get; set; }
    }

    public class FeePaymentReportRowVM
    {
        public string ReceiptNumber { get; set; } = "";
        public DateTime ReceiptDate { get; set; }
        public string FeeType { get; set; } = "";
        public string FeeHeadName { get; set; } = "";     // Fee Description
        public decimal TotalFee { get; set; }             // Amount
        public decimal AlreadyPaid { get; set; }          // PreviousPaidAmount
        public decimal PaidAmount { get; set; }           // Is receipt me collect hua
        public decimal LateFee { get; set; }
        public decimal Concession { get; set; }
        public decimal RemainingDue { get; set; }          // BalanceAmount (is fee item ka after payment)
    }
}