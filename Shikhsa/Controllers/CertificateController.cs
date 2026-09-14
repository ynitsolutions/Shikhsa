using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Shikhsa.DataBase.Repositry;
using Shikhsa.Models;
using Shikhsa.ViewModels;

namespace Shikhsa.Controllers
{
    public class CertificateController : Controller
    {
        private readonly CertificateRepository _certificates;

        public CertificateController(CertificateRepository certificates)
        {
            _certificates = certificates;
        }

        // Dropdown options live here so the builder form and validation stay in sync.
        public static List<SelectListItem> ReasonOptions() => new()
        {
            new("--Select Reason--", ""),
            new("Transfer of Parent/Guardian", "Transfer of Parent/Guardian"),
            new("Completion of Course", "Completion of Course"),
            new("Shifting to another City", "Shifting to another City"),
            new("Ward's / Parent's Request", "Ward's / Parent's Request"),
            new("Discontinuation of Studies", "Discontinuation of Studies"),
            new("Admission to Another School", "Admission to Another School"),
            new("Personal Reason", "Personal Reason"),
            new("Other", "Other"),
        };

        public static List<SelectListItem> ConductOptions() => new()
        {
            new("--Select--", ""),
            new("Excellent", "Excellent"),
            new("Very Good", "Very Good"),
            new("Good", "Good"),
            new("Satisfactory", "Satisfactory"),
        };

        // GET /Certificate/{type}   e.g. /Certificate/tc  or /Certificate/cc
        [HttpGet("Certificate/{type}")]
        public async Task<IActionResult> Index(string type)
        {
            ValidateType("tc");
            var list = await _certificates.GetAllByTypeAsync(type);
            ViewBag.Type = type;
            return View(list);
        }

        // GET /Certificate/{type}/Create
        [HttpGet("Certificate/{type}/Create")]
        public IActionResult Create(string type)
        {
            ValidateType("tc");

            var vm = new CertificateBuilderViewModel
            {
                Type = type,
                Certificate = new Certificates { Type = type },
                Reasons = ReasonOptions(),
                Conducts = ConductOptions(),
            };

            return View(vm);
        }

        // POST /Certificate/{type}/Create
        [HttpPost("Certificate/{type}/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( Certificates certificates, string type = "tc")
        {
            ValidateType("tc");
            certificates.Type = type;

            if (string.IsNullOrWhiteSpace(certificates.StudentName))
            {
                ModelState.AddModelError(nameof(Certificates.StudentName), "Student name is required.");
            }

            if (!ModelState.IsValid)
            {
                var vm = new CertificateBuilderViewModel
                {
                    Type = type,
                    Certificate = certificates,
                    Reasons = ReasonOptions(),
                    Conducts = ConductOptions(),
                };
                return View(vm);
            }

            certificates.CreatedAt = DateTime.UtcNow;
            var saved = await _certificates.CreateAsync(certificates);

            return RedirectToAction(nameof(Print), new { id = saved.Id });
        }

        // GET /Certificate/Print/{id}
        [HttpGet("Certificate/Print/{id:int}")]
        public async Task<IActionResult> Print(int id)
        {
            var certificate = await _certificates.GetByIdAsync(id);
            if (certificate == null) return NotFound();

            ViewBag.Type = certificate.Type;
            return View(certificate);
        }

        // POST /Certificate/Delete/{id}
        [HttpPost("Certificate/Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var certificate = await _certificates.GetByIdAsync(id);
            if (certificate == null) return NotFound();

            var type = certificate.Type;
            await _certificates.DeleteAsync(id);

            return RedirectToAction(nameof(Index), new { type });
        }

        private static void ValidateType(string type)
        {
            if (type != "tc" && type != "cc")
            {
                throw new ArgumentException("Certificate type must be 'tc' or 'cc'.");
            }
        }
    }
}
