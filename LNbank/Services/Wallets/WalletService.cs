using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Client;
using BTCPayServer.Lightning;
using LNbank.Data;
using LNbank.Data.Models;
using LNbank.Hubs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Transaction = LNbank.Data.Models.Transaction;

namespace LNbank.Services.Wallets
{
    public class WalletService : IDisposable, IAsyncDisposable
    {
        private readonly ILogger _logger;
        private readonly BTCPayService _btcpayService;
        private readonly IHubContext<TransactionHub> _transactionHub;
        private readonly ApplicationDbContext _dbContext;
        private readonly Network _network;

        public WalletService(
            IWebHostEnvironment env,
            ILogger<WalletService> logger,
            IHubContext<TransactionHub> transactionHub,
            BTCPayService btcpayService,
            ApplicationDbContext dbContext)
        {
            _logger = logger;
            _btcpayService = btcpayService;
            _transactionHub = transactionHub;
            _dbContext = dbContext;

            // TODO: Configure network properly
            _network = env.IsDevelopment() ? Network.RegTest : Network.Main;
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

        public async Task<Transaction> Receive(Wallet wallet, long sats, string description) =>
            await Receive(wallet, sats, description, LightningInvoiceCreateRequest.ExpiryDefault);

        public async Task<Transaction> Receive(Wallet wallet, long sats, string description, TimeSpan expiry)
        {
            if (sats <= 0) throw new ArgumentException(nameof(sats));

            var data = await _btcpayService.CreateLightningInvoice(new LightningInvoiceCreateRequest
            {
                WalletId = wallet.WalletId,
                Amount = LightMoney.Satoshis(sats),
                Description = description,
                Expiry = expiry
            });

            var entry = await _dbContext.Transactions.AddAsync(new Transaction
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

        public async Task<Transaction> Send(Wallet wallet, BOLT11PaymentRequest bolt11, string paymentRequest)
        {
            var amount = bolt11.MinimumAmount;

            if (bolt11.ExpiryDate <= DateTimeOffset.UtcNow)
            {
                throw new Exception($"Payment request already expired at {bolt11.ExpiryDate}.");
            }

            if (wallet.Balance < amount)
            {
                var balanceSats = wallet.Balance.ToUnit(LightMoneyUnit.Satoshi);
                var amountSats = amount.ToUnit(LightMoneyUnit.Satoshi);
                throw new Exception($"Insufficient balance: {balanceSats} sats, tried to send {amountSats} sats.");
            }

            // pay via the node and fall back to internal payment
            Transaction internalReceivingTransaction = null;
            try
            {
                await _btcpayService.PayLightningInvoice(new LightningInvoicePayRequest
                {
                    PaymentRequest = paymentRequest
                });
            }
            catch (GreenFieldAPIException ex) when (ex.APIError.Code == "could-not-find-route")
            {
                internalReceivingTransaction = await GetTransaction(new TransactionQuery
                {
                    PaymentRequest = paymentRequest,
                    HasInvoiceId = true
                });
                if (internalReceivingTransaction == null) throw;
            }

            if (internalReceivingTransaction != null)
            {
                if (internalReceivingTransaction.IsExpired)
                {
                    throw new Exception($"Payment request already expired at {internalReceivingTransaction.ExpiresAt}.");
                }
                if (internalReceivingTransaction.IsPaid)
                {
                    throw new Exception($"Payment request has already been paid.");
                }
            }

            // https://docs.microsoft.com/en-us/ef/core/saving/transactions#controlling-transactions
            await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();
            var now = DateTimeOffset.UtcNow;
            var entry = await _dbContext.Transactions.AddAsync(new Transaction
            {
                WalletId = wallet.WalletId,
                PaymentRequest = paymentRequest,
                Amount = amount,
                AmountSettled = new LightMoney(amount.MilliSatoshi * -1),
                ExpiresAt = bolt11.ExpiryDate,
                Description = bolt11.ShortDescription,
                PaidAt = now
            });
            await _dbContext.SaveChangesAsync();

            if (internalReceivingTransaction != null)
            {
                await MarkTransactionPaid(internalReceivingTransaction, amount, now);
            }
            await dbTransaction.CommitAsync();

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

        public async Task<IEnumerable<Transaction>> GetTransactions(TransactionsQuery query)
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

        public async Task<IEnumerable<Transaction>> GetPendingTransactions()
        {
            return await GetTransactions(new TransactionsQuery
            {
                IncludingPending = true,
                IncludingExpired = false,
                IncludingPaid = false
            });
        }

        public async Task CheckPendingTransaction(Transaction transaction, CancellationToken cancellationToken = default(CancellationToken))
        {
            var invoice = await _btcpayService.GetLightningInvoice(transaction.InvoiceId, cancellationToken);
            if (invoice.Status == LightningInvoiceStatus.Paid)
            {
                await MarkTransactionPaid(transaction, invoice.AmountReceived, invoice.PaidAt);
            }
        }

        public async Task<Transaction> GetTransaction(TransactionQuery query)
        {
            IQueryable<Transaction> queryable = _dbContext.Transactions.AsQueryable();

            if (query.WalletId != null)
            {
                var walletQuery = new WalletQuery
                {
                    WalletId = query.WalletId,
                    IncludeTransactions = true
                };

                if (query.UserId != null) walletQuery.UserId = query.UserId;

                var wallet = await GetWallet(walletQuery);

                if (wallet == null) return null;

                queryable = wallet.Transactions.AsQueryable();
            }

            if (query.InvoiceId != null)
            {
                queryable = queryable.Where(t => t.InvoiceId == query.InvoiceId);
            }
            else if (query.HasInvoiceId)
            {
                queryable = queryable.Where(t => t.InvoiceId != null);
            }

            if (query.TransactionId != null)
            {
                queryable = queryable.Where(t => t.TransactionId == query.TransactionId);
            }

            if (query.PaymentRequest != null)
            {
                queryable = queryable.Where(t => t.PaymentRequest == query.PaymentRequest);
            }

            return queryable.SingleOrDefault();
        }

        public async Task<Transaction> UpdateTransaction(Transaction transaction)
        {
            var entry = _dbContext.Entry(transaction);
            entry.State = EntityState.Modified;

            await _dbContext.SaveChangesAsync();

            return entry.Entity;
        }

        public async Task RemoveTransaction(Transaction transaction)
        {
            _dbContext.Transactions.Remove(transaction);
            await _dbContext.SaveChangesAsync();
        }

        public BOLT11PaymentRequest ParsePaymentRequest(string payReq)
        {
            return BOLT11PaymentRequest.Parse(payReq, _network);
        }

        private async Task MarkTransactionPaid(Transaction transaction, LightMoney amountSettled, DateTimeOffset? date)
        {
            _logger.LogInformation($"Marking transaction {transaction.TransactionId} as paid.");

            transaction.AmountSettled = amountSettled;
            transaction.PaidAt = date;

            await UpdateTransaction(transaction);
            await _transactionHub.Clients.All.SendAsync("transaction-update", new
            {
                transaction.TransactionId,
                transaction.InvoiceId,
                transaction.WalletId,
                transaction.Status,
                transaction.IsPaid,
                transaction.IsExpired,
                Event = "paid"
            });
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
