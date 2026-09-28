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

        // Prefer the tracked instance when the same DbContext lives for the whole session
        // (UI composition root). Update(detached) would otherwise collide on the key.
        var existing = await _db.Medications.FindAsync([medication.Id], cancellationToken);
        if (existing is null)
        {
            throw new InvalidOperationException($"Medication '{medication.Id}' was not found.");
        }

        _db.Entry(existing).CurrentValues.SetValues(medication);
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
