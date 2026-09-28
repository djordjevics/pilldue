using Pilldue.Business;

namespace Pilldue.Business.Tests;

public class FlagMissedDoseTests
{
    [Fact]
    public async Task Flag_missed_dose_records_event_without_changing_stock()
    {
        var medications = new InMemoryMedicationRepository();
        var missed = new InMemoryMissedDoseEventRepository();
        var app = new PilldueApp(
            medications,
            new InMemoryRefillEventRepository(),
            new InMemorySkipDoseEventRepository(),
            missed,
            new InMemoryAppConfigStore());

        var med = await app.AddMedicationAsync(new Medication
        {
            Name = "Aspirin",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 10,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
        });

        var date = new DateOnly(2026, 5, 10);
        await app.FlagMissedDoseAsync(med.Id, date);

        var loaded = Assert.Single(await app.ListMedicationsAsync());
        Assert.Equal(10, loaded.CurrentStockPills);

        var entry = Assert.Single(await app.ListMissedDosesAsync());
        Assert.Equal(med.Id, entry.MedicationId);
        Assert.Equal(date, entry.Date);
        Assert.Single(await missed.ListByMedicationAsync(med.Id));
    }

    [Fact]
    public async Task Flag_unknown_medication_throws()
    {
        var app = new PilldueApp(
            new InMemoryMedicationRepository(),
            new InMemoryRefillEventRepository(),
            new InMemorySkipDoseEventRepository(),
            new InMemoryMissedDoseEventRepository(),
            new InMemoryAppConfigStore());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => app.FlagMissedDoseAsync(Guid.NewGuid(), new DateOnly(2026, 5, 1)));
    }
}
