using Shikhsa.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace Shikhsa.Models.Common
{
    public class EmailSettings
    {
        public string? Host { get; set; }
        public int Port { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public bool? EnableSsl { get; set; }
        public string? FromEmail { get; set; }
        public string? FromName { get; set; }
    }
    public class EmailLog
    {
        [Key]
        public long EmailLogId { get; set; }

        public string ModuleName { get; set; } = string.Empty;

        public long? ReferenceId { get; set; }

        public string ToEmail { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string? MessageId { get; set; }

        public EmailStatus Status { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime? SentOn { get; set; }

        public DateTime? DeliveredOn { get; set; }

        public DateTime? FailedOn { get; set; }

        public string? ErrorMessage { get; set; }

        public int RetryCount { get; set; }

        public bool IsActive { get; set; } = true;
    }
    public class SendEmailRequest
    {
        public string ModuleName { get; set; }

        public long? ReferenceId { get; set; }

        public string ToEmail { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }
    }
}
