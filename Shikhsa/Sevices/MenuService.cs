////{
////    public class MenuService
////    {
////    }
////}
//// Services/MenuService.cs
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Shikhsa.Data;
//using Shikhsa.Models;
//using Shikhsa.Data;

//namespace Shikhsa.Services
//{
//    public class MenuService
//    {
//        private readonly ApplicationDbContext _db;
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly RoleManager<IdentityRole> _roleManager;

//        public MenuService(
//            ApplicationDbContext db,
//            UserManager<ApplicationUser> userManager,
//            RoleManager<IdentityRole> roleManager)
//        {
//            _db = db;
//            _userManager = userManager;
//            _roleManager = roleManager;
//        }

//        // Returns menus the logged-in user can see
//        public async Task<List<Menu>> GetUserMenusAsync(string userId)
//        {
//            var user = await _userManager.FindByIdAsync(userId);
//            if (user == null) return new List<Menu>();

//            var roles = await _userManager.GetRolesAsync(user);

//            var roleIds = await _db.Roles
//                .Where(r => roles.Contains(r.Name!))
//                .Select(r => r.Id)
//                .ToListAsync();

//            // Get all menu IDs the user's roles can view
//            var allowedMenuIds = await _db.RoleMenus
//                .Where(rm => roleIds.Contains(rm.RoleId) && rm.CanView)
//                .Select(rm => rm.MenuId)
//                .Distinct()
//                .ToListAsync();

//            // Load parent menus with their children
//            var menus = await _db.Menus
//                .Where(m => allowedMenuIds.Contains(m.Id)
//                         && m.ParentId == null
//                         && m.IsActive)
//                .OrderBy(m => m.DisplayOrder)
//                .Include(m => m.Children
//                    .Where(c => allowedMenuIds.Contains(c.Id) && c.IsActive)
//                    .OrderBy(c => c.DisplayOrder))
//                .ToListAsync();

//            return menus;
//        }

//        // Returns permissions for a specific role + menu
//        public async Task<RoleMenu?> GetPermissionsAsync(string roleId, int menuId)
//        {
//            return await _db.RoleMenus
//                .FirstOrDefaultAsync(rm => rm.RoleId == roleId
//                                        && rm.MenuId == menuId);
//        }
//    }
//}
// Services/MenuService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Models;

namespace Shikhsa.Services
{
    public class MenuService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public MenuService(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ── Returns menus visible to the logged-in user ────
        public async Task<List<Menu>> GetUserMenusAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new List<Menu>();

            var roles = await _userManager.GetRolesAsync(user);

            // Get role IDs for the user's roles
            var roleIds = await _db.Roles
                .Where(r => roles.Contains(r.Name!))
                .Select(r => r.Id)
                .ToListAsync();

            // Get menu IDs allowed for these roles (CanView = true)
            var allowedMenuIds = await _db.RoleMenus
                .Where(rm => roleIds.Contains(rm.RoleId) && rm.CanView)
                .Select(rm => rm.MenuId)
                .Distinct()
                .ToListAsync();

            // Load parent menus + their allowed active children
            var menus = await _db.Menus
                .Where(m => allowedMenuIds.Contains(m.Id)
                         || m.ParentId == null
                         && m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .Include(m => m.Children
                    .Where(c => allowedMenuIds.Contains(c.Id) && c.IsActive)
                    .OrderBy(c => c.DisplayOrder))
                .ToListAsync();

            return menus;
        }

        // ── Returns permissions for a role + menu combo ────
        public async Task<RoleMenu?> GetPermissionsAsync(string roleId, int menuId)
        {
            return await _db.RoleMenus
                .FirstOrDefaultAsync(rm => rm.RoleId == roleId
                                        && rm.MenuId == menuId);
        }

        // ── Checks if a user can perform an action on a URL ─
        public async Task<bool> CanAccessAsync(
            string userId, string url, string action = "View")
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var roles = await _userManager.GetRolesAsync(user);
            var roleIds = await _db.Roles
                .Where(r => roles.Contains(r.Name!))
                .Select(r => r.Id)
                .ToListAsync();

            var menu = await _db.Menus
                .FirstOrDefaultAsync(m => m.ActionName != null &&
                    m.ActionName.ToLower() == url.ToLower());

            if (menu == null) return false;

            var perm = await _db.RoleMenus
                .FirstOrDefaultAsync(rm => roleIds.Contains(rm.RoleId)
                                        && rm.MenuId == menu.Id);
            if (perm == null) return false;

            return action switch
            {
                "Create" => perm.CanCreate,
                "Edit" => perm.CanEdit,
                "Delete" => perm.CanDelete,
                _ => perm.CanView
            };
        }
    }
}
