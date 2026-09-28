namespace Pilldue.Business;

public sealed class InMemoryMissedDoseEventRepository : IMissedDoseEventRepository
{
    private readonly List<MissedDoseEvent> _items = [];

    public Task AddAsync(MissedDoseEvent missedDoseEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(missedDoseEvent);
        _items.Add(Clone(missedDoseEvent));
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<MissedDoseEvent>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<MissedDoseEvent> list = _items
            .OrderBy(e => e.Date)
            .ThenBy(e => e.Id)
            .Select(Clone)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<MissedDoseEvent>> ListByMedicationAsync(
        Guid medicationId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<MissedDoseEvent> list = _items
            .Where(e => e.MedicationId == medicationId)
            .OrderBy(e => e.Date)
            .Select(Clone)
            .ToList();
        return Task.FromResult(list);
    }

    private static MissedDoseEvent Clone(MissedDoseEvent source) => new()
    {
        Id = source.Id,
        MedicationId = source.MedicationId,
        Date = source.Date,
    };
}
