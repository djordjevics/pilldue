using Pilldue.Business;

namespace Pilldue.Business.Tests.Planning;

public class StockCoverageQueryTests
{
    private static readonly AppConfig DefaultConfig = new() { DefaultRefillDayOfMonth = 5 };

    [Fact]
    public void Evaluate_may_to_june_31_day_gap_28_stock_does_not_cover()
    {
        var today = new DateOnly(2026, 5, 5);
        var medication = new Medication
        {
            Name = "Example",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 28,
            PrescriptionStartDate = new DateOnly(2026, 1, 5),
        };

        var result = StockCoverageQuery.Evaluate(medication, DefaultConfig, today);

        Assert.Equal(new DateOnly(2026, 6, 5), result.NextRefillDate);
        Assert.Equal(new DateOnly(2026, 6, 1), result.LastCoveredDate);
        Assert.False(result.CoversUntilNextRefill);
        Assert.Equal(3, result.PillsShort);
        Assert.Equal(1, result.PackagesToBuy);
    }

    [Fact]
    public void Evaluate_exactly_enough_stock_covers_31_day_gap()
    {
        var today = new DateOnly(2026, 5, 5);
        var medication = new Medication
        {
            Name = "Example",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 31,
            PrescriptionStartDate = new DateOnly(2026, 1, 5),
        };

        var result = StockCoverageQuery.Evaluate(medication, DefaultConfig, today);

        Assert.Equal(new DateOnly(2026, 6, 5), result.NextRefillDate);
        Assert.True(result.CoversUntilNextRefill);
        Assert.Equal(0, result.PillsShort);
        Assert.Equal(0, result.PackagesToBuy);
        Assert.Equal(new DateOnly(2026, 6, 4), result.LastCoveredDate);
    }

    [Fact]
    public void Evaluate_uses_prescription_start_day_as_refill_day()
    {
        var today = new DateOnly(2026, 5, 10);
        var medication = new Medication
        {
            Name = "Override",
            PackageSizePills = 30,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 4,
            PrescriptionStartDate = new DateOnly(2026, 1, 15),
        };

        var result = StockCoverageQuery.Evaluate(medication, DefaultConfig, today);

        Assert.Equal(new DateOnly(2026, 5, 15), result.NextRefillDate);
        Assert.Equal(5, result.NextRefillDate.DayNumber - today.DayNumber);
        Assert.False(result.CoversUntilNextRefill);
        Assert.Equal(1, result.PillsShort);
        Assert.Equal(1, result.PackagesToBuy);
    }

    [Fact]
    public void Evaluate_before_refill_day_in_month_uses_shorter_gap()
    {
        var today = new DateOnly(2026, 5, 3);
        var medication = new Medication
        {
            Name = "ShortGap",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 2,
            PrescriptionStartDate = new DateOnly(2026, 1, 5),
        };

        var result = StockCoverageQuery.Evaluate(medication, DefaultConfig, today);

        Assert.Equal(new DateOnly(2026, 5, 5), result.NextRefillDate);
        Assert.True(result.CoversUntilNextRefill);
        Assert.Equal(0, result.PillsShort);
    }

    [Fact]
    public void Evaluate_on_refill_day_targets_following_month()
    {
        var today = new DateOnly(2026, 5, 5);
        var medication = new Medication
        {
            Name = "RefillDay",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 0,
            PrescriptionStartDate = new DateOnly(2026, 1, 5),
        };

        var result = StockCoverageQuery.Evaluate(medication, DefaultConfig, today);

        Assert.Equal(new DateOnly(2026, 6, 5), result.NextRefillDate);
        Assert.False(result.CoversUntilNextRefill);
        Assert.Equal(31, result.PillsShort);
        Assert.Equal(2, result.PackagesToBuy);
        Assert.Null(result.LastCoveredDate);
    }

    [Fact]
    public async Task GetStockCoverageAsync_returns_results_for_all_medications()
    {
        var medications = new InMemoryMedicationRepository();
        var refills = new InMemoryRefillEventRepository();
        var skips = new InMemorySkipDoseEventRepository();
        var config = new InMemoryAppConfigStore(new AppConfig { DefaultRefillDayOfMonth = 5 });
        var app = new PilldueApp(medications, refills, skips, config);

        await app.AddMedicationAsync(new Medication
        {
            Name = "A",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 28,
            PrescriptionStartDate = new DateOnly(2026, 1, 5),
        });

        var today = new DateOnly(2026, 5, 5);
        var results = await app.GetStockCoverageAsync(today);

        var result = Assert.Single(results);
        Assert.Equal(new DateOnly(2026, 6, 5), result.NextRefillDate);
        Assert.False(result.CoversUntilNextRefill);
        Assert.Equal(3, result.PillsShort);
        Assert.Equal(1, result.PackagesToBuy);
    }
}
