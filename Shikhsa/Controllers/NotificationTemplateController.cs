using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Attributes;
using Shikhsa.Data;
using Shikhsa.Models.Notification;
using Shikhsa.ViewModels;

namespace Shikhsa.Controllers
{
    public class NotificationTemplateController : Controller
    {
        private readonly NotificationTemplateRepository _repository;
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _env;

        public NotificationTemplateController(
            NotificationTemplateRepository repository,
            ApplicationDbContext context, EmailService emailService, IWebHostEnvironment env) 
        {
            _repository = repository;
            _context = context;
            _emailService = emailService;
            _env = env;
        }

        public async Task<IActionResult> NotificationTemplate()
        {
            var data = await _repository.GetAllAsync();

            return View(data);
        }

        public async Task<IActionResult> SaveNotificationTemplate(long id = 0)
        {
            NotificationTemplate model;

            if (id == 0)
            {
                model = new NotificationTemplate();
            }
            else
            {
                model = await _repository.GetByIdAsync(id);
                ViewBag.SelectedCategories = model.NotificationTemplateCategories
                                  .Select(x => x.NotificationCategoryId)
                                  .ToList();
                if (model == null)
                    return NotFound();
            }

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

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveNotificationTemplate(NotificationTemplate model, long[] categoryIds)
        {
            if (!ModelState.IsValid)
            {
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
                ViewBag.SelectedCategories = model.NotificationTemplateCategories
                                  .Select(x => x.NotificationCategoryId)
                                  .ToList();
                return View("SaveNotificationTemplate", model);
            }
            Console.WriteLine(model.Body);
            string filePath = Path.Combine(_env.ContentRootPath, "BodyBeforeSave.html");
            System.IO.File.WriteAllText(filePath, model.Body);
            var result = await _repository.SaveAsync(model);
            model.NotificationTemplateId = Convert.ToInt64(result.Id);
            if (result.Status == 1)
            {
                var oldMappings = _context.NotificationTemplateCategories
                    .Where(x => x.NotificationTemplateId == model.NotificationTemplateId);

                _context.NotificationTemplateCategories.RemoveRange(oldMappings);

                foreach (var categoryId in categoryIds)
                {
                    _context.NotificationTemplateCategories.Add(
                        new NotificationTemplateCategory
                        {
                            NotificationTemplateId = model.NotificationTemplateId,
                            NotificationCategoryId = categoryId
                        });
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = result.Message;

                return RedirectToAction(nameof(NotificationTemplate));
            }

            TempData["Error"] = result.Message;

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

            return View("SaveNotificationTemplate", model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _repository.DeleteAsync(id);

            return Json(result);
        }
        [SkipPermission]
        [HttpPost]
        public async Task<IActionResult> SendTestEmail(string email,string subject,string body)
        {
            var result = await _emailService.SendEmailAsync(
                "Test",
                null,
                email,
                subject,
                body);

            if (result)
            {
                return Json(new
                {
                    status = 1,
                    message = "Email sent successfully."
                });
            }

            return Json(new
            {
                status = 0,
                message = "Unable to send email."
            });
        }
        #region Category
        public async Task<IActionResult> NotificationCategory()
        {
            var list = await _repository.GetAllCategoryAsync();

            ViewBag.List = list;

            return View(new NotificationCategory());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveNotificationCategory(NotificationCategory model)
        {
            var result = await _repository.SaveCategoryAsync(model);

            TempData[result.Status == 1 ? "Success" : "Error"] = result.Message;

            return RedirectToAction(nameof(NotificationCategory));
        }

        [HttpGet]
        public async Task<IActionResult> SaveNotificationCategory(long id)
        {
            var model = await _repository.GetCategoryByIdAsync(id);
            var list = await _repository.GetAllCategoryAsync();

            ViewBag.List = list;
            if (model == null)
                return NotFound();

      

            return View("NotificationCategory", model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteNotificationCategory(long id)
        {
            var result = await _repository.DeleteAsync(id);

            return Json(result);
        }
        #endregion
        #region Placeholder
        public async Task<IActionResult> PlaceHolder(string search = "")
        {
            NotificationPlaceholderVM vm = new();
            
            vm.Placeholder = new();

            vm.Categories = await _repository.GetCategoriesAsync();

            vm.Placeholders = await _repository.GetPaceHolderAllAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                vm.Placeholders = vm.Placeholders
                    .Where(x =>
                        x.PlaceholderCode.ToLower().Contains(search) ||
                        x.DisplayName.ToLower().Contains(search) ||
                        x.NotificationCategory.CategoryName.ToLower().Contains(search))
                    .ToList();
            }

            ViewBag.Search = search;

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePlaceHolder(NotificationPlaceholderVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categories = await _repository.GetCategoriesAsync();

                vm.Placeholders = await _repository.GetPaceHolderAllAsync();

                return View("Index", vm);
            }

            var result = await _repository.SaveAsync(vm.Placeholder);

            TempData[result.Status == 1 ? "Success" : "Error"] = result.Message;

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> SavePlaceHolder(long id)
        {
            NotificationPlaceholderVM vm = new();

            vm.Placeholder = await _repository.GetPlaceHolderByIdAsync(id) ?? new();

            vm.Categories = await _repository.GetCategoriesAsync();

            vm.Placeholders = await _repository.GetPaceHolderAllAsync();

            return View("Index", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePlaceHolder(long id)
        {
            var result = await _repository.DeletePlaceHolderAsync(id);

            TempData[result.Status == 1 ? "Success" : "Error"] = result.Message;

            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}