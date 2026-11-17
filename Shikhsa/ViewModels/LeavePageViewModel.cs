using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Shikhsa.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shikhsa.ViewModels
{
    public class LeavePageViewModel
    {
        public int LeaveId { get; set; }
        public int CurrentUserId { get; set; }
        public string CurrentRoleName { get; set; } = string.Empty; // e.g. "Student", "Staff", "Admin"
        public bool IsClassTeacher { get; set; }
        public int CurrentBatchId { get; set; }
        public string CurrentBatchName { get; set; } = string.Empty;
        public string CurrentAcademicYear { get; set; }=string.Empty;

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
        [BindNever] 
        public List<LeaveApplication> PendingApprovals { get; set; } = new();
        public string? LeaveTypeCode { get; set; } // Bound to Dropdown
        public List<SelectListItem> LeaveTypeDropdown { get; set; } = new();
        public bool IsStudent => CurrentRoleName.Equals("Student", StringComparison.OrdinalIgnoreCase);
        public bool IsAdmin => CurrentRoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                            || CurrentRoleName.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);
        public bool CanApprove => IsAdmin || IsClassTeacher;

    }

    // Helper DTO for displaying pending applications in the table with applicant names
    //public class LeaveApplicationApprovalDTO
    //{
    //    public int LeaveId { get; set; }
    //    public int ApplicantId { get; set; }
    //    public string ApplicantName { get; set; } = string.Empty;
    //    public string ApplicantType { get; set; } = string.Empty;
    //    public string? LeaveTypeName { get; set; }
    //    public bool IsPaidLeave { get; set; }
    //    public string? LeaveSubject { get; set; }
    //    public decimal NoOfDays { get; set; }
    //    public DateTime FromDate { get; set; }
    //    public DateTime ToDate { get; set; }
    //    public string ApplicationBody { get; set; } = string.Empty;
    //    public LeaveStatus Status { get; set; }
    //    public DateTime AppliedDate { get; set; }
    //    public bool IsTodayOrPending { get; set; }
    //}
}