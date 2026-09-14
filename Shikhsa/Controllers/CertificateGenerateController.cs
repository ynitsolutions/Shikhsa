using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReactiveExtensionsSharp.Subjects;
using Shikhsa.Attributes;
using Shikhsa.Data;
using Shikhsa.DataBase.Repositry;
using Shikhsa.Helpers;
using Shikhsa.Models;
using Shikhsa.Models.Certificate;
using Shikhsa.Services;
using System.Reflection;
using System.Text.Json;

namespace Shikhsa.Controllers
{
    public class CertificateGenerateController : BaseController
    {
        private readonly CertificateTemplateRepository _templateRepository;
        private readonly GeneratedCertificateRepository _generatedRepository;
        private readonly ApplicationDbContext _context;
          private readonly UserManager<ApplicationUser> _userManager;

        // Placeholder categories treated as "fill in by hand every time you print"
        // (Total Working Days, Total Present Days, Reason for Leaving, General
        // Conduct, Remarks, Purpose of Issue, etc). Everything else (Student.*,
        // School.*, Parent.*) is auto-resolved by ResolveAutoValue() below.
        // Create a "Certificate" category on the existing Notification > Category
        // screen and add these fields as placeholders under it - they'll then
        // automatically show up on the Data Entry Form, no code change needed.
        private static readonly string[] ManualCategories =
{
    "Certificate"
};

        public CertificateGenerateController(
            CertificateTemplateRepository templateRepository, UserManager<ApplicationUser> userManager,
     PermissionService permissionService, EmailService emailService, LookupService lookup,
            GeneratedCertificateRepository generatedRepository,
            ApplicationDbContext context): base(userManager, permissionService, context, emailService, lookup)
        {
            _templateRepository = templateRepository;
            _generatedRepository = generatedRepository;
            _context = context;
        }

        // GET /CertificateGenerate/DataEntry?templateId=5&studentId=101
        [HttpGet]
        [SkipPermission]
        public async Task<IActionResult> DataEntry(long templateId, long? studentId = 1)
        {
            var template = await _templateRepository.GetByIdAsync(templateId);
            if (template == null) return NotFound();

            var tokens = TemplateMergeHelper.ExtractTokens(template.Body);

            var placeholders = await _context.NotificationPlaceholders
                .Include(p => p.NotificationCategory)
                .Where(p => p.IsActive)
                .ToListAsync();

            // Only the tokens actually used in this template's body, split into
            // "manual entry" vs "auto resolved" based on their category name.
            var usedPlaceholders = placeholders
                .Where(p => tokens.Contains($"{p.NotificationCategory.CategoryName}.{p.PlaceholderCode}"))
                .ToList();

            var manualFields = usedPlaceholders
                .Where(p => ManualCategories.Contains(p.NotificationCategory.CategoryName))
                .OrderBy(p => p.DisplayOrder)
                .ToList();

            ViewBag.Template = template;
            ViewBag.ManualFields = manualFields; // each has Token = "Certificate.TotalWorkingDays" style
            ViewBag.StudentId = studentId;

            return View();
        }

