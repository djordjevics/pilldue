namespace Pilldue.Business;

/// <summary>Inventory correction when a dose was not taken (stock increases).</summary>
public sealed class SkipDoseEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MedicationId { get; set; }

    public DateOnly Date { get; set; }

    /// <summary>Pills returned to stock (typically one day of daily dosage).</summary>
    public int PillsReturned { get; set; }
}
