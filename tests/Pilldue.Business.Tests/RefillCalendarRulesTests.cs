using Pilldue.Business;

namespace Pilldue.Business.Tests;

public class RefillCalendarRulesTests
{
    [Fact]
    public void ClampDayOfMonth_limits_to_days_in_month()
    {
        Assert.Equal(28, RefillCalendarRules.ClampDayOfMonth(2025, 2, 31));
        Assert.Equal(29, RefillCalendarRules.ClampDayOfMonth(2024, 2, 31));
        Assert.Equal(31, RefillCalendarRules.ClampDayOfMonth(2025, 1, 31));
        Assert.Equal(5, RefillCalendarRules.ClampDayOfMonth(2025, 6, 5));
    }

    [Fact]
    public void PackagesToBuy_matches_28_pills_short_three_example()
    {
        // 31-day gap, 28 pills on hand @ 1/day → 3 short → 1 package of 28.
        Assert.Equal(1, RefillCalendarRules.PackagesToBuy(pillsShort: 3, packageSizePills: 28));
        // Full gap from empty: 31 pills → 2 packages of 28.
        Assert.Equal(2, RefillCalendarRules.PackagesToBuy(pillsShort: 31, packageSizePills: 28));
        Assert.Equal(0, RefillCalendarRules.PackagesToBuy(pillsShort: 0, packageSizePills: 28));
    }

    [Fact]
    public void EffectiveRefillDay_is_prescription_start_day_of_month()
    {
        var med = new Medication
        {
            Name = "A",
            PrescriptionStartDate = new DateOnly(2026, 3, 15),
            RefillDayOfMonthOverride = 1, // ignored
        };

        Assert.Equal(15, RefillCalendarRules.EffectiveRefillDayOfMonth(med));
    }

    [Fact]
    public void NextAndSecondRefillDates_mid_month_before_day()
    {
        var today = new DateOnly(2026, 5, 3);
        var (next, second) = RefillCalendarRules.NextAndSecondRefillDates(today, dayOfMonth: 5);

        Assert.Equal(new DateOnly(2026, 5, 5), next);
        Assert.Equal(new DateOnly(2026, 6, 5), second);
    }

    [Fact]
    public void NextAndSecondRefillDates_after_day_passed_rolls_to_next_month()
    {
        var today = new DateOnly(2026, 5, 6);
        var (next, second) = RefillCalendarRules.NextAndSecondRefillDates(today, dayOfMonth: 5);

        Assert.Equal(new DateOnly(2026, 6, 5), next);
        Assert.Equal(new DateOnly(2026, 7, 5), second);
    }

    [Fact]
    public void NextAndSecondRefillDates_includes_today_when_on_refill_day()
    {
        var today = new DateOnly(2026, 5, 5);
        var (next, second) = RefillCalendarRules.NextAndSecondRefillDates(today, dayOfMonth: 5);

        Assert.Equal(new DateOnly(2026, 5, 5), next);
        Assert.Equal(new DateOnly(2026, 6, 5), second);
    }

    [Fact]
    public void NextAndSecondRefillDates_clamps_day_31_in_february()
    {
        var today = new DateOnly(2025, 2, 1);
        var (next, second) = RefillCalendarRules.NextAndSecondRefillDates(today, dayOfMonth: 31);

        Assert.Equal(new DateOnly(2025, 2, 28), next);
        Assert.Equal(new DateOnly(2025, 3, 31), second);
    }

    [Fact]
    public void NextAndSecondRefillDates_may_to_june_span_is_31_days()
    {
        var today = new DateOnly(2026, 5, 5);
        var (next, second) = RefillCalendarRules.NextAndSecondRefillDates(today, dayOfMonth: 5);

        Assert.Equal(new DateOnly(2026, 5, 5), next);
        Assert.Equal(new DateOnly(2026, 6, 5), second);
        Assert.Equal(31, second.DayNumber - next.DayNumber);
    }

    [Fact]
    public void PrescriptionEndDate_default_six_months_from_start()
    {
        var start = new DateOnly(2026, 1, 15);

        var end = RefillCalendarRules.PrescriptionEndDate(start, durationMonths: 6);

        Assert.Equal(new DateOnly(2026, 7, 15), end);
    }

    [Fact]
    public void PrescriptionEndDate_uses_medication_default_duration_of_six_months()
    {
        var medication = new Medication
        {
            Name = "Test",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 28,
            PrescriptionStartDate = new DateOnly(2026, 3, 5),
        };

        Assert.Equal(6, medication.PrescriptionDurationMonths);

        var end = RefillCalendarRules.PrescriptionEndDate(medication);

        Assert.Equal(new DateOnly(2026, 9, 5), end);
    }

    [Fact]
    public void PrescriptionEndDate_respects_explicit_duration()
    {
        var medication = new Medication
        {
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
            PrescriptionDurationMonths = 3,
        };

        var end = RefillCalendarRules.PrescriptionEndDate(medication);

        Assert.Equal(new DateOnly(2026, 4, 1), end);
    }

    [Fact]
    public void PrescriptionEndDate_clamps_day_when_target_month_is_shorter()
    {
        var end = RefillCalendarRules.PrescriptionEndDate(new DateOnly(2026, 1, 31), durationMonths: 1);

        Assert.Equal(new DateOnly(2026, 2, 28), end);
    }

    [Fact]
    public void PrescriptionEndDate_rejects_non_positive_duration()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RefillCalendarRules.PrescriptionEndDate(new DateOnly(2026, 1, 1), durationMonths: 0));
    }
}
