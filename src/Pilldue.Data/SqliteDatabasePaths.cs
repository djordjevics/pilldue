namespace Pilldue.Data;

/// <summary>
/// Resolves the default on-disk location for the Pilldue SQLite database.
/// </summary>
public static class SqliteDatabasePaths
{
    /// <summary>SQLite file name used for the local database.</summary>
    public const string FileName = "pilldue.db";

    /// <summary>
    /// Default path: <c>%LocalAppData%/Pilldue/pilldue.db</c> on Windows
    /// (and the equivalent local application data folder on other OSes).
    /// Writable when the app is installed under Program Files; not beside the exe.
    /// </summary>
    public static string GetDefaultDatabasePath()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(root, "Pilldue", FileName);
    }

    /// <summary>Builds a SQLite connection string for the given database file path.</summary>
    public static string CreateConnectionString(string databasePath) =>
        $"Data Source={databasePath}";
}
