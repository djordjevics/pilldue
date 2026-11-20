namespace Pilldue.Business;

/// <summary>
/// Today's pharmacy pickup for several medications. Each included medication is one
/// <see cref="IPilldueApp.RefillAsync"/>. A later failure leaves earlier refills saved.
/// </summary>
public static class BulkRefillPass
{
    /// <summary>Prescribed package count, or 1 when that count is not positive.</summary>
    public static int StartingPackageCount(Medication medication)
    {
        ArgumentNullException.ThrowIfNull(medication);
        return medication.PrescribedPackageCount > 0 ? medication.PrescribedPackageCount : 1;
    }

    /// <summary>
    /// Refills each line whose package count is at least 1, dated today.
    /// A negative count throws before any refill. Zero counts are skipped.
    /// </summary>
    public static async Task<int> ApplyAsync(
        IPilldueApp app,
        IReadOnlyList<BulkRefillLine> lines,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(lines);

        foreach (var line in lines)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(line.PackageCount);
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var saved = 0;
        foreach (var line in lines)
        {
            if (line.PackageCount < 1)
            {
                continue;
            }

            await app.RefillAsync(line.MedicationId, line.PackageCount, today, cancellationToken)
                .ConfigureAwait(false);
            saved++;
        }

        return saved;
    }
}

/// <summary>One medication in a bulk refill pass.</summary>
public sealed class BulkRefillLine
{
    public Guid MedicationId { get; init; }

    public int PackageCount { get; init; }
}
