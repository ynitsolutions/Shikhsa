using Microsoft.AspNetCore.Mvc.Rendering;
using Shikhsa.Models;

namespace Shikhsa.ViewModels
{
    public class BulkGradingVM
    {
        public int TermId { get; set; }

        public int ClassId { get; set; }

        public int BatchId { get; set; }

        public List<GradeRangeVM> Grades { get; set; }
    }
    //public class BulkGradingCriteriaVM
    //{
    //    public long BatchId { get; set; }

    //    public long ClassId { get; set; }

    //    public long TermId { get; set; }

    //    public List<GradeRangeVM> Ranges { get; set; } = new();

    //    // Dropdowns
    //    public IEnumerable<SelectListItem> Terms { get; set; } = new List<SelectListItem>();

    //    public IEnumerable<DataListItem> Classes { get; set; } = new List<DataListItem>();

    //    public IEnumerable<Batches> Batches { get; set; } = new List<Batches>();
    //}
    public class GradeRangeVM
    {
        public decimal MinPercentage { get; set; }

        public decimal MaxPercentage { get; set; }

        public string Grade { get; set; }

        public string Description { get; set; }
    }
    public class GradingCriteriaVM
    {
        public GradingCriteria Criteria { get; set; } = new GradingCriteria();

        public List<GradingCriteria> GradingList { get; set; } = new();

        public IEnumerable<SelectListItem> Terms { get; set; } = new List<SelectListItem>();

        public IEnumerable<DataListItem> Classes { get; set; } = new List<DataListItem>();

        public IEnumerable<Batches> Batches { get; set; } = new List<Batches>();
        public List<GradeRangeVM> GradeRanges { get; set; } = new List<GradeRangeVM>
{
    new GradeRangeVM()
};
    }
}
