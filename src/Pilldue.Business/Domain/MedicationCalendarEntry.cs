namespace Pilldue.Business;

/// <summary>Calendar projection for one medication in a date range.</summary>
public sealed class MedicationCalendarEntry
{
    public required Medication Medication { get; init; }

    /// <summary>Last calendar day current stock lasts (see <see cref="RefillCalendarRules"/>).</summary>
    public DateOnly? LastCoveredDate { get; init; }

    /// <summary>When the current prescription validity ends.</summary>
    public DateOnly PrescriptionEndDate { get; init; }
}
