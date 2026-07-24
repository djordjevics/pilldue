using Pilldue.Data;

namespace Pilldue.Data.Tests;

/// <summary>
/// Unit tests for persistence (SQLite repositories, mapping, queries).
/// Keep these focused on Data types — no Business workflows here.
/// </summary>
public class AssemblySmokeTests
{
    [Fact]
    public void Data_assembly_is_loadable()
    {
        var name = typeof(DataAssembly).Assembly.GetName().Name;
        Assert.Equal("Pilldue.Data", name);
    }
}
