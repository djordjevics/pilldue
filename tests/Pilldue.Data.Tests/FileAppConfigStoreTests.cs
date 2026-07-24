using Pilldue.Business;
using Pilldue.Data;

namespace Pilldue.Data.Tests;

public class FileAppConfigStoreTests
{
    [Fact]
    public async Task Load_missing_file_returns_empty_language()
    {
        var path = Path.Combine(Path.GetTempPath(), $"pilldue-config-{Guid.NewGuid():N}.json");
        try
        {
            Assert.False(File.Exists(path));
            var store = new FileAppConfigStore(path);

            var config = await store.LoadAsync();

            Assert.Equal(string.Empty, config.UiLanguage);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task Save_then_load_round_trips_language()
    {
        var path = Path.Combine(Path.GetTempPath(), $"pilldue-config-{Guid.NewGuid():N}.json");
        try
        {
            var store = new FileAppConfigStore(path);

            await store.SaveAsync(new AppConfig
            {
                UiLanguage = AppConfig.SerbianLanguage,
            });
            var loaded = await store.LoadAsync();

            Assert.Equal(AppConfig.SerbianLanguage, loaded.UiLanguage);

            var reloaded = await new FileAppConfigStore(path).LoadAsync();
            Assert.Equal(AppConfig.SerbianLanguage, reloaded.UiLanguage);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task Load_ignores_legacy_default_refill_day_property()
    {
        var path = Path.Combine(Path.GetTempPath(), $"pilldue-config-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(
                path,
                """
                {
                  "defaultRefillDayOfMonth": 12,
                  "uiLanguage": "en"
                }
                """);

            var loaded = await new FileAppConfigStore(path).LoadAsync();
            Assert.Equal(AppConfig.EnglishLanguage, loaded.UiLanguage);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
