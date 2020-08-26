using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Lightning;
using LNblitz.Data.Models;
using LNblitz.Data.Queries;
using LNblitz.Data.Services;
using Microsoft.EntityFrameworkCore.Internal;
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
        private readonly WalletManager _walletManager;
        private readonly BTCPayService _btcpayService;

        public ScopedLightningInvoiceWatcher(
            ILogger<ScopedLightningInvoiceWatcher> logger,
            WalletManager walletManager,
            BTCPayService btcpayService)
        {
            _logger = logger;
            _walletManager = walletManager;
            _btcpayService = btcpayService;
        }

        public async Task CheckInvoices(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var transactions = await _walletManager.GetTransactions(new TransactionsQuery
                {
                    IncludingPending = true,
                    IncludingExpired = false,
                    IncludingPaid = false
                });

                var list = transactions.ToList();
                int count = list.Count();
                if (count > 0)
                {
                    _logger.LogDebug($"{nameof(LightningInvoiceWatcher)} processing {count} transactions.");

                    await Task.WhenAll(list.Select(transaction => CheckInvoice(transaction, stoppingToken)));
                }

                await Task.Delay(5_000, stoppingToken);
            }
        }

        public async Task CheckInvoice(Transaction transaction, CancellationToken stoppingToken)
        {
            var invoice = await _btcpayService.GetInvoice(transaction.InvoiceId, stoppingToken);
            if (invoice.Status == LightningInvoiceStatus.Paid)
            {
                _logger.LogInformation($"Marking invoice {invoice.Id} as paid.");
                transaction.AmountReceived = invoice.AmountReceived;
                transaction.PaidAt = invoice.PaidAt;
                await _walletManager.UpdateTransaction(transaction);
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
