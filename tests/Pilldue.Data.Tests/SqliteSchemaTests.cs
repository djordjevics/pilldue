using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Pilldue.Data;

namespace Pilldue.Data.Tests;

public class SqliteSchemaTests
{
    [Fact]
    public async Task Migrate_creates_medications_refill_events_and_skip_dose_events_tables()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), "pilldue-tests", $"schema-{Guid.NewGuid():N}.db");

        try
        {
            await PilldueDbBootstrap.MigrateAsync(dbPath);

            Assert.True(File.Exists(dbPath));

            await using (var connection = new SqliteConnection(
                SqliteDatabasePaths.CreateConnectionString(dbPath)))
            {
                await connection.OpenAsync();

                var tables = await ListUserTablesAsync(connection);
                Assert.Contains("medications", tables);
                Assert.Contains("refill_events", tables);
                Assert.Contains("skip_dose_events", tables);
            }

            await using (var db = new PilldueDbContext(PilldueDbBootstrap.CreateOptions(dbPath)))
            {
                Assert.Empty(await db.Medications.AsNoTracking().ToListAsync());
                Assert.Empty(await db.RefillEvents.AsNoTracking().ToListAsync());
                Assert.Empty(await db.SkipDoseEvents.AsNoTracking().ToListAsync());
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            TryDelete(dbPath);
        }
    }

    [Fact]
    public async Task Migrate_is_idempotent_when_called_twice()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), "pilldue-tests", $"idempotent-{Guid.NewGuid():N}.db");

        try
        {
            await PilldueDbBootstrap.MigrateAsync(dbPath);
            await PilldueDbBootstrap.MigrateAsync(dbPath);

            await using var connection = new SqliteConnection(
                SqliteDatabasePaths.CreateConnectionString(dbPath));
            await connection.OpenAsync();
            var tables = await ListUserTablesAsync(connection);

            Assert.Contains("medications", tables);
            Assert.Contains("refill_events", tables);
            Assert.Contains("skip_dose_events", tables);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            TryDelete(dbPath);
        }
    }

    [Fact]
    public void Default_database_path_is_under_local_app_data_pilldue()
    {
        var path = SqliteDatabasePaths.GetDefaultDatabasePath();
        var expected = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Pilldue",
            SqliteDatabasePaths.FileName);

        Assert.Equal(expected, path);
    }

    private static async Task<HashSet<string>> ListUserTablesAsync(SqliteConnection connection)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE '__EF%'";

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            names.Add(reader.GetString(0));
        }

        return names;
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
            // Temp cleanup is best-effort on Windows when SQLite still holds a handle briefly.
        }
    }
}
