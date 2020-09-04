using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LNblitz.Services
{
    public class LightningInvoiceWatcher : BackgroundService
    {
        private readonly WalletService _walletService;
        private readonly ILogger<LightningInvoiceWatcher> _logger;

        public LightningInvoiceWatcher(
            WalletService walletService,
            ILogger<LightningInvoiceWatcher> logger)
        {
            _walletService = walletService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(LightningInvoiceWatcher)} starting.");

            await CheckInvoices(cancellationToken);
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

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(LightningInvoiceWatcher)} stopping.");

            await Task.CompletedTask;
        }
    }
}
