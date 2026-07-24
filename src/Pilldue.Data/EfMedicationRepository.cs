using Microsoft.EntityFrameworkCore;
using Pilldue.Business;

namespace Pilldue.Data;

/// <summary>EF Core / SQLite implementation of <see cref="IMedicationRepository"/>.</summary>
public sealed class EfMedicationRepository : IMedicationRepository
{
    private readonly PilldueDbContext _db;

    public EfMedicationRepository(PilldueDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    public async Task<IReadOnlyList<Medication>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Medications
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Medication?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Medications
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task AddAsync(Medication medication, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(medication);

        if (await _db.Medications.AnyAsync(m => m.Id == medication.Id, cancellationToken))
        {
            throw new InvalidOperationException($"Medication '{medication.Id}' already exists.");
        }

        _db.Medications.Add(medication);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Medication medication, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(medication);

        var exists = await _db.Medications.AnyAsync(m => m.Id == medication.Id, cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException($"Medication '{medication.Id}' was not found.");
        }

        _db.Medications.Update(medication);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Medications.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        _db.Medications.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
