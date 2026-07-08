using Shikhsa.Models.Notification;
using System.ComponentModel.DataAnnotations;

namespace Shikhsa.ViewModels
{
    public class BatchVM
    {
        public int BatchId { get; set; }

        [Required]
        [Display(Name = "Academic Year")]
        public string AcademicYear { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Display(Name = "Registration")]
        public bool ActiveForRegistration { get; set; }

        [Display(Name = "Admission")]
        public bool ActiveForAdmission { get; set; }

        [Display(Name = "Payment")]
        public bool ActiveForPayment { get; set; }

        [Display(Name = "Current Year")]
        public bool IsCurrentYear { get; set; }
    }
    public class NotificationPlaceholderVM
    {
        public NotificationPlaceholder Placeholder { get; set; } = new();

        public List<NotificationPlaceholder> Placeholders { get; set; } = new();

        public List<NotificationCategory> Categories { get; set; } = new();
    }
}
