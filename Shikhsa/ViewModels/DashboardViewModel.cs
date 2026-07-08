// ViewModels/DashboardViewModel.cs
namespace Shikhsa.ViewModels
{
    public class DashboardViewModel
    {
        public string UserFullName  { get; set; } = string.Empty;
        public string UserEmail     { get; set; } = string.Empty;
        public string UserRole      { get; set; } = string.Empty;

        public int TotalUsers  { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalRoles  { get; set; }
        public int TotalMenus  { get; set; }

        public List<RecentUserItem> RecentUsers { get; set; } = new();
    }

    public class RecentUserItem
    {
        public string   Id        { get; set; } = string.Empty;
        public string   FullName  { get; set; } = string.Empty;
        public string   Email     { get; set; } = string.Empty;
        public bool     IsActive  { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
