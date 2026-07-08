using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace Shikhsa.Models
{
    public class Batches:BaseEntity
    {
        [Key]
        public int BatchId { get; set; }

        [Required]
        [StringLength(20)]
        public string AcademicYear { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool ActiveForRegistration { get; set; }

        public bool ActiveForAdmission { get; set; }

        public bool ActiveForPayment { get; set; }

        public bool IsCurrentYear { get; set; }

    }
}
