
using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shikhsa.Models
{
    public class ClassTeacherSubjectAssignment : BaseEntity
    {
        [Key]
        public long AssignmentId { get; set; }

        public int BatchId { get; set; }

        public int SectionId { get; set; }

        public int ClassId { get; set; }

        public long StaffId { get; set; }

        public int SubjectId { get; set; }

        // public bool IsClassTeacher { get; set; }

        #region Navigation

        [ForeignKey(nameof(BatchId))]
        public virtual Batches Batch { get; set; }

        [ForeignKey(nameof(StaffId))]
        public virtual StaffMaster Staff { get; set; }

        [ForeignKey(nameof(SubjectId))]
        public virtual SubjectMasters Subject { get; set; }

        #endregion
    }
    public class ClassTeacher : BaseEntity
    {
        [Key]
        public long ClassTeacherId { get; set; }
        public int BatchId { get; set; }
        public int ClassId { get; set; }
        public int SectionId { get; set; }
        public long StaffId { get; set; }
    }
}
