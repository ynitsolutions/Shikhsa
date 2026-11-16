using System;
using System.ComponentModel.DataAnnotations;

namespace Shikhsa.Models
{
    

    public enum LeaveStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }

    public class LeaveApplication
    {
        [Key]
        public int LeaveId { get; set; }

        [Required]
        public int ApplicantId { get; set; } // StudentId or StaffId

        [Required]
        [MaxLength(50)]
        public string ApplicantType { get; set; } = string.Empty; // "Student", "Staff", "Admin"

        // Rule 2: For Staff/Admin (Stored as string directly in table)
        [MaxLength(50)]
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
        public string AcademicYear { get; set; } = "2026-2027";

        public DateTime AppliedDate { get; set; } = DateTime.Now;

        public int? ActionTakenById { get; set; }
        public DateTime? ActionDate { get; set; }
        public string? ApproverRemarks { get; set; }
    }
}