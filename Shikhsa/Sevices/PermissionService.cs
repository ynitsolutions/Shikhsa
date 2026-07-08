
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Shikhsa.Data;
using Shikhsa.Enums;
using Shikhsa.Helpers;
using Shikhsa.Models;
using Shikhsa.ViewModels;
using System.Text.Json;

namespace Shikhsa.Services
{
    public class PermissionService
    {
        private readonly IMemoryCache _cache;
        //private readonly IDistributedCache _cache;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionService(
            IMemoryCache cache,
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _cache = cache;
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        #region CacheUserPermissions
        public async Task CacheUserPermissions(string userId)
        {
            var cacheKey = $"PERMISSION_{userId}";


            var user = await _userManager
                .FindByIdAsync(userId);

            if (user == null)
                return;

            var roles = await _userManager
                .GetRolesAsync(user);

            var roleIds = await _db.Roles
                .Where(x => roles.Contains(x.Name!))
                .Select(x => x.Id)
                .ToListAsync();

            var permissions = await (
                from rm in _db.RoleMenus

                join m in _db.Menus
                    on rm.MenuId equals m.Id

                where roleIds.Contains(rm.RoleId) && m.IsActive

                select new PermissionItemVM
                {
                    MenuId = m.Id,

                    ControllerName = m.ControllerName,

                    ActionName = m.ActionName,

                    CanView = rm.CanView,

                    CanCreate = rm.CanCreate,

                    CanUpdate = rm.CanEdit,

                    CanDelete = rm.CanDelete
                }
            ).ToListAsync();

            var menus = await GetUserMenusAsync(userId);

            var data = new PermissionCacheVM
            {
                UserId = userId,

                Menus = menus,

                Permissions = permissions
            };

            _cache.Set( cacheKey, data,TimeSpan.FromHours(8));
        }

        #endregion

        public PermissionCacheVM? GetPermissions(string userId)
        {
            var cacheKey = $"PERMISSION_{userId}";

            _cache.TryGetValue(
                cacheKey,
                out PermissionCacheVM? data);

            return data;
        }

        // ==========================================
        // REMOVE CACHE
        // ==========================================
        public void RemoveUserCache(string userId)
        {
            var cacheKey = $"PERMISSION_{userId}";

            _cache.Remove(cacheKey);
        }

        // ==========================================
        // CHECK PERMISSION
        // ==========================================
        //public async Task<bool> HasPermission(string userId,string controller,string action)
        //{
        //    var data = GetPermissions(userId);

        //    if (data == null)
        //    {
        //        await CacheUserPermissions(userId);

        //        data = GetPermissions(userId);
        //    }

        //    if (data == null)
        //        return false;

        //    controller = controller?.ToLower() ?? "";
        //    action = action?.ToLower() ?? "";

        //    string pageAction = action;

        //    if (action.StartsWith("save"))
        //        pageAction = action.Replace("save", "");

        //    else if (action.StartsWith("update"))
        //        pageAction = action.Replace("update", "");

        //    else if (action.StartsWith("delete"))
        //        pageAction = action.Replace("delete", "");

        //    else if (action.StartsWith("get"))
        //        pageAction = action.Replace("get", "");

        //    var permission = data.Permissions
        //        .FirstOrDefault(x =>
        //            x.ControllerName!.ToLower() == controller
        //            && x.ActionName!.ToLower() == pageAction);

        //    if (permission == null)
        //        return false;

        //    if (action.StartsWith("save"))
        //        return permission.CanCreate;

        //    if (action.StartsWith("update"))
        //        return permission.CanEdit;

        //    if (action.StartsWith("delete"))
        //        return permission.CanDelete;

