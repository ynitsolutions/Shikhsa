using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shikhsa.Models
{

        public class Tbl_StudentsRegistrations:BaseEntity
        {
            [Key]
            public long StudentId { get; set; }

            public string? ApplicationNo { get; set; }

            [Required]
            public string? FirstName { get; set; }

            public string? MiddleName { get; set; }

            [Required]
            public string? LastName { get; set; }

            [Required]
            public DateTime DOB { get; set; }

            public string? Email { get; set; }

            public string? ContactNo { get; set; }

            public string? LastClass { get; set; }

            public string? AadhaarNumber { get; set; }

            public string? APAARId { get; set; }

            public string? PENNumber { get; set; }

            [Required]
            public string? LocalAddress { get; set; }

            [Required]
            public string? PermanentAddress { get; set; }
        [MapName("Category", nameof(CategoryName))]
        public int? CategoryId { get; set; }
        [MapName("Gender", nameof(GenderName))]
        public int? GenderId { get; set; }
        [MapName("Religion", nameof(ReligionName))]
        public int? ReligionId { get; set; }

            public bool IsHandicap { get; set; }

            public string? HandicapDetails { get; set; }

            public string? IdentificationMark { get; set; }
        [MapName(typeof(Batches), nameof(RegistrationBatchName), lookupKeyProperty: "BatchId", lookupNameProperty: "BatchName")]
        public int? AdmissionBatchId { get; set; }

            public bool? IsInitialClassAdmission { get; set; }
         [MapName("Class", nameof(ClassName))]
        public int? RegClassId { get; set; }
            public long? ParentId { get; set; }
        [MapName("Status", nameof(StatusName))]
        public int? Status { get; set; }

            public bool IsTranspot { get; set; }
            public bool IsHostel { get; set; }
        [MapName("Transport", nameof(TransportName))]
        public int? TranspotId { get; set; }
        [MapName("Hostel", nameof(HostelName))]
        public int? HostelId { get; set; }
        [NotMapped]
        public string? SectionName { get; set; }
        [NotMapped]
        public string? GenderName { get; set; }
        [NotMapped]
        public string? ClassName { get; set; }
        [NotMapped]
        public string? CategoryName { get; set; }
        [NotMapped]
        public string? ReligionName { get; set; }
        [NotMapped]
        public string? CurrentBatchName { get; set; }
        [NotMapped]
        public string? RegistrationBatchName { get; set; }
        [NotMapped]
        public string? StatusName { get; set; }
        [NotMapped]
        public string? HostelName { get; set; }
        [NotMapped]
        public string? TransportName { get; set; }
        [ForeignKey("ParentId")]
            public virtual Tbl_Parents? Parent { get; set; }

        public virtual Tbl_StudentDocument? Document { get; set; }
        public virtual Tbl_PreviousSchoolRecord? PreviousSchoolRecord { get; set; }
        public virtual ICollection<Tbl_Students>? Students { get; set; }
    }
        public class Tbl_Parents:BaseEntity
        {
            [Key]
            public long ParentId { get; set; }

            public string? FatherFirstName { get; set; }
            public string? FatherMiddleName { get; set; }
            public string? FatherLastName { get; set; }

            public string? MotherFirstName { get; set; }
            public string? MotherMiddleName { get; set; }
            public string? MotherLastName { get; set; }

            public string? GuardianFirstName { get; set; }
            public string? GuardianMiddleName { get; set; }
            public string? GuardianLastName { get; set; }

            public string? FatherContactNo { get; set; }
            public string? MotherContactNo { get; set; }
            public string? GuardianContactNo { get; set; }

            public string? FatherAddress { get; set; }
            public string? MotherAddress { get; set; }
            public string? GuardianAddress { get; set; }

            public string? FatherEmail { get; set; }
            public string? MotherEmail { get; set; }
            public string? GuardianEmail { get; set; }
        }
        public class Tbl_StudentDocument:BaseEntity
        {
            [Key]
            public long DocumentId { get; set; }

            public long StudentId { get; set; }
        public string? ApplicationNo { get; set; }

        public string? DocumentType { get; set; }

            public string? FileName { get; set; }

            public string? FilePath { get; set; }

            public byte[]? FileData { get; set; }

            public DateTime? UploadDate { get; set; }
            [ForeignKey("StudentId")]
            public virtual Tbl_StudentsRegistrations Student { get; set; }
        // ApplicationNo based relationship
        public virtual Tbl_StudentsRegistrations? StudentRegistration { get; set; }
    }
        public class Tbl_PreviousSchoolRecord :BaseEntity
        {
            [Key]
            public long PreviousSchoolRecordId { get; set; }

            public long StudentId { get; set; }
        public string? ApplicationNo { get; set; }


        public string? LastSchoolClass { get; set; } = string.Empty;

            public string? LastSchoolName { get; set; } = string.Empty;

            public string? LastSchoolAddress { get; set; } = string.Empty;

            public string? LastSchoolCode { get; set; } = string.Empty;

            public string? LastSchoolBoard { get; set; } = string.Empty;

            public string? LastSchoolUDISECode { get; set; } = string.Empty;

            public string? ReasonForChange { get; set; } = string.Empty;
        [ForeignKey("StudentId")]
        public virtual Tbl_StudentsRegistrations Student { get; set; }
        // ApplicationNo based relationship
        public virtual Tbl_StudentsRegistrations? StudentRegistration { get; set; }
    }
    public class Tbl_Students : BaseEntity 
    {
        [Key]
        public long StudentId { get; set; }

        public string? ApplicationNo { get; set; }

        [Required]
        public string? FirstName { get; set; }

        public string? MiddleName { get; set; }

        [Required]
        public string? LastName { get; set; }

        [Required]
        public DateTime DOB { get; set; }

        public string? Email { get; set; }

        public string? ContactNo { get; set; }

        public string? LastClass { get; set; }

        public string? AadhaarNumber { get; set; }

        public string? APAARId { get; set; }

        public string? PENNumber { get; set; }

        [Required]
        public string? LocalAddress { get; set; }

        [Required]
        public string? PermanentAddress { get; set; }
        [MapName("Category", nameof(CategoryName))]
        public int? CategoryId { get; set; }
        [MapName("Gender", nameof(GenderName))]
        public int? GenderId { get; set; }
        [MapName("Religion", nameof(ReligionName))]
        public int? ReligionId { get; set; }

        public bool IsHandicap { get; set; }

        public string? HandicapDetails { get; set; }

        public string? IdentificationMark { get; set; }
        [MapName(typeof(Batches), nameof(RegistrationBatchName), lookupKeyProperty: "BatchId", lookupNameProperty: "AcademicYear")]
        public int? AdmissionBatchId { get; set; }

        public bool? IsInitialClassAdmission { get; set; }
        [MapName("Class", nameof(ClassName))]
        public int? AdmitClassId { get; set; }
        public long? ParentId { get; set; }
        [MapName("Status", nameof(StatusName))]
        public int? Status { get; set; }
        [MapName("Section", nameof(SectionName))]
        public int? AdmitSectionId { get; set; }
        [MapName(typeof(Batches), nameof(CurrentBatchName), lookupKeyProperty: "BatchId", lookupNameProperty: "AcademicYear")]
        public int? AdmitBatchId { get; set; }
        public string? UserId { get; set; }
        public bool IsTranspot { get; set; }
        public bool IsHostel { get; set; }
        [MapName("Transport", nameof(TransportName))]
        public int? TranspotId { get; set; }
        [MapName("Hostel", nameof(HostelName))]
        public int? HostelId { get; set; }
        public string? ScholarNumber { get; set; }
        public long StudentRegisterId { get; set; }

        [NotMapped]
        public string? SectionName { get; set; }
        [NotMapped]
        public string? GenderName { get; set; }
        [NotMapped]
        public string? ClassName { get; set; }
        [NotMapped]
        public string? CategoryName { get; set; }
        [NotMapped]
        public string? ReligionName { get; set; }
        [NotMapped]
        public string? CurrentBatchName { get; set; }
        [NotMapped]
        public string? RegistrationBatchName { get; set; }
        [NotMapped]
        public string? StatusName { get; set; }
        [NotMapped]
        public string? HostelName { get; set; }
        [NotMapped]
        public string? TransportName { get; set; }
        [ForeignKey("ParentId")]
        public virtual Tbl_Parents? Parent { get; set; }
        //public virtual Tbl_PreviousSchoolRecord? PreviousSchoolRecord { get; set; }
        public virtual Tbl_StudentsRegistrations? StudentRegistration { get; set; }
    }

}

