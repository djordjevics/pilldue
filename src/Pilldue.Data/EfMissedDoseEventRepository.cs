using Microsoft.EntityFrameworkCore;
using Pilldue.Business;

namespace Pilldue.Data;

/// <summary>
/// EF Core + SQLite implementation of <see cref="IMissedDoseEventRepository"/>.
/// </summary>
public sealed class EfMissedDoseEventRepository : IMissedDoseEventRepository
{
    private readonly DbContextOptions<PilldueDbContext> _options;

    public EfMissedDoseEventRepository(DbContextOptions<PilldueDbContext> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    public async Task AddAsync(MissedDoseEvent missedDoseEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(missedDoseEvent);

        await using var db = CreateContext();
        db.MissedDoseEvents.Add(missedDoseEvent);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<MissedDoseEvent>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var db = CreateContext();
        return await db.MissedDoseEvents
            .AsNoTracking()
            .OrderBy(e => e.Date)
            .ThenBy(e => e.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<MissedDoseEvent>> ListByMedicationAsync(
        Guid medicationId,
        CancellationToken cancellationToken = default)
    {
        await using var db = CreateContext();
        return await db.MissedDoseEvents
            .AsNoTracking()
            .Where(e => e.MedicationId == medicationId)
            .OrderBy(e => e.Date)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private PilldueDbContext CreateContext() => new(_options);
}
