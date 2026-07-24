namespace Pilldue.Business;

/// <summary>Calendar projection for one medication across the two-refill window.</summary>
public sealed class MedicationCalendarEntry
{
    public required Medication Medication { get; init; }

    /// <summary>Next upcoming refill day for this medication (effective day-of-month).</summary>
    public DateOnly FirstRefillDate { get; init; }

    /// <summary>Second upcoming refill day for this medication.</summary>
    public DateOnly SecondRefillDate { get; init; }

    /// <summary>
    /// Days in the as-of → second-refill window when stock is insufficient for a full daily dose,
    /// assuming <see cref="Medication.PrescribedPackageCount"/> packages are added at
    /// <see cref="FirstRefillDate"/> before that day's dose.
    /// </summary>
    public IReadOnlyList<DateOnly> StockOutDates { get; init; } = Array.Empty<DateOnly>();

    /// <summary>When the current prescription validity ends.</summary>
    public DateOnly PrescriptionEndDate { get; init; }
}
