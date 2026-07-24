using Pilldue.Business;

namespace Pilldue.Business.Tests;

public class InMemoryPortsTests
{
    [Fact]
    public async Task Refill_and_skip_update_stock_through_facade()
    {
        var medications = new InMemoryMedicationRepository();
        var refills = new InMemoryRefillEventRepository();
        var skips = new InMemorySkipDoseEventRepository();
        var config = new InMemoryAppConfigStore();
        var app = new PilldueApp(medications, refills, skips, config);

        var med = new Medication
        {
            Name = "TestMed",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 0,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
        };

        await app.AddMedicationAsync(med);
        await app.RefillAsync(med.Id, packageCount: 1, date: new DateOnly(2026, 5, 5));
        await app.SkipDoseAsync(med.Id, pillsReturned: 1, date: new DateOnly(2026, 5, 6));

        var loaded = Assert.Single(await app.ListMedicationsAsync());
        Assert.Equal(29, loaded.CurrentStockPills);
        Assert.Single(await refills.ListByMedicationAsync(med.Id));
        Assert.Single(await skips.ListByMedicationAsync(med.Id));
    }
}
