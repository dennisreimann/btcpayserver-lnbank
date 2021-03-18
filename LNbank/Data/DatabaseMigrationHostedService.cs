using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LNbank.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LNbank
{
    public class DatabaseMigrationHostedService : IHostedService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ILogger<DatabaseMigrationHostedService> _logger;

        public DatabaseMigrationHostedService(IDbContextFactory<ApplicationDbContext> dbContextFactory,
            ILogger<DatabaseMigrationHostedService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var context = _dbContextFactory
                .CreateDbContext();
            var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();
            if (pending.Any())
            {
                _logger.LogInformation($"Applying pending migrations: {(string.Join(',', pending))}");
                await context.Database.MigrateAsync(cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
