using Microsoft.EntityFrameworkCore;

namespace LNbank
{
    public interface IDbContextFactory<TContext>where TContext : DbContext
    {
        TContext CreateDbContext();
    }
}
