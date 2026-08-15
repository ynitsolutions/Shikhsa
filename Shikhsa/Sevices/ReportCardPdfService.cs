//using Microsoft.AspNetCore.Hosting;
//using QuestPDF.Elements.Table;
//using QuestPDF.Fluent;
//using QuestPDF.Helpers;
//using QuestPDF.Infrastructure;
//using Shikhsa.ViewModels;

//namespace Shikhsa.Sevices
//{
//    public class ReportCardPdfService
//    {
//        // ================= BRAND COLORS =================
//        private const string NavyDark = "#1B3B6F";
//        private const string PaleBlue = "#EAF0FB";
//        private const string White = "#FFFFFF";
//        private const string TextGrey = "#1F2937";

//        private readonly IWebHostEnvironment _env;

//        public ReportCardPdfService(IWebHostEnvironment env)
//        {
//            _env = env;
//        }

//        public byte[] Generate(ReportCardVM vm)
//        {
//            QuestPDF.Settings.License =
//                LicenseType.Community;

//            return Document.Create(container =>
//            {
//                container.Page(page =>
//                {
//                    page.Size(PageSizes.A4);

//                    page.Margin(18);

//                    page.DefaultTextStyle(
//                        x => x.FontSize(8).FontColor(TextGrey));

//                    page.Content()
//                        .Border(2)
//                        .BorderColor(NavyDark)
//                        .Padding(8)
//                        .Column(col =>
//                        {
//                            col.Spacing(7);

//                            col.Item()
//                                .Element(x => DrawHeader(x, vm));

//                            col.Item()
//                                .Element(x => DrawSessionBanner(x, vm));

//                            col.Item()
//                                .Element(x => DrawStudentInfo(x, vm));

//                            col.Item()
//                                .Element(x => DrawScholastic(x, vm));

//                            col.Item().Row(row =>
//                            {
//                                row.RelativeItem(1.1f)
//                                    .Element(x => DrawCoScholastic(x, vm));

//                                row.ConstantItem(6);

//                                row.RelativeItem(1.0f)
//                                    .Element(x => DrawAttendance(x, vm));

//                                row.ConstantItem(6);

//                                row.RelativeItem(1.3f)
//                                    .Element(x => DrawGradingScale(x, vm));
//                            });

//                            col.Item()
//                                .Element(x => DrawRemark(x, vm));

//                            col.Item()
//                                .Element(x => DrawSummary(x, vm));

//                            col.Item()
//                                .PaddingTop(14)
//                                .Element(x => DrawSignature(x, vm));

//                            col.Item()
//                                .PaddingTop(2)
//                                .AlignCenter()
//                                .Text("Learning Today, Leading Tomorrow")
//                                .Italic()
//                                .FontSize(8)
//                                .FontColor(NavyDark);
//                        });
//                });

//            }).GeneratePdf();
//        }


//        // =========================================================
//        // HEADER
//        // =========================================================

//        private void DrawHeader(
//            IContainer container,
//            ReportCardVM vm)
//        {
//            container
//                .Border(1)
//                .BorderColor(NavyDark)
//                .Background(PaleBlue)
//                .Padding(8)
//                .Row(row =>
//                {
//                    // Logo
//                    var logoPath = ResolveImagePath(vm.Student.LogoPath);

//                    row.ConstantItem(65)
//                        .Height(65)
//                        .Border(1.5f)
//                        .BorderColor(NavyDark)
//                        .Background(White)
//                        .Padding(2)
//                        .Element(logoBox =>
//                        {
//                            if (logoPath != null)
//                            {
//                                logoBox.Image(logoPath).FitArea();
//                            }
//                            else
//                            {
//                                logoBox
//                                    .AlignCenter()
//                                    .AlignMiddle()
//                                    .Text("LOGO")
//                                    .FontSize(8)
//                                    .FontColor(NavyDark);
//                            }
//                        });

//                    row.RelativeItem()
//                        .PaddingHorizontal(10)
//                        .Column(col =>
//                        {
//                            col.Item()
//                                .AlignCenter()
//                                .Text("SCHOOL REPORT CARD")
//                                .FontSize(20)
//                                .Bold()
//                                .FontColor(NavyDark);

//                            col.Item()
//                                .AlignCenter()
//                                .Text(vm.Student.SchoolName)
//                                .FontSize(13)
//                                .Bold()
//                                .FontColor(NavyDark);

//                            if (!string.IsNullOrWhiteSpace(
//                                vm.Student.SchoolMotto))
//                            {
//                                col.Item()
//                                    .AlignCenter()
//                                    .Text(
//                                        vm.Student.SchoolMotto)
//                                    .Italic()
//                                    .FontSize(8);
//                            }

//                            col.Item()
//                                .AlignCenter()
//                                .Text(
//                                    vm.Student.SchoolAddress)
//                                .FontSize(7.5f);

//                            col.Item()
//                                .AlignCenter()
//                                .Text(
//                                    $"Phone: {vm.Student.MobileContactNo}   |   " +
//                                    $"Email: {vm.Student.Email}")
//                                .FontSize(7.5f);

//                            col.Item()
//                                .PaddingTop(3)
//                                .AlignCenter()
//                                .Text(
//                                    $"Board: {vm.Student.Board}")
//                                .FontSize(9)
//                                .Bold()
//                                .FontColor(NavyDark);
//                        });

//                    // Photo
//                    var photoPath = ResolveImagePath(vm.Student.StudentPhoto);

//                    row.ConstantItem(65)
//                        .Height(65)
//                        .Border(1.5f)
//                        .BorderColor(NavyDark)
//                        .Background(White)
//                        .Padding(2)
//                        .Element(photoBox =>
//                        {
//                            if (photoPath != null)
//                            {
//                                photoBox.Image(photoPath).FitArea();
//                            }
//                            else
//                            {
//                                photoBox
//                                    .AlignCenter()
//                                    .AlignMiddle()
//                                    .Text("PHOTO")
//                                    .FontSize(8)
//                                    .FontColor(NavyDark);
//                            }
//                        });
//                });
//        }


//        // =========================================================
//        // IMAGE PATH RESOLUTION
//        // Converts a stored relative/web path (e.g.
//        // "/UploadedImages/SchoolLogo/xxx.png") into a physical path
//        // under wwwroot, and returns null if it can't be found so the
//        // caller can fall back to the placeholder box.
//        // =========================================================

//        private string? ResolveImagePath(string? relativePath)
//        {
//            if (string.IsNullOrWhiteSpace(relativePath))
//                return null;

//            var webRoot = _env.WebRootPath;

//            if (string.IsNullOrWhiteSpace(webRoot))
//                return null;

//            var cleanedPath = relativePath
//                .Replace('/', Path.DirectorySeparatorChar)
//                .TrimStart(Path.DirectorySeparatorChar);

