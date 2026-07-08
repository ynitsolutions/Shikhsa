using Microsoft.AspNetCore.Mvc.Rendering;
using Shikhsa.Data;

namespace Shikhsa.ViewModels
{
    public class MenuManagementVM
    {
        // Menu Form
        public Menu MainMenu { get; set; } = new();

        // Submenu Form
        public Menu SubMenu { get; set; } = new();

        // List
        public List<Menu> Menus { get; set; } = new();

        // Parent Menu Dropdown
        public List<SelectListItem> ParentMenus { get; set; } = new();
    }
    public class PermissionCacheVM
    {
        public string UserId { get; set; } = "";

        public List<MenuCacheVM> Menus { get; set; } = new();

        /*public List<string> AllowedUrls { get; set; } = new();*/
        public List<PermissionItemVM> Permissions { get; set; } = new();
    }
    public class MenuCacheVM
    {
        public int Id { get; set; }
        public int DisplayOrder { get; set; }
        public string Name { get; set; } = "";

        public string? ControllerName { get; set; }

        public string? ActionName { get; set; }

        public int? ParentId { get; set; }

        public string? Icon { get; set; }
        public List<MenuCacheVM> Children { get; set; } = new();
    }
    public class PermissionItemVM
    {
        public int MenuId { get; set; }
        public string ControllerName { get; set; } = "";
        public string? ActionName { get; set; }

        public bool CanView { get; set; }

        public bool CanCreate { get; set; }

        public bool CanUpdate { get; set; }

        public bool CanDelete { get; set; }
    }
}
