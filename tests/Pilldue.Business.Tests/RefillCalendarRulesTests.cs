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
    public void EffectiveRefillDay_uses_override_or_config_default()
    {
        var config = new AppConfig { DefaultRefillDayOfMonth = 5 };
        var inherited = new Medication { Name = "A" };
        var overridden = new Medication { Name = "B", RefillDayOfMonthOverride = 12 };

        Assert.Equal(5, RefillCalendarRules.EffectiveRefillDayOfMonth(inherited, config));
        Assert.Equal(12, RefillCalendarRules.EffectiveRefillDayOfMonth(overridden, config));
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
}
