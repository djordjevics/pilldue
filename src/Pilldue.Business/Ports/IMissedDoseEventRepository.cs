namespace Pilldue.Business;

public interface IMissedDoseEventRepository
{
    Task AddAsync(MissedDoseEvent missedDoseEvent, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MissedDoseEvent>> ListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MissedDoseEvent>> ListByMedicationAsync(
        Guid medicationId,
        CancellationToken cancellationToken = default);
}
