using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace Shikhsa.Models
{
    public class ExamCategory:BaseEntity
    {
        [Key]
        public int ExamCategoryId { get; set; }

        [Required]
        [StringLength(500)]
        public string ExamCategoryName { get; set; }

        [Required]
        [StringLength(200)]
        public string ShortName { get; set; }

        public int DisplayOrder { get; set; }

        public decimal Weightage { get; set; }

        public bool IncludeInFinalResult { get; set; }

        public bool IsMarksEntryAllowed { get; set; }


    }
}
