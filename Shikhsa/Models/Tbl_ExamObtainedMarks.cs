using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace Shikhsa.Models
{
    public class Tbl_ExamObtainedMarks : BaseEntity
    {
        [Key]
        public long ExamObtainedMarkId { get; set; }

        [Required]
        public int BatchId { get; set; }

        [Required]
        public int ClassId { get; set; }

        [Required]
        public int SectionId { get; set; }

        [Required]
        public long StaffId { get; set; }

        [Required]
        public long StudentId { get; set; }

        [Required]
        public int ExamId { get; set; }          // ScholasticExam.Id

        [Required]
        public int SubjectId { get; set; }

        public decimal? ObtainedMarks { get; set; }

        public bool IsAbsent { get; set; }

        [StringLength(200)]
        public string? Remarks { get; set; }

        public bool IsFreeze { get; set; }

        // Navigation
        public virtual Batches? Batch { get; set; }
        public virtual Tbl_StudentsRegistrations? Student { get; set; }
        public virtual ScholasticExam? Exam { get; set; }
        public virtual SubjectMasters? Subject { get; set; }
    }
    public class StudentExamSummary : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        public int BatchId { get; set; }

        public int ClassId { get; set; }

        public int SectionId { get; set; }

        public int ExamCategoryId { get; set; }   // Term

        public long StudentId { get; set; }

        public string? Remarks { get; set; }

        public int? RankInClass { get; set; }

        public bool IsFreeze { get; set; }
        public virtual Tbl_Students Student { get; set; }

        public virtual ExamCategory ExamCategory { get; set; }

        public virtual Batches Batch { get; set; }
        public virtual DataListItem Class { get; set; }
        public virtual DataListItem Section { get; set; }

    }
    public class CoScholasticGrade : BaseEntity
    {
        [Key]
        public long GradeEntryId { get; set; }

        public int BatchId { get; set; }

        public int ClassId { get; set; }

        public int SectionId { get; set; }

        public int ExamCategoryId { get; set; }

        public long StudentId { get; set; }

        public long CoScholasticAreaId { get; set; }

        [StringLength(5)]
        public string Grade { get; set; } = "";
        public bool IsFreeze { get; set; } = false; 

        public Tbl_StudentsRegistrations Student { get; set; }

        public CoScholasticArea CoScholasticArea { get; set; }
    }
}
