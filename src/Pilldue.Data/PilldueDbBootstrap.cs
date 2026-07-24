using Microsoft.EntityFrameworkCore;

namespace Pilldue.Data;

/// <summary>
/// Creates a configured <see cref="PilldueDbContext"/> and applies EF migrations.
/// </summary>
public static class PilldueDbBootstrap
{
    /// <summary>
    /// Builds options that point at <paramref name="databasePath"/> (created if missing).
    /// </summary>
    public static DbContextOptions<PilldueDbContext> CreateOptions(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return new DbContextOptionsBuilder<PilldueDbContext>()
            .UseSqlite(SqliteDatabasePaths.CreateConnectionString(databasePath))
            .Options;
    }

    /// <summary>
    /// Opens the database at <paramref name="databasePath"/> and applies pending migrations
    /// idempotently via <c>Database.MigrateAsync</c>.
    /// </summary>
    public static async Task MigrateAsync(
        string databasePath,
        CancellationToken cancellationToken = default)
    {
        await using var db = new PilldueDbContext(CreateOptions(databasePath));
        await db.Database.MigrateAsync(cancellationToken);
    }

    /// <summary>
    /// Applies pending migrations to an existing context (idempotent).
    /// </summary>
    public static Task MigrateAsync(
        PilldueDbContext db,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        return db.Database.MigrateAsync(cancellationToken);
    }
}
