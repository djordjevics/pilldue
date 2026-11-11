using Pilldue.Business;

namespace Pilldue.Business.Tests;

public class AppConfigDefaultsTests
{
    [Fact]
    public void Default_ui_language_is_empty_for_os_detection()
    {
        Assert.Equal(string.Empty, new AppConfig().UiLanguage);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("  ", "")]
    [InlineData("en", AppConfig.EnglishLanguage)]
    [InlineData("EN", AppConfig.EnglishLanguage)]
    [InlineData("sr", AppConfig.SerbianLanguage)]
    [InlineData("fr", AppConfig.EnglishLanguage)]
    public void Language_to_store_leaves_system_blank(string? stored, string expected)
    {
        Assert.Equal(expected, AppConfig.LanguageToStore(stored));
    }

    [Fact]
    public void Default_theme_follows_the_system()
    {
        Assert.Equal(AppConfig.ThemeSystem, new AppConfig().UiTheme);
    }

    [Theory]
    [InlineData(null, AppConfig.ThemeSystem)]
    [InlineData("", AppConfig.ThemeSystem)]
    [InlineData("  ", AppConfig.ThemeSystem)]
    [InlineData("system", AppConfig.ThemeSystem)]
    [InlineData("SYSTEM", AppConfig.ThemeSystem)]
    [InlineData("light", AppConfig.ThemeLight)]
    [InlineData("Light", AppConfig.ThemeLight)]
    [InlineData("dark", AppConfig.ThemeDark)]
    [InlineData("nope", AppConfig.ThemeSystem)]
    public void Normalize_theme_accepts_light_dark_and_system(string? stored, string expected)
    {
        Assert.Equal(expected, AppConfig.NormalizeTheme(stored));
    }

    [Fact]
    public async Task In_memory_store_round_trips_language_and_theme()
    {
        var store = new InMemoryAppConfigStore();

        await store.SaveAsync(new AppConfig
        {
            UiLanguage = string.Empty,
            UiTheme = "DARK",
        });
        var loaded = await store.LoadAsync();

        Assert.Equal(string.Empty, loaded.UiLanguage);
        Assert.Equal(AppConfig.ThemeDark, loaded.UiTheme);
    }
}
