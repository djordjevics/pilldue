namespace Pilldue.Business;

public interface IMedicationRepository
{
    Task<IReadOnlyList<Medication>> ListAsync(CancellationToken cancellationToken = default);

    Task<Medication?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Medication medication, CancellationToken cancellationToken = default);

    Task UpdateAsync(Medication medication, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
