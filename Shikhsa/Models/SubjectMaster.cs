using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace Shikhsa.Models
{
    public class SubjectMasters : BaseEntity
    {
        [Key]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Subject Name is required")]
        public string SubjectName { get; set; }=string.Empty;

        public string? LanguageSubjectName { get; set; }

        public bool IsLanguageSubject { get; set; }

      
    }
}
