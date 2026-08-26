using Shikhsa.Models;

namespace Shikhsa.ViewModels
{

    // Page-level container — koi naya EF entity/table nahi, sirf existing
    // StaffMaster aur uske related models (StaffAcademic, StaffExperience,
    // StaffEmergencyContact, StaffDocument, StaffSalaryHistory) ko compose
    // karta hai.
    public class StaffProfileViewModel
    {
        public StaffMaster Staff { get; set; }
        public StaffExtraInfo ExtraInfo { get; set; } = new();

        public List<StaffDocument> Documents { get; set; } = new();
        public List<StaffAcademic> Academics { get; set; } = new();
        public List<StaffExperience> Experiences { get; set; } = new();
        public List<StaffEmergencyContact> EmergencyContacts { get; set; } = new();

        public StaffSalaryHistory CurrentSalary { get; set; }
        public List<StaffSalaryHistory> SalaryHistory { get; set; } = new();

        public StaffEmergencyContact PrimaryEmergencyContact =>
            EmergencyContacts.FirstOrDefault(e => e.IsPrimary) ?? EmergencyContacts.FirstOrDefault();
    }

    // StaffMaster me GenderId/DepartmentId/DesignationId waghera sirf IDs hain,
    // display names ke liye ye chhoti DTO (naya EF entity NAHI hai)
    public class StaffExtraInfo
    {
        public string GenderName { get; set; }
        public string BloodGroupName { get; set; }
        public string MaritalStatusName { get; set; }
        public string ReligionName { get; set; }
        public string CategoryName { get; set; }
        public string DepartmentName { get; set; }
        public string DesignationName { get; set; }
        public string StaffTypeName { get; set; }
        public string EmploymentTypeName { get; set; }
        public string EmployeeStatusName { get; set; }
        public string ReportingStaffName { get; set; }
    }
}
