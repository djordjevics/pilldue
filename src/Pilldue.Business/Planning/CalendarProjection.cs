namespace Pilldue.Business;

/// <summary>
/// Flow 3: last covered day and prescription end for medications overlapping a date range.
/// </summary>
public static class CalendarProjection
{
    /// <summary>
    /// Builds a calendar entry when either last-covered or prescription end falls within
    /// <paramref name="rangeStart"/>..<paramref name="rangeEnd"/> (inclusive).
    /// </summary>
    public static MedicationCalendarEntry? Evaluate(
        Medication medication,
        DateOnly rangeStart,
        DateOnly rangeEnd,
        DateOnly asOfDate)
    {
        ArgumentNullException.ThrowIfNull(medication);
        if (rangeEnd < rangeStart)
        {
            throw new ArgumentException("rangeEnd must be on or after rangeStart.", nameof(rangeEnd));
        }

        var lastCovered = RefillCalendarRules.LastCoveredDate(
            asOfDate,
            medication.CurrentStockPills,
            medication.DailyDosagePills);
        var prescriptionEnd = RefillCalendarRules.PrescriptionEndDate(medication);

        var lastCoveredInRange = lastCovered is { } lc && lc >= rangeStart && lc <= rangeEnd;
        var prescriptionEndInRange = prescriptionEnd >= rangeStart && prescriptionEnd <= rangeEnd;
        if (!lastCoveredInRange && !prescriptionEndInRange)
        {
            return null;
        }

        return new MedicationCalendarEntry
        {
            Medication = medication,
            LastCoveredDate = lastCovered,
            PrescriptionEndDate = prescriptionEnd,
        };
    }
}
