using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models;
using Shikhsa.ViewModels;

namespace Shikhsa.Controllers
{
    public class LocalizationController : Controller
    {
        private readonly ApplicationDbContext _context;
        public LocalizationController(
           ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult ChangeLanguage(string lang)
        {
            if (string.IsNullOrWhiteSpace(lang))
            {
                lang = "en";
            }

            HttpContext.Session.SetString(
                "LANGUAGE",
                lang);

            string returnUrl =
                Request.Headers["Referer"].ToString();

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            return Redirect(returnUrl);
        }
        public async Task<IActionResult> Index()
        {
            var languages = await _context.Languages
                .Where(x => x.IsActive)
                .ToListAsync();

            ViewBag.Languages = languages;


            var translationKeys = await _context.Translations
                .Select(x => x.KeyName)
                .Distinct()
                .ToListAsync();


            List<TranslationGridVM> model = new();

            foreach (var key in translationKeys)
            {
                TranslationGridVM vm =
                    new TranslationGridVM();

                vm.KeyName = key;

                foreach (var lang in languages)
                {
                    var value = await _context.Translations
                        .Where(x =>
                            x.KeyName == key &&
                            x.LanguageId == lang.LanguageId)
                        .Select(x => x.TranslatedText)
                        .FirstOrDefaultAsync();

                    vm.Translations.Add(
                        lang.LanguageCode,
                        value ?? "");
                }

                model.Add(vm);
            }

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Save(List<TranslationGridVM> model)
        {
            var languages = await _context.Languages
                .Where(x => x.IsActive)
                .ToListAsync();


            foreach (var item in model)
            {
                if (string.IsNullOrWhiteSpace(item.KeyName))
                    continue;


                foreach (var lang in languages)
                {
                    string value =
                        item.Translations.ContainsKey(
                            lang.LanguageCode)
                        ? item.Translations[lang.LanguageCode]
                        : "";


                    var existing = await _context.Translations
                        .FirstOrDefaultAsync(x =>
                            x.KeyName == item.KeyName &&
                            x.LanguageId == lang.LanguageId);

                    if (existing == null)
                    {
                        _context.Translations.Add(
                            new Translation
                            {
                                KeyName = item.KeyName,
                                LanguageId = lang.LanguageId,
                                TranslatedText = value
                            });
                    }
                    else
                    {
                        existing.TranslatedText = value;
                    }
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Saved Successfully";

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Languages()
        {
            var data = await _context.Languages
                .OrderBy(x => x.LanguageName)
                .ToListAsync();

            return View(data);
        }
        [HttpPost]
        public async Task<IActionResult> SaveLanguage(List<Language> model)
        {
            foreach (var item in model)
            {
                // ============================================
                // VALIDATION
                // ============================================

                if (string.IsNullOrWhiteSpace(
                    item.LanguageName))
                    continue;

                if (string.IsNullOrWhiteSpace(
                    item.LanguageCode))
                    continue;


                // ============================================
                // UPDATE
                // ============================================

                if (item.LanguageId > 0)
                {
                    var existing =
                        await _context.Languages
                        .FirstOrDefaultAsync(x =>
                            x.LanguageId ==
                            item.LanguageId);

                    if (existing != null)
                    {
                        existing.LanguageName =
                            item.LanguageName;

                        existing.LanguageCode =
                            item.LanguageCode;

                        existing.IsDefault =
                            item.IsDefault;

                        existing.IsActive =
                            item.IsActive;

                        existing.UpdatedDate =
                            DateTime.Now;
                        existing.UpdatedBy =
                            User.Identity.Name;

                    }
                }

                // ============================================
                // INSERT
                // ============================================

                else
                {
                    _context.Languages.Add(
                        new Language
                        {
                            LanguageName =
                                item.LanguageName,

                            LanguageCode =
                                item.LanguageCode,

                            IsDefault =
                                item.IsDefault,

                            IsActive =
                                item.IsActive,

                            AddedDate =
                                DateTime.Now,
                            AddedBy =User.Identity.Name
                        });
                }
            }


            // ============================================
            // ONLY ONE DEFAULT LANGUAGE
            // ============================================

            var defaultLanguages =
                await _context.Languages
                .Where(x => x.IsDefault)
                .ToListAsync();

            if (defaultLanguages.Count > 1)
            {
                bool first = true;

                foreach (var lang in defaultLanguages)
                {
                    if (first)
                    {
                        first = false;
                        continue;
                    }

                    lang.IsDefault = false;
                }
            }


            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Languages Saved Successfully";

            return RedirectToAction("Index");
        }
        // =====================================================
        // INACTIVE
        // =====================================================

        public async Task<IActionResult> Inactive(int id)
        {
            var data = await _context.Languages
                .FirstOrDefaultAsync(x =>
                    x.LanguageId == id);

            if (data != null)
            {
                data.IsActive = false;

                data.UpdatedDate =
                    DateTime.Now;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }



        // =====================================================
        // ACTIVE
        // =====================================================

        public async Task<IActionResult> Active(int id)
        {
            var data = await _context.Languages
                .FirstOrDefaultAsync(x =>
                    x.LanguageId == id);

            if (data != null)
            {
                data.IsActive = true;

                data.UpdatedDate =
                    DateTime.Now;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}