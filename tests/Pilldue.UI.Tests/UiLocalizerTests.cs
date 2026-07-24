using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Tests;

public class UiLocalizerTests
{
    [Fact]
    public void Every_english_key_exists_in_serbian()
    {
        foreach (var key in UiLocalizer.RequiredKeys)
        {
            Assert.True(
                UiLocalizer.HasKey(AppConfig.SerbianLanguage, key),
                $"Missing Serbian translation for '{key}'.");
        }
    }

    [Fact]
    public void Apply_switches_menu_title_language()
    {
        UiLocalizer.Apply(new AppConfig { UiLanguage = AppConfig.EnglishLanguage });
        Assert.Equal("Main menu", UiLocalizer.Get("Menu.Title"));

        UiLocalizer.Apply(new AppConfig { UiLanguage = AppConfig.SerbianLanguage });
        Assert.Equal("Glavni meni", UiLocalizer.Get("Menu.Title"));
    }
}
