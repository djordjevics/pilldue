using Pilldue.Business;

namespace Pilldue.Business.Tests;

public class LastCoveredDateTests
{
    private static readonly DateOnly AsOf = new(2026, 5, 1);
    private static readonly DateOnly Start = new(2026, 1, 1);

    [Fact]
    public void LastCoveredDate_zero_stock_returns_null()
    {
        var result = RefillCalendarRules.LastCoveredDate(
            AsOf, stockPills: 0, dosagePills: 1, prescriptionStart: Start);

        Assert.Null(result);
    }

    [Fact]
    public void LastCoveredDate_stock_below_dosage_returns_null()
    {
        var result = RefillCalendarRules.LastCoveredDate(
            AsOf, stockPills: 1, dosagePills: 2, prescriptionStart: Start);

        Assert.Null(result);
    }

    [Fact]
    public void LastCoveredDate_exact_division_includes_as_of_as_day_one()
    {
        // floor(28/1)=28 → last covered = 1 May + 27 days = 28 May
        var result = RefillCalendarRules.LastCoveredDate(
            AsOf, stockPills: 28, dosagePills: 1, prescriptionStart: Start);

        Assert.Equal(new DateOnly(2026, 5, 28), result);
    }

    [Fact]
    public void LastCoveredDate_remainder_uses_floor()
    {
        // floor(29/2)=14 → last covered = 1 May + 13 days = 14 May
        var result = RefillCalendarRules.LastCoveredDate(
            AsOf, stockPills: 29, dosagePills: 2, prescriptionStart: Start);

        Assert.Equal(new DateOnly(2026, 5, 14), result);
    }

    [Fact]
    public void LastCoveredDate_every_other_day_uses_dose_days_only()
    {
        // Start 1 May; dose days 1,3,5,... ; stock 3 @ 1/dose → three doses → last = 5 May
        var asOf = new DateOnly(2026, 5, 1);
        var result = RefillCalendarRules.LastCoveredDate(
            asOf, stockPills: 3, dosagePills: 1, prescriptionStart: asOf, intervalDays: 2);

        Assert.Equal(new DateOnly(2026, 5, 5), result);
    }

    [Fact]
    public void LastCoveredDate_weekly_from_prescription_start()
    {
        // Start 1 May; weekly doses; stock 3 → May 1, 8, 15
        var start = new DateOnly(2026, 5, 1);
        var result = RefillCalendarRules.LastCoveredDate(
            start, stockPills: 3, dosagePills: 1, prescriptionStart: start, intervalDays: 7);

        Assert.Equal(new DateOnly(2026, 5, 15), result);
    }
}
