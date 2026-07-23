using Microsoft.AspNetCore.Mvc.Rendering;
using Shikhsa.Models;
using System.ComponentModel.DataAnnotations;

namespace Shikhsa.ViewModels;

public class CoScholasticGradeEntryVM: StudentFilterVM
{
   

    // Dynamic columns
    public List<CoScholasticColumnVM> Columns { get; set; } = new();

    // Student rows
    public List<CoScholasticStudentVM> Students { get; set; } = new();
}

public class CoScholasticColumnVM
{
    public long CoScholasticAreaId { get; set; }

    public long CoScholasticId { get; set; }

    public string Title { get; set; } = "";

    public string SubjectNameInLanguage { get; set; } = "";
}
public class CoScholasticStudentVM
{
    public long StudentId { get; set; }

    public string AdmissionNo { get; set; }

    public string StudentName { get; set; }

    public bool IsFreeze { get; set; }

    public List<StudentCoScholasticGradeVM> Grades { get; set; } = new();
}

public class StudentCoScholasticGradeVM
{
    public long GradeEntryId { get; set; }

    public long StudentId { get; set; }

    public long CoScholasticAreaId { get; set; }

    public int ExamCategoryId { get; set; }

    public string Grade { get; set; } = "";
}