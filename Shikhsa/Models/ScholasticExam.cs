using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shikhsa.Models
{
    public class ScholasticExam:BaseEntity
    {
        public int Id { get; set; }

        [Required]
        public string ExamName { get; set; }

        public int SubjectId { get; set; }
        /// <summary>
        /// public string? SubjectIds { get; set; }
        /// </summary>

        [Required]
        public int ClassId { get; set; }
        [Required]
        public int ExamType { get; set; }

        [Required]
        public int ExamCategoryId { get; set; }

        public decimal MinMarks { get; set; }

        public decimal MaxMarks { get; set; }

        public int BatchId { get; set; }

        [NotMapped]
        public string? SubjectNames { get; set; }

        [NotMapped]
        public string? ClassName { get; set; }

        [NotMapped]
        public string? ExamCategoryName { get; set; }
        [NotMapped]
        public string? ExamTypeName { get; set; }

        [NotMapped]
        public string? BatchName { get; set; }
        public virtual SubjectMasters Subject { get; set; }
        public virtual Batches Batch { get; set; }
        public virtual ExamCategory ExamCategory { get; set; }
        public virtual DataListItem Class { get; set; }
        public virtual DataListItem ExamTypes { get; set; }
    }
}