//            var physicalPath = Path.Combine(webRoot, cleanedPath);

//            return File.Exists(physicalPath)
//                ? physicalPath
//                : null;
//        }


//        // =========================================================
//        // SESSION BANNER
//        // =========================================================

//        private void DrawSessionBanner(
//            IContainer container,
//            ReportCardVM vm)
//        {
//            container
//                .Background(NavyDark)
//                .Padding(5)
//                .AlignCenter()
//                .Text($"SESSION : {vm.Student.AcademicYear}")
//                .FontColor(White)
//                .Bold()
//                .FontSize(10);
//        }


//        // =========================================================
//        // STUDENT INFORMATION
//        // =========================================================

//        private void DrawStudentInfo(
//            IContainer container,
//            ReportCardVM vm)
//        {
//            container
//                .Border(1)
//                .BorderColor(NavyDark)
//                .Table(table =>
//                {
//                    table.ColumnsDefinition(columns =>
//                    {
//                        columns.RelativeColumn();
//                        columns.RelativeColumn(1.5f);
//                        columns.RelativeColumn();
//                        columns.RelativeColumn(1.5f);
//                    });

//                    InfoCell(
//                        table,
//                        "Student Name",
//                        vm.Student.StudentName);

//                    InfoCell(
//                        table,
//                        "Scholar No.",
//                        vm.Student.ScholarNumber);

//                    InfoCell(
//                        table,
//                        "Class",
//                        vm.Student.ClassName);

//                    InfoCell(
//                        table,
//                        "Section",
//                        vm.Student.SectionName);

//                    InfoCell(
//                        table,
//                        "Father's Name",
//                        vm.Student.FatherName);

//                    InfoCell(
//                        table,
//                        "Mother's Name",
//                        vm.Student.MotherName);

//                    InfoCell(
//                        table,
//                        "DOB",
//                        vm.Student.DOB == default
//                            ? "-"
//                            : vm.Student.DOB
//                                .ToString("dd-MM-yyyy"));

//                    InfoCell(
//                        table,
//                        "Admission No.",
//                        vm.Student.ApplicationNo);
//                });
//        }


//        // =========================================================
//        // SCHOLASTIC
//        // =========================================================

//        private void DrawScholastic(
//            IContainer container,
//            ReportCardVM vm)
//        {
//            var examTypes = vm.ScholasticMarks
//                .SelectMany(x => x.ExamMarks)
//                .GroupBy(x => x.ExamTypeId)
//                .Select(x => x.First())
//                .OrderBy(x => x.DisplayOrder)
//                .ToList();

//            container.Column(section =>
//            {
//                section.Item()
//                    .Background(NavyDark)
//                    .Padding(4)
//                    .AlignCenter()
//                    .Text("SCHOLASTIC AREA")
//                    .Bold()
//                    .FontSize(9)
//                    .FontColor(White);

//                section.Item()
//                    .Border(1)
//                    .BorderColor(NavyDark)
//                    .Table(table =>
//                    {
//                        table.ColumnsDefinition(columns =>
//                        {
//                            columns.RelativeColumn(3);

//                            foreach (var examType in examTypes)
//                                columns.RelativeColumn(1.2f);

//                            columns.RelativeColumn(1.4f);
//                            columns.RelativeColumn(1.2f);
//                            columns.RelativeColumn(1.2f);
//                        });


//                        table.Header(header =>
//                        {
//                            header.Cell()
//                                .Border(0.5f)
//                                .Background(PaleBlue)
//                                .Padding(3)
//                                .Text("Subject")
//                                .Bold();

//                            foreach (var examType in examTypes)
//                            {
//                                header.Cell()
//                                    .Border(0.5f)
//                                    .Background(PaleBlue)
//                                    .Padding(3)
//                                    .AlignCenter()
//                                    .Text(examType.ExamTypeName)
//                                    .Bold();
//                            }

//                            header.Cell()
//                                .Border(0.5f)
//                                .Background(PaleBlue)
//                                .Padding(3)
//                                .AlignCenter()
//                                .Text("Total")
//                                .Bold();

//                            header.Cell()
//                                .Border(0.5f)
//                                .Background(PaleBlue)
//                                .Padding(3)
//                                .AlignCenter()
//                                .Text("Grade")
//                                .Bold();

//                            header.Cell()
//                                .Border(0.5f)
//                                .Background(PaleBlue)
//                                .Padding(3)
//                                .AlignCenter()
//                                .Text("Result")
//                                .Bold();
//                        });


//                        foreach (var subject in vm.ScholasticMarks)
//                        {
//                            table.Cell()
//                                .Border(0.5f)
//                                .Padding(3)
//                                .Text(subject.SubjectName);

//                            foreach (var examType in examTypes)
//                            {
//                                var mark =
//                                    subject.ExamMarks.FirstOrDefault(
//                                        x => x.ExamTypeId ==
//                                             examType.ExamTypeId);

//                                table.Cell()
//                                    .Border(0.5f)
//                                    .Padding(3)
//                                    .AlignCenter()
//                                    .Text(
//                                        mark == null
//                                            ? "-"
//                                            : mark.IsAbsent
//                                                ? "AB"
//                                                : mark.ObtainedMarks
//                                                    .ToString("0.##"));
//                            }

//                            table.Cell()
//                                .Border(0.5f)
//                                .Padding(3)
//                                .AlignCenter()
//                                .Text(
//                                    $"{subject.TotalObtainedMarks:0.##}/" +
//                                    $"{subject.TotalMaximumMarks:0.##}");

//                            table.Cell()
//                                .Border(0.5f)
//                                .Padding(3)
//                                .AlignCenter()
//                                .Text(subject.Grade);

//                            table.Cell()
//                                .Border(0.5f)
//                                .Padding(3)
//                                .AlignCenter()
//                                .Text(subject.Result);
//                        }


//                        // Grand Total row
//                        table.Cell()
//                            .Border(0.5f)
//                            .Background(PaleBlue)
//                            .Padding(3)
//                            .Text("Grand Total")
//                            .Bold();

//                        foreach (var examType in examTypes)
//                        {
//                            var total = vm.ScholasticMarks
//                                .SelectMany(x => x.ExamMarks)
//                                .Where(x =>
//                                    x.ExamTypeId == examType.ExamTypeId &&
//                                    !x.IsAbsent)
//                                .Sum(x => x.ObtainedMarks);

//                            table.Cell()
//                                .Border(0.5f)
//                                .Background(PaleBlue)
//                                .Padding(3)
//                                .AlignCenter()
//                                .Text(total.ToString("0.##"))
//                                .Bold();
//                        }

