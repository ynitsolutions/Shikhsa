using Shikhsa.Models.Common;

namespace Shikhsa.Models
{
    //public class TuitionFeePlan
    //{
    //    public int Id { get; set; }
    //    public string FeeHeading { get; set; } = string.Empty;
    //    public string ClassName { get; set; } = string.Empty;
    //    public string Medium { get; set; } = string.Empty;
    //    public decimal FeeValue { get; set; }
    //    public string AcademicYear { get; set; } = string.Empty;
    //    public string? Batch { get; set; }
    //}
    //public class HostelFeePlan
    //{
    //    public int Id { get; set; }
    //    public string HostelFeeType { get; set; } = string.Empty;
    //    public string HostelName { get; set; } = string.Empty;
    //    public string RoomType { get; set; } = string.Empty;
    //    public string MealPlan { get; set; } = string.Empty;
    //    public decimal HostelFee { get; set; }
    //}
    //public class TransportFeePlan
    //{
    //    public int Id { get; set; }
    //    public string TransportFeeType { get; set; } = string.Empty;
    //    public string AcademicYear { get; set; } = string.Empty;
    //    public string TransportName { get; set; } = string.Empty;
    //    public decimal TransportFee { get; set; }
    //    public string TransportOption { get; set; } = string.Empty; // One Way / Two Way
    //}
    public class TransportFeePlan:BaseEntity
    {
        public long TransportFeePlanId { get; set; }

        public long FeeHeadingId { get; set; }          // FK -> FeeHeading (Transport Fee Type)
        public int TransportId { get; set; }            // FK -> your Transport/Route master table
        public string AcademicYear { get; set; } = string.Empty;
        public decimal TransportFee { get; set; }
        public string TransportOption { get; set; } = string.Empty; // "One Way" / "Two Way"
        public int? Batch { get; set; }


        // Navigation
        public FeeHeading? FeeHeading { get; set; }
    }
    public class HostelFeePlan : BaseEntity
    {
        public long HostelFeePlanId { get; set; }

        public long FeeHeadingId { get; set; }          // FK -> FeeHeading (Hostel Fee Type)
        public int HostelId { get; set; }               // FK -> your Hostel master table
        public string RoomType { get; set; } = string.Empty;
        public string MealPlan { get; set; } = string.Empty;
        public decimal HostelFee { get; set; }
        public int? Batch { get; set; }


        // Navigation
        public FeeHeading? FeeHeading { get; set; }
    }
    public class TuitionFeePlan:BaseEntity
    {
        public long TuitionFeePlanId { get; set; }

        public long FeeHeadingId { get; set; }          // FK -> FeeHeading (e.g. "New Admission")
        public int ClassId { get; set; }                // FK -> your Class master table
        public string Medium { get; set; } = string.Empty;
        public decimal FeeValue { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
        public int? Batch { get; set; }

    
        // Navigation
        public FeeHeading? FeeHeading { get; set; }
    }
}
