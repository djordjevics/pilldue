namespace Pilldue.Business;

/// <summary>
/// Flow 1.3: whether prescribed package count is enough for stock to last until the second refill day.
/// </summary>
public static class ExtraPackagesQuery
{
    /// <summary>
    /// Evaluates packages needed to cover from <paramref name="asOfDate"/> through the second
    /// upcoming refill day (calendar-accurate). When <paramref name="asOfDate"/> is itself a
    /// refill day, the first target is the following month and the second is two months ahead
    /// (same planning window as <see cref="StockCoverageQuery"/>).
    /// Returns null when prescribed packages already suffice (ExtraPackages == 0).
    /// </summary>
    public static ExtraPackagesNeeded? Evaluate(Medication medication, AppConfig config, DateOnly asOfDate)
    {
        ArgumentNullException.ThrowIfNull(medication);
        ArgumentNullException.ThrowIfNull(config);

        var refillDay = RefillCalendarRules.EffectiveRefillDayOfMonth(medication, config);
        var (nextOnOrAfter, secondOnOrAfter) = RefillCalendarRules.NextAndSecondRefillDates(asOfDate, refillDay);

        DateOnly secondRefillDate;
        if (nextOnOrAfter > asOfDate)
        {
            secondRefillDate = secondOnOrAfter;
        }
        else
        {
            // On refill day: next planning target is secondOnOrAfter; second is one month after that.
            var monthAfterNext = new DateOnly(secondOnOrAfter.Year, secondOnOrAfter.Month, 1).AddMonths(1);
            secondRefillDate = RefillCalendarRules.RefillDateInMonth(
                monthAfterNext.Year,
                monthAfterNext.Month,
                refillDay);
        }

        var daysInGap = secondRefillDate.DayNumber - asOfDate.DayNumber;
        var pillsNeeded = daysInGap * medication.DailyDosagePills;
        var pillsShort = Math.Max(0, pillsNeeded - medication.CurrentStockPills);
        var packagesNeeded = RefillCalendarRules.PackagesToBuy(pillsShort, medication.PackageSizePills);

        if (packagesNeeded <= medication.PrescribedPackageCount)
        {
            return null;
        }

        return new ExtraPackagesNeeded
        {
            Medication = medication,
            SecondRefillDate = secondRefillDate,
            PackagesNeeded = packagesNeeded,
            PrescribedPackageCount = medication.PrescribedPackageCount,
        };
    }
}
