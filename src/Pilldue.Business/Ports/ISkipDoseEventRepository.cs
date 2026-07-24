namespace Pilldue.Business;

public interface ISkipDoseEventRepository
{
    Task AddAsync(SkipDoseEvent skipDoseEvent, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SkipDoseEvent>> ListByMedicationAsync(
        Guid medicationId,
        CancellationToken cancellationToken = default);
}
