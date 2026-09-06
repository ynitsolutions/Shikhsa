namespace Shikhsa.Sevices
{
    public sealed class PaymentReconciliationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PaymentReconciliationBackgroundService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);

        public PaymentReconciliationBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<PaymentReconciliationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var reconciliationService = scope.ServiceProvider
                        .GetRequiredService<PaymentReconciliationService>();

                    await reconciliationService.ReconcilePendingTransactionsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Scheduled payment reconciliation failed.");
                }

                try
                {
                    await Task.Delay(Interval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Shutdown
                }
            }
        }
    }
}
