using Shikhsa.Models;

namespace Shikhsa.ViewModels
{
    public class StudentAttendanceRowVM
    {
        public long StudentId { get; set; }

        public int RollNo { get; set; }

        public string AdmissionNo { get; set; }

        public string StudentName { get; set; }

        public int AttendanceTypeId { get; set; }

        public string? Remark { get; set; }

        public bool IsFreeze { get; set; }
    }
    public class StudentAttendanceVM
    {
        public int BatchId { get; set; }

        public long StaffId { get; set; }

        public int ClassId { get; set; }

        public int SectionId { get; set; }

        public DateOnly AttendanceDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        public List<StaffMaster> StaffList { get; set; } = new();

        public List<Batches> Batches { get; set; } = new();

        public List<DataListItem> Classes { get; set; } = new();

        public List<DataListItem> Sections { get; set; } = new();

        public List<AttendanceType> AttendanceTypes { get; set; } = new();

        public List<StudentAttendanceRowVM> Students { get; set; } = new();
    }
}
