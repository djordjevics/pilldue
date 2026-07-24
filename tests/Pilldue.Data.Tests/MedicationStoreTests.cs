using Microsoft.Data.Sqlite;
using Pilldue.Business;
using Pilldue.Data;

namespace Pilldue.Data.Tests;

public class MedicationStoreTests
{
    [Fact]
    public async Task Save_and_load_medication_round_trips()
    {
        var dbPath = CreateTempDbPath();
        try
        {
            await PilldueDbBootstrap.MigrateAsync(dbPath);

            var medication = CreateSampleMedication();

            await using (var db = new PilldueDbContext(PilldueDbBootstrap.CreateOptions(dbPath)))
            {
                var repository = new EfMedicationRepository(db);
                await repository.AddAsync(medication);
            }

            await using (var db = new PilldueDbContext(PilldueDbBootstrap.CreateOptions(dbPath)))
            {
                var repository = new EfMedicationRepository(db);
                var loaded = await repository.GetAsync(medication.Id);

                Assert.NotNull(loaded);
                AssertEqualMedication(medication, loaded);

                var listed = await repository.ListAsync();
                var listedItem = Assert.Single(listed);
                AssertEqualMedication(medication, listedItem);
            }
        }
        finally
        {
            CleanupTempDb(dbPath);
        }
    }

    [Fact]
    public async Task Update_stock_persists_new_quantity()
    {
        var dbPath = CreateTempDbPath();
        try
        {
            await PilldueDbBootstrap.MigrateAsync(dbPath);

            var medication = CreateSampleMedication();
            medication.CurrentStockPills = 10;

            await using (var db = new PilldueDbContext(PilldueDbBootstrap.CreateOptions(dbPath)))
            {
                await new EfMedicationRepository(db).AddAsync(medication);
            }

            medication.CurrentStockPills = 42;
            medication.Name = "Updated Med";
            medication.RefillDayOfMonthOverride = 12;

            await using (var db = new PilldueDbContext(PilldueDbBootstrap.CreateOptions(dbPath)))
            {
                await new EfMedicationRepository(db).UpdateAsync(medication);
            }

            await using (var db = new PilldueDbContext(PilldueDbBootstrap.CreateOptions(dbPath)))
            {
                var loaded = await new EfMedicationRepository(db).GetAsync(medication.Id);
                Assert.NotNull(loaded);
                Assert.Equal(42, loaded.CurrentStockPills);
                Assert.Equal("Updated Med", loaded.Name);
                Assert.Equal(12, loaded.RefillDayOfMonthOverride);
            }
        }
        finally
        {
            CleanupTempDb(dbPath);
        }
    }

    [Fact]
    public async Task List_returns_medications_ordered_by_name()
    {
        var dbPath = CreateTempDbPath();
        try
        {
            await PilldueDbBootstrap.MigrateAsync(dbPath);

            await using (var db = new PilldueDbContext(PilldueDbBootstrap.CreateOptions(dbPath)))
            {
                var repository = new EfMedicationRepository(db);
                await repository.AddAsync(CreateSampleMedication(name: "Zocor"));
                await repository.AddAsync(CreateSampleMedication(name: "Aspirin"));
                await repository.AddAsync(CreateSampleMedication(name: "Metformin"));
            }

            await using (var db = new PilldueDbContext(PilldueDbBootstrap.CreateOptions(dbPath)))
            {
                var listed = await new EfMedicationRepository(db).ListAsync();
                Assert.Equal(new[] { "Aspirin", "Metformin", "Zocor" }, listed.Select(m => m.Name));
            }
        }
        finally
        {
            CleanupTempDb(dbPath);
        }
    }

    [Fact]
    public async Task Delete_removes_medication()
    {
        var dbPath = CreateTempDbPath();
        try
        {
            await PilldueDbBootstrap.MigrateAsync(dbPath);

            var medication = CreateSampleMedication();

            await using (var db = new PilldueDbContext(PilldueDbBootstrap.CreateOptions(dbPath)))
            {
                await new EfMedicationRepository(db).AddAsync(medication);
            }

            await using (var db = new PilldueDbContext(PilldueDbBootstrap.CreateOptions(dbPath)))
            {
                await new EfMedicationRepository(db).DeleteAsync(medication.Id);
            }

            await using (var db = new PilldueDbContext(PilldueDbBootstrap.CreateOptions(dbPath)))
            {
                var repository = new EfMedicationRepository(db);
                Assert.Null(await repository.GetAsync(medication.Id));
                Assert.Empty(await repository.ListAsync());
            }
        }
        finally
        {
            CleanupTempDb(dbPath);
        }
    }

    private static Medication CreateSampleMedication(string name = "TestMed") => new()
    {
        Name = name,
        PackageSizePills = 28,
        PrescribedPackageCount = 2,
        DailyDosagePills = 1,
        CurrentStockPills = 56,
        RefillDayOfMonthOverride = 7,
        PrescriptionStartDate = new DateOnly(2026, 1, 15),
        PrescriptionDurationMonths = 6,
    };

    private static void AssertEqualMedication(Medication expected, Medication actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.PackageSizePills, actual.PackageSizePills);
        Assert.Equal(expected.PrescribedPackageCount, actual.PrescribedPackageCount);
        Assert.Equal(expected.DailyDosagePills, actual.DailyDosagePills);
        Assert.Equal(expected.CurrentStockPills, actual.CurrentStockPills);
        Assert.Equal(expected.RefillDayOfMonthOverride, actual.RefillDayOfMonthOverride);
        Assert.Equal(expected.PrescriptionStartDate, actual.PrescriptionStartDate);
        Assert.Equal(expected.PrescriptionDurationMonths, actual.PrescriptionDurationMonths);
    }

    private static string CreateTempDbPath() =>
        Path.Combine(Path.GetTempPath(), "pilldue-tests", $"medications-{Guid.NewGuid():N}.db");

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
