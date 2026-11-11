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
            Assert.Equal(AppConfig.ThemeSystem, config.UiTheme);
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
                UiTheme = "LIGHT",
            });
            var loaded = await store.LoadAsync();

            Assert.Equal(AppConfig.SerbianLanguage, loaded.UiLanguage);
            Assert.Equal(AppConfig.ThemeLight, loaded.UiTheme);
            var json = await File.ReadAllTextAsync(path);
            Assert.Contains("\"uiLanguage\"", json, StringComparison.Ordinal);
            Assert.Contains("\"uiTheme\"", json, StringComparison.Ordinal);

            var reloaded = await new FileAppConfigStore(path).LoadAsync();
            Assert.Equal(AppConfig.SerbianLanguage, reloaded.UiLanguage);
            Assert.Equal(AppConfig.ThemeLight, reloaded.UiTheme);
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
            Assert.Equal(AppConfig.ThemeSystem, loaded.UiTheme);
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
    public async Task Blank_language_stays_blank_and_unknown_theme_is_system()
    {
        var path = Path.Combine(Path.GetTempPath(), $"pilldue-config-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(
                path,
                """
                {
                  "uiLanguage": "",
                  "uiTheme": "blue"
                }
                """);

            var loaded = await new FileAppConfigStore(path).LoadAsync();
            Assert.Equal(string.Empty, loaded.UiLanguage);
            Assert.Equal(AppConfig.ThemeSystem, loaded.UiTheme);

            await new FileAppConfigStore(path).SaveAsync(new AppConfig
            {
                UiLanguage = string.Empty,
                UiTheme = AppConfig.ThemeDark,
            });
            var saved = await new FileAppConfigStore(path).LoadAsync();
            Assert.Equal(string.Empty, saved.UiLanguage);
            Assert.Equal(AppConfig.ThemeDark, saved.UiTheme);
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
