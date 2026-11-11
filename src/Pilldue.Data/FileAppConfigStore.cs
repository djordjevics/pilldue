using System.Text.Json;
using Pilldue.Business;

namespace Pilldue.Data;

/// <summary>
/// JSON file implementation of <see cref="IAppConfigStore"/>. Separate from EF/SQLite.
/// Missing file yields an empty language (detect from OS at startup) and the system theme.
/// </summary>
public sealed class FileAppConfigStore : IAppConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    private readonly string _path;

    public FileAppConfigStore(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = path;
    }

    public async Task<AppConfig> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_path))
        {
            return CreateDefault();
        }

        await using var stream = File.OpenRead(_path);
        var config = await JsonSerializer.DeserializeAsync<AppConfig>(stream, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        if (config is null)
        {
            return CreateDefault();
        }

        config.UiTheme = AppConfig.NormalizeTheme(config.UiTheme);
        return config;
    }

    public async Task SaveAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var stored = new AppConfig
        {
            UiLanguage = config.UiLanguage,
            UiTheme = AppConfig.NormalizeTheme(config.UiTheme),
        };

        await using var stream = File.Create(_path);
        await JsonSerializer.SerializeAsync(stream, stored, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    private static AppConfig CreateDefault() => new()
    {
        UiLanguage = string.Empty,
        UiTheme = AppConfig.ThemeSystem,
    };
}
