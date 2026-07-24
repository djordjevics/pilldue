using Pilldue.Business;

namespace Pilldue.Business.Tests;

/// <summary>C6: Apply refill (N packages) to stock via facade + in-memory ports.</summary>
public class ApplyRefillTests
{
    [Fact]
    public async Task RefillAsync_adds_N_times_packageSize_and_records_event()
    {
        var medications = new InMemoryMedicationRepository();
        var refills = new InMemoryRefillEventRepository();
        var app = new PilldueApp(
            medications,
            refills,
            new InMemorySkipDoseEventRepository(),
            new InMemoryAppConfigStore());

        var med = await app.AddMedicationAsync(new Medication
        {
            Name = "RefillMe",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 5,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
        });

        var date = new DateOnly(2026, 5, 5);
        await app.RefillAsync(med.Id, packageCount: 2, date: date);

        var loaded = Assert.Single(await app.ListMedicationsAsync());
        Assert.Equal(5 + 2 * 28, loaded.CurrentStockPills);

        var evt = Assert.Single(await refills.ListByMedicationAsync(med.Id));
        Assert.Equal(2, evt.PackageCount);
        Assert.Equal(date, evt.Date);
        Assert.Equal(med.Id, evt.MedicationId);
    }

    [Fact]
    public async Task RefillAsync_rejects_non_positive_package_count()
    {
        var app = new PilldueApp(
            new InMemoryMedicationRepository(),
            new InMemoryRefillEventRepository(),
            new InMemorySkipDoseEventRepository(),
            new InMemoryAppConfigStore());

        var med = await app.AddMedicationAsync(new Medication
        {
            Name = "X",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 0,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
        });

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => app.RefillAsync(med.Id, packageCount: 0, date: new DateOnly(2026, 5, 5)));
    }
}
