using Microsoft.AspNetCore.Mvc.Rendering;
using Shikhsa.Models;
using System.ComponentModel.DataAnnotations;

namespace Shikhsa.ViewModels
{
    public class ClassBatchSubjectVM
    {
        public long? HeaderId { get; set; }

        [Required]
        public int ClassId { get; set; }

        [Required]
        public int BatchId { get; set; }

        public int? CopyFromBatchId { get; set; }

        public List<int> SubjectIds { get; set; }
            = new();

        public List<DataListItem> Classes { get; set; }
            = new();

        public List<Batches> Batches { get; set; }
            = new();

        public List<SubjectMasters> Subjects { get; set; }
            = new();

        public List<ClassBatchSubjectListVM> ListData
        {
            get;
            set;
        }
            = new();
        public int? FilterClassId { get; set; }

        public int? FilterBatchId { get; set; }
    }
    public class ClassBatchSubjectListVM
    {
        public long HeaderId { get; set; }

        public string ClassName { get; set; }

        public string BatchName { get; set; }

        public string Subjects { get; set; }
    }
}
