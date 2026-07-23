using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    public class CoScholasticArea:BaseEntity
    {
        [Key]
        public long CoScholasticAreaId { get; set; }
        public long CoScholasticId { get; set; }
       
        [Required(ErrorMessage = "Please select a class")]
        [Display(Name = "Class")]
        public int ClassId { get; set; }
        [NotMapped]
        public string? ClassName { get; set; }
        //
        [ForeignKey(nameof(CoScholasticId))]
        public virtual CoScholastic CoScholastic { get; set; }
    }
    public class CoScholastic : BaseEntity
    {
        [Key]
        public long CoScholasticId { get; set; }

        [Required(ErrorMessage = "Co-Scholastic Title is required")]
        [StringLength(200)]
        [Display(Name = "Co-Scholastic Title")]
        public string Title { get; set; } = string.Empty;
        [Required(ErrorMessage = "Subject name in regional language is required")]
        [StringLength(200)]
        [Display(Name = "Subject Name (Regional Language)")]
        [Column(TypeName = "nvarchar(max)")]
        public string SubjectNameInLanguage { get; set; } = string.Empty;

    }
}
