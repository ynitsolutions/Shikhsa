// Controllers/DashboardController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models;
using Shikhsa.Services;
using Shikhsa.ViewModels;

namespace Shikhsa.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly LocalizationService _localization;
        public DashboardController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager, LocalizationService localization)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _localization = localization;
        }

        // GET: /Dashboard
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = user != null
                ? await _userManager.GetRolesAsync(user)
                : new List<string>();

            var vm = new DashboardViewModel
            {
                UserFullName = user?.FullName ?? User.Identity?.Name ?? "User",
                UserEmail = user?.Email ?? string.Empty,
                UserRole = roles.FirstOrDefault() ?? "—",
                TotalUsers = await _userManager.Users.CountAsync(),
                ActiveUsers = await _userManager.Users.CountAsync(u => u.IsActive),
                TotalRoles = await _roleManager.Roles.CountAsync(),
                TotalMenus = await _db.Menus.Where(x=>x.ParentId!=null && x.ParentId==0).CountAsync(),
                RecentUsers = await _userManager.Users
                                    .OrderByDescending(u => u.CreatedAt)
                                    .Take(5)
                                    .Select(u => new RecentUserItem
                                    {
                                        Id = u.Id,
                                        FullName = u.FullName,
                                        Email = u.Email ?? string.Empty,
                                        IsActive = u.IsActive,
                                        CreatedAt = u.CreatedAt
                                    })
                                    .ToListAsync()
            };

            return View(vm);
        }
    }
}
