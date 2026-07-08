using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shikhsa.Models
{
    public class DataList:BaseEntity
    {
        [Key]
        public int DataListId { get; set; }

        [Required]
        [StringLength(200)]
        public string DataListName { get; set; }

        public string Description { get; set; }
        public virtual ICollection<DataListItem> DataListItems { get; set; }= new List<DataListItem>();
    }
    public class DataListItem: BaseEntity
    {
        [Key]
        public int DataListItemId { get; set; }

        [Required]
        public int DataListId { get; set; }

        [Required]
        [StringLength(200)]
        public string DataListItemText { get; set; }

        public string DataListItemValue { get; set; }

        public int DisplayOrder { get; set; }

        [ForeignKey("DataListId")]
        public virtual DataList? DataList { get; set; }
    }
}
