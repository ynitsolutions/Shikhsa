using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
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


        public async Task<StudentAttendanceReportVM> GetAttendanceReportAsync(StudentAttendanceReportVM vm)
        {
            // Generate Date Columns
            vm.Dates.Clear();

            for (var date = vm.FromDate; date <= vm.ToDate; date = date.AddDays(1))
            {
                vm.Dates.Add(date);
            }

            // Load Students
           var students = await _context.Tbl_Students
                     .Where(x =>
                         x.IsActive &&
                         x.AdmitBatchId == vm.BatchId &&
                         x.AdmitClassId == vm.ClassId &&
                         x.AdmitSectionId == vm.SectionId)
                     .OrderBy(x => x.FirstName)
                     .Select(x => new
                     {
                         x.StudentId,
                         x.FirstName,
                         x.MiddleName,
                         x.LastName,
                         x.ApplicationNo,
                         x.ScholarNumber
                     })
                     .ToListAsync();

            vm.Students = students.Select(x => new StudentAttendanceReportRowVM
            {
                StudentId = x.StudentId,
                RollNo = 0,
                AdmissionNo = x.ScholarNumber ?? x.ApplicationNo ?? "",
                StudentName = string.Join(" ",
                    new[] { x.FirstName, x.MiddleName, x.LastName }
                    .Where(s => !string.IsNullOrWhiteSpace(s)))
            }).ToList();

            // Attendance Records
            var attendance = await
            (
                from a in _context.StudentAttendances

                join t in _context.AttendanceTypes
                    on a.AttendanceTypeId equals t.AttendanceTypeId

                where
                    a.BatchId == vm.BatchId &&
                    a.ClassId == vm.ClassId &&
                    a.SectionId == vm.SectionId &&
                    a.AttendanceDate >= vm.FromDate &&
                    a.AttendanceDate <= vm.ToDate

                select new
                {
                    a.StudentId,
                    a.AttendanceDate,
                    t.Code,
                    t.IsLeave
                }

            ).ToListAsync();

            // Fast Lookup
            var attendanceLookup = attendance.ToDictionary(
                x => (x.StudentId, x.AttendanceDate),
                x => new
                {
                    x.Code,
                    x.IsLeave
                });

            // Prepare Report
            foreach (var student in vm.Students)
            {
                foreach (var date in vm.Dates)
                {
                    if (attendanceLookup.TryGetValue((student.StudentId, date), out var att))
                    {
                        student.Attendance[date] = att.Code;

                        switch (att.Code.ToUpper())
                        {
                            case "P":
                                student.TotalPresent++;
                                break;
                            case "H":
                                student.TotalPresent++;
                                break;
                            case "O":
                                student.TotalPresent++;
                                break;

                            case "A":
                                student.TotalAbsent++;
                                break;

                            default:

                                if (att.IsLeave)
                                    student.TotalLeave++;

                                else
                                    student.TotalAbsent++;

                                break;
                        }
                    }
                    else
                    {
                        student.Attendance[date] = "";
                    }
                }

                var totalDays =
                    student.TotalPresent +
                    student.TotalAbsent +
                    student.TotalLeave;

                student.AttendancePercentage =
                    totalDays == 0
                        ? 0
                        : Math.Round((decimal)student.TotalPresent * 100 / totalDays, 2);
            }

            return vm;
        }
        public byte[] ExportAttendanceExcel(StudentAttendanceReportVM vm)
        {
            using var workbook = new XLWorkbook();

            var ws = workbook.Worksheets.Add("Attendance");

            int row = 1;

            int col = 1;
            ws.Cell(row, 1).Value = "Attendance Register";

            ws.Range(row, 1, row, vm.Dates.Count + 8).Merge();

            ws.Cell(row, 1).Style.Font.Bold = true;

            ws.Cell(row, 1).Style.Font.FontSize = 16;

            row += 2;
            col = 1;

            ws.Cell(row, col++).Value = "#";

            ws.Cell(row, col++).Value = "Roll";

            ws.Cell(row, col++).Value = "Admission No";

            ws.Cell(row, col++).Value = "Student Name";
            foreach (var date in vm.Dates)
            {
                ws.Cell(row, col).Value = date.ToString("dd/MM");

                col++;
            }
            ws.Cell(row, col++).Value = "Present";

            ws.Cell(row, col++).Value = "Leave";

            ws.Cell(row, col++).Value = "Absent";

            ws.Cell(row, col++).Value = "%";
            ws.Range(row, 1, row, col - 1).Style.Font.Bold = true;

            ws.Range(row, 1, row, col - 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            row++;

            int sr = 1;

            foreach (var student in vm.Students)
            {
                col = 1;

                ws.Cell(row, col++).Value = sr++;

                ws.Cell(row, col++).Value = student.RollNo;

                ws.Cell(row, col++).Value = student.AdmissionNo;

                ws.Cell(row, col++).Value = student.StudentName;

                foreach (var date in vm.Dates)
                {
                    string code = "";

                    student.Attendance.TryGetValue(date, out code);

                    var cell = ws.Cell(row, col);

                    cell.Value = code;
                    switch (code)
                    {
                        case "P":

                            cell.Style.Fill.BackgroundColor = XLColor.LightGreen;

                            break;

                        case "A":

                            cell.Style.Fill.BackgroundColor = XLColor.LightPink;

                            break;

                        case "L":

                            cell.Style.Fill.BackgroundColor = XLColor.LightYellow;

                            break;
                    }

                    col++;
                }
                ws.Cell(row, col++).Value = student.TotalPresent;

                ws.Cell(row, col++).Value = student.TotalLeave;

                ws.Cell(row, col++).Value = student.TotalAbsent;

                ws.Cell(row, col++).Value = student.AttendancePercentage;

                row++;
            }
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Alignment.Horizontal =XLAlignmentHorizontalValues.Center;

            ws.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.SheetView.FreezeRows(3);
            ws.SheetView.FreezeColumns(4);
            ws.Columns().AdjustToContents();
            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }
    }

}