//                        table.Cell()
//                            .Border(0.5f)
//                            .Background(PaleBlue)
//                            .Padding(3)
//                            .AlignCenter()
//                            .Text(
//                                $"{vm.Summary.TotalObtainedMarks:0.##}/" +
//                                $"{vm.Summary.TotalMaximumMarks:0.##}")
//                            .Bold();

//                        table.Cell()
//                            .Border(0.5f)
//                            .Background(PaleBlue)
//                            .Padding(3)
//                            .AlignCenter()
//                            .Text(vm.Summary.Grade)
//                            .Bold();

//                        table.Cell()
//                            .Border(0.5f)
//                            .Background(PaleBlue)
//                            .Padding(3)
//                            .AlignCenter()
//                            .Text(vm.Summary.Result)
//                            .Bold();
//                    });
//            });
//        }


//        // =========================================================
//        // CO-SCHOLASTIC
//        // =========================================================

//        private void DrawCoScholastic(
//            IContainer container,
//            ReportCardVM vm)
//        {
//            container.Column(col =>
//            {
//                col.Item()
//                    .Background(NavyDark)
//                    .Padding(4)
//                    .AlignCenter()
//                    .Text("CO-SCHOLASTIC AREA")
//                    .Bold()
//                    .FontSize(8)
//                    .FontColor(White);

//                col.Item()
//                    .Border(1)
//                    .BorderColor(NavyDark)
//                    .Table(table =>
//                    {
//                        table.ColumnsDefinition(columns =>
//                        {
//                            columns.RelativeColumn(3);
//                            columns.RelativeColumn(1.5f);
//                        });

//                        table.Header(header =>
//                        {
//                            header.Cell()
//                                .Border(0.5f)
//                                .Background(PaleBlue)
//                                .Padding(3)
//                                .Text("Activity")
//                                .Bold();

//                            header.Cell()
//                                .Border(0.5f)
//                                .Background(PaleBlue)
//                                .Padding(3)
//                                .AlignCenter()
//                                .Text("Grade")
//                                .Bold();
//                        });

//                        if (vm.CoScholasticMarks.Count == 0)
//                        {
//                            table.Cell()
//                                .Border(0.5f)
//                                .Padding(3)
//                                .Text("-");

//                            table.Cell()
//                                .Border(0.5f)
//                                .Padding(3)
//                                .AlignCenter()
//                                .Text("-");
//                        }
//                        else
//                        {
//                            foreach (var item in vm.CoScholasticMarks)
//                            {
//                                table.Cell()
//                                    .Border(0.5f)
//                                    .Padding(3)
//                                    .Text(item.Title);

//                                table.Cell()
//                                    .Border(0.5f)
//                                    .Padding(3)
//                                    .AlignCenter()
//                                    .Text(item.Grade);
//                            }
//                        }
//                    });
//            });
//        }


//        // =========================================================
//        // ATTENDANCE
//        // =========================================================

//        private void DrawAttendance(
//            IContainer container,
//            ReportCardVM vm)
//        {
//            container.Column(col =>
//            {
//                col.Item()
//                    .Background(NavyDark)
//                    .Padding(4)
//                    .AlignCenter()
//                    .Text("ATTENDANCE")
//                    .Bold()
//                    .FontSize(8)
//                    .FontColor(White);

//                col.Item()
//                    .Border(1)
//                    .BorderColor(NavyDark)
//                    .Table(table =>
//                    {
//                        table.ColumnsDefinition(columns =>
//                        {
//                            columns.RelativeColumn();
//                            columns.RelativeColumn();
//                        });

//                        AttendanceRow(table, "Working Days", vm.Attendance.WorkingDays.ToString());
//                        AttendanceRow(table, "Present", vm.Attendance.PresentDays.ToString());
//                        AttendanceRow(table, "Absent", vm.Attendance.AbsentDays.ToString());
//                        AttendanceRow(table, "Leave", vm.Attendance.LeaveDays.ToString());
//                        AttendanceRow(table, "Percentage", $"{vm.Attendance.AttendancePercentage:0.##}%");
//                    });
//            });
//        }

//        private void AttendanceRow(
//            TableDescriptor table,
//            string label,
//            string value)
//        {
//            table.Cell()
//                .Border(0.5f)
//                .Background(PaleBlue)
//                .Padding(3)
//                .Text(label)
//                .Bold();

//            table.Cell()
//                .Border(0.5f)
//                .Padding(3)
//                .AlignCenter()
//                .Text(value);
//        }


//        // =========================================================
//        // GRADING SCALE
//        // =========================================================

//        private void DrawGradingScale(
//            IContainer container,
//            ReportCardVM vm)
//        {
//            container.Column(col =>
//            {
//                col.Item()
//                    .Background(NavyDark)
//                    .Padding(4)
//                    .AlignCenter()
//                    .Text("GRADING SCALE")
//                    .Bold()
//                    .FontSize(8)
//                    .FontColor(White);

//                col.Item()
//                    .Border(1)
//                    .BorderColor(NavyDark)
//                    .Table(table =>
//                    {
//                        table.ColumnsDefinition(columns =>
//                        {
//                            columns.RelativeColumn();
//                            columns.RelativeColumn(1.2f);
//                            columns.RelativeColumn(1.8f);
//                        });

//                        table.Header(header =>
//                        {
//                            header.Cell()
//                                .Border(0.5f)
//                                .Background(PaleBlue)
//                                .Padding(3)
//                                .AlignCenter()
//                                .Text("Grade")
//                                .Bold();

//                            header.Cell()
//                                .Border(0.5f)
//                                .Background(PaleBlue)
//                                .Padding(3)
//                                .AlignCenter()
//                                .Text("Marks (%)")
//                                .Bold();

//                            header.Cell()
//                                .Border(0.5f)
//                                .Background(PaleBlue)
//                                .Padding(3)
//                                .AlignCenter()
//                                .Text("Remark")
//                                .Bold();
//                        });

//                        var ranges =
//                            vm.GradeRanges != null && vm.GradeRanges.Count > 0
//                                ? vm.GradeRanges
//                                : DefaultGradeRanges();

//                        foreach (var g in ranges)
//                        {
//                            table.Cell()
//                                .Border(0.5f)
//                                .Padding(3)
//                                .AlignCenter()
//                                .Text(g.Grade);

//                            table.Cell()
//                                .Border(0.5f)
//                                .Padding(3)
//                                .AlignCenter()
//                                .Text($"{g.MinPercentage:0.#}-{g.MaxPercentage:0.#}");

//                            table.Cell()
//                                .Border(0.5f)
//                                .Padding(3)
//                                .AlignCenter()
//                                .Text(g.Description ?? "-");
//                        }
//                    });
//            });
//        }

