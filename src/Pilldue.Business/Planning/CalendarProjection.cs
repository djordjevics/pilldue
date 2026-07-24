namespace Pilldue.Business;

/// <summary>
/// Flow 3: calendar from today through the second refill, with stock-out days
/// assuming the usual prescribed packages are obtained at the first refill.
/// </summary>
public static class CalendarProjection
{
    /// <summary>
    /// Builds the calendar view: range is <paramref name="asOfDate"/> through the latest
    /// second-refill date among medications (or as-of alone when there are none).
    /// Each medication is simulated with its prescription-day refill and a restock of
    /// <c>PrescribedPackageCount × PackageSizePills</c> at its first refill date.
    /// </summary>
    public static CalendarView Build(
        IEnumerable<Medication> medications,
        DateOnly asOfDate)
    {
        ArgumentNullException.ThrowIfNull(medications);

        var list = medications as IList<Medication> ?? medications.ToList();
        var entries = list
            .Select(medication => Evaluate(medication, asOfDate))
            .ToList();

        var rangeEnd = entries.Count == 0
            ? asOfDate
            : entries.Max(e => e.SecondRefillDate);

        return new CalendarView
        {
            RangeStart = asOfDate,
            RangeEnd = rangeEnd,
            Entries = entries,
        };
    }

    /// <summary>
    /// Projects one medication: first/second refill dates and stock-out days with restock assumption.
    /// Refill day-of-month is <see cref="Medication.PrescriptionStartDate"/>.Day.
    /// </summary>
    public static MedicationCalendarEntry Evaluate(
        Medication medication,
        DateOnly asOfDate)
    {
        ArgumentNullException.ThrowIfNull(medication);

        var refillDay = RefillCalendarRules.EffectiveRefillDayOfMonth(medication);
        var (first, second) = RefillCalendarRules.NextAndSecondRefillDates(asOfDate, refillDay);
        var stockOutDates = SimulateStockOutDates(medication, asOfDate, first, second);
        var prescriptionEnd = RefillCalendarRules.PrescriptionEndDate(medication);

        return new MedicationCalendarEntry
        {
            Medication = medication,
            FirstRefillDate = first,
            SecondRefillDate = second,
            StockOutDates = stockOutDates,
            PrescriptionEndDate = prescriptionEnd,
        };
    }

    /// <summary>
    /// Day-by-day stock from <paramref name="asOfDate"/> through <paramref name="secondRefill"/> (inclusive).
    /// On <paramref name="firstRefill"/>, adds prescribed packages before taking that day's dose.
    /// Records only the <em>first</em> day of each contiguous stock-out stretch (stock below dosage).
    /// </summary>
    public static IReadOnlyList<DateOnly> SimulateStockOutDates(
        Medication medication,
        DateOnly asOfDate,
        DateOnly firstRefill,
        DateOnly secondRefill)
    {
        ArgumentNullException.ThrowIfNull(medication);
        if (secondRefill < asOfDate)
        {
            throw new ArgumentException("secondRefill must be on or after asOfDate.", nameof(secondRefill));
        }

        var stock = medication.CurrentStockPills;
        var dosage = medication.DailyDosagePills;
        var restock = medication.PrescribedPackageCount * medication.PackageSizePills;
        var stockOuts = new List<DateOnly>();
        var inOutage = false;

        for (var day = asOfDate; day <= secondRefill; day = day.AddDays(1))
        {
            if (day == firstRefill)
            {
                stock += restock;
            }

            if (stock < dosage)
            {
                if (!inOutage)
                {
                    stockOuts.Add(day);
                    inOutage = true;
                }
            }
            else
            {
                inOutage = false;
                stock -= dosage;
            }
        }

        return stockOuts;
    }
}
