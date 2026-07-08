using Shikhsa.Models;

namespace Shikhsa.ViewModels
{
    public class DataListVM
    {
        public DataList DataList { get; set; } = new();

        public List<DataListItem> Items { get; set; } = new();
    }
    public enum EmailStatus
    {
        Pending = 1,
        Processing = 2,
        Sent = 3,
        Failed = 4,
        Delivered = 5,
        Bounced = 6
    }
    public class AttendanceTypeVM
    {
        public AttendanceType AttendanceType { get; set; } = new();

        public List<AttendanceType> AttendanceTypes { get; set; } = new();
    }
}
