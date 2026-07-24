using Pilldue.Business;
using Pilldue.Data;

namespace Pilldue.IntegrationTests;

public class AssemblySmokeTests
{
    [Fact]
    public void Business_and_Data_assemblies_are_loadable()
    {
        Assert.Equal("Pilldue.Business", typeof(Medication).Assembly.GetName().Name);
        Assert.Equal("Pilldue.Data", typeof(DataAssembly).Assembly.GetName().Name);
    }
}
