using Pilldue.Business;

namespace Pilldue.Business.Tests;

public class AppConfigDefaultsTests
{
    [Fact]
    public void Default_ui_language_is_empty_for_os_detection()
    {
        Assert.Equal(string.Empty, new AppConfig().UiLanguage);
    }
}
