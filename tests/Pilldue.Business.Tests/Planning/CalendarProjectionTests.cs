using Pilldue.Business;

namespace Pilldue.Business.Tests.Planning;

public class CalendarProjectionTests
{
    private static readonly AppConfig ConfigDay6 = new() { DefaultRefillDayOfMonth = 6 };

    private static Medication Med(
        string name,
        int stock,
        int dosage = 1,
        int packageSize = 28,
        int prescribed = 1,
        int? refillOverride = null) =>
        new()
        {
            Name = name,
            PackageSizePills = packageSize,
            PrescribedPackageCount = prescribed,
            DailyDosagePills = dosage,
            CurrentStockPills = stock,
            RefillDayOfMonthOverride = refillOverride,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
            PrescriptionDurationMonths = 6,
        };

    [Fact]
    public void Build_range_is_asOf_through_second_config_refill_day()
    {
        var asOf = new DateOnly(2026, 5, 1);
        var view = CalendarProjection.Build(Array.Empty<Medication>(), ConfigDay6, asOf);

        Assert.Equal(asOf, view.RangeStart);
        Assert.Equal(new DateOnly(2026, 6, 6), view.RangeEnd);
    }

    [Fact]
    public void Evaluate_marks_stock_out_days_before_first_refill()
    {
        // asOf May 1, refill day 6, stock 3 → out on May 4–5 before restock on May 6.
        // One package of 28 is not enough for the 31-day May6→Jun6 gap, so stock-outs resume later.
        var asOf = new DateOnly(2026, 5, 1);
        var entry = CalendarProjection.Evaluate(Med("Short", stock: 3), ConfigDay6, asOf);

        Assert.Equal(new DateOnly(2026, 5, 6), entry.FirstRefillDate);
        Assert.Equal(new DateOnly(2026, 6, 6), entry.SecondRefillDate);
        Assert.Contains(new DateOnly(2026, 5, 4), entry.StockOutDates);
        Assert.Contains(new DateOnly(2026, 5, 5), entry.StockOutDates);
        Assert.DoesNotContain(new DateOnly(2026, 5, 6), entry.StockOutDates);
    }

    [Fact]
    public void Evaluate_assumes_prescribed_restock_at_first_refill()
    {
        // Two packages (56) at first refill cover the 31-day gap after early shortfall.
        var asOf = new DateOnly(2026, 5, 1);
        var entry = CalendarProjection.Evaluate(
            Med("Restocked", stock: 3, prescribed: 2),
            ConfigDay6,
            asOf);

        Assert.Equal(
            new[] { new DateOnly(2026, 5, 4), new DateOnly(2026, 5, 5) },
            entry.StockOutDates);
        Assert.All(entry.StockOutDates, d => Assert.True(d < entry.FirstRefillDate));
    }

    [Fact]
    public void Evaluate_marks_stock_out_again_when_restock_is_not_enough_for_second_gap()
    {
        // Empty stock, package 7, prescribed 1 → restock 7 on May 6.
        // May 6–12 covered; May 13 onward short until June 6.
        var asOf = new DateOnly(2026, 5, 1);
        var med = Med("TinyPack", stock: 0, packageSize: 7, prescribed: 1);
        var entry = CalendarProjection.Evaluate(med, ConfigDay6, asOf);

        Assert.Contains(new DateOnly(2026, 5, 1), entry.StockOutDates);
        Assert.Contains(new DateOnly(2026, 5, 5), entry.StockOutDates);
        Assert.DoesNotContain(new DateOnly(2026, 5, 6), entry.StockOutDates);
        Assert.Contains(new DateOnly(2026, 5, 13), entry.StockOutDates);
        Assert.Contains(new DateOnly(2026, 6, 6), entry.StockOutDates);
    }

    [Fact]
    public void Evaluate_uses_per_med_refill_override_for_restock_day()
    {
        var asOf = new DateOnly(2026, 5, 1);
        var entry = CalendarProjection.Evaluate(
            Med("Override", stock: 2, prescribed: 2, refillOverride: 10),
            ConfigDay6,
            asOf);

        Assert.Equal(new DateOnly(2026, 5, 10), entry.FirstRefillDate);
        Assert.Equal(new DateOnly(2026, 6, 10), entry.SecondRefillDate);
        Assert.Equal(
            new[]
            {
                new DateOnly(2026, 5, 3),
                new DateOnly(2026, 5, 4),
                new DateOnly(2026, 5, 5),
                new DateOnly(2026, 5, 6),
                new DateOnly(2026, 5, 7),
                new DateOnly(2026, 5, 8),
                new DateOnly(2026, 5, 9),
            },
            entry.StockOutDates);
    }

    [Fact]
    public void Build_aggregates_stock_out_days_across_medications()
    {
        var asOf = new DateOnly(2026, 5, 1);
        var view = CalendarProjection.Build(
            new[] { Med("A", stock: 1), Med("B", stock: 100) },
            ConfigDay6,
            asOf);

        Assert.Equal(2, view.Entries.Count);
        Assert.Contains(new DateOnly(2026, 5, 2), view.AllStockOutDates);
        Assert.Contains(view.Entries, e => e.Medication.Name == "B" && e.StockOutDates.Count == 0);
    }

    [Fact]
    public async Task GetCalendarAsync_uses_config_default_day_for_range()
    {
        var store = new InMemoryAppConfigStore(new AppConfig { DefaultRefillDayOfMonth = 6 });
        var app = new PilldueApp(
            new InMemoryMedicationRepository(),
            new InMemoryRefillEventRepository(),
            new InMemorySkipDoseEventRepository(),
            store);

        await app.AddMedicationAsync(Med("InRange", stock: 2));

        var asOf = new DateOnly(2026, 5, 1);
        var view = await app.GetCalendarAsync(asOf);

        Assert.Equal(asOf, view.RangeStart);
        Assert.Equal(new DateOnly(2026, 6, 6), view.RangeEnd);
        var entry = Assert.Single(view.Entries);
        Assert.Equal(new DateOnly(2026, 7, 1), entry.PrescriptionEndDate);
        Assert.NotEmpty(entry.StockOutDates);
    }
}
