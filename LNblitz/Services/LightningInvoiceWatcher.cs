using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LNblitz.Services
{
    internal interface IScopedLightningInvoiceWatcher
    {
        Task CheckInvoices(CancellationToken stoppingToken);
    }

    internal class ScopedLightningInvoiceWatcher : IScopedLightningInvoiceWatcher
    {
        private readonly ILogger _logger;
        private readonly WalletService _walletService;

        public ScopedLightningInvoiceWatcher(
            ILogger<ScopedLightningInvoiceWatcher> logger,
            WalletService walletService)
        {
            _logger = logger;
            _walletService = walletService;
        }

        public async Task CheckInvoices(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var transactions = await _walletService.GetPendingTransactions();
                var list = transactions.ToList();
                int count = list.Count();

                if (count > 0)
                {
                    _logger.LogDebug($"{nameof(LightningInvoiceWatcher)} processing {count} transactions.");

                    await Task.WhenAll(list.Select(transaction => _walletService.CheckPendingTransaction(transaction, stoppingToken)));
                }

                await Task.Delay(5_000, stoppingToken);
            }
        }
    }

    public class LightningInvoiceWatcher : BackgroundService
    {
        public IServiceProvider Services { get; }
        private readonly ILogger<LightningInvoiceWatcher> _logger;

        public LightningInvoiceWatcher(
            IServiceProvider services,
            ILogger<LightningInvoiceWatcher> logger)
        {
            Services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(LightningInvoiceWatcher)} starting.");

            await CheckInvoices(cancellationToken);
        }

        private async Task CheckInvoices(CancellationToken cancellationToken)
        {
            using (var scope = Services.CreateScope())
            {
                var scopedService = scope.ServiceProvider.GetRequiredService<IScopedLightningInvoiceWatcher>();
                await scopedService.CheckInvoices(cancellationToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(LightningInvoiceWatcher)} stopping.");

            await Task.CompletedTask;
        }
    }
}
