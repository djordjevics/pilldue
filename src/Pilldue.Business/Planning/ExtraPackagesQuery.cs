namespace Pilldue.Business;

/// <summary>
/// Flow 1.3: whether the usual prescribed packages plus current stock last until the second refill day.
/// </summary>
public static class ExtraPackagesQuery
{
    /// <summary>
    /// Packages needed from current stock to cover through the second upcoming refill date:
    /// <c>ceil(max(0, daysToSecond * dosage - stock) / packageSize)</c>.
    /// Returns a result only when that exceeds <see cref="Medication.PrescribedPackageCount"/>.
    /// </summary>
    public static ExtraPackagesNeeded? Evaluate(Medication medication, AppConfig config, DateOnly asOfDate)
    {
        ArgumentNullException.ThrowIfNull(medication);
        ArgumentNullException.ThrowIfNull(config);

        var refillDay = RefillCalendarRules.EffectiveRefillDayOfMonth(medication, config);
        var (_, secondRefillDate) = RefillCalendarRules.NextAndSecondRefillDates(asOfDate, refillDay);

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
