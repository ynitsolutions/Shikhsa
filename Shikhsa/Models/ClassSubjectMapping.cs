using System.ComponentModel.DataAnnotations;

namespace Shikhsa.Models
{
    public class ClassBatchSubjectHeader
    {
        [Key]
        public long HeaderId { get; set; }

        public int ClassId { get; set; }

        public int BatchId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public virtual ICollection<ClassBatchSubjectDetail> Details
        {
            get;
            set;
        }
    }
    public class ClassBatchSubjectDetail
    {
        [Key]
        public long DetailId { get; set; }

        public long HeaderId { get; set; }

        public int SubjectId { get; set; }

        public virtual ClassBatchSubjectHeader Header
        {
            get;
            set;
        }
    }
}
