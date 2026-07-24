using Pilldue.Business;

namespace Pilldue.Business.Tests;

public class AppConfigDefaultsTests
{
    [Fact]
    public void Default_refill_day_of_month_is_6()
    {
        Assert.Equal(6, AppConfig.DefaultRefillDayOfMonthValue);
        Assert.Equal(6, new AppConfig().DefaultRefillDayOfMonth);
    }
}