        // POST /CertificateGenerate/DataEntry
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SkipPermission]
        public async Task<IActionResult> DataEntry(long templateId, long? subjectId, Dictionary<string, string> manualValues)
        {
            var template = await _templateRepository.GetByIdAsync(templateId);
            if (template == null) return NotFound();

            var appliesTo = template.CertificateType?.AppliesTo ?? "Student";
            manualValues ??= new Dictionary<string, string>();

            // 1. Ek list banayein jisme saari entities store hongi (jaise SendInternalAsync me kiya hai)
            var entitiesList = new List<object>();

            // 2. Student ya Staff ki details fetch karke list me daalein
            if (subjectId.HasValue)
            {
                if (appliesTo == "Staff")
                {
                    var staff = await _context.StaffMasters.FindAsync(subjectId.Value);
                    if (staff != null)
                    {
                        entitiesList.Add(staff);
                    }
                }
                else // Student
                {
                    var student = await _context.Tbl_Students
                        .Include(x => x.Parent)
                        .Include(x => x.StudentRegistration)
                        .FirstOrDefaultAsync(x => x.StudentId == subjectId.Value);

                    if (student != null)
                    {
                        // 1. Main Student entity ko add karein
                        entitiesList.Add(student);

                        // 2. Agar Parent details maujood hain, to unhe bhi list me daalein
                        if (student.Parent != null)
                        {
                            entitiesList.Add(student.Parent);
                        }

                        // 3. Agar StudentRegistration details maujood hain, to unhe bhi list me daalein
                        if (student.StudentRegistration != null)
                        {
                            entitiesList.Add(student.StudentRegistration);
                        }
                    }
                }
            }

            // 3. School info fetch karke list me daalein
            var school = await GetSchoolInfo(); // Ensure this method is accessible in this controller
            if (school != null)
            {
                entitiesList.Add(school);
            }

            // 4. PlaceholderHelper ka use karke Auto-Resolved placeholders ki dictionary banayein
            var autoPlaceholders = PlaceholderHelper.CreateDictionary(entitiesList.ToArray());

            // 5. Final values dictionary taiyar karein (Pehle auto values daalein, fir manual se override karein)
            var finalValues = new Dictionary<string, string>();

            // Auto values ko map karein
            foreach (var item in autoPlaceholders)
            {
                finalValues[item.Key] = item.Value?.ToString() ?? "";
            }

            // Form se aayi hui manual values ko merge/override karein
            foreach (var kvp in manualValues)
            {
                finalValues[kvp.Key] = kvp.Value;
            }

            // 6. Template.Body ke placeholders ko real values se replace karein
            // Agar PlaceholderHelper.ReplacePlaceholders aapke paas hai to use karein, nahi to TemplateMergeHelper.Merge
            var finalHtml = PlaceholderHelper.ReplacePlaceholders(template.Body ?? "", finalValues);

            // 7. Database me save karein
            var generated = await _generatedRepository.SaveAsync(new GeneratedCertificate
            {
                CertificateTemplateId = templateId,
                SubjectId = subjectId,
                SubjectType = appliesTo,
                ManualValuesJson = JsonSerializer.Serialize(manualValues),
                FinalBodyHtml = finalHtml,
            });

            return RedirectToAction(nameof(Print), new { id = generated.GeneratedCertificateId });
        }

        // GET /CertificateGenerate/Print/17
        [HttpGet]
        [SkipPermission]
        public async Task<IActionResult> Print(long id)
        {
            var generated = await _generatedRepository.GetByIdAsync(id);
            if (generated == null) return NotFound();

            return View(generated);
        }
        [HttpGet]
        [SkipPermission]
        public async Task<IActionResult> GetIssuedDocuments(int subjectId, string subjectType)
        {
            var data = await _generatedRepository.GetIssuedDocumentsAsync(subjectId, subjectType);
            return PartialView("_IssuedDocumentsPartial", data);
        }
        // ------------------------------------------------------------------
        // TODO: wire these up to your real Student / School data.
        // Token examples: "Student.FullName", "Student.FatherName",
        // "School.SchoolName", "School.SchoolShortName".
        // ------------------------------------------------------------------
        [SkipPermission]
        private string ResolveAutoValue(string token, object? subjectEntity, string appliesTo)
        {
            var parts = token.Split('.', 2);
            if (parts.Length != 2) return string.Empty;

            var category = parts[0];
            var field = parts[1];

            if (category == "School")
            {
                // TODO: replace with your real school-settings lookup
                return field switch
                {
                    "SchoolName" => "Good Samaritan School",
                    "SchoolShortName" => "GSS",
                    "Address" => "Near Sector-8, Jasola, New Delhi-25",
                    "SchoolId" => "1925346",
                    "AffiliationNo" => "2730569",
                    _ => string.Empty,
                };
            }

            if ((category == "Student" && appliesTo == "Student") ||
                (category == "Staff" && appliesTo == "Staff"))
            {
                return GetPropertyValue(subjectEntity, field);
            }

            if (category == "Parent")
            {
                // TODO: Parent/Guardian usually lives on the Student record itself
                // (e.g. Student.FatherName) or a separate linked table - once you
                // confirm which, either fold it into the "Student" reflection
                // above (rename the placeholder category to "Student") or add a
                // dedicated lookup here.
                return string.Empty;
            }

            return string.Empty;
        }
        [SkipPermission]
        private static string GetPropertyValue(object? entity, string propertyName)
        {
            if (entity == null) return string.Empty;

            var prop = entity.GetType().GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (prop == null) return string.Empty;

            var value = prop.GetValue(entity);

            return value switch
            {
                null => string.Empty,
                DateTime dt => dt.ToString("dd MMM yyyy"),
                _ => value.ToString() ?? string.Empty,
            };
        }
    }
}

