using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LNblitz.Data.Models;
using LNblitz.Data.Queries;
using LNblitz.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LNblitz.Data.Services
{
    public class WalletManager
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public WalletManager(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<IEnumerable<Wallet>> GetWallets(WalletsQuery query)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var queryable = context.Wallets.Where(w => w.UserId == query.UserId);
            return await queryable.ToListAsync();
        }

        public async Task<Wallet> GetWallet(WalletQuery query)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var queryable = context.Wallets.Where(w => w.UserId == query.UserId && w.WalletId == query.WalletId);

            if (query.IncludeTransactions)
            {
                queryable = queryable.Include(w => w.Transactions).AsNoTracking();
            }

            return await queryable.FirstOrDefaultAsync();
        }

        public async Task AddOrUpdateWallet(Wallet wallet)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (string.IsNullOrEmpty(wallet.WalletId))
            {
                context.Wallets.Add(wallet);
            }
            else
            {
                context.Entry(wallet).State = EntityState.Modified;
            }
            await context.SaveChangesAsync();
        }

        public async Task RemoveWallet(WalletQuery query)
        {
            var wallet = await GetWallet(query);
            await RemoveWallet(wallet);
        }

        public async Task RemoveWallet(Wallet wallet)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            context.Wallets.Remove(wallet);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(TransactionsQuery query)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var queryable = context.Transactions.AsQueryable();

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
                // FIXME: https://docs.microsoft.com/en-us/dotnet/standard/datetime/converting-between-datetime-and-offset#conversions-from-datetimeoffset-to-datetime
                var enumerable = queryable.AsEnumerable(); // Switch to client side filtering
                return enumerable.Where(t => t.ExpiresAt > DateTimeOffset.UtcNow).ToList();
            }

            return await queryable.ToListAsync();
        }

        public async Task<Transaction> GetTransaction(TransactionQuery query)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var wallet = await GetWallet(new WalletQuery
            {
                UserId = query.UserId,
                WalletId = query.WalletId,
                IncludeTransactions = true
            });

            return wallet?.Transactions.SingleOrDefault(t => t.TransactionId == query.TransactionId);
        }

        public async Task CreateTransaction(Transaction transaction)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var btcpay = scope.ServiceProvider.GetRequiredService<BTCPayService>();

            var data = await btcpay.CreateInvoice(new CreateInvoiceRequest
                {
                    Amount = transaction.Amount,
                    Description = transaction.Description
                }
            );

            transaction.InvoiceId = data.Id;
            transaction.ExpiresAt = data.ExpiresAt;
            transaction.PaymentRequest = data.BOLT11;

            await context.Transactions.AddAsync(transaction);
            await context.SaveChangesAsync();
        }

        public async Task UpdateTransaction(Transaction transaction)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            context.Entry(transaction).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        public async Task RemoveTransaction(TransactionQuery query)
        {
            var transaction = await GetTransaction(query);
            await RemoveTransaction(transaction);
        }

        public async Task RemoveTransaction(Transaction transaction)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            context.Transactions.Remove(transaction);
            await context.SaveChangesAsync();
        }
    }
}
