using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shikhsa.Data;
using Shikhsa.Helpers;
using Shikhsa.Models;
using Shikhsa.Models.Common;
using Shikhsa.Repository;
using Shikhsa.Services;
using Shikhsa.ViewModels;

namespace Shikhsa.Controllers
{
    public class FeeHeadingController : BaseController
    {
        private readonly FeeHeadingRepository  _repository;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly EmailService _emailService;
        public FeeHeadingController(FeeHeadingRepository repository, RoleManager<ApplicationRole> roleManager, ApplicationDbContext context,
     UserManager<ApplicationUser> userManager,
     PermissionService permissionService, IWebHostEnvironment env, EmailService email) :base(userManager, permissionService, context, email)
        {
            _repository = repository;
        }
        #region Frequency
        public async Task<IActionResult> FeeFrequency(int id = 0)
        {
            FeeFrequencyPageVM model = new();

            model.List = await _repository.GetAllFrequencyAsync();

            if (id > 0)
            {
                model.Form = await _repository.GetFrequencyByIdAsync(id) ?? new FeeFrequency();
            }

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFeeFrequency(FeeFrequencyPageVM model)
        {
            model.List = await _repository.GetAllFrequencyAsync();

            if (!ModelState.IsValid)
                return View("Index", model);

            if (await _repository.IsDuplicateFrequencyAsync(model.Form.Value, model.Form.FrequencyId))
            {
                ModelState.AddModelError("Form.Value", "Frequency already exists.");

                return View("FeeFrequency", model);
            }

            ResponseModel response;
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";

            if (model.Form.FrequencyId == 0)
            {
                model.Form.AddedDate = DateTime.Now;
                model.Form.AddedBy = userName;
                response = await _repository.FrequencySaveAsync(model.Form);
            }
            else
            {
                model.Form.UpdatedDate = DateTime.Now;
                model.Form.UpdatedBy = userName;
                response = await _repository.FrequencyUpdateAsync(model.Form);
            }

            TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;

            return RedirectToAction(nameof(FeeFrequency));
        }
        public async Task<IActionResult> DeleteFeeFrequency(int id)
        {
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
            var response = await _repository.DeleteFrequencyAsync(id,userName);

            TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;

            return RedirectToAction(nameof(FeeFrequency));
        }
        #endregion
        #region FeeHeading
        public async Task<IActionResult> FeeHeadings(long id = 0)
        {
            FeeHeadingPageVM model = new();

            await BindFrequency(model);

            model.List = await _repository.GetAllFeeHeadingAsync();

            if (id > 0)
            {
                model.Form = await _repository.GetFeeHeadingByIdAsync(id) ?? new FeeHeading();
            }

            return View(model);
        }
        private async Task BindFrequency(FeeHeadingPageVM model)
        {
            var frequency = await _repository.GetActiveFrequencyAsync();

            model.FrequencyList = frequency.Select(x => new SelectListItem
            {
                Value = x.FrequencyId.ToString(),
                Text = x.Text
            }).ToList();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFeeHeadings(FeeHeadingPageVM model)
        {
            await BindFrequency(model);

            model.List = await _repository.GetAllFeeHeadingAsync();

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Model Error: {error.ErrorMessage}");
                   
                    }
                }
                return View("FeeHeadings", model);

            }
            if (await _repository.IsDuplicateFeeHeadingAsync(model.Form.FeeHeadingName, model.Form.FeeHeadingId))
            {
                ModelState.AddModelError("Form.FeeHeadingName", "Fee Heading already exists.");

                return View("FeeHeadings", model);
            }

            ResponseModel response;
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
            if (model.Form.FeeHeadingId == 0)
            {
                model.Form.AddedDate = DateTime.Now;
                model.Form.AddedBy = userName;
                response = await _repository.SaveFeeHeadingAsync(model.Form);
            }
            else
            {
                model.Form.UpdatedDate = DateTime.Now;
                model.Form.UpdatedBy = userName;
                response = await _repository.UpdateFeeHeadingAsync(model.Form);
            }

            TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;

            return RedirectToAction(nameof(FeeHeadings));
        }

