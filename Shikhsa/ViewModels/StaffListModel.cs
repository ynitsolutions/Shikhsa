using Shikhsa.Models;

namespace Shikhsa.ViewModels
{
    public class StaffListModel
    {
        public long StaffId { get; set; }

        public string StaffCode { get; set; }

        public string StaffName { get; set; }

        public string MobileNo { get; set; }

        public string Email { get; set; }

        public string Department { get; set; }

        public string Designation { get; set; }

        public string StaffType { get; set; }

        public DateTime JoiningDate { get; set; }

        public bool IsActive { get; set; }
    }
    public class StaffAttendanceVM
    {
        public DateOnly AttendanceDate { get; set; }

        public string? Search { get; set; }

        //public List<StaffMaster> Staffs { get; set; } = new();

        //public List<StaffAttendance> Attendance { get; set; } = new();
        public List<AttendanceType> AttendanceTypes { get; set; } = new();

        public List<StaffAttendanceRowVM> Staffs { get; set; } = new();
    }
    public class AttendanceSummary
    {
        public int TotalStaff { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int HalfDay { get; set; }
        public int Leave { get; set; }
    }
    public class StaffAttendanceRowVM
{
    public long StaffId { get; set; }

    public string StaffCode { get; set; }

    public string StaffName { get; set; }

    public long AttendanceId { get; set; }

    public int AttendanceTypeId { get; set; }

    public string? Remarks { get; set; }
}
}
