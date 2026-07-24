using Pilldue.Business;

namespace Pilldue.Business.Tests;

/// <summary>
/// Unit tests for domain and application services.
/// Prefer pure logic tests (refill-by math, skip-dose stock bump) without SQLite when possible.
/// </summary>
public class AssemblySmokeTests
{
    [Fact]
    public void Business_assembly_is_loadable()
    {
        var name = typeof(BusinessAssembly).Assembly.GetName().Name;
        Assert.Equal("Pilldue.Business", name);
    }
}
