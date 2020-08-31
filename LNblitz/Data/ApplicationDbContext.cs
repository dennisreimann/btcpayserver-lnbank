using BTCPayServer.Lightning;
using LNblitz.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LNblitz.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Wallet> Wallets { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Transaction>()
                .Property(e => e.Amount)
                .HasConversion(
                    v => v.MilliSatoshi,
                    v => new LightMoney(v));

            modelBuilder
                .Entity<Transaction>()
                .Property(e => e.AmountSettled)
                .HasConversion(
                    v => v.MilliSatoshi,
                    v => new LightMoney(v));
        }
    }
}
