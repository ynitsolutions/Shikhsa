using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shikhsa.Models
{
    public class Certificates
    {
        public long Id { get; set; }

        [Required, MaxLength(2)]
        public string Type { get; set; } = "tc"; // "tc" | "cc"

        public int? StudentId { get; set; }

        // Register details
        [MaxLength(50)]
        public string? BookNo { get; set; }

        [MaxLength(50)]
        public string? SerialNo { get; set; }

        [MaxLength(50)]
        public string? AdmissionNo { get; set; }

        // Student details
        [Required, MaxLength(150)]
        public string StudentName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? FatherName { get; set; }

        [MaxLength(150)]
        public string? MotherName { get; set; }

        [MaxLength(100)]
        public string? Nationality { get; set; } = "Indian";

        [MaxLength(50)]
        public string? Category { get; set; } // SC / ST / OBC / General

        public DateTime? Dob { get; set; }

        [MaxLength(150)]
        public string? DobInWords { get; set; }

        // Academic record (mainly for TC)
        public DateTime? FirstAdmissionDate { get; set; }

        [MaxLength(50)]
        public string? FirstAdmissionClass { get; set; }

        [MaxLength(50)]
        public string? LastClassFigures { get; set; }

        [MaxLength(100)]
        public string? LastClassWords { get; set; }

        [MaxLength(150)]
        public string? LastExamResult { get; set; }

        [MaxLength(150)]
        public string? FailedDetails { get; set; }

        // Comma-separated list, e.g. "English, Hindi, Maths, Science"
        [MaxLength(500)]
        public string? SubjectsStudied { get; set; }

        [MaxLength(10)]
        public string? QualifiedForPromotion { get; set; } // "Yes" / "No"

        [MaxLength(50)]
        public string? PromotedClass { get; set; }

        // Fees
        [MaxLength(100)]
        public string? FeePaidUpto { get; set; }

        [MaxLength(150)]
        public string? FeeConcession { get; set; }

        // Attendance - entered manually by the user on the builder page
        public int? TotalWorkingDays { get; set; }

        public int? TotalPresentDays { get; set; }

        // Activities
        [MaxLength(150)]
        public string? NccScoutGuide { get; set; }

        [MaxLength(500)]
        public string? ExtraCurricular { get; set; }

        [MaxLength(50)]
        public string? GeneralConduct { get; set; }

        // Dates
        public DateTime? ApplicationDate { get; set; }

        public DateTime? IssueDate { get; set; }

        // Leaving details (TC) - dropdown driven
        [MaxLength(150)]
        public string? ReasonForLeaving { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        // Character Certificate specific
        [MaxLength(150)]
        public string? PurposeOfIssue { get; set; }

        [MaxLength(500)]
        public string? CharacterRemarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Not mapped - handy for the print view
        [NotMapped]
        public double? AttendancePercent =>
            (TotalWorkingDays.HasValue && TotalWorkingDays > 0 && TotalPresentDays.HasValue)
                ? Math.Round((double)TotalPresentDays / TotalWorkingDays.Value * 100, 1)
                : null;
    }
}

