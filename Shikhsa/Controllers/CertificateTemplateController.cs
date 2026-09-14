using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.DataBase.Repositry;
using Shikhsa.Models.Certificate;

namespace Shikhsa.Controllers
{
    public class CertificateTemplateController : Controller
    {
        private readonly CertificateTemplateRepository _repository;
        private readonly CertificateTypeRepository _typeRepository;
        private readonly ApplicationDbContext _context;

        public CertificateTemplateController(CertificateTemplateRepository repository, ApplicationDbContext context, CertificateTypeRepository typeRepository)
        {
            _repository = repository;
            _context = context;
            _typeRepository = typeRepository;
        }

        // GET /CertificateTemplate/CertificateTemplate  (list)
        public async Task<IActionResult> CertificateTemplate()
        {
            var data = await _repository.GetAllAsync();
            ViewBag.CertificateTypes = await _typeRepository.GetAllAsync(activeOnly: true);
            return View(data);
        }

        // GET /CertificateTemplate/SaveCertificateTemplate?id=0
        public async Task<IActionResult> SaveCertificateTemplate(long id = 0, long typeId = 0)
        {
            CertificateTemplate model;

            if (id == 0)
            {
                model = new CertificateTemplate { CertificateTypeId = typeId };
            }
            else
            {
                model = await _repository.GetByIdAsync(id);
                if (model == null)
                {
                    return NotFound();
                }

                ViewBag.SelectedCategories = model.CertificateTemplateCategories
                    .Select(x => x.NotificationCategoryId)
                    .ToList();
            }

            await LoadLookupsAsync();
            return View(model);
        }

        // POST /CertificateTemplate/SaveCertificateTemplate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCertificateTemplate(CertificateTemplate model, long[] categoryIds)
        {
            if (!ModelState.IsValid)
            {
                // DEBUG: ModelState ke saare validation errors nikalo
                foreach (var item in ModelState)
                {
                    var key = item.Key;

                    foreach (var error in item.Value.Errors)
                    {
                        var errorMessage = error.ErrorMessage;

                        // Agar ErrorMessage empty hai to exception dekho
                        if (string.IsNullOrWhiteSpace(errorMessage) && error.Exception != null)
                        {
                            errorMessage = error.Exception.Message;

                            if (error.Exception.InnerException != null)
                            {
                                errorMessage += " | Inner: " +
                                                error.Exception.InnerException.Message;
                            }
                        }

                        Console.WriteLine(
                            $"MODELSTATE ERROR => Field: {key} | Error: {errorMessage}"
                        );
                    }
                }

                await LoadLookupsAsync();

                ViewBag.SelectedCategories =
                    categoryIds?.ToList() ?? new List<long>();

                return View("SaveCertificateTemplate", model);
            }
            
            var result = await _repository.SaveAsync(model, User.Identity!.Name!);
            model.CertificateTemplateId = result.Id;

            if (result.Status == 1)
            {
                var oldMappings = _context.CertificateTemplateCategories
                    .Where(x => x.CertificateTemplateId == model.CertificateTemplateId);
                _context.CertificateTemplateCategories.RemoveRange(oldMappings);

                foreach (var categoryId in categoryIds ?? Array.Empty<long>())
                {
                    _context.CertificateTemplateCategories.Add(new CertificateTemplateCategory
                    {
                        CertificateTemplateId = model.CertificateTemplateId,
                        NotificationCategoryId = categoryId,
                    });
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(CertificateTemplate));
            }

            TempData["Error"] = result.Message;
            await LoadLookupsAsync();
            return View("SaveCertificateTemplate", model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _repository.DeleteAsync(id, User.Identity!.Name!);
            return Json(result);
        }

        // Same categories + placeholders already used by Notification Templates -
        // no separate certificate placeholder table needed.
        private async Task LoadLookupsAsync()
        {
            ViewBag.CertificateTypes = await _typeRepository.GetAllAsync(activeOnly: true);
            ViewBag.Categories = await _context.NotificationCategories
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            ViewBag.Placeholders = await _context.NotificationPlaceholders
                .Include(x => x.NotificationCategory)
                .Where(x => x.IsActive)
                .OrderBy(x => x.NotificationCategory.DisplayOrder)
                .ThenBy(x => x.DisplayOrder)
                .ToListAsync();
        }
        #region Document Type
        public async Task<IActionResult> CertificateType()
        {
            var data = await _typeRepository.GetAllAsync();
            return View(data);
        }

        // GET /CertificateType/SaveCertificateType?id=0
        public async Task<IActionResult> SaveCertificateType(long id = 0)
        {
            if (id == 0) return View(new CertificateTypeMaster());

            var model = await _typeRepository.GetByIdAsync(id);
            if (model == null) return NotFound();

            return View(model);
        }

        // POST /CertificateType/SaveCertificateType
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCertificateType(CertificateTypeMaster model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _typeRepository.SaveAsync(model);
            if (result.Status == 1)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(CertificateType));
            }

            TempData["Error"] = result.Message;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCertificateType(long id)
        {
            var result = await _typeRepository.DeleteAsync(id);
            return Json(result);
        }
    }
        #endregion
}



