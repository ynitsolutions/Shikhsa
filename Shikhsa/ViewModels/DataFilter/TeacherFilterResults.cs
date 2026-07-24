using Shikhsa.Models;

namespace Shikhsa.ViewModels.DataFilter
{
    //public class TeacherFilterResult
    //{
    //    public long StaffId { get; set; }

    //    public int BatchId { get; set; }

    //    public int ClassId { get; set; }

    //    public int SectionId { get; set; }

    //    public bool LockStaff { get; set; }

    //    public bool LockBatch { get; set; }

    //    public bool LockClass { get; set; }

    //    public bool LockSection { get; set; }

    //    public List<StaffMaster> Staffs { get; set; } = new();

    //    public List<Batches> Batches { get; set; } = new();

    //    public List<DataListItem> Classes { get; set; } = new();

    //    public List<DataListItem> Sections { get; set; } = new();
    //}
    public class TeacherFilterResult
    {
        public int BatchId { get; set; }

        public long StaffId { get; set; }

        public int ClassId { get; set; }

        public int SectionId { get; set; }

        public bool LockStaff { get; set; }

        public bool LockBatch { get; set; }

        public bool LockClass { get; set; }

        public bool LockSection { get; set; }

        public List<Batches> Batches { get; set; } = new();

        public List<StaffMaster> Staffs { get; set; } = new();

        public List<DataListItem> Classes { get; set; } = new();

        public List<DataListItem> Sections { get; set; } = new();
    }
    public class TeacherFilterRequest
    {
        public int BatchId { get; set; }

        public long StaffId { get; set; }

        public int ClassId { get; set; }

        public int SectionId { get; set; }

        public string ChangedBy { get; set; } = "";
    }
    public class BaseFilterVM
    {
        // Selected Values
        public int BatchId { get; set; }

        public long StaffId { get; set; }

        public int ClassId { get; set; }

        public int SectionId { get; set; }

        // Lock
        public bool LockBatch { get; set; }

        public bool LockStaff { get; set; }

        public bool LockClass { get; set; }

        public bool LockSection { get; set; }

        // Show/Hide (Future Use)
        public bool ShowBatch { get; set; } = true;

        public bool ShowStaff { get; set; } = true;

        public bool ShowClass { get; set; } = true;

        public bool ShowSection { get; set; } = true;

        // Dropdowns
        public List<Batches> Batches { get; set; } = new();

        public List<StaffMaster> Staffs { get; set; } = new();

        public List<DataListItem> Classes { get; set; } = new();

        public List<DataListItem> Sections { get; set; } = new();
    }
}
