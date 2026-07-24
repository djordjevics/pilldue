using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pilldue.Data;

/// <summary>
/// Design-time factory for <c>dotnet ef migrations</c>.
/// </summary>
public sealed class PilldueDbContextFactory : IDesignTimeDbContextFactory<PilldueDbContext>
{
    public PilldueDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PilldueDbContext>()
            .UseSqlite(SqliteDatabasePaths.CreateConnectionString("pilldue-design.db"))
            .Options;

        return new PilldueDbContext(options);
    }
}
