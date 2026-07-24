namespace Pilldue.Business;

public interface IAppConfigStore
{
    Task<AppConfig> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(AppConfig config, CancellationToken cancellationToken = default);
}