//        private List<GradeRangeVM> DefaultGradeRanges()
//        {
//            return new List<GradeRangeVM>
//            {
//                new GradeRangeVM { Grade = "A+", MinPercentage = 91, MaxPercentage = 100, Description = "Outstanding" },
//                new GradeRangeVM { Grade = "A",  MinPercentage = 81, MaxPercentage = 90,  Description = "Excellent" },
//                new GradeRangeVM { Grade = "B+", MinPercentage = 71, MaxPercentage = 80,  Description = "Very Good" },
//                new GradeRangeVM { Grade = "B",  MinPercentage = 61, MaxPercentage = 70,  Description = "Good" },
//                new GradeRangeVM { Grade = "C",  MinPercentage = 51, MaxPercentage = 60,  Description = "Average" },
//                new GradeRangeVM { Grade = "D",  MinPercentage = 41, MaxPercentage = 50,  Description = "Satisfactory" },
//                new GradeRangeVM { Grade = "E",  MinPercentage = 0,  MaxPercentage = 40,  Description = "Needs Improvement" },
//            };
//        }


//        // =========================================================
//        // TEACHER'S REMARK
//        // =========================================================

//        private void DrawRemark(
//            IContainer container,
//            ReportCardVM vm)
//        {
//            container.Column(col =>
//            {
//                col.Item()
//                    .Background(NavyDark)
//                    .Padding(4)
//                    .AlignCenter()
//                    .Text("TEACHER'S REMARK")
//                    .Bold()
//                    .FontSize(8)
//                    .FontColor(White);

//                col.Item()
//                    .Border(1)
//                    .BorderColor(NavyDark)
//                    .Padding(6)
//                    .MinHeight(28)
//                    .Text(vm.Summary.TeacherRemark ?? vm.TeacherRemark ?? "-");
//            });
//        }


//        // =========================================================
//        // SUMMARY
//        // =========================================================

//        private void DrawSummary(
//            IContainer container,
//            ReportCardVM vm)
//        {
//            container
//                .Border(1)
//                .BorderColor(NavyDark)
//                .Table(table =>
//                {
//                    table.ColumnsDefinition(
//                        columns =>
//                        {
//                            columns.RelativeColumn();
//                            columns.RelativeColumn();
//                            columns.RelativeColumn();
//                            columns.RelativeColumn();
//                            columns.RelativeColumn();
//                        });

//                    HeaderCell(table, "Total Maximum");
//                    HeaderCell(table, "Total Obtained");
//                    HeaderCell(table, "Percentage");
//                    HeaderCell(table, "Grade");
//                    HeaderCell(table, "Result");

//                    BodyCell(
//                        table,
//                        vm.Summary.TotalMaximumMarks.ToString("0.##"));

//                    BodyCell(
//                        table,
//                        vm.Summary.TotalObtainedMarks.ToString("0.##"));

//                    BodyCell(
//                        table,
//                        $"{vm.Summary.Percentage:0.##}%");

//                    BodyCell(
//                        table,
//                        vm.Summary.Grade,
//                        true);

//                    BodyCell(
//                        table,
//                        vm.Summary.Result,
//                        true);
//                });
//        }


//        // =========================================================
//        // SIGNATURE
//        // =========================================================

//        private void DrawSignature(
//            IContainer container,
//            ReportCardVM vm)
//        {
//            container.Row(row =>
//            {
//                SignatureBlock(row.RelativeItem(), "Parent / Guardian");
//                SignatureBlock(row.RelativeItem(), "Class Teacher");
//                SignatureBlock(row.RelativeItem(), "Principal");
//            });
//        }

//        private void SignatureBlock(
//            IContainer container,
//            string label)
//        {
//            container
//                .AlignCenter()
//                .PaddingHorizontal(10)
//                .Column(c =>
//                {
//                    c.Item().Height(24);

//                    c.Item()
//                        .LineHorizontal(0.75f)
//                        .LineColor(NavyDark);

//                    c.Item()
//                        .PaddingTop(2)
//                        .AlignCenter()
//                        .Text(label)
//                        .Bold()
//                        .FontColor(NavyDark)
//                        .FontSize(8);
//                });
//        }


//        // =========================================================
//        // TABLE HELPERS
//        // =========================================================

//        private void HeaderCell(
//            TableDescriptor table,
//            string text)
//        {
//            table.Cell()
//                .Border(0.5f)
//                .Background(PaleBlue)
//                .Padding(4)
//                .AlignCenter()
//                .Text(text)
//                .FontSize(7)
//                .Bold()
//                .FontColor(NavyDark);
//        }


//        private void BodyCell(
//            TableDescriptor table,
//            string? text,
//            bool bold = false)
//        {
//            var cell = table.Cell()
//                .Border(0.5f)
//                .Padding(4)
//                .AlignCenter();

//            if (bold)
//            {
//                cell.Text(text ?? "-")
//                    .FontSize(7)
//                    .Bold();
//            }
//            else
//            {
//                cell.Text(text ?? "-")
//                    .FontSize(7);
//            }
//        }


//        private void InfoCell(
//            TableDescriptor table,
//            string label,
//            string? value)
//        {
//            table.Cell()
//                .Border(0.5f)
//                .Background(PaleBlue)
//                .Padding(4)
//                .Text(label)
//                .FontSize(7)
//                .Bold()
//                .FontColor(NavyDark);

//            table.Cell()
//                .Border(0.5f)
//                .Padding(4)
//                .Text(value ?? "-")
//                .FontSize(7);
//        }
//    }
//}

using Microsoft.AspNetCore.Hosting;
using QuestPDF.Elements.Table;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Shikhsa.ViewModels;

namespace Shikhsa.Sevices
{
    public class ReportCardPdfService
    {
        // ================= BRAND COLORS =================
        private const string NavyDark = "#1B3B6F";
        private const string PaleBlue = "#EAF0FB";
        private const string White = "#FFFFFF";
        private const string TextGrey = "#1F2937";

        private readonly IWebHostEnvironment _env;

        public ReportCardPdfService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public byte[] Generate(ReportCardVM vm)
        {
            QuestPDF.Settings.License =
                LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);

                    page.Margin(18);

                    page.DefaultTextStyle(
                        x => x.FontSize(8).FontColor(TextGrey));

