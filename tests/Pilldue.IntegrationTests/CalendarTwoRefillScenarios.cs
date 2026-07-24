using Pilldue.Business;

namespace Pilldue.IntegrationTests;

/// <summary>
/// Calendar spans today → second refill; stock-outs assume prescribed restock at first refill (#59).
/// </summary>
public class CalendarTwoRefillScenarios
{
    [Fact]
    public async Task Calendar_marks_gap_before_first_refill_then_covers_after_assumed_restock()
    {
        var app = new PilldueApp(
            new InMemoryMedicationRepository(),
            new InMemoryRefillEventRepository(),
            new InMemorySkipDoseEventRepository(),
            new InMemoryAppConfigStore(new AppConfig { DefaultRefillDayOfMonth = 6 }));

        await app.AddMedicationAsync(new Medication
        {
            Name = "Atorvastatin",
            PackageSizePills = 28,
            PrescribedPackageCount = 2,
            DailyDosagePills = 1,
            CurrentStockPills = 3,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
        });

        var asOf = new DateOnly(2026, 5, 1);
        var view = await app.GetCalendarAsync(asOf);

        Assert.Equal(asOf, view.RangeStart);
        Assert.Equal(new DateOnly(2026, 6, 6), view.RangeEnd);

        var entry = Assert.Single(view.Entries);
        Assert.Equal(new DateOnly(2026, 5, 6), entry.FirstRefillDate);
        Assert.Equal(
            new[] { new DateOnly(2026, 5, 4) },
            entry.StockOutDates);
        Assert.Equal(view.AllStockOutDates, entry.StockOutDates);
    }

    [Fact]
    public async Task Calendar_with_enough_stock_has_no_stock_out_notes()
    {
        var app = new PilldueApp(
            new InMemoryMedicationRepository(),
            new InMemoryRefillEventRepository(),
            new InMemorySkipDoseEventRepository(),
            new InMemoryAppConfigStore(new AppConfig { DefaultRefillDayOfMonth = 6 }));

        await app.AddMedicationAsync(new Medication
        {
            Name = "Metformin",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 90,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
        });

        var view = await app.GetCalendarAsync(new DateOnly(2026, 5, 1));
        var entry = Assert.Single(view.Entries);
        Assert.Empty(entry.StockOutDates);
        Assert.Empty(view.AllStockOutDates);
    }
}
