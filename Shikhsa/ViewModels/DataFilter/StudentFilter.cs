namespace Shikhsa.ViewModels.DataFilter
{
    public class StudentListFilterVM
    {
        public string? ApplicationNo { get; set; }

        public string? StudentName { get; set; }

        public string? FatherName { get; set; }
        public string? MotherName { get; set; }
        public string? GuardianName { get; set; }
        public string? MobileNo { get; set; }

        public int? CategoryId { get; set; }

        public int? GenderId { get; set; }

        public int? ReligionId { get; set; }

        public int? AdmissionBatchId { get; set; }

        public int? RegClassId { get; set; }
        public int? SectionId { get; set; }
        public List<string> SelectedColumns { get; set; } = new();
    }
    public class StudentListReportVM
    {
        public long StudentId { get; set; }

        public string? ApplicationNo { get; set; }

        public string? StudentName { get; set; }

        public DateTime DOB { get; set; }

        public string? AadhaarNumber { get; set; }

        public string? ContactNo { get; set; }

        public string? Email { get; set; }

        public string? FatherName { get; set; }

        public string? MotherName { get; set; }
        public string? GuardianName { get; set; }
        public string? FatherMobile { get; set; }

        public string? MotherMobile { get; set; }

        public string? LocalAddress { get; set; }

        public string? PermanentAddress { get; set; }

        public string? CategoryName { get; set; }

        public string? GenderName { get; set; }

        public string? ReligionName { get; set; }

        public string? BatchName { get; set; }

        public string? ClassName { get; set; }
        public string? SectionName { get; set; }
    }
    public class StudentReportPageVM
    {
        public StudentListFilterVM Filter { get; set; }

        public List<StudentListReportVM> Students { get; set; }
            = new List<StudentListReportVM>();
    }
    public enum EmailProvider
    {
        Gmail,
        Zoho
    }
}
