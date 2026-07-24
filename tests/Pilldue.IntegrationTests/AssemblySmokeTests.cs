using Pilldue.Business;
using Pilldue.Data;

namespace Pilldue.IntegrationTests;

/// <summary>
/// Smoke check that integration test project references Business and Data.
/// </summary>
public class AssemblySmokeTests
{
    [Fact]
    public void Business_and_Data_assemblies_are_loadable()
    {
        Assert.Equal("Pilldue.Business", typeof(BusinessAssembly).Assembly.GetName().Name);
        Assert.Equal("Pilldue.Data", typeof(DataAssembly).Assembly.GetName().Name);
    }
}
