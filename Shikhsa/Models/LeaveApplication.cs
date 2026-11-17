using Shikhsa.Models.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shikhsa.Models
{
    

    public enum LeaveStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }

    public class LeaveApplication:BaseEntity
    {
        [Key]
        public int LeaveId { get; set; }

        [Required]
        public int ApplicantId { get; set; } // StudentId or StaffId

        [Required]
       
        public string ApplicantType { get; set; } = string.Empty; // "Student", "Staff", "Admin"

        public string? LeaveTypeName { get; set; } // e.g., "Casual Leave", "Sick Leave"
        public bool IsPaidLeave { get; set; } = false;

        // Rule 2: For Student (Subject line)
        [MaxLength(200)]
        public string? LeaveSubject { get; set; }

        [Required]
        [Range(0.5, 365)]
        public decimal NoOfDays { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        // Rule 2: Text area preserving exact email/application formatting
        [Required]
        public string ApplicationBody { get; set; } = string.Empty;

        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        // Used for routing Student leaves to their Class Teacher
        public int? ClassId { get; set; }
        [Required]
        public int BatchId { get; set; }
        [NotMapped]
        public string AcademicYear { get; set; } = "2026-2027";
        [MaxLength(20)]
        public string? LeaveTypeCode { get; set; }
        public DateTime AppliedDate { get; set; } = DateTime.Now;

        public int? ActionTakenById { get; set; }
        public DateTime? ActionDate { get; set; }
        public string? ApproverRemarks { get; set; }
        [NotMapped]
        public string ApplicantName { get; set; } = string.Empty;

        [NotMapped]
        public bool IsTodayOrPending => Status == LeaveStatus.Pending || AppliedDate.Date == DateTime.Today;
    }
}