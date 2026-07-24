namespace Pilldue.Business;

/// <summary>
/// Shared calendar and packaging rules for refill planning.
/// Full query orchestration is implemented in later issues; helpers here lock the formulas.
/// </summary>
public static class RefillCalendarRules
{
    /// <summary>
    /// Last-covered-day rule (inclusive): with <c>daysCovered = floor(stock / dailyDosage)</c>,
    /// if daysCovered is 0 there is no covered day; otherwise the last covered day is
    /// <c>asOfDate.AddDays(daysCovered - 1)</c> (as-of day counts as day 1 when stock &gt;= dosage).
    /// Example: asOf=1 May, stock=28, dosage=1 → last covered = 28 May.
    /// </summary>
    public const string LastCoveredDayRule =
        "Inclusive: lastCovered = asOfDate + floor(stock/dailyDosage) - 1 days when floor > 0; otherwise none.";

    /// <summary>
    /// Gaps between consecutive refill days use real calendar dates (28–31 days), never a fixed 30.
    /// Example: 5 May → 5 June = 31 days; 28 pills @ 1/day → 3 pills short → packagesToBuy = ceil(3/28) = 1
    /// (or 2 packages to fully cover a 31-day gap from empty with package size 28).
    /// </summary>
    public const string CalendarGapRule =
        "Use actual DateOnly difference between refill-day occurrences; month length matters.";

    /// <summary>
    /// Clamps a requested day-of-month into a valid day for the given month
    /// (e.g. day 31 in February → 28 or 29).
    /// </summary>
    public static int ClampDayOfMonth(int year, int month, int dayOfMonth)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(month, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(month, 12);
        ArgumentOutOfRangeException.ThrowIfLessThan(dayOfMonth, 1);

        var daysInMonth = DateTime.DaysInMonth(year, month);
        return Math.Min(dayOfMonth, daysInMonth);
    }

    /// <summary>
    /// Minimum packages required to cover a positive pill shortfall:
    /// <c>ceil(pillsShort / packageSize)</c>. Returns 0 when pillsShort &lt;= 0.
    /// </summary>
    public static int PackagesToBuy(int pillsShort, int packageSizePills)
    {
        if (pillsShort <= 0)
        {
            return 0;
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(packageSizePills, 1);
        return (pillsShort + packageSizePills - 1) / packageSizePills;
    }

    /// <summary>Effective refill day: per-med override or config default.</summary>
    public static int EffectiveRefillDayOfMonth(Medication medication, AppConfig config)
    {
        ArgumentNullException.ThrowIfNull(medication);
        ArgumentNullException.ThrowIfNull(config);
        return medication.RefillDayOfMonthOverride ?? config.DefaultRefillDayOfMonth;
    }
}
