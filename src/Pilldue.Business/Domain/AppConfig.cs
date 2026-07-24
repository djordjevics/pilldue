namespace Pilldue.Business;

/// <summary>Application settings. Default refill day is the 5th of each month.</summary>
public sealed class AppConfig
{
    public const int DefaultRefillDayOfMonthValue = 5;

    /// <summary>Day of month (1–31) used when a medication has no override.</summary>
    public int DefaultRefillDayOfMonth { get; set; } = DefaultRefillDayOfMonthValue;
}
