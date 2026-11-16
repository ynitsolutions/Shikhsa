using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shikhsa.Models;

namespace Shikhsa.ViewModels
{
    public class LeavePageViewModel
    {
        public int CurrentUserId { get; set; }
        public UserType CurrentUserType { get; set; }
        public bool IsClassTeacher { get; set; }
        public string CurrentAcademicYear { get; set; }

        // --- Form Inputs ---
        public string? LeaveTypeName { get; set; } // Dropdown value for Staff
        public bool IsPaidLeave { get; set; }
        public string? LeaveSubject { get; set; } // Input for Student

        [Required(ErrorMessage = "Number of days is required")]
        public decimal NoOfDays { get; set; }

        [Required]
        public DateTime FromDate { get; set; } = DateTime.Today;

        [Required]
        public DateTime ToDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Application text is required")]
        public string ApplicationBody { get; set; } = string.Empty;

        // --- Lists for Tables ---
        // Rule 7: Current Academic Year History of logged-in user
        public List<LeaveApplication> MyLeaveHistory { get; set; } = new();

        // Rule 4, 6, 9: Applications waiting for Admin or Class Teacher approval
        public List<LeaveApplicationApprovalDTO> PendingApprovals { get; set; } = new();
    }

    // Helper DTO for displaying pending applications in the table with applicant names
    public class LeaveApplicationApprovalDTO
    {
        public int LeaveId { get; set; }
        public int ApplicantId { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
        public UserType ApplicantType { get; set; }
        public string? LeaveTypeName { get; set; }
        public bool IsPaidLeave { get; set; }
        public string? LeaveSubject { get; set; }
        public decimal NoOfDays { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string ApplicationBody { get; set; } = string.Empty;
        public LeaveStatus Status { get; set; }
        public DateTime AppliedDate { get; set; }
        public bool IsTodayOrPending { get; set; }
    }
}