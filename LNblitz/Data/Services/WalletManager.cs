using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LNblitz.Data.Models;
using LNblitz.Data.Queries;
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

            if (query.IncludeTransactions)
            {
                queryable = queryable.Include(w => w.Transactions).AsNoTracking();
            }

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

        public async Task<Transaction> CreateReceiveTransaction(Transaction transaction)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var entry = await context.Transactions.AddAsync(transaction);
            await context.SaveChangesAsync();

            return entry.Entity;
        }

        public async Task<Transaction> CreateSendTransaction(Transaction transaction)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var entry = await context.Transactions.AddAsync(transaction);
            await context.SaveChangesAsync();

            return entry.Entity;
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
