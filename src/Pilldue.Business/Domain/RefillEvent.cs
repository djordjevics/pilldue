namespace Pilldue.Business;

/// <summary>Record of adding packages to stock.</summary>
public sealed class RefillEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MedicationId { get; set; }

    public DateOnly Date { get; set; }

    /// <summary>Number of packages added.</summary>
    public int PackageCount { get; set; }
}
