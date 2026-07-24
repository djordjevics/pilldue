using Pilldue.Data;

namespace Pilldue.Data.Tests;

public class AssemblySmokeTests
{
    [Fact]
    public void Data_assembly_is_loadable()
    {
        var name = typeof(DataAssembly).Assembly.GetName().Name;
        Assert.Equal("Pilldue.Data", name);
    }
}
