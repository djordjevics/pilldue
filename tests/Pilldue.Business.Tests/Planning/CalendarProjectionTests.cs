using Pilldue.Business;

namespace Pilldue.Business.Tests.Planning;

public class CalendarProjectionTests
{
        private static Medication Med(
        string name,
        int stock,
        int dosage = 1,
        int packageSize = 28,
        int prescribed = 1,
        int refillDay = 6) =>
        new()
        {
            Name = name,
            PackageSizePills = packageSize,
            PrescribedPackageCount = prescribed,
            DailyDosagePills = dosage,
            CurrentStockPills = stock,
            PrescriptionStartDate = new DateOnly(2026, 1, refillDay),
            PrescriptionDurationMonths = 6,
        };

    [Fact]
    public void Build_empty_meds_range_is_asOf_only()
    {
        var asOf = new DateOnly(2026, 5, 1);
        var view = CalendarProjection.Build(Array.Empty<Medication>(), asOf);

        Assert.Equal(asOf, view.RangeStart);
        Assert.Equal(asOf, view.RangeEnd);
    }

    [Fact]
    public void Build_range_end_is_latest_second_refill_among_meds()
    {
        var asOf = new DateOnly(2026, 5, 1);
        var view = CalendarProjection.Build(
            new[] { Med("A", stock: 90, refillDay: 6), Med("B", stock: 90, refillDay: 20) },
            asOf);

        Assert.Equal(asOf, view.RangeStart);
        Assert.Equal(new DateOnly(2026, 6, 20), view.RangeEnd);
    }

    [Fact]
    public void Evaluate_marks_only_first_day_of_each_stock_out_stretch()
    {
        var asOf = new DateOnly(2026, 5, 1);
        var entry = CalendarProjection.Evaluate(Med("Short", stock: 3, refillDay: 6), asOf);

        Assert.Equal(new DateOnly(2026, 5, 6), entry.FirstRefillDate);
        Assert.Equal(new DateOnly(2026, 6, 6), entry.SecondRefillDate);
        Assert.Equal(new DateOnly(2026, 5, 4), Assert.Single(entry.StockOutDates, d => d < entry.FirstRefillDate));
        Assert.DoesNotContain(new DateOnly(2026, 5, 5), entry.StockOutDates);
        Assert.DoesNotContain(new DateOnly(2026, 5, 6), entry.StockOutDates);
        Assert.Equal(2, entry.StockOutDates.Count);
    }

    [Fact]
    public void Evaluate_assumes_prescribed_restock_at_first_refill()
    {
        var asOf = new DateOnly(2026, 5, 1);
        var entry = CalendarProjection.Evaluate(
            Med("Restocked", stock: 3, prescribed: 2, refillDay: 6),
            asOf);

        Assert.Equal(new[] { new DateOnly(2026, 5, 4) }, entry.StockOutDates);
    }

    [Fact]
    public void Evaluate_marks_stock_out_again_when_restock_is_not_enough_for_second_gap()
    {
        var asOf = new DateOnly(2026, 5, 1);
        var med = Med("TinyPack", stock: 0, packageSize: 7, prescribed: 1, refillDay: 6);
        var entry = CalendarProjection.Evaluate(med, asOf);

        Assert.Equal(
            new[] { new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 13) },
            entry.StockOutDates);
        Assert.DoesNotContain(new DateOnly(2026, 5, 5), entry.StockOutDates);
        Assert.DoesNotContain(new DateOnly(2026, 5, 6), entry.StockOutDates);
        Assert.DoesNotContain(new DateOnly(2026, 6, 6), entry.StockOutDates);
    }

    [Fact]
    public void Evaluate_uses_prescription_start_day_as_refill_day()
    {
        var asOf = new DateOnly(2026, 5, 1);
        var entry = CalendarProjection.Evaluate(
            Med("RxDay", stock: 2, prescribed: 2, refillDay: 10),
            asOf);

        Assert.Equal(new DateOnly(2026, 5, 10), entry.FirstRefillDate);
        Assert.Equal(new DateOnly(2026, 6, 10), entry.SecondRefillDate);
        Assert.Equal(new[] { new DateOnly(2026, 5, 3) }, entry.StockOutDates);
    }

    [Fact]
    public void Build_aggregates_stock_out_days_across_medications()
    {
        var asOf = new DateOnly(2026, 5, 1);
        var view = CalendarProjection.Build(
            new[] { Med("A", stock: 1, prescribed: 2), Med("B", stock: 100) },
            asOf);

        Assert.Equal(2, view.Entries.Count);
        Assert.Equal(new[] { new DateOnly(2026, 5, 2) }, view.AllStockOutDates);
        Assert.Contains(view.Entries, e => e.Medication.Name == "B" && e.StockOutDates.Count == 0);
    }

    [Fact]
    public async Task GetCalendarAsync_range_follows_medication_second_refill()
    {
        var app = new PilldueApp(
            new InMemoryMedicationRepository(),
            new InMemoryRefillEventRepository(),
            new InMemorySkipDoseEventRepository(),
            new InMemoryAppConfigStore());

        await app.AddMedicationAsync(Med("InRange", stock: 2, prescribed: 2, refillDay: 6));

        var asOf = new DateOnly(2026, 5, 1);
        var view = await app.GetCalendarAsync(asOf);

        Assert.Equal(asOf, view.RangeStart);
        Assert.Equal(new DateOnly(2026, 6, 6), view.RangeEnd);
        var entry = Assert.Single(view.Entries);
        Assert.Equal(new DateOnly(2026, 7, 6), entry.PrescriptionEndDate);
        Assert.Equal(new[] { new DateOnly(2026, 5, 3) }, entry.StockOutDates);
    }
}
