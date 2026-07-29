using Shikhsa.Models;

namespace Shikhsa.ViewModels
{
    public class StudentAttendanceRowVM
    {
        public long AttendanceEntryId { get; set; }
        public long StudentId { get; set; }

        public int RollNo { get; set; }

        public string AdmissionNo { get; set; }

        public string StudentName { get; set; }

        public int AttendanceTypeId { get; set; }

        public string? Remark { get; set; }

        public bool IsFreeze { get; set; }
    }
    public class StudentAttendanceVM: StudentFilterVM
    {
      

        public DateOnly AttendanceDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        public List<AttendanceType> AttendanceTypes { get; set; } = new();

        public List<StudentAttendanceRowVM> Students { get; set; } = new();
    }

    public class AttendanceEntryVM
    {
        public int BatchId { get; set; }
        public int ClassId { get; set; }
        public int SectionId { get; set; }
        public int StaffId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public bool IsClassTeacher { get; set; }
        public List<AttendanceType> AttendanceTypes { get; set; } = new();
        public List<AttendanceStudentVM> Students { get; set; } = new();
    }

    //public class AttendanceTypeVM
    //{
    //    public int AttendanceTypeId { get; set; }
    //    public string Code { get; set; } = "";
    //    public string Name { get; set; } = "";
    //    public bool IsLeave { get; set; }
    //    public string? Color { get; set; }
    //}

    public class AttendanceStudentVM
    {
        public int StudentId { get; set; }
        public string AdmissionNo { get; set; } = "";
        public string StudentName { get; set; } = "";
        public int AttendanceEntryId { get; set; }
        public int AttendanceTypeId { get; set; }
        public bool IsFreeze { get; set; }
    }
}
