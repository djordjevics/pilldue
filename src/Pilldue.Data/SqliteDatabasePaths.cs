namespace Pilldue.Data;

/// <summary>
/// Resolves the default on-disk location for the Pilldue SQLite database and UI config.
/// </summary>
public static class SqliteDatabasePaths
{
    /// <summary>Folder name under the user profile (and under the legacy LocalAppData root).</summary>
    public const string FolderName = "Pilldue";

    /// <summary>SQLite file name used for the local database.</summary>
    public const string FileName = "pilldue.db";

    /// <summary>JSON file name for UI language, beside <see cref="FileName"/>.</summary>
    public const string ConfigFileName = "config.json";

    /// <summary>
    /// Default path: <c>%UserProfile%/Pilldue/pilldue.db</c>.
    /// Visible and writable; not beside the exe and not under LocalAppData.
    /// </summary>
    public static string GetDefaultDatabasePath() =>
        Path.Combine(GetProfileFolder(), FileName);

    /// <summary>Default JSON config path: <c>%UserProfile%/Pilldue/config.json</c>.</summary>
    public static string GetDefaultConfigPath() =>
        Path.Combine(GetProfileFolder(), ConfigFileName);

    /// <summary>Previous default database path: <c>%LocalAppData%/Pilldue/pilldue.db</c>.</summary>
    public static string GetLegacyDatabasePath() =>
        Path.Combine(GetLocalAppDataFolder(), FileName);

    /// <summary>
    /// When the profile folder has no database and a LocalAppData database exists,
    /// moves that database (and a sibling <c>config.json</c>) into the profile folder.
    /// </summary>
    public static void MoveLegacyStoreIfNeeded() =>
        MoveLegacyStoreIfNeeded(GetDefaultDatabasePath(), GetLegacyDatabasePath());

    /// <summary>
    /// Moves <paramref name="legacyDatabasePath"/> and a sibling <c>config.json</c>
    /// into the directory of <paramref name="destinationDatabasePath"/> when that
    /// destination database does not already exist.
    /// </summary>
    public static void MoveLegacyStoreIfNeeded(string destinationDatabasePath, string legacyDatabasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationDatabasePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(legacyDatabasePath);

        if (PathsEqual(destinationDatabasePath, legacyDatabasePath))
        {
            return;
        }

        if (File.Exists(destinationDatabasePath) || !File.Exists(legacyDatabasePath))
        {
            return;
        }

        var destinationDirectory = Path.GetDirectoryName(destinationDatabasePath)
            ?? throw new InvalidOperationException("Destination database path has no directory.");
        Directory.CreateDirectory(destinationDirectory);

        MoveFile(legacyDatabasePath, destinationDatabasePath);
        MoveIfExists(legacyDatabasePath + "-wal", destinationDatabasePath + "-wal");
        MoveIfExists(legacyDatabasePath + "-shm", destinationDatabasePath + "-shm");
        MoveIfExists(legacyDatabasePath + "-journal", destinationDatabasePath + "-journal");

        var legacyDirectory = Path.GetDirectoryName(legacyDatabasePath);
        if (string.IsNullOrEmpty(legacyDirectory))
        {
            return;
        }

        var legacyConfig = Path.Combine(legacyDirectory, ConfigFileName);
        var destinationConfig = Path.Combine(destinationDirectory, ConfigFileName);
        if (File.Exists(legacyConfig) && !File.Exists(destinationConfig))
        {
            MoveFile(legacyConfig, destinationConfig);
        }
    }

    /// <summary>Builds a SQLite connection string for the given database file path.</summary>
    public static string CreateConnectionString(string databasePath) =>
        $"Data Source={databasePath}";

    private static string GetProfileFolder()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(root, FolderName);
    }

    private static string GetLocalAppDataFolder()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(root, FolderName);
    }

    private static void MoveIfExists(string source, string destination)
    {
        if (File.Exists(source))
        {
            MoveFile(source, destination);
        }
    }

    private static void MoveFile(string source, string destination) =>
        File.Move(source, destination);

    private static bool PathsEqual(string left, string right) =>
        string.Equals(
            Path.GetFullPath(left),
            Path.GetFullPath(right),
            StringComparison.OrdinalIgnoreCase);
}
