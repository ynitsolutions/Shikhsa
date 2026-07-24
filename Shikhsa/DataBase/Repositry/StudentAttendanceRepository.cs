using Shikhsa.Data;
using Shikhsa.Models;
using Shikhsa.ViewModels;

namespace Shikhsa.DataBase.Repositry
{
    public class StudentAttendanceRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentAttendanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<StudentAttendanceRowVM> LoadStudents(StudentAttendanceVM vm)
        {
            int Admitted = _context.DataListItems.Where(x => x.DataListItemValue == "Admitted" && x.IsActive).Select(x => x.DataListItemId).FirstOrDefault();
            var students = _context.Tbl_Students
                .Where(x =>
                    x.AdmitBatchId == vm.BatchId &&
                    x.AdmitClassId == vm.ClassId &&
                    x.AdmitSectionId == vm.SectionId &&x.Status==Admitted &&
                    x.IsActive)
                .OrderBy(x => x.FirstName).ThenBy(x=> x.MiddleName).ThenBy(x=> x.LastName)
                .Select(x => new StudentAttendanceRowVM
                {
                    StudentId = x.StudentId,
                   
                    AdmissionNo = x.ApplicationNo,
                    StudentName = x.FirstName+" "+x.MiddleName+" "+x.LastName,
                    AttendanceTypeId = 1
                })
                .ToList();

            var attendance = _context.StudentAttendances
                .Where(x =>
                    x.BatchId == vm.BatchId &&
                    x.ClassId == vm.ClassId &&
                    x.SectionId == vm.SectionId &&
                    x.AttendanceDate == vm.AttendanceDate)
                .ToList();

            foreach (var student in students)
            {
                var att = attendance.FirstOrDefault(x => x.StudentId == student.StudentId);

                if (att != null)
                {
                    student.AttendanceTypeId = att.AttendanceTypeId;
                    student.Remark = att.Remark;
                    student.IsFreeze = att.IsFreeze;
                }
            }

            return students;
        }
        public int Save(StudentAttendanceVM vm, string userName)
        {
            foreach (var row in vm.Students)
            {
                var attendance = _context.StudentAttendances
                    .FirstOrDefault(x =>
                        x.StudentId == row.StudentId &&
                        x.AttendanceDate == vm.AttendanceDate);

                if (attendance == null)
                {
                    attendance = new StudentAttendance
                    {
                        BatchId = vm.BatchId,
                        ClassId = vm.ClassId,
                        SectionId = vm.SectionId,
                        StudentId = row.StudentId,
                        AttendanceDate = vm.AttendanceDate,
                        AddedBy = userName,
                        AddedDate = DateTime.Now
                    };

                    _context.StudentAttendances.Add(attendance);
                }

                if (attendance.IsFreeze)
                    continue;

                attendance.AttendanceTypeId = row.AttendanceTypeId;
                attendance.Remark = row.Remark;
                attendance.IsFreeze = row.IsFreeze;
                attendance.UpdatedBy = userName;
                attendance.UpdatedDate = DateTime.Now;
            }

            return _context.SaveChanges();
        }
    }

}
