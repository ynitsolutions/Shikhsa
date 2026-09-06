using Microsoft.EntityFrameworkCore;
using Shikhsa.Data;
using Shikhsa.Services;

namespace Shikhsa.Sevices
{
    public sealed class PaymentReconciliationService
    {
        private readonly ApplicationDbContext _context;
        private readonly AtomPaymentService _atomPaymentService;
        private readonly ILogger<PaymentReconciliationService> _logger;

        public PaymentReconciliationService(
            ApplicationDbContext context,
            AtomPaymentService atomPaymentService,
            ILogger<PaymentReconciliationService> logger)
        {
            _context = context;
            _atomPaymentService = atomPaymentService;
            _logger = logger;
        }

        public async Task<int> ReconcilePendingTransactionsAsync(
            CancellationToken cancellationToken = default)
        {
            // Pending transactions jo kam se kam 5 min purane hain
            // (turant check karne ka koi fayda nahi, gateway ko
            // process karne ka time do)

            var cutoff = DateTime.Now.AddMinutes(-5);

            var pending = await _context.PaymentTransactionDetail
                .Where(x =>
                    x.GatewayName == "ATOM" &&
                    x.TransactionStatus == "Pending" &&
                    x.TxnDate < cutoff)
                .ToListAsync(cancellationToken);

            int updatedCount = 0;

            foreach (var transaction in pending)
            {
                if (string.IsNullOrWhiteSpace(transaction.GatewayOrderId))
                    continue;

                var result = await _atomPaymentService.CheckTransactionStatusAsync(
                    transaction.GatewayOrderId,
                    transaction.TxnDate,
                    cancellationToken);

                if (result.StatusCode == null)
                {
                    // Gateway se hi jawab nahi mila — status jaisa tha
                    // waisa hi rehne do, next cycle me phir try hoga.
                    _logger.LogWarning(
                        "Status check inconclusive. MerchantTxnId={MerchantTxnId}, Error={Error}",
                        transaction.GatewayOrderId, result.ErrorMessage);

                    continue;
                }

                transaction.GatewayResponse = result.RawResponse ?? transaction.GatewayResponse;

                if (result.Success)
                {
                    transaction.TransactionStatus = "Success";
                    transaction.TransactionError = result.StatusDescription;
                    transaction.GatewayPaymentId = result.AtomTxnId;
                    transaction.ReferenceNo = result.AtomTxnId;
                    transaction.PaymentId = result.StatusCode;
                    transaction.PaymentCompletedOn = DateTime.Now;

                    // Receipt banana ho to yahan
                    // FinalizeReceiptAsync jaisa method call karo
                    // (agar PaymentController ke bahar hai to us
                    // logic ko ek shared service me nikaal lo)
                }
                else
                {
                    // ATOM ne definitively "failed"/"declined" bola
                    var terminalCodes = new[] { "OTS0001", "OTS0002" }; // TODO: apne actual failure codes confirm karo
                    if (terminalCodes.Contains(result.StatusCode) ||
                        transaction.TxnDate < DateTime.Now.AddHours(-24))
                    {
                        transaction.TransactionStatus = "Failed";
                        transaction.TransactionError =
                            result.StatusDescription ?? "Payment failed (via status check).";
                    }
                    // else: abhi bhi genuinely pending hai, chhod do
                }

                updatedCount++;
            }

            if (updatedCount > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation(
                "Payment reconciliation completed. Checked={Checked}, Updated={Updated}",
                pending.Count, updatedCount);

            return updatedCount;
        }
    }
}
