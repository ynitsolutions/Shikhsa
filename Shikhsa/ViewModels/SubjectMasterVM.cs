using Shikhsa.Models;

namespace Shikhsa.ViewModels
{
    public class SubjectMasterVM
    {
        public SubjectMasters Subject { get; set; } = new();

        public List<SubjectMasters> SubjectList { get; set; } = new();
    }
}
