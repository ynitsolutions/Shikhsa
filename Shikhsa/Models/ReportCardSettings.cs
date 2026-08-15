using Shikhsa.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shikhsa.Models
{
    public class ReportCardSetting : BaseEntity
    {
        [Key]
        public int ReportCardSettingId { get; set; }

        public int? BatchId { get; set; }

        [Required]
        [StringLength(20)]
        public string BoardType { get; set; } = string.Empty;

        public bool ShowLogo { get; set; } = true;

        public bool ShowStudentPhoto { get; set; } = true;

        public bool ShowQRCode { get; set; } = true;

        public bool ShowWatermark { get; set; } = false;

        public bool ShowTeacherSignature { get; set; } = true;

        public bool ShowPrincipalSignature { get; set; } = true;

        public bool UseDigitalSignature { get; set; } = false;

        [StringLength(100)]
        public string? WatermarkText { get; set; }

        [StringLength(300)]
        public string? HeaderText { get; set; }

        [StringLength(300)]
        public string? FooterText { get; set; }

        #region Navigation Properties

        [ForeignKey(nameof(BatchId))]
        public virtual Batches? Batch { get; set; }

        #endregion
    }
}
