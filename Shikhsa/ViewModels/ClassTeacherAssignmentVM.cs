using Microsoft.AspNetCore.Mvc.Rendering;

namespace Shikhsa.ViewModels
{
    public class ClassTeacherAssignmentVM
    {
        public long BatchId { get; set; }

        public long SectionId { get; set; }

        public long CopyFromBatchId { get; set; }

        public List<SelectListItem> Batches { get; set; } = new();

        public List<SelectListItem> Sections { get; set; } = new();

        public List<SelectListItem> CopyBatches { get; set; } = new();

        public List<ClassAccordionVM> Classes { get; set; } = new();
        public List<SubjectCacheVM> SubjectCache { get; set; } = new();
    }
    public class ClassAccordionVM
    {
        public long ClassId { get; set; }

        public string ClassName { get; set; }

        public List<TeacherAssignmentVM> Teachers { get; set; } = new();
    }
    public class TeacherAssignmentVM
    {
        public Guid RowId { get; set; } = Guid.NewGuid();

        public long StaffId { get; set; }

        public string StaffName { get; set; }

        public bool IsClassTeacher { get; set; }

        public List<int> SubjectIds { get; set; }
            = new();

        public List<SelectListItem> Subjects
            = new();

        public List<SelectListItem> Teachers
            = new();

    }
    public class SaveClassTeacherAssignmentVM
    {
        public int BatchId { get; set; }

        public int SectionId { get; set; }

        public List<SaveClassVM> Classes { get; set; } = new();
    }

    public class SaveClassVM
    {
        public int ClassId { get; set; }

        public List<SaveTeacherVM> Teachers { get; set; } = new();
    }

    public class SaveTeacherVM
    {
        public long StaffId { get; set; }

        public bool IsClassTeacher { get; set; }

        public List<int> SubjectIds { get; set; } = new();
    }
    public class ClassTeacherDashboardVM
    {
        public int ClassId { get; set; }

        public string? ClassName { get; set; }

        public int SectionId { get; set; }

        public string? SectionName { get; set; }

        public long StaffId { get; set; }

        public string? StaffName { get; set; }

        public string? PhotoPath { get; set; }

        public string? SubjectText { get; set; }

        public bool IsPresentToday { get; set; }

        public string? AttendanceText { get; set; }

        public string? AttendanceColor { get; set; }
    }
    public class Select2VM
    {
        public long id { get; set; }

        public string text { get; set; }
    }
    public class SubjectCacheVM
    {
        public int ClassId { get; set; }

        public int SubjectId { get; set; }

        public string SubjectName { get; set; } = string.Empty;
    }
}
