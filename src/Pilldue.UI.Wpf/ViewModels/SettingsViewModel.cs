using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public sealed class SettingsChoice
{
    public SettingsChoice(string label, string value)
    {
        Label = label;
        Value = value;
    }

    public string Label { get; }

    public string Value { get; }
}

public partial class SettingsViewModel : DialogViewModel
{
    private readonly IPilldueApp _app;
    private readonly IUiInteraction _interaction;
    private readonly AppConfig _config;

    public SettingsViewModel(IPilldueApp app, IUiInteraction interaction, AppConfig config)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(interaction);
        ArgumentNullException.ThrowIfNull(config);
        _app = app;
        _interaction = interaction;
        _config = config;

        Languages =
        [
            new SettingsChoice(UiLocalizer.Get("Settings.System"), string.Empty),
            new SettingsChoice(UiLocalizer.Get("Lang.English"), AppConfig.EnglishLanguage),
            new SettingsChoice(UiLocalizer.Get("Lang.Serbian"), AppConfig.SerbianLanguage),
        ];
        Themes =
        [
            new SettingsChoice(UiLocalizer.Get("Settings.Light"), AppConfig.ThemeLight),
            new SettingsChoice(UiLocalizer.Get("Settings.Dark"), AppConfig.ThemeDark),
            new SettingsChoice(UiLocalizer.Get("Settings.System"), AppConfig.ThemeSystem),
        ];
        SelectedLanguage = Select(Languages, AppConfig.LanguageToStore(_config.UiLanguage));
        SelectedTheme = Select(Themes, AppConfig.NormalizeTheme(_config.UiTheme));
    }

    public IReadOnlyList<SettingsChoice> Languages { get; }

    public IReadOnlyList<SettingsChoice> Themes { get; }

    [ObservableProperty]
    private SettingsChoice? _selectedLanguage;

    [ObservableProperty]
    private SettingsChoice? _selectedTheme;

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedLanguage is null || SelectedTheme is null)
        {
            return;
        }

        try
        {
            _config.UiLanguage = SelectedLanguage.Value;
            _config.UiTheme = SelectedTheme.Value;
            await _app.SaveConfigAsync(_config);
            UiLocalizer.Apply(_config);
            WpfTheme.Apply(_config);
            CloseResult = true;
        }
        catch (Exception ex)
        {
            _interaction.ShowMessage(ex.Message, UiLocalizer.Get("Desktop.Settings"), UiMessageKind.Error);
        }
    }

    private static SettingsChoice Select(IReadOnlyList<SettingsChoice> choices, string value) =>
        choices.FirstOrDefault(choice => string.Equals(choice.Value, value, StringComparison.OrdinalIgnoreCase))
        ?? choices[0];
}
