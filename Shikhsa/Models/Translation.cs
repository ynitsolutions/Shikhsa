using Shikhsa.Models.Common;

namespace Shikhsa.Models
{
    public class Translation:BaseEntity
    {
        public long TranslationId { get; set; }

        public string KeyName { get; set; }

        public int LanguageId { get; set; }

        public string TranslatedText { get; set; }

        public virtual Language Language { get; set; }
    }
}
