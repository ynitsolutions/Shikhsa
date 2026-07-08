using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace Shikhsa.Models
{
    public class GradingCriteria:BaseEntity
    {
        [Key]
        public int GradingCriteriaId { get; set; }

        public int TermId { get; set; }

        public int ClassId { get; set; }

        public int BatchId { get; set; }

        public decimal MinPercentage { get; set; }

        public decimal MaxPercentage { get; set; }

        [Required]
        public string Grade { get; set; }

        public string Description { get; set; }

       

        public virtual ExamCategory Term { get; set; }

        public virtual DataListItem Class { get; set; }

        public virtual Batches Batch { get; set; }
    }
}
