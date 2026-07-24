using Pilldue.Business;

namespace Pilldue.Business.Tests;

/// <summary>C9: facade orchestration across main flows with in-memory ports.</summary>
public class FacadeWiringTests
{
    [Fact]
    public async Task Main_flows_add_query_refill_skip_and_calendar()
    {
        var app = new PilldueApp(
            new InMemoryMedicationRepository(),
            new InMemoryRefillEventRepository(),
            new InMemorySkipDoseEventRepository(),
            new InMemoryAppConfigStore(new AppConfig { DefaultRefillDayOfMonth = 5 }));

        var asOf = new DateOnly(2026, 5, 5);
        var med = await app.AddMedicationAsync(new Medication
        {
            Name = "FacadeMed",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 0,
            PrescriptionStartDate = new DateOnly(2026, 1, 5),
        });

        var coverage = Assert.Single(await app.GetStockCoverageAsync(asOf));
        Assert.False(coverage.CoversUntilNextRefill);
        Assert.Equal(31, coverage.PillsShort);

        var shortList = Assert.Single(await app.ListShortBeforeNextRefillAsync(asOf));
        Assert.Equal(med.Id, shortList.Medication.Id);

        var needExtra = Assert.Single(await app.ListNeedExtraForSecondRefillAsync(asOf));
        Assert.Equal(1, needExtra.ExtraPackages);

        await app.RefillAsync(med.Id, packageCount: 1, date: asOf);
        await app.SkipDoseAsync(med.Id, pillsReturned: 1, date: asOf.AddDays(1));

        var loaded = Assert.Single(await app.ListMedicationsAsync());
        Assert.Equal(28 + 1, loaded.CurrentStockPills);

        var calendar = await app.GetCalendarAsync(asOf);
        Assert.Contains(calendar.Entries, e => e.Medication.Id == med.Id);
        Assert.Equal(asOf, calendar.RangeStart);
    }
}
