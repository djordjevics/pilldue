namespace Pilldue.Business;

/// <summary>
/// Flow 1.1: whether current stock lasts until the next refill day using calendar-accurate gaps.
/// </summary>
public static class StockCoverageQuery
{
    /// <summary>
    /// Evaluates stock coverage for one medication as of <paramref name="asOfDate"/>.
    /// Uses <see cref="RefillCalendarRules"/> for refill dates, last covered day, and packages to buy.
    /// When <paramref name="asOfDate"/> falls on a refill day, the target is the following month's
    /// refill (e.g. 5 May → 5 June = 31 days).
    /// </summary>
    public static StockCoverageResult Evaluate(Medication medication, AppConfig config, DateOnly asOfDate)
    {
        ArgumentNullException.ThrowIfNull(medication);
        ArgumentNullException.ThrowIfNull(config);

        var refillDay = RefillCalendarRules.EffectiveRefillDayOfMonth(medication, config);
        var (nextOnOrAfter, second) = RefillCalendarRules.NextAndSecondRefillDates(asOfDate, refillDay);
        // On the refill day itself, plan through the next occurrence (month gap).
        var nextRefillDate = nextOnOrAfter > asOfDate ? nextOnOrAfter : second;

        var daysInGap = nextRefillDate.DayNumber - asOfDate.DayNumber;
        var lastCoveredDate = RefillCalendarRules.LastCoveredDate(
            asOfDate,
            medication.CurrentStockPills,
            medication.DailyDosagePills);

        var pillsNeeded = daysInGap * medication.DailyDosagePills;
        var pillsShort = Math.Max(0, pillsNeeded - medication.CurrentStockPills);
        var coversUntilNextRefill = pillsShort == 0;
        var packagesToBuy = RefillCalendarRules.PackagesToBuy(pillsShort, medication.PackageSizePills);

        return new StockCoverageResult
        {
            Medication = medication,
            NextRefillDate = nextRefillDate,
            LastCoveredDate = lastCoveredDate,
            CoversUntilNextRefill = coversUntilNextRefill,
            PillsShort = pillsShort,
            PackagesToBuy = packagesToBuy,
        };
    }
}
