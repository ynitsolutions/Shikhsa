using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Helpers;
using Shikhsa.Models;
using Shikhsa.Services;
using Shikhsa.ViewModels;
using System.Text.Json;
namespace Shikhsa.Controllers
{
    public class AccountController : BaseController
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly PermissionService _permissionService;
        private readonly EmailService _email;
        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger, PermissionService permissionService, ApplicationDbContext context, EmailService email) : base(userManager, permissionService, context,email)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _permissionService = permissionService;
            _context = context;
            _email = email;
        }

        // ── GET: /Account/Login ────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            var school = await GetSchoolInfo();
            ViewBag.SchoolName = school?.SchoolName;
            ViewBag.TagLine = school?.TagLine;
            ViewBag.LogoPath = school?.LogoPath;
            ViewBag.mobileNo = school?.MobileContactNo;
            ViewBag.email = school?.Email;
            ViewBag.Website = school?.Website;
            // If already logged in, redirect to dashboard
            if (_signInManager.IsSignedIn(User))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    var sessionUser = new UserSessionVM
                    {
                        Id = user.Id,
                        UserName = user.UserName ?? "",
                        Email = user.Email ?? "",
                        FullName = user.FullName ?? "",
                        PhoneNumber = user.PhoneNumber ?? "",
                        RoleName = roles.FirstOrDefault() ?? ""
                    };

                    HttpContext.Session.SetString("CurrentUser",JsonSerializer.Serialize(sessionUser));
                    await _permissionService.CacheUserPermissions(user.Id);
                }
                    return RedirectToAction("Index", "Dashboard");
            }
            // Clear external cookies
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }
        // ── POST: /Account/Login ───────────────────────────
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager .PasswordSignInAsync(model.Email,model.Password,model.RememberMe,false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("","Invalid Login");

                return View(model);
            }

            var user =await _userManager.FindByNameAsync(model.Email);


            if (user == null)
            {
                ModelState.AddModelError("", "User not found");
                return View(model);
            }
            var roles = await _userManager.GetRolesAsync(user);

            var sessionUser = new UserSessionVM
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                FullName = user.FullName ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                RoleName = roles.FirstOrDefault() ?? ""
            };
            HttpContext.Session.SetString( "CurrentUser",JsonSerializer.Serialize(sessionUser));
            var school = await GetSchoolInfo();
            HttpContext.Session.SetString("SchoolName", school.SchoolName);
            HttpContext.Session.SetString("SchoolLogo", school.LogoPath);
            HttpContext.Session.SetString("SchoolTagLine", school.TagLine);
            await _permissionService.CacheUserPermissions(user.Id);

            await _permissionService.CacheUserPermissions(user.Id);

            return RedirectToAction( "Index","Dashboard");
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (!string.IsNullOrWhiteSpace(userId))
            {
                 _permissionService.RemoveUserCache(userId);
            }
            HttpContext.Session.Clear();
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login","Account");
        }

        // ── GET: /Account/AccessDenied ─────────────────────
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
