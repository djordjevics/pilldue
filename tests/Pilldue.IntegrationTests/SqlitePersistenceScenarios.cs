using Microsoft.Data.Sqlite;
using Pilldue.Business;
using Pilldue.Data;

namespace Pilldue.IntegrationTests;

/// <summary>
/// Composition uses EF + SQLite so meds survive across app/context lifetimes.
/// </summary>
public class SqlitePersistenceScenarios
{
    [Fact]
    public async Task Medication_added_via_ef_repos_survives_new_context()
    {
        var dbPath = Path.Combine(
            Path.GetTempPath(),
            "pilldue-tests",
            $"persist-{Guid.NewGuid():N}.db");

        try
        {
            await PilldueDbBootstrap.MigrateAsync(dbPath);
            var options = PilldueDbBootstrap.CreateOptions(dbPath);

            Guid id;
            await using (var db = new PilldueDbContext(options))
            {
                var app = new PilldueApp(
                    new EfMedicationRepository(db),
                    new EfRefillEventRepository(db),
                    new EfSkipDoseEventRepository(options),
                    new InMemoryAppConfigStore());

                var med = await app.AddMedicationAsync(new Medication
                {
                    Name = "PersistentMed",
                    PackageSizePills = 28,
                    PrescribedPackageCount = 1,
                    DailyDosagePills = 1,
                    CurrentStockPills = 14,
                    PrescriptionStartDate = new DateOnly(2026, 1, 1),
                });
                id = med.Id;
            }

            await using (var db = new PilldueDbContext(options))
            {
                var app = new PilldueApp(
                    new EfMedicationRepository(db),
                    new EfRefillEventRepository(db),
                    new EfSkipDoseEventRepository(options),
                    new InMemoryAppConfigStore());

                var listed = Assert.Single(await app.ListMedicationsAsync());
                Assert.Equal(id, listed.Id);
                Assert.Equal("PersistentMed", listed.Name);
                Assert.Equal(14, listed.CurrentStockPills);
            }
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
