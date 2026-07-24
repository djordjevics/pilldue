using Pilldue.Business;

namespace Pilldue.Business.Tests;

public class AssemblySmokeTests
{
    [Fact]
    public void Business_assembly_exposes_domain_types()
    {
        var name = typeof(Medication).Assembly.GetName().Name;
        Assert.Equal("Pilldue.Business", name);
    }
}
