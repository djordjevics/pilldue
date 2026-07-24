using Pilldue.Business;

namespace Pilldue.IntegrationTests;

/// <summary>
/// Calendar range is driven by medications' second refill dates (no config default day).
/// </summary>
public class CalendarRangeScenarios
{
    [Fact]
    public async Task Calendar_range_end_is_latest_second_refill_among_meds()
    {
        var app = new PilldueApp(
            new InMemoryMedicationRepository(),
            new InMemoryRefillEventRepository(),
            new InMemorySkipDoseEventRepository(),
            new InMemoryAppConfigStore());

        await app.AddMedicationAsync(new Medication
        {
            Name = "Early",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 90,
            PrescriptionStartDate = new DateOnly(2026, 1, 5),
        });
        await app.AddMedicationAsync(new Medication
        {
            Name = "Late",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 90,
            PrescriptionStartDate = new DateOnly(2026, 1, 20),
        });

        var asOf = new DateOnly(2026, 5, 1);
        var view = await app.GetCalendarAsync(asOf);

        Assert.Equal(asOf, view.RangeStart);
        Assert.Equal(new DateOnly(2026, 6, 20), view.RangeEnd);
    }

    [Fact]
    public async Task Empty_medication_list_calendar_range_is_as_of_only()
    {
        var app = new PilldueApp(
            new InMemoryMedicationRepository(),
            new InMemoryRefillEventRepository(),
            new InMemorySkipDoseEventRepository(),
            new InMemoryAppConfigStore());

        var asOf = new DateOnly(2026, 5, 1);
        var view = await app.GetCalendarAsync(asOf);

        Assert.Equal(asOf, view.RangeStart);
        Assert.Equal(asOf, view.RangeEnd);
        Assert.Empty(view.Entries);
    }
}
