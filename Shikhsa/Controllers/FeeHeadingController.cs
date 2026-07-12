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
            _context = context;
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
        private async Task TaskBindFeePlansDropdown()
        {
            ViewBag.Classes = GetDataListItems("Class");
            ViewBag.Hostels = GetDataListItems("Hostel List");
            ViewBag.Transports = GetDataListItems("Transport");
            ViewBag.RoomType = GetDataListItems("Room Type");
            ViewBag.MealType = GetDataListItems("Meal Type");

            ViewBag.Batches = await _context.Batches
                .Where(x => x.IsActive && x.ActiveForAdmission)
                .ToListAsync();
            ViewBag.FeeHeadingList = await _repository.GetAllFeeHeadingAsync();

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
            await TaskBindFeePlansDropdown();


            var vm = new FeePlanIndexViewModel
                    { 
                        TuitionPlans = await _repository.GetAllTuitionFeePlanAsync(),
                        TransportPlans = await _repository.GetAllTransportFeePlanAsync(),
                        HostelPlans = await _repository.GetAllHostelFeePlanAsync()
                    };
                    return View(vm);
            }
     

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTuitionFee(FeePlanIndexViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please fill all required fields.";
                    return RedirectToAction(nameof(FeePlans));
                }

                var currentUser = HttpContext.Session.GetCurrentUser();
                string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";

                ResponseModel response = await _repository.SaveOrUpdateTuitionFeePlanAsync(vm.NewTuition, userName);

                TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;

                return RedirectToAction(nameof(FeePlans));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(FeePlans));
            }
        }
        public async Task<IActionResult> SaveTuitionFee(int id)
            {
                await TaskBindFeePlansDropdown();
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
            public async Task<IActionResult> DeleteTuitionFee(long id)
            {
                var currentUser = HttpContext.Session.GetCurrentUser();
                string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
                ResponseModel response = await _repository.DeleteTuitionFeePlanAsync(id, userName);
                TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;
                return RedirectToAction(nameof(FeePlans));
            }
        #endregion
        #region TransportFeePlan
        public async Task<IActionResult> SaveTransportFee(int id)
        {
            await TaskBindFeePlansDropdown();
            var vm = new FeePlanIndexViewModel
            {
                NewTransport= await _repository.GetTransportFeePlanByIdAsync(id) ?? new TransportFeePlan(),
                TuitionPlans = await _repository.GetAllTuitionFeePlanAsync(),
                TransportPlans = await _repository.GetAllTransportFeePlanAsync(),
                HostelPlans = await _repository.GetAllHostelFeePlanAsync()
            };
            return View("FeePLans", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTransportFee(FeePlanIndexViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please fill all required fields.";
                    return RedirectToAction(nameof(FeePlans));
                }

                var currentUser = HttpContext.Session.GetCurrentUser();
                string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";

                ResponseModel response = await _repository.SaveUpdateTransportFeePlanAsync(vm.NewTransport, userName);

                TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;

                return RedirectToAction(nameof(FeePlans));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(FeePlans));
            }
        }
    
        [HttpPost]
        public async Task<IActionResult> DeleteTransportFee(int id)
        {
            var currentUser = HttpContext.Session.GetCurrentUser();
            string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";
            ResponseModel response = await _repository.DeleteTransportFeePlanAsync(id, userName);
            TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;
            return RedirectToAction(nameof(FeePlans));
        }
        #endregion
        #region HostelFeePlan
        public async Task<IActionResult> SaveHostelFee(int id)
        {
            await TaskBindFeePlansDropdown();
            var vm = new FeePlanIndexViewModel
            {
                NewHostel = await _repository.GetHostelFeePlanByIdAsync(id) ?? new HostelFeePlan(),
                TuitionPlans = await _repository.GetAllTuitionFeePlanAsync(),
                TransportPlans = await _repository.GetAllTransportFeePlanAsync(),
                HostelPlans = await _repository.GetAllHostelFeePlanAsync()
            };
            return View("FeePLans", vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveHostelFee(FeePlanIndexViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please fill all required fields.";
                    return RedirectToAction(nameof(FeePlans));
                }

                var currentUser = HttpContext.Session.GetCurrentUser();
                string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";

                ResponseModel response = await _repository.SaveUpdateHostelFeePlanAsync(vm.NewHostel,userName);

                TempData[response.Status == 1 ? "Success" : "Error"] = response.Message;

                return RedirectToAction(nameof(FeePlans));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(FeePlans));
            }
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
