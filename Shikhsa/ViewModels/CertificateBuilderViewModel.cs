using Microsoft.AspNetCore.Mvc.Rendering;
using Shikhsa.Models;

namespace Shikhsa.ViewModels
{
    public class CertificateBuilderViewModel
    {
        public string Type { get; set; } = "tc"; // "tc" | "cc"

        public Certificates Certificate { get; set; } = new Certificates();
        public List<SelectListItem> Reasons { get; set; } = new();

        public List<SelectListItem> Conducts { get; set; } = new();
    }
}
