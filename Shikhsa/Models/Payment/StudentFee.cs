using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shikhsa.Models.Payment
{
    /* Student की actual generated fee + balance */
    public class StudentFee : BaseEntity
    {
        [Key]
        public long StudentFeeId { get; set; }

        public long StudentId { get; set; }
        public string? ApplicationNo { get; set; }

        public long FeeId { get; set; }

        public int ClassId { get; set; }

        public int BatchId { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }
        public string? FeeType { get; set; }
        // Original fee
        public decimal FeeAmount { get; set; }

        // Ab tak total kitna pay hua
        public decimal PaidAmount { get; set; }

        // Current remaining balance
        public decimal BalanceAmount { get; set; }

        public bool IsFullyPaid { get; set; }
        [NotMapped]
        public string? FeeHeadingName { get; set; }
        public virtual Tbl_Students? Student { get; set; }
        public virtual ICollection<FeeReceiptDetail> ReceiptDetails
        {
            get;
            set;
        } = new List<FeeReceiptDetail>();

        public virtual ICollection<PaymentTransactionFeeDetail> PaymentDetails
        {
            get;
            set;
        } = new List<PaymentTransactionFeeDetail>();

        public virtual ICollection<StudentFeeCreditAdjustment> CreditAdjustments
        {
            get;
            set;
        } = new List<StudentFeeCreditAdjustment>();

    }
    /* हर payment की receipt */
    public class FeeReceipt : BaseEntity
    {
        [Key]
        public long FeeReceiptId { get; set; }

        public long StudentId { get; set; }
        
        public string? ApplicationNo { get; set; }
        public int ClassId { get; set; }

        public int BatchId { get; set; }

        public int? PaymentModeId { get; set; }

        public DateTime ReceiptDate { get; set; } = DateTime.Now;

        // Receipt number
        public string? ReceiptNumber { get; set; }

        // Is transaction me actual kitna receive hua
        public decimal ReceiptAmount { get; set; }

        public decimal TotalFee { get; set; }

        public decimal LateFee { get; set; }

        public string? Concession { get; set; } = string.Empty;

        public decimal ConcessionAmount { get; set; }

        // Payment se pehle old balance
        public decimal OldBalance { get; set; }

        // Is receipt ke baad remaining balance
        public decimal BalanceAmount { get; set; }

        public decimal DueAmount { get; set; }

        // Actual paid amount
        public decimal PaidAmount { get; set; }
        public decimal FeeCollectedAmount { get; set; }

        public string? Remark { get; set; }

        public string? ApplicationNumber { get; set; }

        public string? BankName { get; set; }

        public string? CheckId { get; set; }

        public int CurrentYear { get; set; }

        // Online payment ke case me
        public long? PaymentTransactionId { get; set; }

        public virtual Tbl_Students? Student { get; set; }

        public virtual DataListItem? PaymentMode { get; set; }

        public virtual PaymentTransactionDetail? PaymentTransaction { get; set; }

        public virtual ICollection<FeeReceiptDetail> Details { get; set; }
            = new List<FeeReceiptDetail>();

        public virtual ICollection<StudentFeeCredit> Credits { get; set; }
            = new List<StudentFeeCredit>();
        [NotMapped]
        public string? BatchName { get; set; }


        [NotMapped]
        public string? AlreadyPaidAmount { get; set; }

        [NotMapped]
        public string? FeeType { get; set; }

        [NotMapped]
        public string? FeeDescription { get; set; }

        [NotMapped]
        public string? FeeMonth { get; set; }

        [NotMapped]
        public string? FeeYear { get; set; }

    }
    /* Receipt में कौन-सी fee/month पर कितना लगा */
    public class FeeReceiptDetail : BaseEntity
    {
        [Key]
        public long FeeReceiptDetailId { get; set; }

        public long FeeReceiptId { get; set; }

        public long StudentFeeId { get; set; }

        public long FeeId { get; set; }
        public string? FeeType { get; set; }
        public string? FeeDescription { get; set; }
        public decimal PreviousPaidAmount { get; set; }    // 👈 NEW: is receipt se PEHLE kitna paid tha
        public decimal PreviousBalanceAmount { get; set; } // 👈 NEW: is receipt se PEHLE kitna due tha
        public int Month { get; set; }

        public int Year { get; set; }

        // Original fee
        public decimal Amount { get; set; }

        // Is receipt se is fee par kitna paid
        public decimal PaidAmount { get; set; }

        // Is fee ka payment ke baad balance
        public decimal BalanceAmount { get; set; }

        // Previous advance se kitna adjust hua
        public decimal AdjustedAmount { get; set; }

        [ForeignKey(nameof(FeeReceiptId))]
        public virtual FeeReceipt? FeeReceipt { get; set; }


        [ForeignKey(nameof(StudentFeeId))]
        public virtual StudentFee? StudentFee { get; set; }
    }
    /* Payment/gateway transaction */
    public class PaymentTransactionDetail : BaseEntity
    {
        [Key]
        public long PaymentTransactionId { get; set; }

        public long StudentId { get; set; }
        public string? ApplicationNo { get; set; }
        public int? PaymentModeId { get; set; }

        public decimal Amount { get; set; }

        public DateTime TxnDate { get; set; } = DateTime.Now;

        // Pending / Success / Failed / Cancelled
        public string? TransactionStatus { get; set; }

        public string? TransactionError { get; set; }

        public string? TransactionId { get; set; }

        public string? TrackId { get; set; }

        public string? ReferenceNo { get; set; }

        public string? Type { get; set; }

        public string? Card { get; set; }

        public string? CardType { get; set; }

        public string? Member { get; set; }

        public string? PaymentId { get; set; }

        public string? ApplicationNumber { get; set; }

        // Gateway
        public string? GatewayName { get; set; }

        public string? GatewayOrderId { get; set; }

        public string? GatewayPaymentId { get; set; }

        public string? GatewaySignature { get; set; }

        public string? GatewayResponse { get; set; }

        public DateTime? PaymentCompletedOn { get; set; }

        public virtual Tbl_Students? Student { get; set; }

        public virtual DataListItem? PaymentMode { get; set; }

        public virtual ICollection<PaymentTransactionFeeDetail> FeeDetails
        { get; set; }
            = new List<PaymentTransactionFeeDetail>();

        public virtual ICollection<FeeReceipt> Receipts
        { get; set; }
            = new List<FeeReceipt>();
    }
    /* Transaction में कौन-सी fees शामिल थीं */
    public class PaymentTransactionFeeDetail : BaseEntity
    {
        [Key]
        public long PaymentTransactionFeeDetailId { get; set; }

        public long PaymentTransactionId { get; set; }

        public long StudentFeeId { get; set; }

        public long FeeId { get; set; }
        public string FeeType { get; set; } = "Tuition";
        public int Month { get; set; }

        public int Year { get; set; }

        public decimal FeeAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal AdjustedAmount { get; set; }

        public virtual PaymentTransactionDetail? PaymentTransaction { get; set; }

        public virtual StudentFee? StudentFee { get; set; }
    }
    /* Extra/advance payment */
    public class StudentFeeCredit : BaseEntity
    {
        [Key]
        public long FeeCreditId { get; set; }

        public long StudentId { get; set; }
        public string? ApplicationNo { get; set; }
        // जिस receipt से credit बना
        public long FeeReceiptId { get; set; }

        // Total credit generated
        public decimal CreditAmount { get; set; }

        // Next fees me kitna use ho chuka
        public decimal UsedAmount { get; set; }

        // Remaining credit
        public decimal BalanceAmount { get; set; }

        public bool IsFullyUsed { get; set; }

        public virtual Tbl_Students? Student { get; set; }

        public virtual FeeReceipt? FeeReceipt { get; set; }
    }
    /* Advance कहाँ adjust हुआ */
    public class StudentFeeCreditAdjustment : BaseEntity
    {
        [Key]
        public long FeeCreditAdjustmentId { get; set; }

        public long FeeCreditId { get; set; }

        public long StudentFeeId { get; set; }

        public long FeeReceiptId { get; set; }

        public decimal AdjustedAmount { get; set; }

        public DateTime AdjustmentDate { get; set; } = DateTime.Now;
        // Tuition / Transport / Hostel
        public string FeeType { get; set; } = "Tuition";


        // Actual Fee Plan ID
        public long FeeId { get; set; }
        public virtual StudentFeeCredit? FeeCredit { get; set; }

        public virtual StudentFee? StudentFee { get; set; }

        public virtual FeeReceipt? FeeReceipt { get; set; }
    }
}
