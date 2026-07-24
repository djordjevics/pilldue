namespace Pilldue.Business;

/// <summary>Medication that needs more than the usual prescribed packages to reach the second refill day.</summary>
public sealed class ExtraPackagesNeeded
{
    public required Medication Medication { get; init; }

    public DateOnly SecondRefillDate { get; init; }

    public int PackagesNeeded { get; init; }

    public int PrescribedPackageCount { get; init; }

    public int ExtraPackages => Math.Max(0, PackagesNeeded - PrescribedPackageCount);
}
