// Data/Menu.cs
using Microsoft.AspNetCore.Identity;
using Shikhsa.Models;
using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shikhsa.Data
{
    public class Menu: BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }
        public string? Icon { get; set; }
        public int? ParentId { get; set; }
        public int DisplayOrder { get; set; } = 0;
      //  public bool IsActive { get; set; } = true;

        public Menu? Parent { get; set; }
        public ICollection<Menu> Children { get; set; } = new List<Menu>();
        public ICollection<RoleMenu> RoleMenus { get; set; } = new List<RoleMenu>();
    }
    public class MenuPermissionItem : BaseEntity
    {
        [Key]
        public int PermissionItemId { get; set; }

        [Required]
        public int SubMenuId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ControllerName { get; set; }

        [StringLength(100)]
        public string? ActionName { get; set; }

        public int DisplayOrder { get; set; }

        [ForeignKey(nameof(SubMenuId))]
        public virtual Menu? SubMenu { get; set; }
    }
    public class RolePermissionItem
    {
        [Key]
        public long Id { get; set; }

        public string RoleId { get; set; } = "";

        public string? UserId { get; set; }

        public int PermissionItemId { get; set; }

        public bool CanView { get; set; }

        public bool CanCreate { get; set; }

        public bool CanUpdate { get; set; }

        public bool CanDelete { get; set; }

        [ForeignKey(nameof(RoleId))]
        public ApplicationRole? Role { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        [ForeignKey(nameof(PermissionItemId))]
        public MenuPermissionItem? PermissionItem { get; set; }
    }
    public class PermissionItem : BaseEntity
    {
        [Key]
        public int PermissionItemId { get; set; }

        [Required]
        public int SubMenuId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = "";

        [StringLength(100)]
        public string? ControllerName { get; set; }

        [StringLength(100)]
        public string? ActionName { get; set; }

        public int DisplayOrder { get; set; }

        [ForeignKey(nameof(SubMenuId))]
        public virtual Menu? SubMenu { get; set; }
    }
    public class MenuTab : BaseEntity
    {
        [Key]
        public int TabId { get; set; }

        public int SubMenuId { get; set; }

        [Required]
        public string TabName { get; set; } = "";

        public int DisplayOrder { get; set; }

        [ForeignKey(nameof(SubMenuId))]
        public Menu? SubMenu { get; set; }
    }
}
