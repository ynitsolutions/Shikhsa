using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [NotMapped]
        public string? ZohoRefreshToken { get; set; }
    }
    public sealed class ZohoPaymentSettings
    {
        public string AccountId { get; set; } = "60076225611";       // zohopay.{account_id}
        public string ClientId { get; set; } = "1005.H8VGZXU32KR2DRKFSXOHXMUVEU5QLE";
        public string ClientSecret { get; set; } = "0c90e20ecef2c7fe1f893b7e016ad2838676b18be3";
        public string RefreshToken { get; set; } = "1005.6e4aba5a45f537f793a3ee6c696c3f7b.84557ebd3ae6f41a2821b7e1963ef646";
        public string SigningKey { get; set; } = "0c467dd62f85249a931af1d16995b1b17c43dd7ab5668712cce085f51cbc1fb8fca9bf9da2e5e6578ad885038608e8a5";        // hosted checkout redirect signature verify ke liye
        public string WebhookSigningKey { get; set; } = ""; // webhook verify ke liye (alag key hai)
        public string AccountsBaseUrl { get; set; } = "https://accounts.zoho.in";  // ya .com — apne DC ke hisaab se
        public string ApiBaseUrl { get; set; } = "https://accounts.zoho.in"; // TODO: confirm exact host from tumhare Zoho Payments account (in/com)
        public string CheckoutBaseUrl { get; set; } = "https://accounts.zoho.in";
        public string SuccessUrl { get; set; } = "";
        public string FailureUrl { get; set; } = "";
        public string ApiKey { get; set; } = "1003.606c287fdecc969fb8b374b61142d228.f30589966cdc497395b72ce7091525b4";
    }
    //public sealed class ZohoOptions
    //{
    //    public const string SectionName = "ZohoPayments";

    //    [Required] public string AccountId { get; set; } = string.Empty;
    //    [Required] public string ClientId { get; set; } = string.Empty;
    //    [Required] public string ClientSecret { get; set; } = string.Empty;
    //    [Required] public string RefreshToken { get; set; } = string.Empty;
    //    [Required] public string SigningKey { get; set; } = string.Empty;
    //    [Required] public string WebhookSigningKey { get; set; } = string.Empty;

    //    // Sandbox vs Production — controlled via appsettings
    //    public bool UseSandbox { get; set; } = false;

    //    public string AccountsBaseUrl { get; set; } = "https://accounts.zoho.in";
    //    public string ApiBaseUrl { get; set; } = "https://payments.zoho.in/api/v1";
    //    public string CheckoutBaseUrl { get; set; } = "https://payments.zoho.in";

    //    public string SuccessUrl { get; set; } = string.Empty;
    //    public string FailureUrl { get; set; } = string.Empty;

    //    public string Currency { get; set; } = "INR";
    //}
}

