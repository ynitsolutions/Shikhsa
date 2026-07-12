using Microsoft.AspNetCore.Mvc.Rendering;
using Shikhsa.Models;

namespace Shikhsa.ViewModels
{
    public class ExamCategoryVM
    {
        public ExamCategory ExamCategory { get; set; } = new();

        public List<ExamCategory> ExamCategoryList { get; set; }
            = new();
    }
    public class ScholasticExamVM
    {
        public ScholasticExam Exam { get; set; } = new();

        public List<int> SelectedSubjects { get; set; } = new();

        public List<SubjectMasters> Subjects { get; set; } = new();

        public List<DataListItem> Classes { get; set; } = new();
        public List<DataListItem> ExamType { get; set; } = new();
        public List<ExamCategory> ExamCategories { get; set; } = new();

        public List<Batches> Batches { get; set; } = new();

        public List<ScholasticExam> List { get; set; } = new();
    }
    public class CoScholasticAreaVM
    {
        public CoScholasticArea CoScholasticArea { get; set; } = new();

        // Filter
        public long? SearchCoScholasticId { get; set; }
        public int? SearchClassId { get; set; }

        // Dropdown
        public List<SelectListItem> CoScholasticList { get; set; } = new();
        public List<DataListItem> ClassList { get; set; } = new();
    }
    public class CoScholasticPageVM
    {
        public CoScholastic CoScholastic { get; set; } = new();

        public List<CoScholastic> CoScholasticList { get; set; } = new();
    }
}
