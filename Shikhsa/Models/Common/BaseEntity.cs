namespace Shikhsa.Models.Common
{
    public class BaseEntity
    {
        public DateTime AddedDate { get; set; }
           = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        public string? AddedBy { get; set; }

        public string? UpdatedBy { get; set; }

        public bool IsActive { get; set; }
            = true;
    }
}
