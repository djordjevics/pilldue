using Microsoft.Data.Sqlite;
using Pilldue.Business;
using Pilldue.Data;

namespace Pilldue.IntegrationTests;

/// <summary>
/// Remove medication via the facade; EF cascades refill and skip-dose rows (#67).
/// </summary>
public class RemoveMedicationScenarios
{
    [Fact]
    public async Task Remove_medication_clears_med_and_cascaded_events()
    {
        var dbPath = Path.Combine(
            Path.GetTempPath(),
            "pilldue-tests",
            $"remove-{Guid.NewGuid():N}.db");

        try
        {
            await PilldueDbBootstrap.MigrateAsync(dbPath);
            var options = PilldueDbBootstrap.CreateOptions(dbPath);

            await using var db = new PilldueDbContext(options);
            var refills = new EfRefillEventRepository(db);
            var skips = new EfSkipDoseEventRepository(options);
            var app = new PilldueApp(
                new EfMedicationRepository(db),
                refills,
                skips,
                new InMemoryMissedDoseEventRepository(),
                new InMemoryAppConfigStore());

            var med = await app.AddMedicationAsync(new Medication
            {
                Name = "GoneMed",
                PackageSizePills = 28,
                PrescribedPackageCount = 1,
                DailyDosagePills = 1,
                CurrentStockPills = 0,
                PrescriptionStartDate = new DateOnly(2026, 1, 6),
            });

            await app.RefillAsync(med.Id, packageCount: 1, date: new DateOnly(2026, 5, 5));
            await app.SkipDoseAsync(med.Id, pillsReturned: 1, date: new DateOnly(2026, 5, 6));

            Assert.Single(await refills.ListByMedicationAsync(med.Id));
            Assert.Single(await skips.ListByMedicationAsync(med.Id));

            await app.RemoveMedicationAsync(med.Id);

            Assert.Empty(await app.ListMedicationsAsync());
            Assert.Empty(await refills.ListByMedicationAsync(med.Id));
            Assert.Empty(await skips.ListByMedicationAsync(med.Id));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }
}
