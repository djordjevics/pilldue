using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Pilldue.Business;
using Pilldue.Data;

namespace Pilldue.Data.Tests;

public class SqliteDatabasePathsTests
{
    [Fact]
    public void Default_config_path_is_beside_the_database_in_the_profile_folder()
    {
        var database = SqliteDatabasePaths.GetDefaultDatabasePath();
        var config = SqliteDatabasePaths.GetDefaultConfigPath();

        Assert.Equal(Path.GetDirectoryName(database), Path.GetDirectoryName(config));
        Assert.Equal(SqliteDatabasePaths.ConfigFileName, Path.GetFileName(config));
        Assert.EndsWith(
            Path.Combine(SqliteDatabasePaths.FolderName, SqliteDatabasePaths.ConfigFileName),
            config,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Legacy_database_path_stays_under_local_app_data()
    {
        var path = SqliteDatabasePaths.GetLegacyDatabasePath();
        var expected = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            SqliteDatabasePaths.FolderName,
            SqliteDatabasePaths.FileName);

        Assert.Equal(expected, path);
    }

    [Fact]
    public void Design_time_database_stays_out_of_the_user_folder()
    {
        using var db = new PilldueDbContextFactory().CreateDbContext([]);
        var connection = db.Database.GetConnectionString();

        Assert.Equal(SqliteDatabasePaths.CreateConnectionString("pilldue-design.db"), connection);
        Assert.DoesNotContain(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                SqliteDatabasePaths.FolderName),
            connection,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Move_legacy_store_keeps_medications_and_language()
    {
        var root = CreateRoot();
        var legacyDb = Path.Combine(root, "legacy", SqliteDatabasePaths.FileName);
        var legacyConfig = Path.Combine(root, "legacy", SqliteDatabasePaths.ConfigFileName);
        var profileDb = Path.Combine(root, "profile", SqliteDatabasePaths.FileName);
        var profileConfig = Path.Combine(root, "profile", SqliteDatabasePaths.ConfigFileName);

        try
        {
            await using (var session = await PilldueComposition.CreateSessionAsync(legacyDb, legacyConfig))
            {
                await session.App.AddMedicationAsync(SampleMedication("LegacyMed"));
                await session.App.SaveConfigAsync(new AppConfig
                {
                    UiLanguage = AppConfig.SerbianLanguage,
                });
            }

            SqliteConnection.ClearAllPools();
            SqliteDatabasePaths.MoveLegacyStoreIfNeeded(profileDb, legacyDb);

            Assert.False(File.Exists(legacyDb));
            Assert.False(File.Exists(legacyConfig));

            await using var moved = await PilldueComposition.CreateSessionAsync(profileDb, profileConfig);
            var listed = Assert.Single(await moved.App.ListMedicationsAsync());
            Assert.Equal("LegacyMed", listed.Name);
            var config = await moved.App.GetConfigAsync();
            Assert.Equal(AppConfig.SerbianLanguage, config.UiLanguage);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            TryDeleteDirectory(root);
        }
    }

    [Fact]
    public void Move_leaves_an_existing_profile_database_in_place()
    {
        var root = CreateRoot();
        var legacyDb = Path.Combine(root, "legacy", SqliteDatabasePaths.FileName);
        var legacyConfig = Path.Combine(root, "legacy", SqliteDatabasePaths.ConfigFileName);
        var profileDb = Path.Combine(root, "profile", SqliteDatabasePaths.FileName);
        var profileConfig = Path.Combine(root, "profile", SqliteDatabasePaths.ConfigFileName);

        try
        {
            WriteFile(legacyDb, "legacy-db");
            WriteFile(legacyConfig, "legacy-config");
            WriteFile(profileDb, "profile-db");
            WriteFile(profileConfig, "profile-config");

            SqliteDatabasePaths.MoveLegacyStoreIfNeeded(profileDb, legacyDb);

            Assert.Equal("legacy-db", File.ReadAllText(legacyDb));
            Assert.Equal("legacy-config", File.ReadAllText(legacyConfig));
            Assert.Equal("profile-db", File.ReadAllText(profileDb));
            Assert.Equal("profile-config", File.ReadAllText(profileConfig));
        }
        finally
        {
            TryDeleteDirectory(root);
        }
    }

    [Fact]
    public void Move_does_nothing_when_the_legacy_database_is_missing()
    {
        var root = CreateRoot();
        var legacyDb = Path.Combine(root, "legacy", SqliteDatabasePaths.FileName);
        var profileDb = Path.Combine(root, "profile", SqliteDatabasePaths.FileName);

        try
        {
            SqliteDatabasePaths.MoveLegacyStoreIfNeeded(profileDb, legacyDb);

            Assert.False(File.Exists(profileDb));
            Assert.False(Directory.Exists(Path.GetDirectoryName(profileDb)));
        }
        finally
        {
            TryDeleteDirectory(root);
        }
    }

    [Fact]
    public void Move_brings_sqlite_sidecar_files_with_the_database()
    {
        var root = CreateRoot();
        var legacyDb = Path.Combine(root, "legacy", SqliteDatabasePaths.FileName);
        var profileDb = Path.Combine(root, "profile", SqliteDatabasePaths.FileName);

        try
        {
            WriteFile(legacyDb, "db");
            WriteFile(legacyDb + "-wal", "wal");
            WriteFile(legacyDb + "-shm", "shm");
            WriteFile(legacyDb + "-journal", "journal");

            SqliteDatabasePaths.MoveLegacyStoreIfNeeded(profileDb, legacyDb);

            Assert.Equal("db", File.ReadAllText(profileDb));
            Assert.Equal("wal", File.ReadAllText(profileDb + "-wal"));
            Assert.Equal("shm", File.ReadAllText(profileDb + "-shm"));
            Assert.Equal("journal", File.ReadAllText(profileDb + "-journal"));
            Assert.False(File.Exists(legacyDb));
            Assert.False(File.Exists(legacyDb + "-wal"));
        }
        finally
        {
            TryDeleteDirectory(root);
        }
    }

    private static Medication SampleMedication(string name) => new()
    {
        Name = name,
        PackageSizePills = 28,
        PrescribedPackageCount = 1,
        DailyDosagePills = 1,
        CurrentStockPills = 7,
        PrescriptionStartDate = new DateOnly(2026, 1, 1),
    };

    private static string CreateRoot() =>
        Path.Combine(Path.GetTempPath(), "pilldue-tests", Guid.NewGuid().ToString("N"));

    private static void WriteFile(string path, string contents)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, contents);
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (IOException)
        {
            // Temp cleanup is best-effort on Windows when SQLite still holds a handle briefly.
        }
    }
}
