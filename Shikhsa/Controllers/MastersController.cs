using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shikhsa.Attributes;
using Shikhsa.Data;
using Shikhsa.Enums;
using Shikhsa.Helpers;
using Shikhsa.Models;
using Shikhsa.Services;
using Shikhsa.ViewModels;

namespace Shikhsa.Controllers
{
    [AllowAnonymous]
    public class MastersController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly EmailService _emailService;

        public MastersController(
     ApplicationDbContext context,
     RoleManager<ApplicationRole> roleManager,
     UserManager<ApplicationUser> userManager,
     PermissionService permissionService, IWebHostEnvironment env, EmailService email
 ) :
            base(userManager, permissionService, context, email)
        {
            _context = context;
            _roleManager = roleManager;
            _env = env;
        }
        #region Menu

        public async Task<IActionResult> Menus(int? id)
        {
            Menu model = new Menu();

            // EDIT MODE
            if (id != null)
            {
                var data = await _context.Menus.FindAsync(id);

                if (data != null)
                    model = data;
            }

            ViewBag.MenuList = await _context.Menus
                .Where(x => x.ParentId == null)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            return View(model);
        }

        [HttpPost]

        public async Task<IActionResult> SaveMenus(Menu model)
        {
            try
            {
                if (model.Id == 0)
                {
                    _context.Menus.Add(model);

                    SuccessMessage("Menu Added Successfully");
                }
                else
                {
                    _context.Menus.Update(model);

                    SuccessMessage("Menu Updated Successfully");
                }

                await _context.SaveChangesAsync();


            }
            catch (Exception ex)
            {
                ErrorMessage(ex.Message);


            }
            return RedirectToAction("Menus");
        }

        public async Task<IActionResult> DeleteMenu(int id)
        {
            try
            {
                var data = await _context.Menus.FindAsync(id);

                if (data != null)
                {
                    data.IsActive = !data.IsActive;

                    _context.Menus.Update(data);

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex.Message);
            }
            return RedirectToAction(nameof(Menus));
        }
        #endregion Menu
        #region SubMenu

