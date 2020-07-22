using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LNblitz.Data.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LNblitz.Models;

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

            var transaction = wallet?.Transactions.SingleOrDefault(t => t.TransactionId == query.TransactionId);

            return transaction;
        }

        public async Task AddOrUpdateTransaction(Transaction transaction)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (string.IsNullOrEmpty(transaction.TransactionId))
            {
                await context.Transactions.AddAsync(transaction);
            }
            else
            {
                context.Entry(transaction).State = EntityState.Modified;
            }
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
