namespace Pilldue.Business;

/// <summary>
/// Shared calendar and packaging rules for refill planning.
/// Full query orchestration is implemented in later issues; helpers here lock the formulas.
/// </summary>
public static class RefillCalendarRules
{
    /// <summary>
    /// Last-covered-day rule (inclusive): with <c>dosesCovered = floor(stock / dosage)</c>,
    /// walk forward from as-of and consume one dose on each dose day (see
    /// <see cref="IsDoseDay"/>). Returns the date of the last dose that stock covers,
    /// or null when floor is 0.
    /// For daily dosing (interval 1): asOf=1 May, stock=28, dosage=1 → last covered = 28 May.
    /// </summary>
    public const string LastCoveredDayRule =
        "Inclusive: consume dosage on dose days from asOf; lastCovered is the date of the floor(stock/dosage)-th dose; otherwise none.";

    /// <summary>
    /// Gaps between consecutive refill days use real calendar dates (28–31 days), never a fixed 30.
    /// Pill need counts dose days in the gap × dosage (interval 1 ⇒ every calendar day).
    /// Example: 5 May → 5 June = 31 days; 28 pills @ 1/day → 3 pills short → packagesToBuy = ceil(3/28) = 1
    /// (or 2 packages to fully cover a 31-day gap from empty with package size 28).
    /// </summary>
    public const string CalendarGapRule =
        "Use actual DateOnly difference between refill-day occurrences; month length matters; dose days use DoseIntervalDays from prescription start.";

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

    /// <summary>
    /// Effective refill day-of-month for a medication: the day of
    /// <see cref="Medication.PrescriptionStartDate"/> (clamped per month when applied).
    /// </summary>
    public static int EffectiveRefillDayOfMonth(Medication medication)
    {
        ArgumentNullException.ThrowIfNull(medication);
        return medication.PrescriptionStartDate.Day;
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
    /// True when <paramref name="date"/> is a dose day: on or after prescription start and
    /// aligned every <paramref name="intervalDays"/> days from that start.
    /// </summary>
    public static bool IsDoseDay(DateOnly date, DateOnly prescriptionStart, int intervalDays)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(intervalDays, 1);
        if (date < prescriptionStart)
        {
            return false;
        }

        return (date.DayNumber - prescriptionStart.DayNumber) % intervalDays == 0;
    }

    /// <summary>
    /// Number of dose days in <c>[fromInclusive, toExclusive)</c>.
    /// </summary>
    public static int CountDoseDays(
        DateOnly fromInclusive,
        DateOnly toExclusive,
        DateOnly prescriptionStart,
        int intervalDays)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(intervalDays, 1);
        if (toExclusive <= fromInclusive)
        {
            return 0;
        }

        var count = 0;
        for (var day = fromInclusive; day < toExclusive; day = day.AddDays(1))
        {
            if (IsDoseDay(day, prescriptionStart, intervalDays))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Last calendar day current stock lasts for dose consumption starting at
    /// <paramref name="asOfDate"/>. Returns <c>null</c> when stock cannot cover a dose.
    /// See <see cref="LastCoveredDayRule"/>.
    /// </summary>
    public static DateOnly? LastCoveredDate(
        DateOnly asOfDate,
        int stockPills,
        int dosagePills,
        DateOnly prescriptionStart,
        int intervalDays = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(stockPills, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(dosagePills, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(intervalDays, 1);

        var dosesCovered = stockPills / dosagePills;
        if (dosesCovered == 0)
        {
            return null;
        }

        var taken = 0;
        // Bound the search: worst case every day is a dose day.
        var limit = asOfDate.AddDays(dosesCovered * intervalDays + intervalDays);
        for (var day = asOfDate; day <= limit; day = day.AddDays(1))
        {
            if (!IsDoseDay(day, prescriptionStart, intervalDays))
            {
                continue;
            }

            taken++;
            if (taken == dosesCovered)
            {
                return day;
            }
        }

        return null;
    }

    /// <summary>
    /// Convenience overload using the medication's dosage interval and prescription start.
    /// </summary>
    public static DateOnly? LastCoveredDate(
        DateOnly asOfDate,
        Medication medication)
    {
        ArgumentNullException.ThrowIfNull(medication);
        return LastCoveredDate(
            asOfDate,
            medication.CurrentStockPills,
            medication.DailyDosagePills,
            medication.PrescriptionStartDate,
            EffectiveDoseIntervalDays(medication));
    }

    /// <summary>Effective dose interval; values below 1 are treated as daily.</summary>
    public static int EffectiveDoseIntervalDays(Medication medication)
    {
        ArgumentNullException.ThrowIfNull(medication);
        return medication.DoseIntervalDays < 1 ? 1 : medication.DoseIntervalDays;
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
