using Shikhsa.Models.Common;

namespace Shikhsa.Models
{
    public class Language: BaseEntity
    {
        public int LanguageId { get; set; }

        public string LanguageName { get; set; }

        public string LanguageCode { get; set; }
        public bool IsDefault { get; set; }

        // public bool IsActive { get; set; }
    }
}
