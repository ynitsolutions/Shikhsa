using System.ComponentModel.DataAnnotations;

namespace Shikhsa.Models.Payment
{
    public class PaymentGatewaySetting
    {
        [Key]
        public int PaymentGatewaySettingId { get; set; }

        [Required]
        [MaxLength(50)]
        public string GatewayName { get; set; } = "ATOM";

        [Required]
        [MaxLength(100)]
        public string MerchId { get; set; } = string.Empty;

        [Required]
        public string MerchPassword { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ProductId { get; set; } = "NSE";

        [Required]
        public string AuthUrl { get; set; } =
            "https://caller.atomtech.in/ots/aipay/auth";

        [MaxLength(20)]
        public string CheckoutEnvironment { get; set; } = "uat";

        public string CheckoutCdn { get; set; } =
            "https://pgtest.atomtech.in/staticdata/ots/js/atomcheckout.js";
        public string StatusCheckUrl { get; set; } = ""; // e.g. https://caller.atomtech.in/ots/aipay/status  — confirm from NTT DATA docs

        [Required]
        public string RequestEncryptKey { get; set; } = string.Empty;

        [Required]
        public string RequestSalt { get; set; } = string.Empty;

        [Required]
        public string ResponseDecryptKey { get; set; } = string.Empty;

        [Required]
        public string ResponseSalt { get; set; } = string.Empty;

        [Required]
        public string ResponseHashKey { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public string? AddedBy { get; set; }

        public string? UpdatedBy { get; set; }
    }
}
