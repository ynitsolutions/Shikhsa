using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;

namespace Shikhsa.Services
{
    public class LocalizationService
    {
        private readonly ApplicationDbContext _context;

        private readonly IHttpContextAccessor
            _httpContextAccessor;

        public LocalizationService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;

            _httpContextAccessor =
                httpContextAccessor;
        }

        public string Get(string key)
        {
            try
            {
                string langCode =
                    _httpContextAccessor
                    .HttpContext?
                    .Session
                    .GetString("LANGUAGE")
                    ?? "en";

                var value = _context.Translations
                    .Include(x => x.Language)
                    .FirstOrDefault(x =>
                        x.KeyName == key &&
                        x.Language.LanguageCode == langCode);

                return value?.TranslatedText ?? key;
            }
            catch
            {
                return key;
            }
        }
    }
}