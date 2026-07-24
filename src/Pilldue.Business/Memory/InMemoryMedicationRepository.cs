namespace Pilldue.Business;

public sealed class InMemoryMedicationRepository : IMedicationRepository
{
    private readonly Dictionary<Guid, Medication> _items = new();

    public Task<IReadOnlyList<Medication>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Medication> list = _items.Values
            .OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
            .Select(Clone)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<Medication?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_items.TryGetValue(id, out var med) ? Clone(med) : null);
    }

    public Task AddAsync(Medication medication, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(medication);
        if (!_items.TryAdd(medication.Id, Clone(medication)))
        {
            throw new InvalidOperationException($"Medication '{medication.Id}' already exists.");
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Medication medication, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(medication);
        if (!_items.ContainsKey(medication.Id))
        {
            throw new InvalidOperationException($"Medication '{medication.Id}' was not found.");
        }

        _items[medication.Id] = Clone(medication);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _items.Remove(id);
        return Task.CompletedTask;
    }

    private static Medication Clone(Medication source) => new()
    {
        Id = source.Id,
        Name = source.Name,
        PackageSizePills = source.PackageSizePills,
        PrescribedPackageCount = source.PrescribedPackageCount,
        DailyDosagePills = source.DailyDosagePills,
        CurrentStockPills = source.CurrentStockPills,
        RefillDayOfMonthOverride = source.RefillDayOfMonthOverride,
        PrescriptionStartDate = source.PrescriptionStartDate,
        PrescriptionDurationMonths = source.PrescriptionDurationMonths,
    };
}
