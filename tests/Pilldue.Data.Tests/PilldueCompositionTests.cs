using Microsoft.Data.Sqlite;
using Pilldue.Business;
using Pilldue.Data;

namespace Pilldue.Data.Tests;

public class PilldueCompositionTests
{
    [Fact]
    public async Task CreateSession_migrates_and_returns_working_app()
    {
        var dir = Path.Combine(Path.GetTempPath(), "pilldue-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var dbPath = Path.Combine(dir, "comp.db");
        var configPath = Path.Combine(dir, "config.json");

        try
        {
            await using var session = await PilldueComposition.CreateSessionAsync(dbPath, configPath);
            var med = await session.App.AddMedicationAsync(new Medication
            {
                Name = "CompMed",
                PackageSizePills = 28,
                PrescribedPackageCount = 1,
                DailyDosagePills = 1,
                CurrentStockPills = 7,
                PrescriptionStartDate = new DateOnly(2026, 1, 1),
            });

            var listed = Assert.Single(await session.App.ListMedicationsAsync());
            Assert.Equal(med.Id, listed.Id);
            Assert.Equal("CompMed", listed.Name);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            try
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, recursive: true);
                }
            }
            catch (IOException)
            {
                // best-effort on Windows
            }
        }
    }
}
