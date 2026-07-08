using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;


namespace Shikhsa.ViewModels
{
    public class StaffUserVM
    {
        public long StaffId { get; set; }

        public string RoleId { get; set; } = "";
        public string? UserId { get; set; }
        public List<SelectListItem> StaffList { get; set; } = new();

        public List<SelectListItem> RoleList { get; set; } = new();

        public List<StaffUserListVM> UserList { get; set; } = new();
    }
    public class StaffUserListVM
    {
        public string UserId { get; set; } = "";

        public long StaffId { get; set; }

        public string StaffName { get; set; } = "";

        public string UserName { get; set; } = "";

        public string Email { get; set; } = "";

        public string RoleName { get; set; } = "";

        public string Password { get; set; } = "";
        public bool? IsActive { get; set; }
    }

    public class ChangePasswordVM
    {
        public string UserId { get; set; } = "";

        public string UserName { get; set; } = "";

        public bool IsAdmin { get; set; }

        public string? OldPassword { get; set; }

        [Required]
        public string NewPassword { get; set; } = "";

        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = "";
    }
}
