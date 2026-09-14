// File path: Shikhsa/ViewModels/AtomViewModels.cs
namespace Shikhsa.ViewModels
{
    public class AtomCheckoutVM
    {
        public string MerchantTransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string AtomTokenId { get; set; } = string.Empty;
        public string MerchId { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerMobile { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string CheckoutCdn { get; set; } = string.Empty;
        public string Environment { get; set; } = "uat";

        // Required credentials for form encryption references
        public string Password { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string RequestEncryptKey { get; set; } = string.Empty;
        public string RequestSalt { get; set; } = string.Empty;
    }

    public class AtomPaymentResultVM
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? ReferenceNo { get; set; }
        public long? ReceiptId { get; set; }
    }
}
