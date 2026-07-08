using Microsoft.AspNetCore.Mvc.Rendering;
using Shikhsa.Models;

namespace Shikhsa.ViewModels
{
    public class FeeHeadingPageVM
    {
        public FeeHeading Form { get; set; } = new();

        public List<SelectListItem> FrequencyList { get; set; } = new();

        public List<FeeHeading> List { get; set; } = new();
    }
    public class FeeFrequencyPageVM
    {
        public FeeFrequency Form { get; set; } = new();

        public List<FeeFrequency> List { get; set; } = new();
    }
    public class FeePlanIndexViewModel
    {
        public TuitionFeePlan NewTuition { get; set; } = new();
        public List<TuitionFeePlan> TuitionPlans { get; set; } = new();

        public TransportFeePlan NewTransport { get; set; } = new();
        public List<TransportFeePlan> TransportPlans { get; set; } = new();

        public HostelFeePlan NewHostel { get; set; } = new();
        public List<HostelFeePlan> HostelPlans { get; set; } = new();
    }
}
