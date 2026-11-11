namespace Pilldue.Business;

public sealed class InMemoryAppConfigStore : IAppConfigStore
{
    private AppConfig _config;

    public InMemoryAppConfigStore()
        : this(new AppConfig())
    {
    }

    public InMemoryAppConfigStore(AppConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _config = Copy(config);
    }

    public Task<AppConfig> LoadAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Copy(_config));
    }

    public Task SaveAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);
        _config = Copy(config);
        return Task.CompletedTask;
    }

    private static AppConfig Copy(AppConfig config) => new()
    {
        UiLanguage = config.UiLanguage,
        UiTheme = AppConfig.NormalizeTheme(config.UiTheme),
    };
}
