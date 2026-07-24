using Pilldue.Business;
using Pilldue.Data;

namespace Pilldue.Data.Tests;

public class FileAppConfigStoreTests
{
    [Fact]
    public async Task Load_missing_file_returns_default_refill_day_5()
    {
        var path = Path.Combine(Path.GetTempPath(), $"pilldue-config-{Guid.NewGuid():N}.json");
        try
        {
            Assert.False(File.Exists(path));
            var store = new FileAppConfigStore(path);

            var config = await store.LoadAsync();

            Assert.Equal(AppConfig.DefaultRefillDayOfMonthValue, config.DefaultRefillDayOfMonth);
            Assert.Equal(5, config.DefaultRefillDayOfMonth);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task Save_then_load_round_trips_override()
    {
        var path = Path.Combine(Path.GetTempPath(), $"pilldue-config-{Guid.NewGuid():N}.json");
        try
        {
            var store = new FileAppConfigStore(path);

            await store.SaveAsync(new AppConfig { DefaultRefillDayOfMonth = 12 });
            var loaded = await store.LoadAsync();

            Assert.Equal(12, loaded.DefaultRefillDayOfMonth);

            // Fresh store instance still sees persisted value
            var reloaded = await new FileAppConfigStore(path).LoadAsync();
            Assert.Equal(12, reloaded.DefaultRefillDayOfMonth);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
