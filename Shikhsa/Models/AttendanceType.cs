using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shikhsa.Models
{
    public class AttendanceType:BaseEntity
    {
        [Key]
        public int AttendanceTypeId { get; set; }

        [Required]
        [StringLength(10)]
        public string Code { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = "";
        public bool IsLeave { get; set; }
        public int DisplayOrder { get; set; }
        public string? Color { get; set; }

    }
    public class StaffAttendance : BaseEntity
    {
        [Key]
        public long AttendanceId { get; set; }

        [Required]
        public long StaffId { get; set; }

        [Required]
        public DateOnly AttendanceDate { get; set; }

        [Required]
        public int AttendanceTypeId { get; set; }

        [StringLength(300)]
        public string? Remarks { get; set; }
        [ForeignKey(nameof(StaffId))]
        public virtual StaffMaster Staff { get; set; }

        [ForeignKey(nameof(AttendanceTypeId))]
        public virtual AttendanceType AttendanceType { get; set; }
    }

}
