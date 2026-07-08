// Data/RoleMenu.cs
using Microsoft.AspNetCore.Identity;
using Shikhsa.Models;
using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shikhsa.Data
{
    public class RoleMenu:BaseEntity
    {
        public int Id { get; set; }
        public string RoleId { get; set; } = string.Empty;
        public int MenuId { get; set; }

        public bool CanView { get; set; } = true;
        public bool CanCreate { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;

        public ApplicationRole? Role { get; set; }
        public Menu? Menu { get; set; }
        public int? TabId { get; set; }

        [ForeignKey(nameof(TabId))]
        public MenuTab? Tab { get; set; }
    }
}
