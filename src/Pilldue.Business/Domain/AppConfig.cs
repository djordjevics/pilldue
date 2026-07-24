namespace Pilldue.Business;

/// <summary>Application settings (UI language). Refill day is per medication from prescription start.</summary>
public sealed class AppConfig
{
    public const string EnglishLanguage = "en";

    public const string SerbianLanguage = "sr";

    /// <summary>
    /// UI language code: <c>en</c> or <c>sr</c> (Serbian Latin).
    /// Empty/null means detect from OS UI culture at startup.
    /// </summary>
    public string UiLanguage { get; set; } = string.Empty;
}
