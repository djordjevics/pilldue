namespace Pilldue.Business;

/// <summary>
/// Facade that persists meds/refills/skips via ports and runs planning queries.
/// Remaining planning queries throw until business issues C4–C9 implement them.
/// </summary>
public sealed class PilldueApp : IPilldueApp
{
    private readonly IMedicationRepository _medications;
    private readonly IRefillEventRepository _refills;
    private readonly ISkipDoseEventRepository _skips;
    private readonly IAppConfigStore _config;

    public PilldueApp(
        IMedicationRepository medications,
        IRefillEventRepository refills,
        ISkipDoseEventRepository skips,
        IAppConfigStore config)
    {
        _medications = medications;
        _refills = refills;
        _skips = skips;
        _config = config;
    }

    public Task<AppConfig> GetConfigAsync(CancellationToken cancellationToken = default)
        => _config.LoadAsync(cancellationToken);

    public Task<IReadOnlyList<Medication>> ListMedicationsAsync(CancellationToken cancellationToken = default)
        => _medications.ListAsync(cancellationToken);

    public async Task<Medication> AddMedicationAsync(
        Medication medication,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(medication);
        await _medications.AddAsync(medication, cancellationToken).ConfigureAwait(false);
        return (await _medications.GetAsync(medication.Id, cancellationToken).ConfigureAwait(false))!;
    }

    public async Task<Medication> UpdateMedicationAsync(
        Medication medication,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(medication);
        await _medications.UpdateAsync(medication, cancellationToken).ConfigureAwait(false);
        return (await _medications.GetAsync(medication.Id, cancellationToken).ConfigureAwait(false))!;
    }

    public async Task<IReadOnlyList<StockCoverageResult>> GetStockCoverageAsync(
        DateOnly asOfDate,
        CancellationToken cancellationToken = default)
    {
        var config = await _config.LoadAsync(cancellationToken).ConfigureAwait(false);
        var medications = await _medications.ListAsync(cancellationToken).ConfigureAwait(false);

        return medications
            .Select(medication => StockCoverageQuery.Evaluate(medication, config, asOfDate))
            .ToList();
    }

    public async Task<IReadOnlyList<StockCoverageResult>> ListShortBeforeNextRefillAsync(
        DateOnly asOfDate,
        CancellationToken cancellationToken = default)
    {
        var coverage = await GetStockCoverageAsync(asOfDate, cancellationToken).ConfigureAwait(false);
        return coverage
            .Where(result => !result.CoversUntilNextRefill)
            .ToList();
    }

    public async Task<IReadOnlyList<ExtraPackagesNeeded>> ListNeedExtraForSecondRefillAsync(
        DateOnly asOfDate,
        CancellationToken cancellationToken = default)
    {
        var config = await _config.LoadAsync(cancellationToken).ConfigureAwait(false);
        var medications = await _medications.ListAsync(cancellationToken).ConfigureAwait(false);

        return medications
            .Select(medication => ExtraPackagesQuery.Evaluate(medication, config, asOfDate))
            .Where(result => result is not null)
            .Select(result => result!)
            .ToList();
    }

    public async Task RefillAsync(
        Guid medicationId,
        int packageCount,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(packageCount, 1);

        var medication = await _medications.GetAsync(medicationId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Medication '{medicationId}' was not found.");

        medication.CurrentStockPills += packageCount * medication.PackageSizePills;
        await _medications.UpdateAsync(medication, cancellationToken).ConfigureAwait(false);
        await _refills.AddAsync(
            new RefillEvent
            {
                MedicationId = medicationId,
                Date = date,
                PackageCount = packageCount,
            },
            cancellationToken).ConfigureAwait(false);
    }

    public async Task SkipDoseAsync(
        Guid medicationId,
        int pillsReturned,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pillsReturned, 1);

        var medication = await _medications.GetAsync(medicationId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Medication '{medicationId}' was not found.");

        medication.CurrentStockPills += pillsReturned;
        await _medications.UpdateAsync(medication, cancellationToken).ConfigureAwait(false);
        await _skips.AddAsync(
            new SkipDoseEvent
            {
                MedicationId = medicationId,
                Date = date,
                PillsReturned = pillsReturned,
            },
            cancellationToken).ConfigureAwait(false);
    }

    public Task<IReadOnlyList<MedicationCalendarEntry>> GetCalendarAsync(
        DateOnly rangeStart,
        DateOnly rangeEnd,
        DateOnly asOfDate,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Implement in business issues C2/C8/C9.");
}
