using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Pilldue.Business;
using Pilldue.Data;

namespace Pilldue.Data.Tests;

public class SkipDoseEventStoreTests
{
    [Fact]
    public async Task Append_and_list_by_medication_round_trips()
    {
        await using var fixture = await TempSkipDoseDb.CreateAsync();
        var medicationId = await fixture.SeedMedicationAsync("Atorvastatin");

        var skipEvent = new SkipDoseEvent
        {
            Id = Guid.NewGuid(),
            MedicationId = medicationId,
            Date = new DateOnly(2026, 7, 10),
            PillsReturned = 2,
        };

        await fixture.Repository.AddAsync(skipEvent);

        var listed = await fixture.Repository.ListByMedicationAsync(medicationId);

        Assert.Single(listed);
        AssertSkipDoseEventEqual(skipEvent, listed[0]);
    }

    [Fact]
    public async Task List_by_medication_orders_by_date()
    {
        await using var fixture = await TempSkipDoseDb.CreateAsync();
        var medicationId = await fixture.SeedMedicationAsync("Metformin");

        var later = new SkipDoseEvent
        {
            Id = Guid.NewGuid(),
            MedicationId = medicationId,
            Date = new DateOnly(2026, 7, 20),
            PillsReturned = 1,
        };
        var earlier = new SkipDoseEvent
        {
            Id = Guid.NewGuid(),
            MedicationId = medicationId,
            Date = new DateOnly(2026, 7, 5),
            PillsReturned = 1,
        };

        await fixture.Repository.AddAsync(later);
        await fixture.Repository.AddAsync(earlier);

        var listed = await fixture.Repository.ListByMedicationAsync(medicationId);

        Assert.Equal(new[] { earlier.Id, later.Id }, listed.Select(e => e.Id));
    }

    [Fact]
    public async Task List_by_medication_excludes_other_medications()
    {
        await using var fixture = await TempSkipDoseDb.CreateAsync();
        var firstMedicationId = await fixture.SeedMedicationAsync("Ibuprofen");
        var secondMedicationId = await fixture.SeedMedicationAsync("Aspirin");

        var firstEvent = new SkipDoseEvent
        {
            Id = Guid.NewGuid(),
            MedicationId = firstMedicationId,
            Date = new DateOnly(2026, 7, 1),
            PillsReturned = 1,
        };
        var secondEvent = new SkipDoseEvent
        {
            Id = Guid.NewGuid(),
            MedicationId = secondMedicationId,
            Date = new DateOnly(2026, 7, 2),
            PillsReturned = 1,
        };

        await fixture.Repository.AddAsync(firstEvent);
        await fixture.Repository.AddAsync(secondEvent);

        var listed = await fixture.Repository.ListByMedicationAsync(firstMedicationId);

        Assert.Single(listed);
        Assert.Equal(firstEvent.Id, listed[0].Id);
    }

    [Fact]
    public async Task Append_persists_all_skip_dose_fields()
    {
        await using var fixture = await TempSkipDoseDb.CreateAsync();
        var medicationId = await fixture.SeedMedicationAsync("Escitalopram");

        var skipEvent = new SkipDoseEvent
        {
            Id = Guid.NewGuid(),
            MedicationId = medicationId,
            Date = new DateOnly(2026, 3, 15),
            PillsReturned = 3,
        };

        await fixture.Repository.AddAsync(skipEvent);

        await using var db = new PilldueDbContext(fixture.Options);
        var loaded = await db.SkipDoseEvents.AsNoTracking().SingleAsync(e => e.Id == skipEvent.Id);

        AssertSkipDoseEventEqual(skipEvent, loaded);
    }

    private static void AssertSkipDoseEventEqual(SkipDoseEvent expected, SkipDoseEvent actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.MedicationId, actual.MedicationId);
        Assert.Equal(expected.Date, actual.Date);
        Assert.Equal(expected.PillsReturned, actual.PillsReturned);
    }

    private sealed class TempSkipDoseDb : IAsyncDisposable
    {
        private readonly string _dbPath;

        private TempSkipDoseDb(
            string dbPath,
            DbContextOptions<PilldueDbContext> options,
            EfSkipDoseEventRepository repository)
        {
            _dbPath = dbPath;
            Options = options;
            Repository = repository;
        }

        public DbContextOptions<PilldueDbContext> Options { get; }

        public EfSkipDoseEventRepository Repository { get; }

        public static async Task<TempSkipDoseDb> CreateAsync()
        {
            var dbPath = Path.Combine(
                Path.GetTempPath(),
                "pilldue-tests",
                $"skip-dose-{Guid.NewGuid():N}.db");

            await PilldueDbBootstrap.MigrateAsync(dbPath);
            var options = PilldueDbBootstrap.CreateOptions(dbPath);
            var repository = new EfSkipDoseEventRepository(options);
            return new TempSkipDoseDb(dbPath, options, repository);
        }

        public async Task<Guid> SeedMedicationAsync(string name)
        {
            var medication = new Medication
            {
                Id = Guid.NewGuid(),
                Name = name,
                PackageSizePills = 28,
                PrescribedPackageCount = 1,
                DailyDosagePills = 1,
                CurrentStockPills = 10,
                PrescriptionStartDate = new DateOnly(2026, 1, 1),
                PrescriptionDurationMonths = 6,
            };

            await using var db = new PilldueDbContext(Options);
            db.Medications.Add(medication);
            await db.SaveChangesAsync();
            return medication.Id;
        }

        public ValueTask DisposeAsync()
        {
            SqliteConnection.ClearAllPools();
            try
            {
                if (File.Exists(_dbPath))
                {
                    File.Delete(_dbPath);
                }
            }
            catch (IOException)
            {
                // Temp cleanup is best-effort on Windows when SQLite still holds a handle briefly.
            }

            return ValueTask.CompletedTask;
        }
    }
}
