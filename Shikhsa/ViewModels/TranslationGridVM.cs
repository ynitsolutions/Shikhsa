namespace Shikhsa.ViewModels
{
    public class TranslationGridVM
    {
        public string KeyName { get; set; }

        // KEY   = LanguageCode
        // VALUE = Translation

        // Example:
        // en => Name
        // hi => नाम
        // fr => Nom

        public Dictionary<string, string> Translations
        { get; set; } = new();
    }
}
