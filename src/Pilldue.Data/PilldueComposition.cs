using Pilldue.Business;

namespace Pilldue.Data;

/// <summary>
/// Composition helpers: open SQLite, migrate, and build <see cref="IPilldueApp"/>.
/// UI hosts call this instead of wiring EF repositories themselves.
/// </summary>
public static class PilldueComposition
{
    /// <summary>
    /// Creates a session for the default local database and config paths.
    /// Dispose the session when the host exits (owns the long-lived DbContext).
    /// </summary>
    public static Task<PilldueSession> CreateDefaultSessionAsync(
        CancellationToken cancellationToken = default)
    {
        var dbPath = SqliteDatabasePaths.GetDefaultDatabasePath();
        var configPath = SqliteDatabasePaths.GetDefaultConfigPath();
        return CreateSessionAsync(dbPath, configPath, cancellationToken);
    }

    /// <summary>
    /// Creates a session for explicit database and config file paths (tests / alternate hosts).
    /// </summary>
    public static async Task<PilldueSession> CreateSessionAsync(
        string databasePath,
        string configPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(configPath);

        await PilldueDbBootstrap.MigrateAsync(databasePath, cancellationToken).ConfigureAwait(false);
        var options = PilldueDbBootstrap.CreateOptions(databasePath);
        var db = new PilldueDbContext(options);

        IPilldueApp app = new PilldueApp(
            new EfMedicationRepository(db),
            new EfRefillEventRepository(db),
            new EfSkipDoseEventRepository(options),
            new EfMissedDoseEventRepository(options),
            new FileAppConfigStore(configPath));

        return new PilldueSession(app, db);
    }
}

/// <summary>Owns the EF context for the lifetime of a UI host process.</summary>
public sealed class PilldueSession : IAsyncDisposable
{
    private readonly PilldueDbContext _db;

    public PilldueSession(IPilldueApp app, PilldueDbContext db)
    {
        App = app ?? throw new ArgumentNullException(nameof(app));
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public IPilldueApp App { get; }

    public ValueTask DisposeAsync() => _db.DisposeAsync();
}
