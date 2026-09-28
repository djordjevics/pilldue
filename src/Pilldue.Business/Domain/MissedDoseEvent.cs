namespace Pilldue.Business;

/// <summary>
/// Record that a dose was missed (visible later). Does not change stock —
/// inventory correction stays on <see cref="SkipDoseEvent"/>.
/// </summary>
public sealed class MissedDoseEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MedicationId { get; set; }

    public DateOnly Date { get; set; }
}