        public async Task<IActionResult> DeleteFeeHeadings(long id)
        {
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
            var response = await _repository.DeleteFeeHeadingAsync(id,userName);

            TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;

            return RedirectToAction(nameof(FeeHeadings));
        }
        #endregion
        #region FeePlan
            #region TuitionFeePlan
            public async Task<IActionResult> FeePlans()
                {
            ViewBag.Classes = GetDataListItems("Class");

           ViewBag.Batches = await _context.Batches
                .Where(x => x.IsActive && x.ActiveForAdmission)
                .ToListAsync();
            ViewBag.FeeHeadingList = await _repository.GetAllFeeHeadingAsync();
            var vm = new FeePlanIndexViewModel
                    { 
                        TuitionPlans = await _repository.GetAllTuitionFeePlanAsync(),
                        TransportPlans = await _repository.GetAllTransportFeePlanAsync(),
                        HostelPlans = await _repository.GetAllHostelFeePlanAsync()
                    };
                    return View(vm);
                }
            [HttpPost]
            public async Task<IActionResult> SaveTuition(TuitionFeePlan model)
            {
                    ResponseModel response;
                    var currentUser = HttpContext.Session.GetCurrentUser();
                    string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
                    if (model.TuitionFeePlanId == 0)
                    {
                        model.AddedDate = DateTime.Now;
                        model.AddedBy = userName;
                        response = await _repository.SaveTuitionFeePlanAsync(model);

                    }
                    else
                    {
                        model.UpdatedDate = DateTime.Now;
                        model.UpdatedBy = userName;
                        response=await _repository.UpdateTuitionFeePlanAsync(model);
                    }
                    TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;
                    return RedirectToAction(nameof(FeePlans));
            }

            public async Task<IActionResult> SaveTuition(int id)
            {
                var vm = new FeePlanIndexViewModel
                {
                    NewTuition = await _repository.GetTuitionFeePlanByIdAsync(id) ?? new TuitionFeePlan(),
                    TuitionPlans = await _repository.GetAllTuitionFeePlanAsync(),
                    TransportPlans = await _repository.GetAllTransportFeePlanAsync(),
                    HostelPlans = await _repository.GetAllHostelFeePlanAsync()
                };
                return View("FeePLans", vm);
            }
            [HttpPost]
            public async Task<IActionResult> DeleteTuition(long id)
            {
                var currentUser = HttpContext.Session.GetCurrentUser();
                string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
                ResponseModel response = await _repository.DeleteTuitionFeePlanAsync(id, userName);
                TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;
                return RedirectToAction(nameof(FeePlans));
            }
        #endregion
        #region TransportFeePlan
             
        [HttpPost]
        public async Task<IActionResult> SaveTransport(TransportFeePlan model)
        {
            ResponseModel response;
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
            if (model.TransportFeePlanId == 0)
            {
                model.AddedDate = DateTime.Now;
                model.AddedBy = userName;
                response = await _repository.SaveTransportFeePlanAsync(model);

            }
            else
            {
                model.UpdatedDate = DateTime.Now;
                model.UpdatedBy = userName;
                response = await _repository.UpdateTransportFeePlanAsync(model);
            }
            TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;
            return RedirectToAction(nameof(FeePlans));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteTransport(int id)
        {
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
            ResponseModel response = await _repository.DeleteTransportFeePlanAsync(id, userName);
            TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;
            return RedirectToAction(nameof(FeePlans));
        }
        #endregion
        #region HostelFeePlan
        [HttpPost]
        public async Task<IActionResult> SaveHostel(HostelFeePlan model)
        {
            ResponseModel response;
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
            if (model.HostelFeePlanId== 0)
            {
                model.AddedDate = DateTime.Now;
                model.AddedBy = userName;
                response = await _repository.SaveHostelFeePlanAsync(model);

            }
            else
            {
                model.UpdatedDate = DateTime.Now;
                model.UpdatedBy = userName;
                response = await _repository.UpdateHostelFeePlanAsync(model);
            }
            TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;
            return RedirectToAction(nameof(FeePlans));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteHostelFee(int id)
        {
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
            ResponseModel response = await _repository.DeleteHostelFeePlanAsync(id, userName);
            TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;
            return RedirectToAction(nameof(FeePlans));
        }
        #endregion
        #endregion
    }
}
