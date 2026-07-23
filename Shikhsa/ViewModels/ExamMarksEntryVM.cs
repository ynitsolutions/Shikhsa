using Shikhsa.Models;
using System.ComponentModel.DataAnnotations;

namespace Shikhsa.ViewModels
{
    public class ExamMarksEntryVM: StudentFilterVM
    {
       


        public int ExamType { get; set; }

        public List<ExamCategory> ExamCategories { get; set; } = new();

        public List<DataListItem> ExamTypes { get; set; } = new();
        // Dropdown

      
        // Dynamic Columns

        public List<ExamMarkColumnVM> Columns { get; set; } = new();

        // Students

        public List<ExamMarksRowVM> Students { get; set; } = new();
    }
    public class ExamMarkColumnVM
    {
        public int ExamId { get; set; }

        public int SubjectId { get; set; }

        public string SubjectName { get; set; } = "";

        public string ExamName { get; set; } = "";

        public decimal MaxMarks { get; set; }

        public decimal MinMarks { get; set; }
        public string ExamTypeName { get; set; } = "";

        public int DisplayOrder { get; set; }
    }
    public class ExamMarksRowVM
    {
        public long StudentId { get; set; }

        public string AdmissionNo { get; set; } = "";

        public string RollNo { get; set; } = "";

        public string StudentName { get; set; } = "";
        public string? Remarks { get; set; } = "";
        public decimal Percent { get; set; }
        public decimal Total { get; set; }
        public string Grade { get; set; }
        public int RankInClass { get; set; }
        public bool IsFreeze { get; set; }
        public List<ExamMarkVM> Marks { get; set; } = new();
    }
    public class ExamMarkVM
    {
        public long ExamObtainedMarkId { get; set; }

        public int ExamId { get; set; }

        public int SubjectId { get; set; }

        public decimal? Marks { get; set; }

        public bool IsAbsent { get; set; }

        public bool IsFreeze { get; set; }

        //public string? Remarks { get; set; }
    }
    public class StudentFilterVM
    {
        public int BatchId { get; set; }

        public long StaffId { get; set; }

        public int ClassId { get; set; }

        public int SectionId { get; set; }
        public int ExamCategoryId { get; set; }
        public bool? IsClassTeacher { get; set; }

        // Dropdowns
        public List<Batches> Batches { get; set; } = new();

        public List<StaffMaster> Staffs { get; set; } = new();

        public List<DataListItem> Classes { get; set; } = new();

        public List<DataListItem> Sections { get; set; } = new();
        public List<ExamCategory> ExamCategories { get; set; } = new();
    }
}
