using Pilldue.Business;

namespace Pilldue.Business.Tests;

/// <summary>C7: Apply skipped dose to stock; last covered moves out.</summary>
public class ApplySkipDoseTests
{
    [Fact]
    public async Task SkipDoseAsync_increases_stock_records_event_and_moves_last_covered()
    {
        var medications = new InMemoryMedicationRepository();
        var skips = new InMemorySkipDoseEventRepository();
        var app = new PilldueApp(
            medications,
            new InMemoryRefillEventRepository(),
            skips,
            new InMemoryAppConfigStore());

        var asOf = new DateOnly(2026, 5, 1);
        var med = await app.AddMedicationAsync(new Medication
        {
            Name = "SkipMe",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 10,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
        });

        var before = RefillCalendarRules.LastCoveredDate(asOf, 10, 1);
        Assert.Equal(new DateOnly(2026, 5, 10), before);

        await app.SkipDoseAsync(med.Id, pillsReturned: med.DailyDosagePills, date: new DateOnly(2026, 5, 6));

        var loaded = Assert.Single(await app.ListMedicationsAsync());
        Assert.Equal(11, loaded.CurrentStockPills);

        var evt = Assert.Single(await skips.ListByMedicationAsync(med.Id));
        Assert.Equal(1, evt.PillsReturned);
        Assert.Equal(new DateOnly(2026, 5, 6), evt.Date);

        var after = RefillCalendarRules.LastCoveredDate(
            asOf,
            loaded.CurrentStockPills,
            loaded.DailyDosagePills);
        Assert.Equal(new DateOnly(2026, 5, 11), after);
    }

    [Fact]
    public async Task SkipDoseAsync_rejects_non_positive_pills()
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
            CurrentStockPills = 10,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
        });

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => app.SkipDoseAsync(med.Id, pillsReturned: 0, date: new DateOnly(2026, 5, 6)));
    }
}
