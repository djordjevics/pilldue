using Pilldue.Business;

namespace Pilldue.Business.Tests;

public class LastCoveredDateTests
{
    private static readonly DateOnly AsOf = new(2026, 5, 1);

    [Fact]
    public void LastCoveredDate_zero_stock_returns_null()
    {
        var result = RefillCalendarRules.LastCoveredDate(AsOf, stockPills: 0, dailyDosagePills: 1);

        Assert.Null(result);
    }

    [Fact]
    public void LastCoveredDate_stock_below_dosage_returns_null()
    {
        var result = RefillCalendarRules.LastCoveredDate(AsOf, stockPills: 1, dailyDosagePills: 2);

        Assert.Null(result);
    }

    [Fact]
    public void LastCoveredDate_exact_division_includes_as_of_as_day_one()
    {
        // floor(28/1)=28 → last covered = 1 May + 27 days = 28 May
        var result = RefillCalendarRules.LastCoveredDate(AsOf, stockPills: 28, dailyDosagePills: 1);

        Assert.Equal(new DateOnly(2026, 5, 28), result);
    }

    [Fact]
    public void LastCoveredDate_remainder_uses_floor()
    {
        // floor(29/2)=14 → last covered = 1 May + 13 days = 14 May
        var result = RefillCalendarRules.LastCoveredDate(AsOf, stockPills: 29, dailyDosagePills: 2);

        Assert.Equal(new DateOnly(2026, 5, 14), result);
    }
}
