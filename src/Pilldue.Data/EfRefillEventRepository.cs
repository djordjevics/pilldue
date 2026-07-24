using Microsoft.EntityFrameworkCore;
using Pilldue.Business;

namespace Pilldue.Data;

public sealed class EfRefillEventRepository : IRefillEventRepository
{
    private readonly PilldueDbContext _db;

    public EfRefillEventRepository(PilldueDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    public async Task AddAsync(RefillEvent refillEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(refillEvent);
        _db.RefillEvents.Add(refillEvent);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RefillEvent>> ListByMedicationAsync(
        Guid medicationId,
        CancellationToken cancellationToken = default)
    {
        return await _db.RefillEvents
            .AsNoTracking()
            .Where(e => e.MedicationId == medicationId)
            .OrderBy(e => e.Date)
            .ToListAsync(cancellationToken);
    }
}
