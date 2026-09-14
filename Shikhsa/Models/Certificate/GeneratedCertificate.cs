using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shikhsa.Models.Certificate
{
    public class GeneratedCertificate:BaseEntity
    {
        public long GeneratedCertificateId { get; set; }

        public long CertificateTemplateId { get; set; }
        public CertificateTemplate CertificateTemplate { get; set; } = null!;
        [NotMapped]
        public string TemplateName { get; set; }
        // Who the certificate was generated for. SubjectType follows the
        // template's CertificateType.AppliesTo ("Student" or "Staff") so the
        // same table works for a Transfer Certificate AND a Staff Experience
        // Certificate.
        // TODO: point this at your real Students/Staff tables if you want a
        // proper FK - kept loose here since both live in different tables.
        public long? SubjectId { get; set; }
        [MaxLength(10)]
        public string? SubjectType { get; set; } // "Student" | "Staff"
        // JSON dump of the manual field values the user typed on the Data Entry
        // Form (Total Working Days, Present Days, Reason for Leaving, etc.) -
        // kept for audit / re-print without re-typing.
        public string? ManualValuesJson { get; set; }

        // The final HTML after every {{Category.PlaceholderCode}} token has
        // been replaced with either an auto-resolved student/school value or
        // a manually entered one. This is what actually gets printed.
        [Required]
        public string FinalBodyHtml { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
    public class SaveResult
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public long Id { get; set; }
    }
}
