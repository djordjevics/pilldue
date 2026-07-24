using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Pilldue.Business;
using Pilldue.Data;

namespace Pilldue.Data.Tests;

public class EfRefillEventRepositoryTests
{
    [Fact]
    public async Task AddAsync_persists_event_listable_by_medication()
    {
        var dbPath = CreateTempDbPath();
        try
        {
            await PilldueDbBootstrap.MigrateAsync(dbPath);
            await using var db = new PilldueDbContext(PilldueDbBootstrap.CreateOptions(dbPath));
            var medication = await SeedMedicationAsync(db);
            var repository = new EfRefillEventRepository(db);

            var refillEvent = new RefillEvent
            {
                MedicationId = medication.Id,
                Date = new DateOnly(2026, 5, 5),
                PackageCount = 2,
            };

            await repository.AddAsync(refillEvent);

            var listed = await repository.ListByMedicationAsync(medication.Id);
            var loaded = Assert.Single(listed);
            Assert.Equal(refillEvent.Id, loaded.Id);
            Assert.Equal(medication.Id, loaded.MedicationId);
            Assert.Equal(new DateOnly(2026, 5, 5), loaded.Date);
            Assert.Equal(2, loaded.PackageCount);
        }
        finally
        {
            CleanupTempDb(dbPath);
        }
    }

    [Fact]
    public async Task ListByMedicationAsync_returns_only_matching_medication_ordered_by_date()
    {
        var dbPath = CreateTempDbPath();
        try
        {
            await PilldueDbBootstrap.MigrateAsync(dbPath);
            await using var db = new PilldueDbContext(PilldueDbBootstrap.CreateOptions(dbPath));
            var medicationA = await SeedMedicationAsync(db, "Med A");
            var medicationB = await SeedMedicationAsync(db, "Med B");
            var repository = new EfRefillEventRepository(db);

            var later = new RefillEvent
            {
                MedicationId = medicationA.Id,
                Date = new DateOnly(2026, 6, 5),
                PackageCount = 1,
            };
            var earlier = new RefillEvent
            {
                MedicationId = medicationA.Id,
                Date = new DateOnly(2026, 4, 5),
                PackageCount = 3,
            };
            var otherMed = new RefillEvent
            {
                MedicationId = medicationB.Id,
                Date = new DateOnly(2026, 5, 5),
                PackageCount = 1,
            };

            await repository.AddAsync(later);
            await repository.AddAsync(otherMed);
            await repository.AddAsync(earlier);

            var listed = await repository.ListByMedicationAsync(medicationA.Id);

            Assert.Equal(2, listed.Count);
            Assert.Equal(earlier.Id, listed[0].Id);
            Assert.Equal(later.Id, listed[1].Id);
        }
        finally
        {
            CleanupTempDb(dbPath);
        }
    }

    private static string CreateTempDbPath() =>
        Path.Combine(Path.GetTempPath(), "pilldue-tests", $"refill-events-{Guid.NewGuid():N}.db");

    private static async Task<Medication> SeedMedicationAsync(
        PilldueDbContext db,
        string name = "TestMed")
    {
        var medication = new Medication
        {
            Name = name,
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 0,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
        };

        db.Medications.Add(medication);
        await db.SaveChangesAsync();
        return medication;
    }

    private static void CleanupTempDb(string dbPath)
    {
        SqliteConnection.ClearAllPools();
        try
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
        catch (IOException)
        {
            // Temp cleanup is best-effort on Windows when SQLite still holds a handle briefly.
        }
    }
}
