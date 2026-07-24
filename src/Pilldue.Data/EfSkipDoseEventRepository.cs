using Microsoft.EntityFrameworkCore;
using Pilldue.Business;

namespace Pilldue.Data;

/// <summary>
/// EF Core + SQLite implementation of <see cref="ISkipDoseEventRepository"/>.
/// </summary>
public sealed class EfSkipDoseEventRepository : ISkipDoseEventRepository
{
    private readonly DbContextOptions<PilldueDbContext> _options;

    public EfSkipDoseEventRepository(DbContextOptions<PilldueDbContext> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    public async Task AddAsync(SkipDoseEvent skipDoseEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(skipDoseEvent);

        await using var db = CreateContext();
        db.SkipDoseEvents.Add(skipDoseEvent);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<SkipDoseEvent>> ListByMedicationAsync(
        Guid medicationId,
        CancellationToken cancellationToken = default)
    {
        await using var db = CreateContext();
        return await db.SkipDoseEvents
            .AsNoTracking()
            .Where(e => e.MedicationId == medicationId)
            .OrderBy(e => e.Date)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private PilldueDbContext CreateContext() => new(_options);
}
