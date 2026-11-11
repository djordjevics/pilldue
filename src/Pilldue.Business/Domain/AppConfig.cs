namespace Pilldue.Business;

/// <summary>Application settings (UI language and theme). Refill day is per medication from prescription start.</summary>
public sealed class AppConfig
{
    public const string EnglishLanguage = "en";

    public const string SerbianLanguage = "sr";

    public const string ThemeSystem = "system";

    public const string ThemeLight = "light";

    public const string ThemeDark = "dark";

    /// <summary>
    /// UI language code: <c>en</c> or <c>sr</c> (Serbian Latin).
    /// Empty/null means detect from OS UI culture at startup.
    /// </summary>
    public string UiLanguage { get; set; } = string.Empty;

    /// <summary>
    /// WPF theme: <c>light</c>, <c>dark</c>, or <c>system</c>.
    /// Empty, null, and unknown values follow the Windows app theme.
    /// </summary>
    public string UiTheme { get; set; } = ThemeSystem;

    /// <summary>
    /// Value written to <see cref="UiLanguage"/>. Blank stays blank so startup still follows the OS.
    /// </summary>
    public static string LanguageToStore(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return string.Empty;
        }

        return language.Trim().Equals(SerbianLanguage, StringComparison.OrdinalIgnoreCase)
            ? SerbianLanguage
            : EnglishLanguage;
    }

    public static string NormalizeTheme(string? theme)
    {
        if (string.IsNullOrWhiteSpace(theme))
        {
            return ThemeSystem;
        }

        if (theme.Trim().Equals(ThemeLight, StringComparison.OrdinalIgnoreCase))
        {
            return ThemeLight;
        }

        if (theme.Trim().Equals(ThemeDark, StringComparison.OrdinalIgnoreCase))
        {
            return ThemeDark;
        }

        return ThemeSystem;
    }
}
