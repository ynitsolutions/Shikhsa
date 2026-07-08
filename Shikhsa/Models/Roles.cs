
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Shikhsa.Models
{
    public class ApplicationRole : IdentityRole
    {
        [StringLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime? UpdatedOn { get; set; }

        [StringLength(450)]
        public string? CreatedBy { get; set; }

        [StringLength(450)]
        public string? UpdatedBy { get; set; }
    }
    
}
