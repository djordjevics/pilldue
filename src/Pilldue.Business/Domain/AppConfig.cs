namespace Pilldue.Business;

/// <summary>Application settings. Default refill day is the 5th of each month.</summary>
public sealed class AppConfig
{
    public const int DefaultRefillDayOfMonthValue = 5;

    public const string EnglishLanguage = "en";

    public const string SerbianLanguage = "sr";

    /// <summary>Day of month (1–31) used when a medication has no override.</summary>
    public int DefaultRefillDayOfMonth { get; set; } = DefaultRefillDayOfMonthValue;

    /// <summary>
    /// UI language code: <c>en</c> or <c>sr</c> (Serbian Latin).
    /// Empty/null means detect from OS UI culture at startup.
    /// </summary>
    public string UiLanguage { get; set; } = string.Empty;
}
