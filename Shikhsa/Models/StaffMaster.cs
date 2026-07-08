using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Shikhsa.Models.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace Shikhsa.Models
{
    [Table("StaffMaster")]
    public class StaffMaster:BaseEntity
    {
        [Key]
        public long StaffId { get; set; }

        public string? StaffCode { get; set; }

        #region Personal Details

        [Required]
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(100)]
        public string? LastName { get; set; }
        [NotMapped]
        public string? FullName
        {
            get
            {
                return $"{FirstName} {MiddleName} {LastName}"
                    .Replace("  ", " ")
                    .Trim();
            }
        }
        [Required]
        public DateTime DOB { get; set; }
        [NotMapped]
        public int Age
        {
            get
            {
                var age = DateTime.Today.Year - DOB.Year;

                if (DOB.Date > DateTime.Today.AddYears(-age))
                    age--;

                return age;
            }
        }
        public int? GenderId { get; set; }

        public int? BloodGroupId { get; set; }

        public int? MaritalStatusId { get; set; }

        public int? ReligionId { get; set; }

        public int? CategoryId { get; set; }

        public string? Nationality { get; set; }

        [Required]
        public string? Address { get; set; }

        public string? PermanentAddress { get; set; }

        public bool? SameAsCurrentAddress { get; set; }

        #endregion

        #region Contact

        [Required(ErrorMessage = "Mobile Number is required")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile Number must be 10 digits.")]
        [RegularExpression(@"^[6-9]\d{9}$",
        ErrorMessage = "Please enter a valid 10-digit mobile number.")]
        public string? MobileNo { get; set; } = string.Empty;

        public string? AlternateMobileNo { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        public string? AlternateEmail { get; set; }

        #endregion

        #region Family

        public string? FatherFirstName { get; set; }

        public string? FatherMiddleName { get; set; }

        public string? FatherLastName { get; set; }

        public string? MotherFirstName { get; set; }

        public string? MotherMiddleName { get; set; }

        public string? MotherLastName { get; set; }

        public string? SpouseName { get; set; }

        public string? SpouseMobile { get; set; }

        #endregion

        #region Identity
        [StringLength(12)]
        public string? AadhaarNumber { get; set; }

        public string? PANNumber { get; set; }

        public string? PassportNumber { get; set; }

        public string? DrivingLicenseNumber { get; set; }

        public string? UINNumber { get; set; }

        #endregion

        #region Employment

        public int? DepartmentId { get; set; }
                  
        public int? DesignationId { get; set; }
                  
        public int? StaffTypeId { get; set; }
                  
        public int? EmploymentTypeId { get; set; }

        public long? ReportingStaffId { get; set; }

        public int? ShiftId { get; set; }

        public DateTime JoiningDate { get; set; }

        public DateTime? ConfirmationDate { get; set; }

        public int? ProbationMonths { get; set; }

        public DateTime? LeavingDate { get; set; }

        public string? LeavingReason { get; set; }

        public int? EmployeeStatusId { get; set; }

        #endregion

    

        #region Bank

        public int? BankId { get; set; }

        public string? BranchName { get; set; }

        public string? IFSCCode { get; set; }

        public string? AccountNumber { get; set; }

        public string? AccountHolderName { get; set; }

        #endregion

        #region Government

        public string? UANNumber { get; set; }

        public string? ESICNumber { get; set; }

        public string? ProfessionalTaxNumber { get; set; }

        #endregion

        #region Login

        public string? UserId { get; set; }   // FK to AspNetUsers.Id
        public virtual ApplicationUser? User { get; set; }
        #endregion

        #region Files

        public string? PhotoPath { get; set; }

        public string? SignaturePath { get; set; }
        [NotMapped]
        [XmlIgnore]
        public IFormFile? PhotoFile { get; set; }

        [NotMapped]
        [XmlIgnore]
        public IFormFile? SignatureFile { get; set; }

        [NotMapped]
        [XmlIgnore]
        public IFormFile? AadhaarFile { get; set; }

        [NotMapped]
        [XmlIgnore]
        public IFormFile? PassbookFile { get; set; }
        #endregion



        // Navigation Properties
        [ValidateNever]
        [XmlArray("Academics")]
        [XmlArrayItem("Academic")]
        public virtual List<StaffAcademic>? Academics { get; set; } = new List<StaffAcademic>();
        [ValidateNever]
        [XmlArray("Experiences")]
        [XmlArrayItem("Experiences")]
        public virtual List<StaffExperience>? Experiences { get; set; } = new List<StaffExperience>();
        [ValidateNever]
        [XmlArray("EmergencyContacts")]
        [XmlArrayItem("EmergencyContacts")]
        public virtual List<StaffEmergencyContact>? EmergencyContacts { get; set; } = new List<StaffEmergencyContact>();
        [ValidateNever]
        [XmlArray("Documents")]
        [XmlArrayItem("Documents")]
        public virtual List<StaffDocument>? Documents { get; set; } = new List<StaffDocument>();
        [ValidateNever]
        [XmlElement("LeaveSetting")]
        public virtual StaffLeaveSetting? LeaveSetting { get; set; }
        [ValidateNever]
        [XmlArray("SalaryHistories")]
        [XmlArrayItem("SalaryHistories")]
        public virtual List<StaffSalaryHistory>? SalaryHistories { get; set; }
    = new List<StaffSalaryHistory>();

    }
    public class StaffAcademic:BaseEntity
    {
        [Key]
        public long AcademicId { get; set; }

        [Required]
        public long StaffId { get; set; }

        // Degree (10th, 12th, Graduation, B.Ed etc.)
        [Required]
        public int DegreeId { get; set; }          // DataListItemId
        public string? DegreeName { get; set; }
        // Science, Commerce, Arts etc.
        public int? StreamId { get; set; }         // DataListItemId
        public string? StreamName { get; set; }
        [Required]
        [StringLength(250)]
        public string? InstituteName { get; set; }

        [Required]
        [StringLength(250)]
        public string? UniversityName { get; set; }

        [Required]
        public int PassingYear { get; set; }

        [StringLength(50)]
        public string? RollNumber { get; set; }

        public decimal TotalMarks { get; set; }

        public decimal ObtainedMarks { get; set; }

        public decimal Percentage { get; set; }

        public int? GradeId { get; set; }          // DataListItemId
        public string? GradeName { get; set; }

        [StringLength(500)]
        public string? MarksheetFile { get; set; }

        public string? Remarks { get; set; }

       

        [ForeignKey(nameof(StaffId))]
        public virtual StaffMaster Staff { get; set; }
    }
    public class StaffExperience:BaseEntity
    {
        [Key]
        public long ExperienceId { get; set; }

        [Required]
        public long StaffId { get; set; }

        [Required]
        public int DesignationId { get; set; }      // DataListItemId
        public string? DesignationName { get; set; }
        public int? DepartmentId { get; set; }      // DataListItemId
        public string? DepartmentName { get; set; }
        [Required]
        [StringLength(250)]
        public string? OrganisationName { get; set; }

        [StringLength(150)]
        public string? SubjectName { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        // Calculated while saving
        public int TotalExperienceYears { get; set; }

        public int TotalExperienceMonths { get; set; }

        public int TotalExperienceDays { get; set; }

        public decimal LastDrawnSalary { get; set; }

        [StringLength(500)]
        public string? ExperienceLetterFile { get; set; }

        public string? Remarks { get; set; }

       

        [ForeignKey(nameof(StaffId))]
        public virtual StaffMaster Staff { get; set; }
    }

    public class StaffEmergencyContact:BaseEntity
    {
        [Key]
        public long EmergencyContactId { get; set; }

        [Required]
        public long StaffId { get; set; }

        [Required]
        [StringLength(150)]
        public string? ContactName { get; set; }

        // DataListItemId (Father, Mother, Brother, Sister, Spouse etc.)
        [Required]
        public int RelationshipId { get; set; }

        [Required]
        [StringLength(15)]
        public string? MobileNo { get; set; }

        [StringLength(15)]
        public string? AlternateMobileNo { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public bool IsPrimary { get; set; }

        public string? Remarks { get; set; }


        [ForeignKey(nameof(StaffId))]
        public virtual StaffMaster Staff { get; set; }
    }
    public class StaffDocument:BaseEntity
    {
        [Key]
        public long DocumentId { get; set; }

        [Required]
        public long StaffId { get; set; }

        // Aadhaar, PAN, Resume, Marksheet etc.
        [Required]
        public int DocumentTypeId { get; set; }
        public string? DocumentNumber { get; set; }
        [StringLength(100)]
        public string? DocumentTypeName { get; set; }

        [Required]
        [StringLength(500)]
        public string? FilePath { get; set; }

        [StringLength(255)]
        public string? OriginalFileName { get; set; }

        [StringLength(50)]
        public string? FileExtension { get; set; }

        public decimal FileSizeKB { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string? Remarks { get; set; }

    

        [ForeignKey(nameof(StaffId))]
        public virtual StaffMaster Staff { get; set; }
    }
    public class StaffLeaveSetting:BaseEntity
    {
        [Key]
        public long LeaveSettingId { get; set; }

        public long StaffId { get; set; }

        // DataListItemId
        public int LeaveGroupId { get; set; }

        public decimal CasualLeave { get; set; }

        public decimal SickLeave { get; set; }

        public decimal EarnedLeave { get; set; }

        public decimal MaternityLeave { get; set; }

        public decimal PaternityLeave { get; set; }

        public decimal CompOff { get; set; }

        public decimal LeaveCarryForward { get; set; }

        public decimal MaximumLeaveBalance { get; set; }

        // Shift (DataListItemId)
        public int? ShiftId { get; set; }

        [StringLength(50)]
        public string? BiometricDeviceId { get; set; }

        [StringLength(50)]
        public string? MachineUserId { get; set; }

        public bool AutoAttendance { get; set; }

        public bool OvertimeAllowed { get; set; }

        public decimal MaxOTHours { get; set; }

        public decimal OTRate { get; set; }

        public int GraceMinutes { get; set; }

        public int LateMarkAfterMinutes { get; set; }

        public int HalfDayAfterMinutes { get; set; }

        public bool EarlyExitAllowed { get; set; }

        public int WeeklyOffId { get; set; }

        public int AttendanceModeId { get; set; }

        public int DefaultAttendanceStatusId { get; set; }

       

        [ForeignKey(nameof(StaffId))]
        public virtual StaffMaster Staff { get; set; }
    }
    public class StaffActivityLog
    {
        [Key]
        public long ActivityLogId { get; set; }

        public long StaffId { get; set; }

        [Required]
        [StringLength(100)]
        public string? ModuleName { get; set; }

        [Required]
        [StringLength(100)]
        public string? ActivityName { get; set; }

        public string? Remarks { get; set; }

        [StringLength(100)]
        public string? IPAddress { get; set; }

        public long ActionBy { get; set; }

        public DateTime ActionDate { get; set; } = DateTime.Now;

        [ForeignKey(nameof(StaffId))]
        public virtual StaffMaster Staff { get; set; }
    }
    public class StaffPreviousEmployment:BaseEntity
    {
        [Key]
        public long PreviousEmploymentId { get; set; }

        [Required]
        public long StaffId { get; set; }

        [Required]
        [StringLength(250)]
        public string? OrganisationName { get; set; }

        // DataListItemId
        [Required]
        public int DesignationId { get; set; }

        // DataListItemId
        public int? DepartmentId { get; set; }

        [StringLength(150)]
        public string? SubjectName { get; set; }

        [StringLength(200)]
        public string? ReportingManager { get; set; }

        [StringLength(15)]
        public string? ManagerMobileNo { get; set; }

        [StringLength(200)]
        public string? ManagerEmail { get; set; }

        [Required]
        public DateTime JoiningDate { get; set; }

        [Required]
        public DateTime LeavingDate { get; set; }

        public decimal LastSalary { get; set; }

        [StringLength(500)]
        public string? LeavingReason { get; set; }

        [StringLength(500)]
        public string? ExperienceLetterFile { get; set; }

        public bool IsVerified { get; set; }

        public DateTime? VerificationDate { get; set; }

        [StringLength(500)]
        public string? VerificationRemarks { get; set; }

       
        [ForeignKey(nameof(StaffId))]
        public virtual StaffMaster Staff { get; set; }
    }
    public class StaffSalaryHistory:BaseEntity
    {
        [Key]
        public long SalaryHistoryId { get; set; }

        [Required]
        public long StaffId { get; set; }

        [Required]
        public DateTime EffectiveFrom { get; set; }

        public decimal BasicSalary { get; set; }

        public decimal HRA { get; set; }

        public decimal DA { get; set; }

        public decimal MedicalAllowance { get; set; }

        public decimal ConveyanceAllowance { get; set; }

        public decimal SpecialAllowance { get; set; }

        public decimal OtherAllowance { get; set; }

        public decimal GrossSalary { get; set; }

        public decimal PFDeduction { get; set; }

        public decimal ESICDeduction { get; set; }

        public decimal PTDeduction { get; set; }

        public decimal TDSDeduction { get; set; }

        public decimal OtherDeduction { get; set; }

        public decimal TotalDeduction { get; set; }

        public decimal NetSalary { get; set; }

        [StringLength(500)]
        public string? RevisionReason { get; set; }

        public long? ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public bool IsCurrentSalary { get; set; }

       

        [ForeignKey(nameof(StaffId))]
        public virtual StaffMaster Staff { get; set; }
    }
    public class StaffPromotionHistory:BaseEntity
    {
        [Key]
        public long PromotionHistoryId { get; set; }

        [Required]
        public long StaffId { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; }

        public int? OldDepartmentId { get; set; }

        public int? NewDepartmentId { get; set; }

        public int? OldDesignationId { get; set; }

        public int? NewDesignationId { get; set; }

        public decimal OldSalary { get; set; }

        public decimal NewSalary { get; set; }

        public long? OldReportingManagerId { get; set; }

        public long? NewReportingManagerId { get; set; }

        [StringLength(500)]
        public string? PromotionReason { get; set; }

        public long? ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }

        

        [ForeignKey(nameof(StaffId))]
        public virtual StaffMaster Staff { get; set; }
    }
    public class StaffTransferHistory:BaseEntity
    {
        [Key]
        public long TransferHistoryId { get; set; }

        [Required]
        public long StaffId { get; set; }

        [Required]
        public DateTime TransferDate { get; set; }

        public int? FromDepartmentId { get; set; }

        public int? ToDepartmentId { get; set; }

        [StringLength(250)]
        public string? FromLocation { get; set; }

        [StringLength(250)]
        public string? ToLocation { get; set; }

        public long? OldReportingManagerId { get; set; }

        public long? NewReportingManagerId { get; set; }

        [StringLength(500)]
        public string? TransferReason { get; set; }

        public long? ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }

       

        [ForeignKey(nameof(StaffId))]
        public virtual StaffMaster Staff { get; set; }
    }
    public class StaffAwardPunishment:BaseEntity
    {
        [Key]
        public long AwardPunishmentId { get; set; }

        [Required]
        public long StaffId { get; set; }

        [Required]
        public DateTime ActionDate { get; set; }

        // DataListItemId
        [Required]
        public int ActionTypeId { get; set; }

        [Required]
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public decimal? RewardAmount { get; set; }

        public decimal? PenaltyAmount { get; set; }

        [StringLength(500)]
        public string? AttachmentFile { get; set; }

        public long? ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }

        

        [ForeignKey(nameof(StaffId))]
        public virtual StaffMaster Staff { get; set; }
    }
}