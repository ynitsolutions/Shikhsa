using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shikhsa.Models
{ 
    public class FeeFrequency:BaseEntity
    {
        [Key]
        public int FrequencyId { get; set; }

        [Required]
        [StringLength(50)]
        public string Value { get; set; }

        [Required]
        [StringLength(100)]
        public string Text { get; set; }

        public int DisplayOrder { get; set; }

       
    }
    public class FeeHeading:BaseEntity
    {
        [Key]
        public long FeeHeadingId { get; set; }

        [Required]
        [StringLength(200)]
        public string FeeHeadingName { get; set; }

        [Required]
        public int FrequencyId { get; set; }

        [ForeignKey(nameof(FrequencyId))]
        public FeeFrequency? Frequency { get; set; }

        public bool Jan { get; set; }
        public bool Feb { get; set; }
        public bool Mar { get; set; }
        public bool Apr { get; set; }
        public bool May { get; set; }
        public bool Jun { get; set; }
        public bool Jul { get; set; }
        public bool Aug { get; set; }
        public bool Sep { get; set; }
        public bool Oct { get; set; }
        public bool Nov { get; set; }
        public bool Dec { get; set; }
    }
}