using Pilldue.Business;

namespace Pilldue.Business.Tests;

public class RemoveMedicationTests
{
    [Fact]
    public async Task Remove_medication_drops_it_from_list()
    {
        var medications = new InMemoryMedicationRepository();
        var app = new PilldueApp(
            medications,
            new InMemoryRefillEventRepository(),
            new InMemorySkipDoseEventRepository(),
            new InMemoryAppConfigStore());

        var med = await app.AddMedicationAsync(new Medication
        {
            Name = "ToRemove",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 10,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
        });

        await app.RemoveMedicationAsync(med.Id);

        Assert.Empty(await app.ListMedicationsAsync());
    }

    [Fact]
    public async Task Remove_unknown_medication_throws()
    {
        var app = new PilldueApp(
            new InMemoryMedicationRepository(),
            new InMemoryRefillEventRepository(),
            new InMemorySkipDoseEventRepository(),
            new InMemoryAppConfigStore());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => app.RemoveMedicationAsync(Guid.NewGuid()));
    }
}
