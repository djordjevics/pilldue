namespace Pilldue.Business;

public sealed class InMemorySkipDoseEventRepository : ISkipDoseEventRepository
{
    private readonly List<SkipDoseEvent> _items = [];

    public Task AddAsync(SkipDoseEvent skipDoseEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(skipDoseEvent);
        _items.Add(Clone(skipDoseEvent));
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<SkipDoseEvent>> ListByMedicationAsync(
        Guid medicationId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SkipDoseEvent> list = _items
            .Where(e => e.MedicationId == medicationId)
            .OrderBy(e => e.Date)
            .Select(Clone)
            .ToList();
        return Task.FromResult(list);
    }

    private static SkipDoseEvent Clone(SkipDoseEvent source) => new()
    {
        Id = source.Id,
        MedicationId = source.MedicationId,
        Date = source.Date,
        PillsReturned = source.PillsReturned,
    };
}
