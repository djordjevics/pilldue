using Pilldue.Business;
using Pilldue.UI.Localization;
using Pilldue.UI.Wpf;

namespace Pilldue.UI.Wpf.Tests;

public sealed class SettingsViewModelTests : IDisposable
{
    public SettingsViewModelTests()
    {
        UiLocalizer.Apply(new AppConfig { UiLanguage = AppConfig.EnglishLanguage, UiTheme = AppConfig.ThemeLight });
    }

    public void Dispose()
    {
        UiLocalizer.Apply(new AppConfig { UiLanguage = AppConfig.EnglishLanguage, UiTheme = AppConfig.ThemeLight });
    }

    [Fact]
    public async Task Save_writes_language_and_theme_and_cancel_leaves_both()
    {
        var session = new TestSession();
        var interaction = new FakeUiInteraction();
        var config = await session.App.GetConfigAsync();
        var vm = new SettingsViewModel(session.App, interaction, config);

        vm.SelectedLanguage = vm.Languages.Single(choice => choice.Value == AppConfig.SerbianLanguage);
        vm.SelectedTheme = vm.Themes.Single(choice => choice.Value == AppConfig.ThemeDark);
        vm.CancelCommand.Execute(null);

        Assert.Equal(false, vm.CloseResult);
        Assert.Equal(AppConfig.EnglishLanguage, config.UiLanguage);
        Assert.Equal(AppConfig.ThemeLight, config.UiTheme);
        Assert.Equal(AppConfig.EnglishLanguage, UiLocalizer.Language);
        var stored = await session.App.GetConfigAsync();
        Assert.Equal(AppConfig.EnglishLanguage, stored.UiLanguage);
        Assert.Equal(AppConfig.ThemeLight, stored.UiTheme);

        await vm.SaveCommand.ExecuteAsync(null);

        Assert.Equal(true, vm.CloseResult);
        Assert.Equal(AppConfig.SerbianLanguage, config.UiLanguage);
        Assert.Equal(AppConfig.ThemeDark, config.UiTheme);
        Assert.Equal(AppConfig.SerbianLanguage, UiLocalizer.Language);
        var saved = await session.App.GetConfigAsync();
        Assert.Equal(AppConfig.SerbianLanguage, saved.UiLanguage);
        Assert.Equal(AppConfig.ThemeDark, saved.UiTheme);
    }
}
