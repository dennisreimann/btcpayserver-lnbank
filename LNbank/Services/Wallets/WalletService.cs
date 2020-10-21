using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Lightning;
using LNbank.Data;
using LNbank.Data.Models;
using LNbank.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Transaction = LNbank.Data.Models.Transaction;

namespace LNbank.Services.Wallets
{
    public class WalletService : IDisposable, IAsyncDisposable
    {
        private readonly ILogger _logger;
        private readonly BTCPayService _btcpayService;
        private readonly IHubContext<InvoiceHub> _invoiceHub;
        private readonly ApplicationDbContext _dbContext;

        public WalletService(
            ILogger<WalletService> logger,
            BTCPayService btcpayService,
            IHubContext<InvoiceHub> invoiceHub,
            ApplicationDbContext dbContext)
        {
            _logger = logger;
            _btcpayService = btcpayService;
            _invoiceHub = invoiceHub;
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Wallet>> GetWallets(WalletsQuery query)
        {
            var queryable = _dbContext.Wallets.Where(w => w.UserId == query.UserId);

            if (query.IncludeTransactions)
            {
                queryable = queryable.Include(w => w.Transactions).AsNoTracking();
            }

            return await queryable.ToListAsync();
        }

        public async Task<Wallet> GetWallet(WalletQuery query)
        {
            var queryable = _dbContext.Wallets
                .Where(w => w.UserId == query.UserId && w.WalletId == query.WalletId);

            if (query.IncludeTransactions)
            {
                queryable = queryable.Include(w => w.Transactions).AsNoTracking();
            }

            return await queryable.FirstOrDefaultAsync();
        }

        public async Task<Data.Models.Transaction> Receive(Wallet wallet, long sats, string description)
        {
            var data = await _btcpayService.CreateInvoice(new CreateInvoiceRequest
            {
                Amount = LightMoney.Satoshis(sats),
                Description = description
            });

            var entry = await _dbContext.Transactions.AddAsync(new Data.Models.Transaction
            {
                WalletId = wallet.WalletId,
                InvoiceId = data.Id,
                Amount = data.Amount,
                ExpiresAt = data.ExpiresAt,
                PaymentRequest = data.BOLT11,
                Description = description
            });
            await _dbContext.SaveChangesAsync();

            return entry.Entity;
        }

        public async Task<Data.Models.Transaction> Send(Wallet wallet, BOLT11PaymentRequest bolt11, string paymentRequest)
        {
            var amount = bolt11.MinimumAmount;

            if (wallet.Balance < amount)
            {
                throw new Exception($"Insufficient balance: {wallet.Balance} msat, tried to send {amount} msat.");
            }

            await _btcpayService.PayInvoice(new PayInvoiceRequest { PaymentRequest = paymentRequest });

            var entry = await _dbContext.Transactions.AddAsync(new Data.Models.Transaction
            {
                WalletId = wallet.WalletId,
                PaymentRequest = paymentRequest,
                Amount = amount,
                AmountSettled = new LightMoney(amount.MilliSatoshi * -1),
                ExpiresAt = bolt11.ExpiryDate,
                Description = bolt11.ShortDescription,
                PaidAt = DateTimeOffset.UtcNow
            });
            await _dbContext.SaveChangesAsync();

            return entry.Entity;
        }

        public async Task<Wallet> AddOrUpdateWallet(Wallet wallet)
        {
            EntityEntry<Wallet> entry;

            if (string.IsNullOrEmpty(wallet.WalletId))
            {
                entry = _dbContext.Wallets.Add(wallet);
            }
            else
            {
                entry = _dbContext.Entry(wallet);
                entry.State = EntityState.Modified;
            }
            await _dbContext.SaveChangesAsync();

            return entry.Entity;
        }

        public async Task RemoveWallet(Wallet wallet)
        {
            _dbContext.Wallets.Remove(wallet);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Data.Models.Transaction>> GetTransactions(TransactionsQuery query)
        {
            var queryable = _dbContext.Transactions.AsQueryable();

            if (query.UserId != null) query.IncludeWallet = true;

            if (query.WalletId != null)
            {
                queryable = queryable.Where(t => t.WalletId == query.WalletId);
            }
            if (query.IncludeWallet)
            {
                queryable = queryable.Include(t => t.Wallet).AsNoTracking();
            }
            if (query.UserId != null)
            {
                queryable = queryable.Where(t => t.Wallet.UserId == query.UserId);
            }

            if (!query.IncludingPaid)
            {
                queryable = queryable.Where(t => t.PaidAt == null);
            }

            if (!query.IncludingPending)
            {
                queryable = queryable.Where(t => t.PaidAt != null);
            }

            if (!query.IncludingExpired)
            {
                var enumerable = queryable.AsEnumerable(); // Switch to client side filtering
                return enumerable.Where(t => t.ExpiresAt > DateTimeOffset.UtcNow).ToList();
            }

            return await queryable.ToListAsync();
        }

        public async Task<IEnumerable<Data.Models.Transaction>> GetPendingTransactions()
        {
            return await GetTransactions(new TransactionsQuery
            {
                IncludingPending = true,
                IncludingExpired = false,
                IncludingPaid = false
            });
        }

        public async Task CheckPendingTransaction(Data.Models.Transaction transaction, CancellationToken stoppingToken)
        {
            var invoice = await _btcpayService.GetInvoice(transaction.InvoiceId, stoppingToken);
            if (invoice.Status == LightningInvoiceStatus.Paid)
            {
                _logger.LogInformation($"Marking invoice {invoice.Id} as paid.");

                transaction.AmountSettled = invoice.AmountReceived;
                transaction.PaidAt = invoice.PaidAt;

                await _invoiceHub.Clients.All.SendAsync("Message", $"Transaction {transaction.TransactionId} ({invoice.Id}) paid");
                await UpdateTransaction(transaction);
            }
        }

        public async Task<Data.Models.Transaction> GetTransaction(TransactionQuery query)
        {
            if (query.UserId == null && query.WalletId == null)
            {
                return _dbContext.Transactions.SingleOrDefault(t => t.TransactionId == query.TransactionId);
            }

            var wallet = await GetWallet(new WalletQuery
            {
                UserId = query.UserId,
                WalletId = query.WalletId,
                IncludeTransactions = true
            });

            return wallet?.Transactions.SingleOrDefault(t => t.TransactionId == query.TransactionId);
        }

        public async Task<Data.Models.Transaction> UpdateTransaction(Data.Models.Transaction transaction)
        {
            var entry = _dbContext.Entry(transaction);
            entry.State = EntityState.Modified;

            await _dbContext.SaveChangesAsync();

            return entry.Entity;
        }

        public async Task RemoveTransaction(Data.Models.Transaction transaction)
        {
            _dbContext.Transactions.Remove(transaction);
            await _dbContext.SaveChangesAsync();
        }

        public BOLT11PaymentRequest ParsePaymentRequest(string payReq)
        {
            // TODO: Configure network
            return BOLT11PaymentRequest.Parse(payReq, Network.RegTest);
        }

        public async ValueTask DisposeAsync()
        {
            await _dbContext.DisposeAsync();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
