using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shikhsa.Models.Notification
{
    public class NotificationTemplateCategory
    {
        [Key]
        public long NotificationTemplateCategoryId { get; set; }

        [Required]
        public long NotificationTemplateId { get; set; }

        [Required]
        public long NotificationCategoryId { get; set; }

        public int DisplayOrder { get; set; }

        [ForeignKey(nameof(NotificationTemplateId))]
        public virtual NotificationTemplate? NotificationTemplate { get; set; }

        [ForeignKey(nameof(NotificationCategoryId))]
        public virtual NotificationCategory? NotificationCategory { get; set; }
    }
    public class NotificationCategory
    {
        [Key]
        public long NotificationCategoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Icon { get; set; }

        [StringLength(20)]
        public string? Color { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
        public virtual ICollection<NotificationTemplateCategory>? NotificationTemplateCategories { get; set; }
        public virtual ICollection<NotificationPlaceholder> NotificationPlaceholders { get; set; }
    = new List<NotificationPlaceholder>();
    }
    public class NotificationPlaceholder
    {
        [Key]
        public long NotificationPlaceholderId { get; set; }

        [Required]
        public long NotificationCategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string PlaceholderCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? SampleValue { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(NotificationCategoryId))]
        public virtual NotificationCategory? NotificationCategory { get; set; }
    }
    public class NotificationTemplate : BaseEntity
    {
        [Key]
        public long NotificationTemplateId { get; set; }

        [Required]
        [StringLength(100)]
        public string TemplateCode { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string TemplateName { get; set; } = string.Empty;

        [Required]
        public long NotificationCategoryId { get; set; }

        public NotificationChannel Channel { get; set; } = NotificationChannel.Email;

        [StringLength(250)]
        public string? Subject { get; set; }

        public string Body { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

       
        //[ForeignKey(nameof(NotificationCategoryId))]
        //public virtual NotificationCategory? NotificationCategory { get; set; }
        public virtual ICollection<NotificationTemplateCategory>? NotificationTemplateCategories { get; set; }
    }
    public class NotificationLog
    {
        [Key]
        public long NotificationLogId { get; set; }

        public long? NotificationTemplateId { get; set; }

        public long? ReferenceId { get; set; }

        [StringLength(250)]
        public string? ToAddress { get; set; }

        [StringLength(250)]
        public string? Subject { get; set; }

        public string? Body { get; set; }

        public NotificationChannel Channel { get; set; }

        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

        public string? ErrorMessage { get; set; }

        public int RetryCount { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime? SentOn { get; set; }
    }
    public enum NotificationChannel
    {
        Email = 1,
        Sms = 2,
        WhatsApp = 3
    }
    public enum NotificationStatus
    {
        Pending = 1,
        Sent = 2,
        Failed = 3
    }
}