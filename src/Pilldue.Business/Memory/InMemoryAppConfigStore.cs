namespace Pilldue.Business;

public sealed class InMemoryAppConfigStore : IAppConfigStore
{
    private AppConfig _config = new();

    public Task<AppConfig> LoadAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AppConfig
        {
            DefaultRefillDayOfMonth = _config.DefaultRefillDayOfMonth,
            UiLanguage = _config.UiLanguage,
        });
    }

    public Task SaveAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);
        _config = new AppConfig
        {
            DefaultRefillDayOfMonth = config.DefaultRefillDayOfMonth,
            UiLanguage = config.UiLanguage,
        };
        return Task.CompletedTask;
    }
}
