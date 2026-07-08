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
}
