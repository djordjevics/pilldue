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
        _config = new AppConfig
        {
            UiLanguage = config.UiLanguage,
        };
    }

    public Task<AppConfig> LoadAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AppConfig
        {
            UiLanguage = _config.UiLanguage,
        });
    }

    public Task SaveAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);
        _config = new AppConfig
        {
            UiLanguage = config.UiLanguage,
        };
        return Task.CompletedTask;
    }
}
