using Pilldue.Business;

namespace Pilldue.IntegrationTests;

/// <summary>
/// Cancel path for refill is a UI concern; this scenario locks that aborting before
/// <see cref="IPilldueApp.RefillAsync"/> leaves stock and history unchanged (#58).
/// </summary>
public class RefillCancelScenarios
{
    [Fact]
    public async Task Skipping_RefillAsync_leaves_stock_and_history_unchanged()
    {
        var refills = new InMemoryRefillEventRepository();
        var app = new PilldueApp(
            new InMemoryMedicationRepository(),
            refills,
            new InMemorySkipDoseEventRepository(),
            new InMemoryAppConfigStore());

        var med = await app.AddMedicationAsync(new Medication
        {
            Name = "CancelMe",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 10,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
        });

        // Simulates UI cancel: user never calls RefillAsync.
        var listed = Assert.Single(await app.ListMedicationsAsync());
        Assert.Equal(10, listed.CurrentStockPills);
        Assert.Empty(await refills.ListByMedicationAsync(med.Id));
    }

    [Fact]
    public async Task Completed_refill_still_updates_stock_when_not_cancelled()
    {
        var refills = new InMemoryRefillEventRepository();
        var app = new PilldueApp(
            new InMemoryMedicationRepository(),
            refills,
            new InMemorySkipDoseEventRepository(),
            new InMemoryAppConfigStore());

        var med = await app.AddMedicationAsync(new Medication
        {
            Name = "DoRefill",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 10,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
        });

        await app.RefillAsync(med.Id, packageCount: 1, date: new DateOnly(2026, 5, 6));

        var listed = Assert.Single(await app.ListMedicationsAsync());
        Assert.Equal(38, listed.CurrentStockPills);
        Assert.Single(await refills.ListByMedicationAsync(med.Id));
    }
}