                    page.Content()
                        .Border(2)
                        .BorderColor(NavyDark)
                        .Padding(8)
                        .Column(col =>
                        {
                            col.Spacing(7);

                            col.Item()
                                .Element(x => DrawHeader(x, vm));

                            col.Item()
                                .Element(x => DrawSessionBanner(x, vm));

                            col.Item()
                                .Element(x => DrawStudentInfo(x, vm));

                            col.Item()
                                .Element(x => DrawScholastic(x, vm));

                            col.Item().Row(row =>
                            {
                                row.RelativeItem(1.1f)
                                    .Element(x => DrawCoScholastic(x, vm));

                                row.ConstantItem(6);

                                row.RelativeItem(1.0f)
                                    .Element(x => DrawAttendance(x, vm));

                                row.ConstantItem(6);

                                row.RelativeItem(1.3f)
                                    .Element(x => DrawGradingScale(x, vm));
                            });

                            col.Item()
                                .Element(x => DrawRemark(x, vm));

                            col.Item()
                                .Element(x => DrawSummary(x, vm));

                            col.Item()
                                .PaddingTop(14)
                                .Element(x => DrawSignature(x, vm));

                            col.Item()
                                .PaddingTop(2)
                                .AlignCenter()
                                .Text("Learning Today, Leading Tomorrow")
                                .Italic()
                                .FontSize(8)
                                .FontColor(NavyDark);
                        });
                });

            }).GeneratePdf();
        }


        // =========================================================
        // HEADER
        // =========================================================

        private void DrawHeader(
            IContainer container,
            ReportCardVM vm)
        {
            container
                .Border(1)
                .BorderColor(NavyDark)
                .Background(PaleBlue)
                .Padding(8)
                .Row(row =>
                {
                    // Logo
                    var logoPath = ResolveImagePath(vm.Student.LogoPath);

                    row.ConstantItem(65)
                        .Height(65)
                        .Border(1.5f)
                        .BorderColor(NavyDark)
                        .Background(White)
                        .Padding(2)
                        .Element(logoBox =>
                        {
                            if (logoPath != null)
                            {
                                logoBox.Image(logoPath).FitArea();
                            }
                            else
                            {
                                logoBox
                                    .AlignCenter()
                                    .AlignMiddle()
                                    .Text("LOGO")
                                    .FontSize(8)
                                    .FontColor(NavyDark);
                            }
                        });

                    row.RelativeItem()
                        .PaddingHorizontal(10)
                        .Column(col =>
                        {
                            col.Item()
                                .AlignCenter()
                                .Text("SCHOOL REPORT CARD")
                                .FontSize(20)
                                .Bold()
                                .FontColor(NavyDark);

                            col.Item()
                                .AlignCenter()
                                .Text(vm.Student.SchoolName)
                                .FontSize(13)
                                .Bold()
                                .FontColor(NavyDark);

                            if (!string.IsNullOrWhiteSpace(
                                vm.Student.SchoolMotto))
                            {
                                col.Item()
                                    .AlignCenter()
                                    .Text(
                                        vm.Student.SchoolMotto)
                                    .Italic()
                                    .FontSize(8);
                            }

                            col.Item()
                                .AlignCenter()
                                .Text(
                                    vm.Student.SchoolAddress)
                                .FontSize(7.5f);

                            col.Item()
                                .AlignCenter()
                                .Text(
                                    $"Phone: {vm.Student.MobileContactNo}   |   " +
                                    $"Email: {vm.Student.Email}")
                                .FontSize(7.5f);

                            col.Item()
                                .PaddingTop(3)
                                .AlignCenter()
                                .Text(
                                    $"Board: {vm.Student.Board}")
                                .FontSize(9)
                                .Bold()
                                .FontColor(NavyDark);
                        });

                    // Photo
                    var photoPath = ResolveImagePath(vm.Student.StudentPhoto);

                    row.ConstantItem(65)
                        .Height(65)
                        .Border(1.5f)
                        .BorderColor(NavyDark)
                        .Background(White)
                        .Padding(2)
                        .Element(photoBox =>
                        {
                            if (photoPath != null)
                            {
                                photoBox.Image(photoPath).FitArea();
                            }
                            else
                            {
                                photoBox
                                    .AlignCenter()
                                    .AlignMiddle()
                                    .Text("PHOTO")
                                    .FontSize(8)
                                    .FontColor(NavyDark);
                            }
                        });
                });
        }


        // =========================================================
        // IMAGE PATH RESOLUTION
        // Converts a stored relative/web path (e.g.
        // "/UploadedImages/SchoolLogo/xxx.png") into a physical path
        // under wwwroot, and returns null if it can't be found so the
        // caller can fall back to the placeholder box.
        // =========================================================

        private string? ResolveImagePath(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return null;

            var webRoot = _env.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRoot))
                return null;

            var cleanedPath = relativePath
                .Replace('/', Path.DirectorySeparatorChar)
                .TrimStart(Path.DirectorySeparatorChar);

            var physicalPath = Path.Combine(webRoot, cleanedPath);

            return File.Exists(physicalPath)
                ? physicalPath
                : null;
        }


        // =========================================================
        // SESSION BANNER
        // =========================================================

        private void DrawSessionBanner(
            IContainer container,
            ReportCardVM vm)
        {
            container
                .Background(NavyDark)
                .Padding(5)
                .AlignCenter()
                .Text($"SESSION : {vm.Student.AcademicYear}")
                .FontColor(White)
                .Bold()
                .FontSize(10);
        }


        // =========================================================
        // STUDENT INFORMATION
        // =========================================================

        private void DrawStudentInfo(
            IContainer container,
            ReportCardVM vm)
        {
            container
                .Border(1)
                .BorderColor(NavyDark)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn();
                        columns.RelativeColumn(1.5f);
                    });

                    InfoCell(
                        table,
                        "Student Name",
                        vm.Student.StudentName);

                    InfoCell(
                        table,
                        "Scholar No.",
                        vm.Student.ScholarNumber);

                    InfoCell(
                        table,
                        "Class",
                        vm.Student.ClassName);

                    InfoCell(
                        table,
                        "Section",
                        vm.Student.SectionName);

                    InfoCell(
                        table,
                        "Father's Name",
                        vm.Student.FatherName);

                    InfoCell(
                        table,
                        "Mother's Name",
                        vm.Student.MotherName);

                    InfoCell(
                        table,
                        "DOB",
                        vm.Student.DOB == default
                            ? "-"
                            : vm.Student.DOB
                                .ToString("dd-MM-yyyy"));

                    InfoCell(
                        table,
                        "Admission No.",
                        vm.Student.ApplicationNo);
                });
        }


        // =========================================================
        // SCHOLASTIC
        // Branches to the consolidated "Final" table when
        // vm.IsFinalReport is true; otherwise draws the normal
        // single-category flat table exactly as before.
        // =========================================================

        private void DrawScholastic(
            IContainer container,
            ReportCardVM vm)
        {
            if (vm.IsFinalReport && vm.FinalScholasticMarks.Count > 0)
            {
                DrawFinalScholastic(container, vm);
                return;
            }

            var examTypes = vm.ScholasticMarks
                .SelectMany(x => x.ExamMarks)
                .GroupBy(x => x.ExamTypeId)
                .Select(x => x.First())
                .OrderBy(x => x.DisplayOrder)
                .ToList();

            container.Column(section =>
            {
                section.Item()
                    .Background(NavyDark)
                    .Padding(4)
                    .AlignCenter()
                    .Text("SCHOLASTIC AREA")
                    .Bold()
                    .FontSize(9)
                    .FontColor(White);

                section.Item()
                    .Border(1)
                    .BorderColor(NavyDark)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);

                            foreach (var examType in examTypes)
                                columns.RelativeColumn(1.2f);

                            columns.RelativeColumn(1.4f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.2f);
                        });


                        table.Header(header =>
                        {
                            header.Cell()
                                .Border(0.5f)
                                .Background(PaleBlue)
                                .Padding(3)
                                .Text("Subject")
                                .Bold();

                            foreach (var examType in examTypes)
                            {
                                header.Cell()
                                    .Border(0.5f)
                                    .Background(PaleBlue)
                                    .Padding(3)
                                    .AlignCenter()
                                    .Text(examType.ExamTypeName)
                                    .Bold();
                            }

                            header.Cell()
                                .Border(0.5f)
                                .Background(PaleBlue)
                                .Padding(3)
                                .AlignCenter()
                                .Text("Total")
                                .Bold();

                            header.Cell()
                                .Border(0.5f)
                                .Background(PaleBlue)
                                .Padding(3)
                                .AlignCenter()
                                .Text("Grade")
                                .Bold();

                            header.Cell()
                                .Border(0.5f)
                                .Background(PaleBlue)
                                .Padding(3)
                                .AlignCenter()
                                .Text("Result")
                                .Bold();
                        });


                        foreach (var subject in vm.ScholasticMarks)
                        {
                            table.Cell()
                                .Border(0.5f)
                                .Padding(3)
                                .Text(subject.SubjectName);

                            foreach (var examType in examTypes)
                            {
                                var mark =
                                    subject.ExamMarks.FirstOrDefault(
                                        x => x.ExamTypeId ==
                                             examType.ExamTypeId);

                                table.Cell()
                                    .Border(0.5f)
                                    .Padding(3)
                                    .AlignCenter()
                                    .Text(
                                        mark == null
                                            ? "-"
                                            : mark.IsAbsent
                                                ? "AB"
                                                : mark.ObtainedMarks
                                                    .ToString("0.##"));
                            }

                            table.Cell()
                                .Border(0.5f)
                                .Padding(3)
                                .AlignCenter()
                                .Text(
                                    $"{subject.TotalObtainedMarks:0.##}/" +
                                    $"{subject.TotalMaximumMarks:0.##}");

                            table.Cell()
                                .Border(0.5f)
                                .Padding(3)
                                .AlignCenter()
                                .Text(subject.Grade);

                            table.Cell()
                                .Border(0.5f)
                                .Padding(3)
                                .AlignCenter()
                                .Text(subject.Result);
                        }


                        // Grand Total row
                        table.Cell()
                            .Border(0.5f)
                            .Background(PaleBlue)
                            .Padding(3)
                            .Text("Grand Total")
                            .Bold();

                        foreach (var examType in examTypes)
                        {
                            var total = vm.ScholasticMarks
                                .SelectMany(x => x.ExamMarks)
                                .Where(x =>
                                    x.ExamTypeId == examType.ExamTypeId &&
                                    !x.IsAbsent)
                                .Sum(x => x.ObtainedMarks);

                            table.Cell()
                                .Border(0.5f)
                                .Background(PaleBlue)
                                .Padding(3)
                                .AlignCenter()
                                .Text(total.ToString("0.##"))
                                .Bold();
                        }

                        table.Cell()
                            .Border(0.5f)
                            .Background(PaleBlue)
                            .Padding(3)
                            .AlignCenter()
                            .Text(
                                $"{vm.Summary.TotalObtainedMarks:0.##}/" +
                                $"{vm.Summary.TotalMaximumMarks:0.##}")
                            .Bold();

                        table.Cell()
                            .Border(0.5f)
                            .Background(PaleBlue)
                            .Padding(3)
                            .AlignCenter()
                            .Text(vm.Summary.Grade)
                            .Bold();

                        table.Cell()
                            .Border(0.5f)
                            .Background(PaleBlue)
                            .Padding(3)
                            .AlignCenter()
                            .Text(vm.Summary.Result)
                            .Bold();
                    });
            });
        }


        // =========================================================
        // "FINAL" REPORT CARD - CONSOLIDATED SCHOLASTIC TABLE
        // One column-group per exam category (First Periodic,
        // Second Periodic, Third Periodic, Half Yearly, Annual Exam
        // etc.), each showing either a Th/Pr breakdown + Total +
        // Grade, or just Total + Grade when the category has only
        // one exam type recorded (this is what naturally collapses
        // "Annual Exam" the way it appears in the reference layout).
        //
        // NOTE: relies on TableCellDescriptor.RowSpan()/ColumnSpan(),
        // available in current QuestPDF versions. If your installed
        // QuestPDF package predates that API, upgrade the package.
        // =========================================================

        private void DrawFinalScholastic(
            IContainer container,
            ReportCardVM vm)
        {
            var layout = vm.FinalCategoryLayout;
            var rows = vm.FinalScholasticMarks;

            // Columns per category: breakdown categories get
            // (exam types) + Total + Grade; simple categories get
            // just Total + Grade.
            int ColumnsFor(FinalCategoryLayoutVM cat) =>
                cat.ShowBreakdown
                    ? cat.ExamTypeNames.Count + 2
                    : 2;

            container.Column(section =>
            {
                section.Item()
                    .Background(NavyDark)
                    .Padding(4)
                    .AlignCenter()
                    .Text("SCHOLASTIC AREA (ACADEMIC PERFORMANCE)")
                    .Bold()
                    .FontSize(9)
                    .FontColor(White);

                section.Item()
                    .Border(1)
                    .BorderColor(NavyDark)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2.2f); // Subject

                            foreach (var cat in layout)
                            {
                                for (int i = 0; i < ColumnsFor(cat); i++)
                                    columns.RelativeColumn(1f);
                            }
                        });


                        table.Header(header =>
                        {
                            // Row 1: Subject (spans both header rows) +
                            // one spanning cell per category group.

                            header.Cell()
                                .RowSpan(2)
                                .Border(0.5f)
                                .Background(PaleBlue)
                                .Padding(3)
                                .AlignMiddle()
                                .Text("Subject")
                                .Bold();

                            foreach (var cat in layout)
                            {
                                header.Cell()
                                    .ColumnSpan((uint)ColumnsFor(cat))
                                    .Border(0.5f)
                                    .Background(PaleBlue)
                                    .Padding(3)
                                    .AlignCenter()
                                    .Text(cat.CategoryName)
                                    .Bold();
                            }

                            // Row 2: sub-column labels per category.

                            foreach (var cat in layout)
                            {
                                if (cat.ShowBreakdown)
                                {
                                    foreach (var typeName in cat.ExamTypeNames)
                                    {
                                        header.Cell()
                                            .Border(0.5f)
                                            .Background(PaleBlue)
                                            .Padding(3)
                                            .AlignCenter()
                                            .Text(typeName)
                                            .FontSize(6.5f)
                                            .Bold();
                                    }
                                }

                                header.Cell()
                                    .Border(0.5f)
                                    .Background(PaleBlue)
                                    .Padding(3)
                                    .AlignCenter()
                                    .Text("Total")
                                    .FontSize(6.5f)
                                    .Bold();

                                header.Cell()
                                    .Border(0.5f)
                                    .Background(PaleBlue)
                                    .Padding(3)
                                    .AlignCenter()
                                    .Text("Grade")
                                    .FontSize(6.5f)
                                    .Bold();
                            }
                        });


                        // ---- Subject rows

                        foreach (var subject in rows)
                        {
                            table.Cell()
                                .Border(0.5f)
                                .Padding(3)
                                .Text(subject.SubjectName);

                            foreach (var catMark in subject.Categories)
                            {
                                if (catMark.ShowBreakdown)
                                {
                                    foreach (var typeMark in catMark.ExamTypeMarks)
                                    {
                                        table.Cell()
                                            .Border(0.5f)
                                            .Padding(3)
                                            .AlignCenter()
                                            .Text(
                                                typeMark.IsAbsent
                                                    ? "AB"
                                                    : typeMark.ObtainedMarks
                                                        .ToString("0.##"));
                                    }
                                }

                                table.Cell()
                                    .Border(0.5f)
                                    .Padding(3)
                                    .AlignCenter()
                                    .Text(
                                        catMark.IsAbsent
                                            ? "AB"
                                            : catMark.TotalObtainedMarks
                                                .ToString("0.##"));

                                table.Cell()
                                    .Border(0.5f)
                                    .Padding(3)
                                    .AlignCenter()
                                    .Text(
                                        catMark.IsAbsent
                                            ? "-"
                                            : catMark.Grade);
                            }
                        }


                        // ---- Grand Total row

                        table.Cell()
                            .Border(0.5f)
                            .Background(PaleBlue)
                            .Padding(3)
                            .Text("Grand Total")
                            .Bold();

                        foreach (var cat in layout)
                        {
                            if (cat.ShowBreakdown)
                            {
                                foreach (var typeName in cat.ExamTypeNames)
                                {
                                    var total = rows
                                        .SelectMany(r => r.Categories)
                                        .Where(c => c.ExamCategoryId == cat.ExamCategoryId)
                                        .SelectMany(c => c.ExamTypeMarks)
                                        .Where(t =>
                                            t.ExamTypeName == typeName &&
                                            !t.IsAbsent)
                                        .Sum(t => t.ObtainedMarks);

                                    table.Cell()
                                        .Border(0.5f)
                                        .Background(PaleBlue)
                                        .Padding(3)
                                        .AlignCenter()
                                        .Text(total.ToString("0.##"))
                                        .Bold();
                                }
                            }

                            var catTotal = rows
                                .SelectMany(r => r.Categories)
                                .Where(c =>
                                    c.ExamCategoryId == cat.ExamCategoryId &&
                                    !c.IsAbsent)
                                .Sum(c => c.TotalObtainedMarks);

                            table.Cell()
                                .Border(0.5f)
                                .Background(PaleBlue)
                                .Padding(3)
                                .AlignCenter()
                                .Text(catTotal.ToString("0.##"))
                                .Bold();

                            table.Cell()
                                .Border(0.5f)
                                .Background(PaleBlue)
                                .Padding(3)
                                .AlignCenter()
                                .Text("-")
                                .Bold();
                        }
                    });
            });
        }


        // =========================================================
        // CO-SCHOLASTIC
        // =========================================================

        private void DrawCoScholastic(
            IContainer container,
            ReportCardVM vm)
        {
            container.Column(col =>
            {
                col.Item()
                    .Background(NavyDark)
                    .Padding(4)
                    .AlignCenter()
                    .Text("CO-SCHOLASTIC AREA")
                    .Bold()
                    .FontSize(8)
                    .FontColor(White);

                col.Item()
                    .Border(1)
                    .BorderColor(NavyDark)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            header.Cell()
                                .Border(0.5f)
                                .Background(PaleBlue)
                                .Padding(3)
                                .Text("Activity")
                                .Bold();

                            header.Cell()
                                .Border(0.5f)
                                .Background(PaleBlue)
                                .Padding(3)
                                .AlignCenter()
                                .Text("Grade")
                                .Bold();
                        });

                        if (vm.CoScholasticMarks.Count == 0)
                        {
                            table.Cell()
                                .Border(0.5f)
                                .Padding(3)
                                .Text("-");

                            table.Cell()
                                .Border(0.5f)
                                .Padding(3)
                                .AlignCenter()
                                .Text("-");
                        }
                        else
                        {
                            foreach (var item in vm.CoScholasticMarks)
                            {
                                table.Cell()
                                    .Border(0.5f)
                                    .Padding(3)
                                    .Text(item.Title);

                                table.Cell()
                                    .Border(0.5f)
                                    .Padding(3)
                                    .AlignCenter()
                                    .Text(item.Grade);
                            }
                        }
                    });
            });
        }


        // =========================================================
        // ATTENDANCE
        // =========================================================

        private void DrawAttendance(
            IContainer container,
            ReportCardVM vm)
        {
            container.Column(col =>
            {
                col.Item()
                    .Background(NavyDark)
                    .Padding(4)
                    .AlignCenter()
                    .Text("ATTENDANCE")
                    .Bold()
                    .FontSize(8)
                    .FontColor(White);

                col.Item()
                    .Border(1)
                    .BorderColor(NavyDark)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        AttendanceRow(table, "Working Days", vm.Attendance.WorkingDays.ToString());
                        AttendanceRow(table, "Present", vm.Attendance.PresentDays.ToString());
                        AttendanceRow(table, "Absent", vm.Attendance.AbsentDays.ToString());
                        AttendanceRow(table, "Leave", vm.Attendance.LeaveDays.ToString());
                        AttendanceRow(table, "Percentage", $"{vm.Attendance.AttendancePercentage:0.##}%");
                    });
            });
        }

        private void AttendanceRow(
            TableDescriptor table,
            string label,
            string value)
        {
            table.Cell()
                .Border(0.5f)
                .Background(PaleBlue)
                .Padding(3)
                .Text(label)
                .Bold();

            table.Cell()
                .Border(0.5f)
                .Padding(3)
                .AlignCenter()
                .Text(value);
        }


        // =========================================================
        // GRADING SCALE
        // =========================================================

        private void DrawGradingScale(
            IContainer container,
            ReportCardVM vm)
        {
            container.Column(col =>
            {
                col.Item()
                    .Background(NavyDark)
                    .Padding(4)
                    .AlignCenter()
                    .Text("GRADING SCALE")
                    .Bold()
                    .FontSize(8)
                    .FontColor(White);

                col.Item()
                    .Border(1)
                    .BorderColor(NavyDark)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.8f);
                        });

                        table.Header(header =>
                        {
                            header.Cell()
                                .Border(0.5f)
                                .Background(PaleBlue)
                                .Padding(3)
                                .AlignCenter()
                                .Text("Grade")
                                .Bold();

                            header.Cell()
                                .Border(0.5f)
                                .Background(PaleBlue)
                                .Padding(3)
                                .AlignCenter()
                                .Text("Marks (%)")
                                .Bold();

                            header.Cell()
                                .Border(0.5f)
                                .Background(PaleBlue)
                                .Padding(3)
                                .AlignCenter()
                                .Text("Remark")
                                .Bold();
                        });

                        var ranges =
                            vm.GradeRanges != null && vm.GradeRanges.Count > 0
                                ? vm.GradeRanges
                                : DefaultGradeRanges();

                        foreach (var g in ranges)
                        {
                            table.Cell()
                                .Border(0.5f)
                                .Padding(3)
                                .AlignCenter()
                                .Text(g.Grade);

                            table.Cell()
                                .Border(0.5f)
                                .Padding(3)
                                .AlignCenter()
                                .Text($"{g.MinPercentage:0.#}-{g.MaxPercentage:0.#}");

                            table.Cell()
                                .Border(0.5f)
                                .Padding(3)
                                .AlignCenter()
                                .Text(g.Description ?? "-");
                        }
                    });
            });
        }

        private List<GradeRangeVM> DefaultGradeRanges()
        {
            return new List<GradeRangeVM>
            {
                new GradeRangeVM { Grade = "A+", MinPercentage = 91, MaxPercentage = 100, Description = "Outstanding" },
                new GradeRangeVM { Grade = "A",  MinPercentage = 81, MaxPercentage = 90,  Description = "Excellent" },
                new GradeRangeVM { Grade = "B+", MinPercentage = 71, MaxPercentage = 80,  Description = "Very Good" },
                new GradeRangeVM { Grade = "B",  MinPercentage = 61, MaxPercentage = 70,  Description = "Good" },
                new GradeRangeVM { Grade = "C",  MinPercentage = 51, MaxPercentage = 60,  Description = "Average" },
                new GradeRangeVM { Grade = "D",  MinPercentage = 41, MaxPercentage = 50,  Description = "Satisfactory" },
                new GradeRangeVM { Grade = "E",  MinPercentage = 0,  MaxPercentage = 40,  Description = "Needs Improvement" },
            };
        }


        // =========================================================
        // TEACHER'S REMARK
        // =========================================================

        private void DrawRemark(
            IContainer container,
            ReportCardVM vm)
        {
            container.Column(col =>
            {
                col.Item()
                    .Background(NavyDark)
                    .Padding(4)
                    .AlignCenter()
                    .Text("TEACHER'S REMARK")
                    .Bold()
                    .FontSize(8)
                    .FontColor(White);

                col.Item()
                    .Border(1)
                    .BorderColor(NavyDark)
                    .Padding(6)
                    .MinHeight(28)
                    .Text(vm.Summary.TeacherRemark ?? vm.TeacherRemark ?? "-");
            });
        }


        // =========================================================
        // SUMMARY
        // =========================================================

        private void DrawSummary(
            IContainer container,
            ReportCardVM vm)
        {
            container
                .Border(1)
                .BorderColor(NavyDark)
                .Table(table =>
                {
                    table.ColumnsDefinition(
                        columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                    HeaderCell(table, "Total Maximum");
                    HeaderCell(table, "Total Obtained");
                    HeaderCell(table, "Percentage");
                    HeaderCell(table, "Grade");
                    HeaderCell(table, "Result");

                    BodyCell(
                        table,
                        vm.Summary.TotalMaximumMarks.ToString("0.##"));

                    BodyCell(
                        table,
                        vm.Summary.TotalObtainedMarks.ToString("0.##"));

                    BodyCell(
                        table,
                        $"{vm.Summary.Percentage:0.##}%");

                    BodyCell(
                        table,
                        vm.Summary.Grade,
                        true);

                    BodyCell(
                        table,
                        vm.Summary.Result,
                        true);
                });
        }


        // =========================================================
        // SIGNATURE
        // =========================================================

        private void DrawSignature(
            IContainer container,
            ReportCardVM vm)
        {
            container.Row(row =>
            {
                SignatureBlock(row.RelativeItem(), "Parent / Guardian");
                SignatureBlock(row.RelativeItem(), "Class Teacher");
                SignatureBlock(row.RelativeItem(), "Principal");
            });
        }

        private void SignatureBlock(
            IContainer container,
            string label)
        {
            container
                .AlignCenter()
                .PaddingHorizontal(10)
                .Column(c =>
                {
                    c.Item().Height(24);

                    c.Item()
                        .LineHorizontal(0.75f)
                        .LineColor(NavyDark);

                    c.Item()
                        .PaddingTop(2)
                        .AlignCenter()
                        .Text(label)
                        .Bold()
                        .FontColor(NavyDark)
                        .FontSize(8);
                });
        }


        // =========================================================
        // TABLE HELPERS
        // =========================================================

        private void HeaderCell(
            TableDescriptor table,
            string text)
        {
            table.Cell()
                .Border(0.5f)
                .Background(PaleBlue)
                .Padding(4)
                .AlignCenter()
                .Text(text)
                .FontSize(7)
                .Bold()
                .FontColor(NavyDark);
        }


        private void BodyCell(
            TableDescriptor table,
            string? text,
            bool bold = false)
        {
            var cell = table.Cell()
                .Border(0.5f)
                .Padding(4)
                .AlignCenter();

            if (bold)
            {
                cell.Text(text ?? "-")
                    .FontSize(7)
                    .Bold();
            }
            else
            {
                cell.Text(text ?? "-")
                    .FontSize(7);
            }
        }


        private void InfoCell(
            TableDescriptor table,
            string label,
            string? value)
        {
            table.Cell()
                .Border(0.5f)
                .Background(PaleBlue)
                .Padding(4)
                .Text(label)
                .FontSize(7)
                .Bold()
                .FontColor(NavyDark);

            table.Cell()
                .Border(0.5f)
                .Padding(4)
                .Text(value ?? "-")
                .FontSize(7);
        }
    }
}