        public async Task<IActionResult> SubMenus(int? id)
        {
            Menu model = new Menu();

            // EDIT MODE
            if (id != null)
            {
                var data = await _context.Menus.FindAsync(id);

                if (data != null)
                    model = data;
            }

            // MAIN MENUS
            ViewBag.ParentMenus = new SelectList(
                await _context.Menus
                    .Where(x => x.ParentId == null)
                    .OrderBy(x => x.Name)
                    .ToListAsync(),
                "Id",
                "Name"
            );

            // SUBMENU LIST
            ViewBag.SubMenuList = await _context.Menus
                .Include(x => x.Parent)
                .Where(x => x.ParentId != null)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> SaveSubMenus(Menu model)
        {
            try
            {
                if (model.Id == 0)
                {
                    _context.Menus.Add(model);
                    SuccessMessage("Sub Menu Saved Successfully");
                }
                else
                {
                    _context.Menus.Update(model);
                    SuccessMessage("Sub Menu Updated Successfully");
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage(ex.Message);
            }
            return RedirectToAction(nameof(SubMenus));
        }

        // =========================
        // ACTIVE / INACTIVE
        // =========================
        public async Task<IActionResult> DeleteSubMenus(int id)
        {
            var data = await _context.Menus.FindAsync(id);

            if (data != null)
            {
                data.IsActive = !data.IsActive;

                _context.Menus.Update(data);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("SubMenus");
        }
        #endregion SubMenu
        #region Menu Permission_old
        //public async Task<IActionResult> RoleMenuPermission()
        //{
        //    var vm = await LoadRolePermissionData();

        //    return View(vm);
        //}

        //[HttpPost]
        //public async Task<IActionResult> SaveRoleMenu(RolePermissionVM vm)
        //{
        //    try
        //    {
        //        // VALIDATION

        //        if (string.IsNullOrWhiteSpace(vm.RoleId))
        //        {
        //            ErrorMessage("Please Select Role");

        //            vm = await LoadRolePermissionData();

        //            return View("RoleMenuPermission", vm);
        //        }

        //        if (vm.SubMenus == null || !vm.SubMenus.Any())
        //        {
        //            ErrorMessage("No Permission Data Found");

        //            vm = await LoadRolePermissionData();

        //            return View("RoleMenuPermission", vm);
        //        }

        //        foreach (var item in vm.SubMenus)
        //        {
        //            // CHECK EXISTING RECORD

        //            var existingPermission =
        //                await _context.RoleMenuPermissions
        //                .FirstOrDefaultAsync(x =>
        //                    x.RoleId == vm.RoleId
        //                    && x.MenuId == item.MenuId);

        //            // INSERT

        //            if (existingPermission == null)
        //            {

        //                existingPermission =
        //                    new RoleMenuPermission
        //                    {
        //                        RoleId = vm.RoleId,

        //                        MenuId = item.MenuId,

        //                        CanView = item.CanView,

        //                        CanCreate = item.CanCreate,

        //                        CanUpdate = item.CanUpdate,

        //                        CanDelete = item.CanDelete,
        //                        AddedBy = User.Identity.Name,

        //                    };

        //                await _context.RoleMenuPermissions
        //                    .AddAsync(existingPermission);
        //            }

        //            // UPDATE

        //            else
        //            {
        //                existingPermission.CanView =
        //                    item.CanView;

        //                existingPermission.CanCreate =
        //                    item.CanCreate;

        //                existingPermission.CanUpdate =
        //                    item.CanUpdate;

        //                existingPermission.CanDelete =
        //                    item.CanDelete;
        //                existingPermission.UpdatedBy = User.Identity.Name;
        //                _context.RoleMenuPermissions
        //                    .Update(existingPermission);
        //            }
        //        }

        //        await _context.SaveChangesAsync();

        //        SuccessMessage("Permissions Saved Successfully");

        //        vm = await LoadRolePermissionData();

        //        return View("RoleMenuPermission", vm);
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMessage(ex.Message);

        //        vm = await LoadRolePermissionData();

        //        return View("RoleMenuPermission", vm);
        //    }
        //}
        //[HttpPost]
        //public async Task<IActionResult> SaveRoleMenuPermission(RolePermissionVM vm)
        //{
        //    try
        //    {
        //        // VALIDATION

        //        if (string.IsNullOrWhiteSpace(vm.RoleId))
        //        {
        //            ErrorMessage("Please Select Role");

        //            vm = await LoadRolePermissionData();

        //            return View("RoleMenuPermission", vm);
        //        }

        //        if (vm.SubMenus == null || !vm.SubMenus.Any())
        //        {
        //            ErrorMessage("No Permission Data Found");

        //            vm = await LoadRolePermissionData();

        //            return View("RoleMenuPermission", vm);
        //        }
        //        foreach (var item in vm.SubMenus)
        //        {
        //            var existingRoleMenu =
        //                await _context.RoleMenus
        //                .FirstOrDefaultAsync(x =>
        //                    x.RoleId == vm.RoleId
        //                    && x.MenuId == item.MenuId);

        //            if (existingRoleMenu == null)
        //            {
        //                existingRoleMenu = new RoleMenu
        //                {
        //                    RoleId = vm.RoleId,

        //                    MenuId = item.MenuId,

        //                    CanView = item.CanView,

        //                    CanCreate = item.CanCreate,

        //                    CanEdit = item.CanUpdate,

        //                    CanDelete = item.CanDelete,

        //                    AddedBy = User.Identity?.Name
        //                };

        //                await _context.RoleMenus
        //                    .AddAsync(existingRoleMenu);
        //            }
        //            else
        //            {
        //                existingRoleMenu.CanView =
        //                    item.CanView;

        //                existingRoleMenu.CanCreate =
        //                    item.CanCreate;

        //                existingRoleMenu.CanEdit =
        //                    item.CanUpdate;

        //                existingRoleMenu.CanDelete =
        //                    item.CanDelete;

        //                existingRoleMenu.UpdatedBy =
        //                    User.Identity?.Name;

        //                _context.RoleMenus
        //                    .Update(existingRoleMenu);
        //            }

        //        }
        //        await _context.SaveChangesAsync();

        //        SuccessMessage("Permissions Saved Successfully");

        //        vm = await LoadRolePermissionData();

        //        return View("RoleMenuPermission", vm);
        //    }

        //    catch (Exception ex)
        //    {
        //        ErrorMessage(ex.Message);

        //        vm = await LoadRolePermissionData();

        //        return View("RoleMenuPermission", vm);
        //    }
        //}


        //[HttpGet]
        //[SkipPermission]
        //public async Task<IActionResult> GetSubMenus(string roleId, int menuId)
        //{
        //    var subMenus = await GetSubMenuData(roleId, menuId);

        //    return PartialView(
        //        "_SubMenuPermission",
        //        subMenus);
        //}
        //[SkipPermission]
        //private async Task<List<SubMenuPermissionVM>> GetSubMenuData(string roleId, int menuId)
        //{
        //    // =========================
        //    // GET ALL SUBMENUS
        //    // =========================

        //    var subMenus = await _context.Menus
        //        .Where(x =>
        //            x.ParentId == menuId
        //            && x.IsActive)
        //        .OrderBy(x => x.DisplayOrder)
        //        .ToListAsync();

        //    // =========================
        //    // GET EXISTING PERMISSIONS
        //    // =========================

        //    var permissions = await _context
        //        .RoleMenus
        //        .Where(x => x.RoleId == roleId)
        //        .ToListAsync();

        //    // =========================
        //    // MAP DATA
        //    // =========================

        //    var result = subMenus
        //        .Select(x =>
        //        {
        //            var permission = permissions
        //                .FirstOrDefault(p =>
        //                    p.MenuId == x.Id);

        //            return new SubMenuPermissionVM
        //            {
        //                MenuId = x.Id,

        //                MenuName = x.Name,

        //                CanView =
        //                    permission?.CanView ?? false,

        //                CanCreate =
        //                    permission?.CanCreate ?? false,

        //                CanUpdate =
        //                    permission?.CanEdit ?? false,

        //                CanDelete =
        //                    permission?.CanDelete ?? false
        //            };
        //        })
        //        .ToList();

        //    return result;
        //}
        //[SkipPermission]
        //private async Task<RolePermissionVM> LoadRolePermissionData()
        //{
        //    var vm = new RolePermissionVM();

        //    // ROLES

        //    vm.Roles = await _roleManager.Roles
        //        .Select(x => new SelectListItem
        //        {
        //            Value = x.Id,
        //            Text = x.Name
        //        })
        //        .ToListAsync();

        //    // MENUS

        //    vm.Menus = await _context.Menus
        //        .Where(x =>
        //            x.ParentId == null
        //            && x.IsActive)
        //        .OrderBy(x => x.DisplayOrder)
        //        .Select(x => new SelectListItem
        //        {
        //            Value = x.Id.ToString(),
        //            Text = x.Name
        //        })
        //        .ToListAsync();

        //    // PERMISSION LIST

        //    vm.PermissionList =
        //        await (from rp in _context.RoleMenus

        //               join r in _roleManager.Roles
        //               on rp.RoleId equals r.Id

        //               join m in _context.Menus
        //               on rp.MenuId equals m.Id

        //               select new RolePermissionListVM
        //               {
        //                   Id = rp.Id,

        //                   RoleName = r.Name,

        //                   MenuName = m.Name,

        //                   CanView = rp.CanView,

        //                   CanCreate = rp.CanCreate,

        //                   CanUpdate = rp.CanEdit,

        //                   CanDelete = rp.CanDelete
        //               })
        //               .ToListAsync();

        //    return vm;
        //}

        #endregion
        #region Menu Permission

        private static readonly string[] _excludedRoleNames =
            { "Admin", "Developer", "YN IT Solutions" };

        public async Task<IActionResult> RoleMenuPermission()
        {
            var vm = await LoadRolePermissionData();
            return View(vm);
        }

        // =====================================
        // ROLE CHANGE -> GET STAFF LIST FOR THAT ROLE
        // =====================================
        [HttpGet]
        [SkipPermission]
        public async Task<IActionResult> GetStaffByRole(string roleId)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                return Json(new List<SelectListItem>());

            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
                return Json(new List<SelectListItem>());

            // Admin/Developer/YN IT Solutions ke liye staff dropdown ki zaroorat nahi
            if (_excludedRoleNames.Contains(role.Name))
                return Json(new List<SelectListItem>());

            var result = await (
                from s in _context.StaffMasters
                join ur in _context.UserRoles
                    on s.UserId equals ur.UserId
                where s.IsActive
                      && ur.RoleId == roleId
                select new SelectListItem
                {
                    Value = s.UserId,      // ya s.StaffId.ToString() agar StaffId save karna hai
                    Text = s.FirstName + " " + s.MiddleName+" "+s.LastName
                }
            ).Distinct().ToListAsync();

            return Json(result);
        }

        // =====================================
        // SAVE PERMISSIONS (staff select -> RoleMenuPermissions | no staff -> RoleMenu)
        // =====================================
        [HttpPost]
        public async Task<IActionResult> SaveRoleMenuPermission(RolePermissionVM vm)
        {
            try
            {
                // VALIDATION

                if (string.IsNullOrWhiteSpace(vm.RoleId))
                {
                    ErrorMessage("Please Select Role");
                    vm = await LoadRolePermissionData();
                    return View("RoleMenuPermission", vm);
                }

                if (vm.SubMenus == null || !vm.SubMenus.Any())
                {
                    ErrorMessage("No Permission Data Found");
                    vm = await LoadRolePermissionData();
                    return View("RoleMenuPermission", vm);
                }

                bool hasStaffSelected = !string.IsNullOrWhiteSpace(vm.UserId);

                // =========================================
                // CASE 1: STAFF SELECTED -> SAVE IN RoleMenuPermissions
                // =========================================
                if (hasStaffSelected)
                {
                    foreach (var item in vm.SubMenus)
                    {
                        var existing = await _context.RoleMenuPermissions
                            .FirstOrDefaultAsync(x =>
                                x.RoleId == vm.RoleId
                                && x.MenuId == item.MenuId
                                && x.UserId == vm.UserId);

                        if (existing == null)
                        {
                            existing = new RoleMenuPermission
                            {
                                RoleId = vm.RoleId,
                                MenuId = item.MenuId,
                                UserId = vm.UserId,
                                CanView = item.CanView,
                                CanCreate = item.CanCreate,
                                CanUpdate = item.CanUpdate,
                                CanDelete = item.CanDelete,
                                AddedBy = User.Identity?.Name,
                                AddedDate = DateTime.Now,
                                IsActive = true
                            };

                            await _context.RoleMenuPermissions.AddAsync(existing);
                        }
                        else
                        {
                            existing.CanView = item.CanView;
                            existing.CanCreate = item.CanCreate;
                            existing.CanUpdate = item.CanUpdate;
                            existing.CanDelete = item.CanDelete;
                            existing.UpdatedBy = User.Identity?.Name;
                            existing.UpdatedDate = DateTime.Now;

                            _context.RoleMenuPermissions.Update(existing);
                        }
                    }
                }

                // =========================================
                // CASE 2: NO STAFF SELECTED -> SAVE IN RoleMenu (role-level)
                // =========================================
                else
                {
                    foreach (var item in vm.SubMenus)
                    {
                        var existingRoleMenu = await _context.RoleMenus
                            .FirstOrDefaultAsync(x =>
                                x.RoleId == vm.RoleId
                                && x.MenuId == item.MenuId);

                        if (existingRoleMenu == null)
                        {
                            existingRoleMenu = new RoleMenu
                            {
                                RoleId = vm.RoleId,
                                MenuId = item.MenuId,
                                CanView = item.CanView,
                                CanCreate = item.CanCreate,
                                CanEdit = item.CanUpdate,
                                CanDelete = item.CanDelete,
                                AddedBy = User.Identity?.Name,
                                AddedDate = DateTime.Now,
                                IsActive = true
                            };

                            await _context.RoleMenus.AddAsync(existingRoleMenu);
                        }
                        else
                        {
                            existingRoleMenu.CanView = item.CanView;
                            existingRoleMenu.CanCreate = item.CanCreate;
                            existingRoleMenu.CanEdit = item.CanUpdate;
                            existingRoleMenu.CanDelete = item.CanDelete;
                            existingRoleMenu.UpdatedBy = User.Identity?.Name;
                            existingRoleMenu.UpdatedDate = DateTime.Now;

                            _context.RoleMenus.Update(existingRoleMenu);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                SuccessMessage("Permissions Saved Successfully");
                vm = await LoadRolePermissionData();
                return View("RoleMenuPermission", vm);
            }
            catch (Exception ex)
            {
                ErrorMessage(ex.Message);
                vm = await LoadRolePermissionData();
                return View("RoleMenuPermission", vm);
            }
        }

        // =====================================
        // GET SUBMENUS (staff select -> read from RoleMenuPermissions | else RoleMenu)
        // =====================================
        [HttpGet]
        [SkipPermission]
        public async Task<IActionResult> GetSubMenus(string roleId, int menuId, string? userId)
        {
            var subMenus = await GetSubMenuData(roleId, menuId, userId);
            return PartialView("_SubMenuPermission", subMenus);
        }

        [SkipPermission]
        private async Task<List<SubMenuPermissionVM>> GetSubMenuData(string roleId, int menuId, string? userId)
        {
            var subMenus = await _context.Menus
                .Where(x => x.ParentId == menuId && x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            bool hasStaffSelected = !string.IsNullOrWhiteSpace(userId);

            if (hasStaffSelected)
            {
                // STAFF-LEVEL: RoleMenuPermissions se read karo
                var permissions = await _context.RoleMenuPermissions
                    .Where(x => x.RoleId == roleId && x.UserId == userId)
                    .ToListAsync();

                return subMenus.Select(x =>
                {
                    var p = permissions.FirstOrDefault(perm => perm.MenuId == x.Id);
                    return new SubMenuPermissionVM
                    {
                        MenuId = x.Id,
                        MenuName = x.Name,
                        CanView = p?.CanView ?? false,
                        CanCreate = p?.CanCreate ?? false,
                        CanUpdate = p?.CanUpdate ?? false,
                        CanDelete = p?.CanDelete ?? false
                    };
                }).ToList();
            }
            else
            {
                // ROLE-LEVEL: RoleMenu se read karo
                var permissions = await _context.RoleMenus
                    .Where(x => x.RoleId == roleId)
                    .ToListAsync();

                return subMenus.Select(x =>
                {
                    var p = permissions.FirstOrDefault(perm => perm.MenuId == x.Id);
                    return new SubMenuPermissionVM
                    {
                        MenuId = x.Id,
                        MenuName = x.Name,
                        CanView = p?.CanView ?? false,
                        CanCreate = p?.CanCreate ?? false,
                        CanUpdate = p?.CanEdit ?? false,
                        CanDelete = p?.CanDelete ?? false
                    };
                }).ToList();
            }
        }

        [SkipPermission]
        private async Task<RolePermissionVM> LoadRolePermissionData()
        {
            var vm = new RolePermissionVM();

            vm.Roles = await _roleManager.Roles
                .Select(x => new SelectListItem { Value = x.Id, Text = x.Name })
                .ToListAsync();

            vm.Menus = await _context.Menus
                .Where(x => x.ParentId == null && x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name })
                .ToListAsync();

            // Role-level permission list
            var roleLevelList = await (
                from rp in _context.RoleMenus
                join r in _roleManager.Roles on rp.RoleId equals r.Id
                join m in _context.Menus on rp.MenuId equals m.Id
                select new RolePermissionListVM
                {
                    Id = rp.Id,
                    RoleName = r.Name,
                    MenuName = m.Name,
                    CanView = rp.CanView,
                    CanCreate = rp.CanCreate,
                    CanUpdate = rp.CanEdit,
                    CanDelete = rp.CanDelete
                }).ToListAsync();

            vm.PermissionList = roleLevelList;

            return vm;
        }
        [SkipPermission]
        [HttpPost]
        public async Task<IActionResult> AddSubMenuItem(int parentMenuId, string name, string controllerName, string actionName)
        {
            if (parentMenuId == 0 || string.IsNullOrWhiteSpace(name)
                || string.IsNullOrWhiteSpace(controllerName) || string.IsNullOrWhiteSpace(actionName))
                return Json(new { success = false, message = "Sabhi fields bharna zaroori hai." });

            bool exists = await _context.Menus.AnyAsync(x =>
                x.ParentId == parentMenuId && x.ControllerName == controllerName && x.ActionName == actionName);

            if (exists)
                return Json(new { success = false, message = "Ye permission item pehle se maujood hai." });

            int maxOrder = await _context.Menus
                .Where(x => x.ParentId == parentMenuId)
                .Select(x => (int?)x.DisplayOrder).MaxAsync() ?? 0;

            var menu = new Menu
            {
                ParentId = parentMenuId,
                Name = name.Trim(),
                ControllerName = controllerName.Trim(),
                ActionName = actionName.Trim(),
                IsActive = true,
                DisplayOrder = maxOrder + 1
            };

            await _context.Menus.AddAsync(menu);
            await _context.SaveChangesAsync();

            return Json(new { success = true, menuId = menu.Id, name = menu.Name });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSubMenuItem(int menuId)
        {
            var menu = await _context.Menus.FindAsync(menuId);
            if (menu == null)
                return Json(new { success = false, message = "Item nahi mila." });

            _context.RoleMenus.RemoveRange(_context.RoleMenus.Where(x => x.MenuId == menuId));
            _context.RoleMenuPermissions.RemoveRange(_context.RoleMenuPermissions.Where(x => x.MenuId == menuId));

            _context.Menus.Remove(menu);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        [HttpGet]
        [SkipPermission]
        public async Task<IActionResult> GetSubMenusByMenu(int menuId)
        {
            var data = await _context.Menus
                .Where(x => x.ParentId == menuId && x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                })
                .ToListAsync();

            return Json(data);
        }
        //[HttpGet]
        //[SkipPermission]
        //public async Task<IActionResult> GetPermissionItems(int subMenuId)
        //{
        //    var data = await _context.PermissionItems
        //        .Where(x => x.SubMenuId == subMenuId && x.IsActive)
        //        .OrderBy(x => x.DisplayOrder)
        //        .ToListAsync();

        //    return PartialView("_PermissionItems", data);
        //}
    //    [HttpPost]
    //    [SkipPermission]
    //    public async Task<IActionResult> AddPermissionItem(
    //int subMenuId,
    //string name,
    //string controllerName,
    //string actionName)
    //    {
    //        if (subMenuId == 0)
    //            return Json(new
    //            {
    //                success = false,
    //                message = "Please select Sub Menu."
    //            });

    //        if (string.IsNullOrWhiteSpace(name))
    //            return Json(new
    //            {
    //                success = false,
    //                message = "Permission Name required."
    //            });

    //        bool exists = await _context.PermissionItems.AnyAsync(x =>
    //            x.SubMenuId == subMenuId &&
    //            x.Name == name &&
    //            x.IsActive);

    //        if (exists)
    //            return Json(new
    //            {
    //                success = false,
    //                message = "Permission already exists."
    //            });

    //        int order = await _context.PermissionItems
    //            .Where(x => x.SubMenuId == subMenuId)
    //            .Select(x => (int?)x.DisplayOrder)
    //            .MaxAsync() ?? 0;

    //        PermissionItem item = new PermissionItem
    //        {
    //            SubMenuId = subMenuId,
    //            Name = name.Trim(),
    //            ControllerName = controllerName.Trim(),
    //            ActionName = actionName.Trim(),
    //            DisplayOrder = order + 1,
    //            AddedBy = User.Identity?.Name,
    //            AddedDate = DateTime.Now,
    //            IsActive = true
    //        };

    //        _context.PermissionItems.Add(item);

    //        await _context.SaveChangesAsync();

    //        return Json(new
    //        {
    //            success = true,
    //            id = item.PermissionItemId
    //        });
    //    }
    //    [HttpPost]
    //    [SkipPermission]
    //    public async Task<IActionResult> DeletePermissionItem(int id)
    //    {
    //        var item = await _context.PermissionItems
    //            .FindAsync(id);

    //        if (item == null)
    //            return Json(new
    //            {
    //                success = false,
    //                message = "Permission not found."
    //            });

    //        item.IsActive = false;

    //        _context.PermissionItems.Update(item);

    //        await _context.SaveChangesAsync();

    //        return Json(new
    //        {
    //            success = true
    //        });
    //    }
    //    [HttpPost]
    //    [SkipPermission]
    //    public async Task<IActionResult> UpdatePermissionItem(
    //int permissionItemId,
    //string name,
    //string controllerName,
    //string actionName)
    //    {
    //        var item = await _context.PermissionItems
    //            .FindAsync(permissionItemId);

    //        if (item == null)
    //            return Json(new
    //            {
    //                success = false
    //            });

    //        item.Name = name.Trim();
    //        item.ControllerName = controllerName.Trim();
    //        item.ActionName = actionName.Trim();
    //        item.UpdatedBy = User.Identity?.Name;
    //        item.UpdatedDate = DateTime.Now;

    //        _context.PermissionItems.Update(item);

    //        await _context.SaveChangesAsync();

    //        return Json(new
    //        {
    //            success = true
    //        });
    //    }
        [HttpGet]
        [SkipPermission]
        public async Task<IActionResult> GetChildMenus(int parentId)
        {
            var list = await _context.Menus
                .Where(x => x.ParentId == parentId && x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new
                {
                    id = x.Id,
                    name = x.Name,
                    controllerName = x.ControllerName,
                    actionName = x.ActionName
                })
                .ToListAsync();

            return Json(list);
        }
        [HttpPost]
        [SkipPermission]
        public async Task<IActionResult> AddChildMenu(
    int parentId,
    string name,
    string controllerName,
    string actionName)
        {
            if (parentId <= 0)
                return Json(new
                {
                    success = false,
                    message = "Please select Parent."
                });

            if (string.IsNullOrWhiteSpace(name))
                return Json(new
                {
                    success = false,
                    message = "Name is required."
                });

            bool exists = await _context.Menus.AnyAsync(x =>
                x.ParentId == parentId &&
                x.Name == name &&
                x.IsActive);

            if (exists)
                return Json(new
                {
                    success = false,
                    message = "Already exists."
                });

            int order = await _context.Menus
                .Where(x => x.ParentId == parentId)
                .Select(x => (int?)x.DisplayOrder)
                .MaxAsync() ?? 0;

            Menu menu = new Menu
            {
                ParentId = parentId,
                Name = name.Trim(),
                ControllerName = string.IsNullOrWhiteSpace(controllerName) ? null : controllerName.Trim(),
                ActionName = string.IsNullOrWhiteSpace(actionName) ? null : actionName.Trim(),
                DisplayOrder = order + 1,
                IsActive = true,
                AddedBy = User.Identity?.Name,
                AddedDate = DateTime.Now
            };

            _context.Menus.Add(menu);

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                id = menu.Id,
                name = menu.Name
            });
        }
        [HttpPost]
        [SkipPermission]
        public async Task<IActionResult> UpdateChildMenu(
    int id,
    string name,
    string controllerName,
    string actionName)
        {
            var menu = await _context.Menus.FindAsync(id);

            if (menu == null)
                return Json(new
                {
                    success = false,
                    message = "Record not found."
                });

            bool exists = await _context.Menus.AnyAsync(x =>
                x.Id != id &&
                x.ParentId == menu.ParentId &&
                x.Name == name &&
                x.IsActive);

            if (exists)
                return Json(new
                {
                    success = false,
                    message = "Name already exists."
                });

            menu.Name = name.Trim();
            menu.ControllerName = string.IsNullOrWhiteSpace(controllerName) ? null : controllerName.Trim();
            menu.ActionName = string.IsNullOrWhiteSpace(actionName) ? null : actionName.Trim();
            menu.UpdatedBy = User.Identity?.Name;
            menu.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true
            });
        }
        [HttpPost]
        [SkipPermission]
        public async Task<IActionResult> DeleteChildMenu(int id)
        {
            var menu = await _context.Menus
                .Include(x => x.Children)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (menu == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Record not found."
                });
            }

            if (menu.Children.Any(c => c.IsActive))
            {
                return Json(new
                {
                    success = false,
                    message = "Please delete child records first."
                });
            }

            menu.IsActive = false;
            menu.UpdatedBy = User.Identity?.Name;
            menu.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true
            });
        }
        [HttpGet]
        [SkipPermission]
        public async Task<IActionResult> GetChildMenu(int id)
        {
            var menu = await _context.Menus
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    id = x.Id,
                    parentId = x.ParentId,
                    name = x.Name,
                    controllerName = x.ControllerName,
                    actionName = x.ActionName
                })
                .FirstOrDefaultAsync();

            if (menu == null)
            {
                return Json(new
                {
                    success = false
                });
            }

            return Json(new
            {
                success = true,
                data = menu
            });
        }
        #endregion Menu Permission
        #region DataList
        public async Task<IActionResult> DataList()
        {
            var data = await _context.DataLists
                .Include(x => x.DataListItems)
                .ToListAsync();

            return View(data);
        }
        [HttpGet]
        public async Task<IActionResult> SaveDataList(int? id = 0)
        {
            var vm = new DataListVM();
            if (id == 0)
            {


                vm.Items.Add(new DataListItem());
            }
            else
            {

                var data = await _context.DataLists
                    .Include(x => x.DataListItems)
                    .FirstOrDefaultAsync(x => x.DataListId == id);

                if (data == null)
                {
                    return NotFound();
                }

                vm = new DataListVM
                {
                    DataList = data,
                    Items = data.DataListItems.ToList()
                };
            }
            return View(vm);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SaveDataList(DataListVM vm)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return View(vm);
        //        }
        //        /* INSERT*/
        //        if (vm.DataList.DataListId == 0)
        //        {
        //            /* Save Master*/
        //            _context.DataLists.Add(vm.DataList);
        //            await _context.SaveChangesAsync();

        //            /* Save Child Items*/
        //            foreach (var item in vm.Items)
        //            {
        //                if (!string.IsNullOrWhiteSpace(item.DataListItemText))
        //                {
        //                    item.DataListId = vm.DataList.DataListId;

        //                    _context.DataListItems.Add(item);
        //                }
        //            }

        //            await _context.SaveChangesAsync();
        //            SuccessMessage("Data List Created Successfully");
        //        }
        //        /* UPDATE*/

        //        else
        //        {
        //            var existingDataList = await _context.DataLists
        //                .Include(x => x.DataListItems)
        //                .FirstOrDefaultAsync(x => x.DataListId == vm.DataList.DataListId);

        //            if (existingDataList == null)
        //            {
        //                return NotFound();
        //            }

        //            /* Update Master*/
        //            existingDataList.DataListName = vm.DataList.DataListName;
        //            existingDataList.Description = vm.DataList.Description;


        //            _context.DataListItems.RemoveRange(existingDataList.DataListItems);


        //            foreach (var item in vm.Items)
        //            {
        //                if (!string.IsNullOrWhiteSpace(item.DataListItemText))
        //                {
        //                    item.DataListId = existingDataList.DataListId;

        //                    _context.DataListItems.Add(item);
        //                }
        //            }

        //            await _context.SaveChangesAsync();
        //            SuccessMessage("Data List Updated Successfully");

        //        }

        //        return RedirectToAction(nameof(DataList));
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMessage(ex.Message);
        //        return View(vm);
        //    }
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDataList(DataListVM vm)
        {
            try
            {
                var currentUser = HttpContext.Session.GetCurrentUser();
                string userName = currentUser?.UserName ?? User.Identity?.Name ?? "";

                if (!ModelState.IsValid)
                {
                    return View(vm);
                }

                if (vm.DataList.DataListId == 0)
                {
                    vm.DataList.AddedBy = userName;
                    vm.DataList.AddedDate = DateTime.Now;
                    vm.DataList.IsActive = true;

                    _context.DataLists.Add(vm.DataList);
                    await _context.SaveChangesAsync();

                    foreach (var item in vm.Items.Where(x => !string.IsNullOrWhiteSpace(x.DataListItemText)))
                    {
                        item.DataListId = vm.DataList.DataListId;
                        item.AddedBy = userName;
                        item.AddedDate = DateTime.Now;
                        item.IsActive = true;

                        _context.DataListItems.Add(item);
                    }

                    await _context.SaveChangesAsync();

                    SuccessMessage("Data List Created Successfully");
                }
                else
                {
                    var existingDataList = await _context.DataLists
                        .Include(x => x.DataListItems)
                        .FirstOrDefaultAsync(x => x.DataListId == vm.DataList.DataListId);

                    if (existingDataList == null)
                    {
                        ErrorMessage("Record not found.");
                        return RedirectToAction(nameof(DataList));
                    }

                    existingDataList.DataListName = vm.DataList.DataListName;
                    existingDataList.Description = vm.DataList.Description;
                    existingDataList.UpdatedBy = userName;
                    existingDataList.UpdatedDate = DateTime.Now;

                    var formItemIds = vm.Items
                        .Where(x => x.DataListItemId > 0 && !string.IsNullOrWhiteSpace(x.DataListItemText))
                        .Select(x => x.DataListItemId)
                        .ToList();

                    foreach (var dbItem in existingDataList.DataListItems)
                    {
                        if (!formItemIds.Contains(dbItem.DataListItemId))
                        {
                            dbItem.IsActive = false;
                            dbItem.UpdatedBy = userName;
                            dbItem.UpdatedDate = DateTime.Now;
                        }
                    }

                    foreach (var item in vm.Items)
                    {
                        if (string.IsNullOrWhiteSpace(item.DataListItemText))
                            continue;

                        if (item.DataListItemId > 0)
                        {
                            var dbItem = existingDataList.DataListItems
                                .FirstOrDefault(x => x.DataListItemId == item.DataListItemId);

                            if (dbItem != null)
                            {
                                dbItem.DataListItemText = item.DataListItemText;
                                dbItem.IsActive = true;
                                dbItem.UpdatedBy = userName;
                                dbItem.UpdatedDate = DateTime.Now;
                            }
                        }
                        else
                        {
                            item.DataListId = existingDataList.DataListId;
                            item.AddedBy = userName;
                            item.IsActive = true;

                            _context.DataListItems.Add(item);
                        }
                    }

                    await _context.SaveChangesAsync();

                    SuccessMessage("Data List Updated Successfully");
                }

                return RedirectToAction(nameof(DataList));
            }
            catch (Exception ex)
            {
                ErrorMessage(ex.Message);
                return View(vm);
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteDataList(int id)
        {
            try
            {
                var data = await _context.DataLists.FindAsync(id);

                if (data != null)
                {
                    data.IsActive = !data.IsActive;

                    _context.DataLists.Update(data);

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex.Message);
            }
            return RedirectToAction(nameof(DataList));
        }
        #endregion DataList
        #region Batches
        public ActionResult Batches(int? id)
        {
            BatchVM model = new BatchVM();

            if (id.HasValue)
            {
                var batch = _context.Batches.Find(id);

                if (batch != null)
                {
                    model.BatchId = batch.BatchId;
                    model.AcademicYear = batch.AcademicYear;
                    model.StartDate = batch.StartDate;
                    model.EndDate = batch.EndDate;
                    model.ActiveForRegistration = batch.ActiveForRegistration;
                    model.ActiveForAdmission = batch.ActiveForAdmission;
                    model.ActiveForPayment = batch.ActiveForPayment;
                    model.IsCurrentYear = batch.IsCurrentYear;
                }
            }

            ViewBag.BatchList = _context.Batches
                                        .OrderByDescending(x => x.StartDate)
                                        .ToList();

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveBatches(BatchVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.BatchList = _context.Batches.ToList();
                return View("Batches", model);
            }

            string validation = ValidateBatch(model);

            if (!string.IsNullOrEmpty(validation))
            {
                ModelState.AddModelError("", validation);
                WarningMessage(validation);

                ViewBag.BatchList = _context.Batches.ToList();

                return View("Batches", model);
            }

            if (model.IsCurrentYear)
            {
                var currentYears = _context.Batches
                                           .Where(x => x.IsCurrentYear &&
                                                  x.BatchId != model.BatchId)
                                           .ToList();

                foreach (var item in currentYears)
                {
                    item.IsCurrentYear = false;
                }
            }

            if (model.BatchId == 0)
            {
                Batches batch = new Batches
                {
                    AcademicYear = model.AcademicYear,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    ActiveForRegistration = model.ActiveForRegistration,
                    ActiveForAdmission = model.ActiveForAdmission,
                    ActiveForPayment = model.ActiveForPayment,
                    IsCurrentYear = model.IsCurrentYear,
                    IsActive = true
                };

                _context.Batches.Add(batch);
            }
            else
            {
                var batch = _context.Batches.Find(model.BatchId);

                batch.AcademicYear = model.AcademicYear;
                batch.StartDate = model.StartDate;
                batch.EndDate = model.EndDate;
                batch.ActiveForRegistration = model.ActiveForRegistration;
                batch.ActiveForAdmission = model.ActiveForAdmission;
                batch.ActiveForPayment = model.ActiveForPayment;
                batch.IsCurrentYear = model.IsCurrentYear;
            }

            _context.SaveChanges();
            SuccessMessage(model.BatchId == 0 ? "Batch Saved Successfully" : "Batch Updated Successfully");
            //TempData["Success"] = model.BatchId == 0
            //    ? "Batch Saved Successfully."
            //    : "Batch Updated Successfully.";

            return RedirectToAction("Batches");
        }
        public ActionResult DeleteBatches(int id)
        {
            var batch = _context.Batches.Find(id);

            if (batch != null)
            {
                _context.Batches.Remove(batch);
                _context.SaveChanges();
            }

            return RedirectToAction("Batches");
        }

        private string ValidateBatch(BatchVM model)
        {
            var currentBatch = _context.Batches
                .FirstOrDefault(x => x.IsCurrentYear &&
                                     x.BatchId != model.BatchId);

            if (model.ActiveForRegistration)
            {
                if (currentBatch != null &&
                    model.StartDate <= currentBatch.StartDate)
                {
                    return "Registration can be enabled only for future academic years.";
                }
            }

            if (model.ActiveForAdmission)
            {
                bool isCurrent = model.IsCurrentYear;
                bool isRegistration = model.ActiveForRegistration;

                if (!isCurrent && !isRegistration)
                {
                    return "Admission can be enabled only for Current Year or Registration Year.";
                }
            }

            if (model.ActiveForPayment && !model.ActiveForAdmission)
            {

                return "Payment can be enabled only when Admission is active.";
            }

            return string.Empty;
        }
        #endregion Batches
        #region School Masters
        public async Task<IActionResult> SchoolMasters()
        {
            var model = await _context.SchoolMasters
                                      .FirstOrDefaultAsync();

            if (model == null)
            {
                model = new SchoolMaster();
            }
            var classid = _context.DataLists.Where(x => x.DataListName == "Class" || x.DataListName == "Classes").Select(x => x.DataListId);
            ViewBag.ClassList = await _context.DataListItems.Where(x => classid.Contains(x.DataListId))
                .Select(x => new SelectListItem
                {
                    Value = x.DataListItemId.ToString(),
                    Text = x.DataListItemText
                })
                .ToListAsync();

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> SaveSchoolMasters(SchoolMaster model)
        {
            try
            {
                var classid = _context.DataLists.Where(x => x.DataListName == "Class" || x.DataListName == "Classes").Select(x => x.DataListId);
                ViewBag.ClassList = await _context.DataListItems.Where(x => classid.Contains(x.DataListId))
                    .Select(x => new SelectListItem
                    {
                        Value = x.DataListItemId.ToString(),
                        Text = x.DataListItemText
                    })
                    .ToListAsync();

                //if (!ModelState.IsValid)
                //    return View("SchoolMasters", model);

                if (model.SchoolLogo != null)
                {
                    string folder = Path.Combine(
                        _env.WebRootPath, "UploadedImages", "SchoolLogo");

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    // Example: 20260604025530123.svg
                    string fileName = $"{DateTime.Now:yyyyMMddHHmmssfffffff}{Path.GetExtension(model.SchoolLogo.FileName)}";

                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.SchoolLogo.CopyToAsync(stream);
                    }

                    model.LogoPath = $"/UploadedImages/SchoolLogo/{fileName}";
                }

                if (model.SchoolId == 0)
                {
                    model.AddedDate = DateTime.Now;
                    model.AddedBy = User.Identity.Name;

                    _context.SchoolMasters.Add(model);
                }
                else
                {
                    var dbSchool = await _context.SchoolMasters
                                                 .FindAsync(model.SchoolId);

                    if (dbSchool == null)
                        return NotFound();

                    dbSchool.SchoolName = model.SchoolName;
                    dbSchool.SchoolShortName = model.SchoolShortName;
                    dbSchool.TagLine = model.TagLine;
                    dbSchool.SchoolMotto = model.SchoolMotto;
                    dbSchool.SchoolAddress = model.SchoolAddress;

                    dbSchool.State = model.State;
                    dbSchool.District = model.District;
                    dbSchool.City = model.City;
                    dbSchool.PinCode = model.PinCode;

                    dbSchool.MobileContactNo = model.MobileContactNo;
                    dbSchool.LandlineNo = model.LandlineNo;
                    dbSchool.Email = model.Email;
                    dbSchool.Website = model.Website;

                    dbSchool.Board = model.Board;
                    dbSchool.AffiliationNo = model.AffiliationNo;
                    dbSchool.RegistrationCode = model.RegistrationCode;

                    dbSchool.InitialClassId = model.InitialClassId;
                    dbSchool.AcademicSessionStartMonth = model.AcademicSessionStartMonth;

                    dbSchool.PrincipalName = model.PrincipalName;
                    dbSchool.PrincipalMobileNo = model.PrincipalMobileNo;
                    dbSchool.PrincipalEmail = model.PrincipalEmail;

                    dbSchool.ManagerName = model.ManagerName;
                    dbSchool.ManagerMobileNo = model.ManagerMobileNo;

                    dbSchool.UDISECode = model.UDISECode;
                    dbSchool.PANNo = model.PANNo;
                    dbSchool.GSTNo = model.GSTNo;

                    dbSchool.EstablishedYear = model.EstablishedYear;
                    dbSchool.RecognitionText = model.RecognitionText;
                    dbSchool.ReportCardFooterText = model.ReportCardFooterText;
                    dbSchool.TCFooterText = model.TCFooterText;
                    dbSchool.IsActive = model.IsActive;
                    dbSchool.UpdatedBy = User.Identity.Name;
                    dbSchool.UpdatedDate = DateTime.Now;

                    if (!string.IsNullOrEmpty(model.LogoPath))
                    {
                        dbSchool.LogoPath = model.LogoPath;
                    }
                }

                await _context.SaveChangesAsync();

                //TempData["Success"] = model.SchoolId == 0
                //    ? "School information saved successfully."
                //    : "School information updated successfully.";
                SuccessMessage(model.SchoolId == 0 ? "School information saved successfully." : "School information updated successfully.");

                return RedirectToAction(nameof(SchoolMasters));
            }
            catch (Exception ex)
            {
                ErrorMessage(ex.Message);

                return View("SchoolMasters", model);
            }
        }

        #endregion School Masters
        #region Roles
        public async Task<IActionResult> Roles(string id = null)
        {
            ViewBag.Roles = _roleManager.Roles
                                       .OrderBy(x => x.Name)
                                       .ToList();

            if (string.IsNullOrEmpty(id))
                return View(new ApplicationRole());

            var role = await _roleManager.FindByIdAsync(id);

            return View(role);
        }
        [HttpPost]
        public async Task<IActionResult> Roles(ApplicationRole model)
        {
            try
            {
                //if (string.IsNullOrEmpty(model.Id))
                //{
                //    var role = new ApplicationRole
                //    {
                //        Name = model.Name,
                //        IsActive = true,
                //        CreatedBy= User.Identity.Name,
                //    };

                //    await _roleManager.CreateAsync(role);
                //    SuccessMessage("Role created successfully.");

                //}
                //else
                //{
                var role = await _roleManager.FindByIdAsync(model.Id);

                if (role != null)
                {
                    role.Name = model.Name;
                    role.Description = model.Description;
                    role.NormalizedName = model.NormalizedName;
                    role.UpdatedOn = DateTime.Now;
                    role.UpdatedBy = User.Identity?.Name;

                    await _roleManager.UpdateAsync(role);
                    SuccessMessage("Role updated successfully.");
                }
                else
                {
                    var newrole = new ApplicationRole
                    {
                        Name = model.Name,
                        Description = model.Description,
                        NormalizedName = model.NormalizedName,
                        IsActive = true,
                        CreatedBy = User.Identity.Name,
                    };
                    await _roleManager.CreateAsync(newrole);
                    SuccessMessage("Role created successfully.");
                }
                //}

                return RedirectToAction(nameof(Roles));
            }
            catch (Exception ex)
            {
                ErrorMessage(ex.Message);
                return View(model);
            }
        }
        [HttpPost]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoles(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ErrorMessage("Role not found.");
                return RedirectToAction(nameof(Roles));
            }

            role.IsActive = !role.IsActive;
            role.UpdatedOn = DateTime.Now;
            role.UpdatedBy = User.Identity?.Name;

            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                SuccessMessage(role.IsActive
                    ? "Role activated successfully."
                    : "Role deactivated successfully.");
            }
            else
            {
                ErrorMessage("Unable to update role status.");
            }

            return RedirectToAction(nameof(Roles));
        }
        #endregion Roles
        #region Subject Master
        public async Task<IActionResult> SubjectMaster(int? id)
        {
            SubjectMasterVM model = new SubjectMasterVM();

            if (id.HasValue)
            {
                model.Subject = _context.SubjectMasters
                                        .FirstOrDefault(x => x.SubjectId == id);
            }

            model.SubjectList = _context.SubjectMasters
                                        .OrderBy(x => x.SubjectName)
                                        .ToList();

            return View(model);
        }
        [HttpPost]
        public IActionResult SaveSubjectMaster(SubjectMasterVM model)
        {
            if (!ModelState.IsValid)
            {
                model.SubjectList = _context.SubjectMasters
                                            .OrderBy(x => x.SubjectName)
                                            .ToList();

                return View("SubjectMaster", model);
            }

            // Duplicate Validation
            bool exists = _context.SubjectMasters.Any(x =>
                x.SubjectName.Trim().ToLower() ==
                model.Subject.SubjectName.Trim().ToLower()
                && x.SubjectId != model.Subject.SubjectId);

            if (exists)
            {
                ErrorMessage("Subject already exists.");

                model.SubjectList = _context.SubjectMasters
                                            .OrderBy(x => x.SubjectName)
                                            .ToList();

                return View("SubjectMaster", model);
            }

            if (model.Subject.SubjectId > 0)
            {
                // Edit
                var dbSubject = _context.SubjectMasters
                                        .FirstOrDefault(x => x.SubjectId == model.Subject.SubjectId);

                if (dbSubject != null)
                {
                    dbSubject.SubjectName = model.Subject.SubjectName;
                    dbSubject.LanguageSubjectName = model.Subject.LanguageSubjectName;
                    dbSubject.IsLanguageSubject = model.Subject.IsLanguageSubject;
                }

                SuccessMessage("Subject Updated Successfully.");
            }
            else
            {
                // Create
                _context.SubjectMasters.Add(new SubjectMasters
                {
                    SubjectName = model.Subject.SubjectName,
                    LanguageSubjectName = model.Subject.LanguageSubjectName,
                    IsLanguageSubject = model.Subject.IsLanguageSubject,
                    IsActive = true
                });

                SuccessMessage("Subject Saved Successfully.");
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(SubjectMaster));
        }

        public IActionResult DeleteSubjectMaster(int id)
        {
            var subject = _context.SubjectMasters
                                  .FirstOrDefault(x => x.SubjectId == id);

            if (subject != null)
            {
                _context.SubjectMasters.Remove(subject);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(SubjectMaster));
        }
        #endregion Subject Master
        #region CLass Subject Mapping
        public async Task<IActionResult> ClassBatchSubjectMapping(long? headerId,int? filterClassId,int? filterBatchId)
        {
            ClassBatchSubjectVM model = new();

            model.Classes = GetDataListItems("Class");

            model.Batches = await _context.Batches
                .Where(x => x.IsActive && x.ActiveForAdmission)
                .OrderBy(x => x.StartDate)
                .ToListAsync();

            model.Subjects = await _context.SubjectMasters
                .Where(x => x.IsActive)
                .OrderBy(x => x.SubjectName)
                .ToListAsync();

            model.FilterClassId = filterClassId;
            model.FilterBatchId = filterBatchId;

            //----------------------------------------------------
            // EDIT
            //----------------------------------------------------

            if (headerId.HasValue)
            {
                var header = await _context.ClassBatchSubjectHeaders

                    .Include(x => x.Details)

                    .FirstOrDefaultAsync(x => x.HeaderId == headerId);

                if (header != null)
                {
                    model.HeaderId = header.HeaderId;

                    model.ClassId = header.ClassId;

                    model.BatchId = header.BatchId;

                    model.SubjectIds = header.Details

                        .Select(x => x.SubjectId)

                        .ToList();
                }
            }

            //----------------------------------------------------
            // GRID
            //----------------------------------------------------

            var query = _context.ClassBatchSubjectHeaders

                .Include(x => x.Details)

                .AsQueryable();

            if (filterClassId.HasValue)
                query = query.Where(x => x.ClassId == filterClassId);

            if (filterBatchId.HasValue)
                query = query.Where(x => x.BatchId == filterBatchId);

            model.ListData = await query

                .Select(x => new ClassBatchSubjectListVM
                {
                    HeaderId = x.HeaderId,

                    ClassName = _context.DataListItems

                        .Where(c => c.DataListItemId == x.ClassId)

                        .Select(c => c.DataListItemText)

                        .FirstOrDefault(),

                    BatchName = _context.Batches

                        .Where(c => c.BatchId == x.BatchId)

                        .Select(c => c.AcademicYear)

                        .FirstOrDefault(),

                    Subjects = string.Join(",",

                        x.Details.Select(d =>

                            _context.SubjectMasters

                            .Where(s => s.SubjectId == d.SubjectId)

                            .Select(s => s.SubjectName)

                            .FirstOrDefault()))
                })

                .ToListAsync();

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveClassBatchSubjectMapping(ClassBatchSubjectVM model)
        {
            model.Classes = GetDataListItems("Class");

            model.Batches = await _context.Batches
                .Where(x => x.IsActive && x.ActiveForAdmission)
                .ToListAsync();

            model.Subjects = await _context.SubjectMasters
                .Where(x => x.IsActive)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                return await ClassBatchSubjectMapping(model.HeaderId,model.FilterClassId,model.FilterBatchId);
            }

            //--------------------------------------------------
            // If Copy Batch selected then ignore manual selection
            //--------------------------------------------------

            List<int> subjectIds = new();

            if (model.CopyFromBatchId.HasValue)
            {
                subjectIds = await _context.ClassBatchSubjectHeaders

                    .Where(x =>
                        x.ClassId == model.ClassId &&
                        x.BatchId == model.CopyFromBatchId)

                    .SelectMany(x => x.Details)

                    .Select(x => x.SubjectId)

                    .Distinct()

                    .ToListAsync();

                if (subjectIds.Count == 0)
                {
                    ErrorMessage("No subject mapping found in selected Copy Batch.");

                    return RedirectToAction(nameof(ClassBatchSubjectMapping));
                }
            }
            else
            {
                subjectIds = model.SubjectIds.Distinct().ToList();
            }

            //--------------------------------------------------
            // UPDATE
            //--------------------------------------------------

            if (model.HeaderId.HasValue)
            {
                var header = await _context.ClassBatchSubjectHeaders

                    .Include(x => x.Details)

                    .FirstOrDefaultAsync(x => x.HeaderId == model.HeaderId);

                if (header == null)
                {
                    ErrorMessage("Record not found.");

                    return RedirectToAction(nameof(ClassBatchSubjectMapping));
                }

                header.ClassId = model.ClassId;

                header.BatchId = model.BatchId;

                header.UpdatedOn = DateTime.Now;

                _context.ClassBatchSubjectDetails.RemoveRange(header.Details);

                await _context.SaveChangesAsync();

                foreach (var subjectId in subjectIds)
                {
                    _context.ClassBatchSubjectDetails.Add(
                        new ClassBatchSubjectDetail
                        {
                            HeaderId = header.HeaderId,
                            SubjectId = subjectId
                        });
                }

                await _context.SaveChangesAsync();

                SuccessMessage("Updated Successfully");

                return RedirectToAction(nameof(ClassBatchSubjectMapping));
            }

            //--------------------------------------------------
            // DUPLICATE CHECK
            //--------------------------------------------------

            bool exists = await _context.ClassBatchSubjectHeaders

                .AnyAsync(x =>
                    x.ClassId == model.ClassId &&
                    x.BatchId == model.BatchId);

            if (exists)
            {
                ErrorMessage("This Class & Batch already exists.");

                return RedirectToAction(nameof(ClassBatchSubjectMapping));
            }

            //--------------------------------------------------
            // SAVE
            //--------------------------------------------------

            var newHeader = new ClassBatchSubjectHeader
            {
                ClassId = model.ClassId,
                BatchId = model.BatchId,
                CreatedOn = DateTime.Now,
                IsActive = true
            };

            _context.ClassBatchSubjectHeaders.Add(newHeader);

            await _context.SaveChangesAsync();

            foreach (var subjectId in subjectIds)
            {
                _context.ClassBatchSubjectDetails.Add(
                    new ClassBatchSubjectDetail
                    {
                        HeaderId = newHeader.HeaderId,
                        SubjectId = subjectId
                    });
            }

            await _context.SaveChangesAsync();

            SuccessMessage("Saved Successfully");

            return RedirectToAction(nameof(ClassBatchSubjectMapping));
        }
        [SkipPermission]
        [HttpGet]
        public async Task<JsonResult> GetSubjectsFromBatch(int classId, int batchId)
        {
            var subjectIds = await _context.ClassBatchSubjectHeaders

                .Where(x =>
                    x.ClassId == classId &&
                    x.BatchId == batchId)

                .SelectMany(x => x.Details)

                .Select(x => x.SubjectId)

                .Distinct()

                .ToListAsync();

            return Json(subjectIds);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteClassBatchSubjectMapping(long id)
        {
            var header = await _context.ClassBatchSubjectHeaders

                .Include(x => x.Details)

                .FirstOrDefaultAsync(x => x.HeaderId == id);

            if (header == null)
            {
                ErrorMessage("Record not found.");

                return RedirectToAction(nameof(ClassBatchSubjectMapping));
            }

            _context.ClassBatchSubjectDetails.RemoveRange(header.Details);

            _context.ClassBatchSubjectHeaders.Remove(header);

            await _context.SaveChangesAsync();

            SuccessMessage("Deleted Successfully");

            return RedirectToAction(nameof(ClassBatchSubjectMapping));
        }
        #endregion

    }
}

