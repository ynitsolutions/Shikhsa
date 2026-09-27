using Shikhsa.Models;
using Shikhsa.Models.Certificate;

namespace Shikhsa.ViewModels
{
    public class SubjectMasterVM
    {
        public SubjectMasters Subject { get; set; } = new();

        public List<SubjectMasters> SubjectList { get; set; } = new();
    }
    public class CertificateTemplatePageVM
    {
        public CertificateTemplate Form { get; set; } = new();

        public List<CertificateTemplate> Templates { get; set; } = new();

        //public List<CertificateTypeMaster> CertificateTypes { get; set; } = new();

        public List<long> SelectedCategories { get; set; } = new();
    }
}
