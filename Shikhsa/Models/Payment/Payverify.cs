namespace Shikhsa.Models.Payment
{
    /* ============================================
       ATOM (NTT DATA / Paynetz) auth call ka result
       — Checkout page ke liye token banane ke liye
       ============================================ */
    public sealed class AtomAuthResult
    {
        public bool Success { get; set; }

        public string? AtomTokenId { get; set; }

        public string? MerchId { get; set; }

        public string? MerchTxnId { get; set; }

        public string? ErrorCode { get; set; }

        public string? ErrorMessage { get; set; }

        public string? RawResponse { get; set; }
    }

    /* ============================================
       ATOM se checkout complete hone ke baad jo
       server-to-server / browser callback (ReturnUrl)
       aata hai, uske fields.
       NOTE: exact field names Atom/NTT DATA ke actual
       merchant integration doc se confirm kar lena —
       yeh best-effort mapping hai based on standard
       OTS/Paynetz response params.
       ============================================ */
    public sealed class AtomCallbackResponse
    {
        public string? MerchId { get; set; }

        public string? MerchTxnId { get; set; }

        public string? AtomTxnId { get; set; }

        public string? BankTxnId { get; set; }

        public decimal Amount { get; set; }

        public string? StatusCode { get; set; }

        public string? StatusDescription { get; set; }

        public string? PaymentMode { get; set; }

        public string? BankName { get; set; }

        public string? SignatureOrHash { get; set; }

        // Poora raw form/body — audit trail + reconciliation ke liye
        public string? RawResponse { get; set; }
        public bool? IsActive { get; set; }
        // ATOM ke success codes generally "OTS0000" jaisा hote hain —
        // exact list gateway docs se confirm karo
        public bool IsSuccess =>
            !string.IsNullOrWhiteSpace(StatusCode) &&
            (StatusCode.Equals("OTS0000", StringComparison.OrdinalIgnoreCase) ||
             StatusDescription?.Equals("Transaction Successful", StringComparison.OrdinalIgnoreCase) == true);
    }

    /* ============================================
       Status-check API (reconciliation) ka result.
       PaymentReconciliationService isi shape ko
       consume karta hai.
       ============================================ */
    public sealed class AtomStatusCheckResult
    {
        public bool Success { get; set; }

        public string? StatusCode { get; set; }

        public string? StatusDescription { get; set; }

        public string? AtomTxnId { get; set; }

        public string? RawResponse { get; set; }

        public string? ErrorMessage { get; set; }
    }
}