        //    return permission.CanView;
        //}
        public async Task<bool> HasPermission(
    string userId,
    string controller,
    string action,
    IDictionary<string, object?> actionArguments)
        {
            var data =  GetPermissions(userId);

            if (data == null)
            {
                await CacheUserPermissions(userId);

                data =  GetPermissions(userId);
            }

            if (data == null)
                return false;

            string pageAction = action;

            PermissionType permissionType;

            // =========================
            // DELETE
            // =========================

            if (action.StartsWith("Delete"))
            {
                pageAction =
                    action.Replace("Delete", "");

                permissionType =
                    PermissionType.Delete;
            }

            // =========================
            // SAVE
            // =========================

            else if (action.StartsWith("Save"))
            {
                pageAction =
                    action.Replace("Save", "");

                bool isEdit = false;

                foreach (var item in actionArguments.Values)
                {
                    if (item == null)
                        continue;

                    var idProp =
                        item.GetType().GetProperty("Id");

                    if (idProp != null)
                    {
                        var value =
                            Convert.ToInt32(
                                idProp.GetValue(item));

                        isEdit = value > 0;

                        break;
                    }
                }

                permissionType =
                    isEdit
                    ? PermissionType.Edit
                    : PermissionType.Create;
            }

            // =========================
            // NORMAL PAGE
            // =========================

            else
            {
                permissionType =
                    PermissionType.View;
            }

            var permission =
                data.Permissions.FirstOrDefault(x =>

                    !string.IsNullOrWhiteSpace(
                        x.ControllerName)

                    &&

                    !string.IsNullOrWhiteSpace(
                        x.ActionName)

                    &&

                    x.ControllerName.Equals(
                        controller,
                        StringComparison.OrdinalIgnoreCase)

                    &&

                    x.ActionName.Equals(
                        pageAction,
                        StringComparison.OrdinalIgnoreCase));

            if (permission == null)
                return false;

            return permissionType switch
            {
                PermissionType.View =>
                    permission.CanView,

                PermissionType.Create =>
                    permission.CanCreate,

                PermissionType.Edit =>
                    permission.CanUpdate,

                PermissionType.Delete =>
                    permission.CanDelete,

                _ => false
            };
        }
        public async Task<bool> HasViewPermissionForTag(
    string action,
    string type)
        {
            var sessionUser =
                _httpContextAccessor.HttpContext?
                .Session
                .GetObject<UserSessionVM>(
                    "CurrentUser");

            if (sessionUser == null)
                return false;

            var cache =
                 GetPermissions(
                    sessionUser.Id);

            if (cache == null)
                return false;

            var permission =
                cache.Permissions.FirstOrDefault(x =>
                    x.ActionName == action);

            if (permission == null)
                return false;

            return type switch
            {
                "View" => permission.CanView,
                "Create" => permission.CanCreate,
                "Edit" => permission.
                CanUpdate,
                "Delete" => permission.CanDelete,
                _ => false
            };
        }
        // ==========================================
        // GET USER MENUS
        // ==========================================
        private async Task<List<MenuCacheVM>> GetUserMenusAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return new List<MenuCacheVM>();
            var roleIds = await (from ur in _db.UserRoles
                                 join r in _db.Roles
                                 on ur.RoleId equals r.Id
                                 where ur.UserId == userId
                                 select r.Id
                                  ).ToListAsync();
            var allowedMenuIds = await _db.RoleMenus
                .Where(x => roleIds.Contains(x.RoleId) && x.CanView).Select(x => x.MenuId)
                .Distinct().ToListAsync();
            var data = await (from p in _db.Menus
                              join c in _db.Menus
                              on p.Id equals c.ParentId into childGroup
                              from child in childGroup.DefaultIfEmpty()
                              where (p.ParentId == null || p.ParentId == 0)
                              && p.IsActive && (allowedMenuIds.Contains(p.Id) || (child != null && allowedMenuIds.Contains(child.Id)
                              ))
                              orderby p.DisplayOrder, child.DisplayOrder

                              select new
                              {
                                  ParentMenuId = p.Id,

                                  ParentMenuName = p.Name,

                                  ParentController = p.ControllerName,

                                  ParentAction = p.ActionName,

                                  ParentIcon = p.Icon,

                                  ParentDisplayOrder = p.DisplayOrder,

                                  ChildMenuId = child != null ? child.Id : 0,

                                  ChildMenuName = child != null ? child.Name : "",

                                  ChildController = child != null ? child.ControllerName : "",

                                  ChildAction = child != null ? child.ActionName : "",

                                  ChildIcon = child != null ? child.Icon : "",

                                  ChildDisplayOrder = child != null ? child.DisplayOrder : 0,

                                  ChildParentId = child != null ? child.ParentId : null,

                                  ChildIsActive = child != null ? child.IsActive : false
                              }).ToListAsync();
            var menus = data
                .GroupBy(x => new
                {
                    x.ParentMenuId,
                    x.ParentMenuName,
                    x.ParentController,
                    x.ParentAction,
                    x.ParentIcon,
                    x.ParentDisplayOrder
                })
                .Select(g => new MenuCacheVM
                {
                    Id = g.Key.ParentMenuId,
                    Name = g.Key.ParentMenuName,
                    ControllerName = g.Key.ParentController,
                    ActionName =g.Key.ParentAction,
                    Icon = g.Key.ParentIcon,
                    DisplayOrder =g.Key.ParentDisplayOrder,
                    Children = g.Where(x => x.ChildMenuId != 0
                            && x.ChildIsActive
                            && allowedMenuIds.Contains(x.ChildMenuId))
                    .Select(x => new MenuCacheVM
                        {
                            Id = x.ChildMenuId,
                            Name = x.ChildMenuName,
                            ControllerName = x.ChildController,
                            ActionName =x.ChildAction,
                            Icon = x.ChildIcon,
                            ParentId =x.ChildParentId
                        }).ToList()
                }).OrderBy(x => x.DisplayOrder) .ToList();

            return menus;
        }

        private string GetPageAction(string action)
        {
            if (action.StartsWith("Save"))
                return action.Replace("Save", "");

            if (action.StartsWith("Update"))
                return action.Replace("Update", "");

            if (action.StartsWith("Delete"))
                return action.Replace("Delete", "");

            if (action.StartsWith("Get"))
                return action.Replace("Get", "");

            return action;
        }
    }
}

