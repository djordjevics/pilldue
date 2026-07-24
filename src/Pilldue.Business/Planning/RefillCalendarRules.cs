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

    /// <summary>
    /// Refill date in a given month for <paramref name="dayOfMonth"/>,
    /// clamping invalid days to the month's last day via <see cref="ClampDayOfMonth"/>.
    /// </summary>
    public static DateOnly RefillDateInMonth(int year, int month, int dayOfMonth)
    {
        var day = ClampDayOfMonth(year, month, dayOfMonth);
        return new DateOnly(year, month, day);
    }

    /// <summary>
    /// Next and second upcoming refill dates on or after <paramref name="today"/>
    /// for the given day-of-month (clamped per month).
    /// </summary>
    public static (DateOnly Next, DateOnly Second) NextAndSecondRefillDates(DateOnly today, int dayOfMonth)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dayOfMonth, 1);

        var candidate = RefillDateInMonth(today.Year, today.Month, dayOfMonth);
        DateOnly next;
        if (candidate >= today)
        {
            next = candidate;
        }
        else
        {
            var following = new DateOnly(today.Year, today.Month, 1).AddMonths(1);
            next = RefillDateInMonth(following.Year, following.Month, dayOfMonth);
        }

        var monthAfterNext = new DateOnly(next.Year, next.Month, 1).AddMonths(1);
        var second = RefillDateInMonth(monthAfterNext.Year, monthAfterNext.Month, dayOfMonth);
        return (next, second);
    }

    /// <summary>
    /// Last calendar day current stock lasts, inclusive of <paramref name="asOfDate"/> when
    /// <c>floor(stock / dailyDosage) &gt; 0</c>. Returns <c>null</c> when that floor is 0
    /// (no covered day). See <see cref="LastCoveredDayRule"/>.
    /// </summary>
    public static DateOnly? LastCoveredDate(DateOnly asOfDate, int stockPills, int dailyDosagePills)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(stockPills, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(dailyDosagePills, 1);

        var daysCovered = stockPills / dailyDosagePills;
        if (daysCovered == 0)
        {
            return null;
        }

        return asOfDate.AddDays(daysCovered - 1);
    }

    /// <summary>
    /// Prescription end date: <c>startDate.AddMonths(durationMonths)</c>
    /// (default duration is <see cref="Medication.PrescriptionDurationMonths"/> = 6).
    /// Day-of-month clamps when the target month is shorter (e.g. 31 Jan + 1 month → 28/29 Feb).
    /// </summary>
    public const string PrescriptionEndRule =
        "endDate = PrescriptionStartDate.AddMonths(PrescriptionDurationMonths); default duration is 6 months.";

    /// <summary>
    /// End of prescription validity from start date and duration in months.
    /// </summary>
    public static DateOnly PrescriptionEndDate(DateOnly startDate, int durationMonths)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(durationMonths, 1);
        return startDate.AddMonths(durationMonths);
    }

    /// <summary>
    /// End of prescription validity for a medication
    /// (<see cref="Medication.PrescriptionStartDate"/> + <see cref="Medication.PrescriptionDurationMonths"/>).
    /// </summary>
    public static DateOnly PrescriptionEndDate(Medication medication)
    {
        ArgumentNullException.ThrowIfNull(medication);
        return PrescriptionEndDate(medication.PrescriptionStartDate, medication.PrescriptionDurationMonths);
    }
}
