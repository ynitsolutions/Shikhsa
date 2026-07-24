using Shikhsa.Models;
using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shikhsa.Models
{
    public class StudentAttendance : BaseEntity
    {
        [Key]
        public long AttendanceId { get; set; }

        public int BatchId { get; set; }

        public int ClassId { get; set; }

        public int SectionId { get; set; }

        public long StudentId { get; set; }

        public DateOnly AttendanceDate { get; set; }

        public int AttendanceTypeId { get; set; }

        [StringLength(250)]
        public string? Remark { get; set; }

        public bool IsFreeze { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual Tbl_Students Student { get; set; }

        [ForeignKey(nameof(AttendanceTypeId))]
        public virtual AttendanceType AttendanceType { get; set; }

        [ForeignKey(nameof(BatchId))]
        public virtual Batches Batch { get; set; }

        [ForeignKey(nameof(ClassId))]
        public virtual DataListItem Class { get; set; }

        [ForeignKey(nameof(SectionId))]
        public virtual DataListItem Section { get; set; }
    }
}