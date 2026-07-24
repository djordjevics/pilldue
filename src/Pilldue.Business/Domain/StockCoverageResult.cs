namespace Pilldue.Business;

/// <summary>Result of checking whether stock lasts until the next refill day.</summary>
public sealed class StockCoverageResult
{
    public required Medication Medication { get; init; }

    public DateOnly NextRefillDate { get; init; }

    public DateOnly? LastCoveredDate { get; init; }

    public bool CoversUntilNextRefill { get; init; }

    public int PillsShort { get; init; }

    /// <summary>Minimum packages to close the gap: ceil(PillsShort / PackageSize).</summary>
    public int PackagesToBuy { get; init; }
}
