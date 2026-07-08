// ViewModels/LoginViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Shikhsa.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
    public class UserSessionVM
    {
        public string Id { get; set; } = "";

        public string UserName { get; set; } = "";

        public string Email { get; set; } = "";

        public string FullName { get; set; } = "";

        public string PhoneNumber { get; set; } = "";

        public string RoleName { get; set; } = "";

        public string ProfileImage { get; set; } = "";
    }
}