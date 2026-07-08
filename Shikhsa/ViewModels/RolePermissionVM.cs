using Microsoft.AspNetCore.Mvc.Rendering;
using Shikhsa.Data;
using Shikhsa.Models.Common;

namespace Shikhsa.ViewModels
{
    public class RolePermissionVM:BaseEntity
    {
        public string RoleId { get; set; } = "";

        public int MenuId { get; set; }
        public string? UserId { get; set; }

        public List<SelectListItem> Roles { get; set; } = new();

        public List<SelectListItem> Menus { get; set; } = new();
      
        public List<RolePermissionListVM> PermissionList { get; set; } = new();
        public int? SubMenuId { get; set; }
        public int? TabId { get; set; }

        public List<PermissionItemVM> SubMenus { get; set; }
            = new();

        public List<PermissionItemVM> PermissionItems { get; set; }
            = new();

    }
    public class RolePermissionListVM
    {
        public int Id { get; set; }

        public string RoleName { get; set; } = "";

        public string MenuName { get; set; } = "";

        public bool CanView { get; set; }

        public bool CanCreate { get; set; }

        public bool CanUpdate { get; set; }

        public bool CanDelete { get; set; }
    }
    public class SubMenuPermissionVM
    {
        public int MenuId { get; set; }

        public string MenuName { get; set; } = "";

        public bool CanView { get; set; }

        public bool CanCreate { get; set; }

        public bool CanUpdate { get; set; }

        public bool CanDelete { get; set; }
    }
    //public class PermissionItemVM
    //{
    //    public int PermissionItemId { get; set; }

    //    public int SubMenuId { get; set; }

    //    public string Name { get; set; } = "";

    //    public string? ControllerName { get; set; }

    //    public string? ActionName { get; set; }

    //    public bool CanView { get; set; }

    //    public bool CanCreate { get; set; }

    //    public bool CanUpdate { get; set; }

    //    public bool CanDelete { get; set; }
    //}
    //public class PermissionItemVM
    //{
    //    public int PermissionItemId { get; set; }

    //    public int SubMenuId { get; set; }

    //    public string Name { get; set; } = "";

    //    public string? ControllerName { get; set; }

    //    public string? ActionName { get; set; }

    //    public bool CanView { get; set; }

    //    public bool CanCreate { get; set; }

    //    public bool CanUpdate { get; set; }

    //    public bool CanDelete { get; set; }
    //}
}
