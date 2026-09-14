using Shikhsa.Models.Common;
using Shikhsa.Models.Notification;
using System.ComponentModel.DataAnnotations;

namespace Shikhsa.Models.Certificate
{
    public class CertificateTemplate:BaseEntity
    {
        public long CertificateTemplateId { get; set; }

        [Required, MaxLength(150)]
        public string TemplateName { get; set; } = string.Empty;

        [Required, MaxLength(80)]
        public string TemplateCode { get; set; } = string.Empty; // e.g. TC_STANDARD, CC_STANDARD
        public long CertificateTypeId { get; set; }
        public CertificateTypeMaster? CertificateType { get; set; } = null!;
        // Rich HTML body containing {{Category.PlaceholderCode}} tokens,
        // authored in the same WYSIWYG editor used for Notification Templates.
        [Required]
        public string Body { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;



        // Many-to-many against the SAME NotificationCategory table used by
        // Notification Templates - reused as-is, no new category table.
        public ICollection<CertificateTemplateCategory> CertificateTemplateCategories { get; set; }
            = new List<CertificateTemplateCategory>();
    }

    // Join entity - same shape as NotificationTemplateCategory, pointed at CertificateTemplate.
    public class CertificateTemplateCategory
    {
        public long CertificateTemplateCategoryId { get; set; }

        public long CertificateTemplateId { get; set; }
        public CertificateTemplate CertificateTemplate { get; set; } = null!;

        public long NotificationCategoryId { get; set; }
        public NotificationCategory NotificationCategory { get; set; } = null!;
    }
}
