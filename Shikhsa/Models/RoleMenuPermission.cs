using Shikhsa.Models.Common;

namespace Shikhsa.Models
{
    public class RoleMenuPermission:BaseEntity
    {
        public int Id { get; set; }

        public string RoleId { get; set; } = "";
        public string UserId { get; set; } = "";
        public int MenuId { get; set; }

        public bool CanView { get; set; }

        public bool CanCreate { get; set; }

        public bool CanUpdate { get; set; }

        public bool CanDelete { get; set; }
    }
}
