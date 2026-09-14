using System.ComponentModel.DataAnnotations;

namespace Shikhsa.Models.Certificate
{
    public class CertificateTypeMaster
    {
        [Key]
        public long CertificateTypeId { get; set; }

        [Required, MaxLength(100)]
        public string TypeName { get; set; } = string.Empty; // "Bonafide Certificate"

        [Required, MaxLength(40)]
        public string TypeCode { get; set; } = string.Empty; // "BONAFIDE"

        // Who this certificate is issued to - drives which placeholder
        // category (Student vs Staff) auto-resolves, and what the Data Entry
        // Form's person picker is labelled.
        [Required, MaxLength(10)]
        public string AppliesTo { get; set; } = "Student"; // "Student" | "Staff"

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }
    }
}
