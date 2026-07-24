namespace Pilldue.Business;

public interface IRefillEventRepository
{
    Task AddAsync(RefillEvent refillEvent, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RefillEvent>> ListByMedicationAsync(
        Guid medicationId,
        CancellationToken cancellationToken = default);
}
