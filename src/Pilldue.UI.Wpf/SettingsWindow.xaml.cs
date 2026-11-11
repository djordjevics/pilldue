using System.Windows;
using System.Windows.Controls;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public partial class SettingsWindow : Window
{
    private readonly IPilldueApp _app;
    private readonly AppConfig _config;

    public SettingsWindow(IPilldueApp app, AppConfig config)
    {
        _app = app ?? throw new ArgumentNullException(nameof(app));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        InitializeComponent();

        Select(
            LanguageBox,
            [
                new Choice(UiLocalizer.Get("Settings.System"), string.Empty),
                new Choice(UiLocalizer.Get("Lang.English"), AppConfig.EnglishLanguage),
                new Choice(UiLocalizer.Get("Lang.Serbian"), AppConfig.SerbianLanguage),
            ],
            AppConfig.LanguageToStore(_config.UiLanguage));

        Select(
            ThemeBox,
            [
                new Choice(UiLocalizer.Get("Settings.Light"), AppConfig.ThemeLight),
                new Choice(UiLocalizer.Get("Settings.Dark"), AppConfig.ThemeDark),
                new Choice(UiLocalizer.Get("Settings.System"), AppConfig.ThemeSystem),
            ],
            AppConfig.NormalizeTheme(_config.UiTheme));
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        if (LanguageBox.SelectedItem is not Choice language || ThemeBox.SelectedItem is not Choice theme)
        {
            return;
        }

        try
        {
            _config.UiLanguage = language.Value;
            _config.UiTheme = theme.Value;
            await _app.SaveConfigAsync(_config);
            UiLocalizer.Apply(_config);
            WpfTheme.Apply(_config);
            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, UiLocalizer.Get("Desktop.Settings"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void Select(ComboBox box, IReadOnlyList<Choice> choices, string value)
    {
        box.ItemsSource = choices;
        box.DisplayMemberPath = nameof(Choice.Label);
        box.SelectedItem = choices.FirstOrDefault(choice =>
            string.Equals(choice.Value, value, StringComparison.OrdinalIgnoreCase)) ?? choices[0];
    }

    private sealed record Choice(string Label, string Value);
}
