using Pilldue.Business;

namespace Pilldue.IntegrationTests;

/// <summary>
/// Default restock / refill day is the 6th (#60); planning and calendar honor it.
/// </summary>
public class DefaultRefillDayScenarios
{
    [Fact]
    public async Task New_app_config_default_is_day_6_for_coverage_and_calendar()
    {
        var app = new PilldueApp(
            new InMemoryMedicationRepository(),
            new InMemoryRefillEventRepository(),
            new InMemorySkipDoseEventRepository(),
            new InMemoryAppConfigStore());

        var config = await app.GetConfigAsync();
        Assert.Equal(6, config.DefaultRefillDayOfMonth);

        await app.AddMedicationAsync(new Medication
        {
            Name = "DaySixMed",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 28,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
        });

        // On day 6, next refill is next month's 6th (31-day May→June gap from 6 May).
        var asOf = new DateOnly(2026, 5, 6);
        var coverage = Assert.Single(await app.GetStockCoverageAsync(asOf));
        Assert.Equal(new DateOnly(2026, 6, 6), coverage.NextRefillDate);
        Assert.False(coverage.CoversUntilNextRefill);
        Assert.Equal(3, coverage.PillsShort);

        var calendar = await app.GetCalendarAsync(asOf);
        Assert.Equal(asOf, calendar.RangeStart);
        Assert.Equal(new DateOnly(2026, 6, 6), calendar.RangeEnd);
    }
}
