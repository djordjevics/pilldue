using System.Windows;
using Pilldue.Business;

namespace Pilldue.UI.Wpf;

/// <summary>Applies the built-in WPF light, dark, or system theme. No custom palette.</summary>
public static class WpfTheme
{
    public static void Apply(AppConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        if (Application.Current is null)
        {
            return;
        }

        var mode = AppConfig.NormalizeTheme(config.UiTheme) switch
        {
            AppConfig.ThemeLight => ThemeMode.Light,
            AppConfig.ThemeDark => ThemeMode.Dark,
            _ => ThemeMode.System,
        };

        Application.Current.ThemeMode = mode;
        foreach (Window window in Application.Current.Windows)
        {
            window.ThemeMode = mode;
        }
    }
}
