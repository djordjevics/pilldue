namespace Pilldue.Business;

public sealed class InMemoryRefillEventRepository : IRefillEventRepository
{
    private readonly List<RefillEvent> _items = [];

    public Task AddAsync(RefillEvent refillEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(refillEvent);
        _items.Add(Clone(refillEvent));
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<RefillEvent>> ListByMedicationAsync(
        Guid medicationId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RefillEvent> list = _items
            .Where(e => e.MedicationId == medicationId)
            .OrderBy(e => e.Date)
            .Select(Clone)
            .ToList();
        return Task.FromResult(list);
    }

    private static RefillEvent Clone(RefillEvent source) => new()
    {
        Id = source.Id,
        MedicationId = source.MedicationId,
        Date = source.Date,
        PackageCount = source.PackageCount,
    };
}
