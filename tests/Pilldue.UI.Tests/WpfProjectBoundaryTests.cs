namespace Pilldue.UI.Tests;

public class WpfProjectBoundaryTests
{
    [Fact]
    public void Wpf_screens_reference_business_only()
    {
        var csproj = ReadRepoFile("src/Pilldue.UI.Wpf/Pilldue.UI.Wpf.csproj");
        Assert.Contains("Pilldue.Business", csproj, StringComparison.Ordinal);
        Assert.DoesNotContain("Pilldue.Data", csproj, StringComparison.Ordinal);
        Assert.DoesNotContain("Pilldue.UI.csproj", csproj, StringComparison.Ordinal);
        Assert.DoesNotContain("Spectre", csproj, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("EntityFramework", csproj, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQLite", csproj, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("System.Data", csproj, StringComparison.Ordinal);

        foreach (var file in Directory.EnumerateFiles(RepoPath("src/Pilldue.UI.Wpf"), "*", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            Assert.DoesNotContain("Pilldue.Data", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Spectre", text, StringComparison.Ordinal);
            Assert.DoesNotContain("EntityFramework", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Microsoft.Data.Sqlite", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Desktop_host_opens_the_session_and_passes_the_app()
    {
        var host = ReadRepoFile("src/Pilldue.UI.Desktop/App.xaml.cs");
        Assert.Contains("PilldueComposition.CreateDefaultSessionAsync", host, StringComparison.Ordinal);
        Assert.Contains("new MainWindow(_session.App)", host, StringComparison.Ordinal);

        var screen = ReadRepoFile("src/Pilldue.UI.Wpf/MainWindow.xaml.cs");
        Assert.Contains("ListMedicationsAsync", screen, StringComparison.Ordinal);
        Assert.Contains("AddMedicationAsync", screen, StringComparison.Ordinal);
        Assert.Contains("RefillAsync", screen, StringComparison.Ordinal);
        Assert.Contains("RemoveMedicationAsync", screen, StringComparison.Ordinal);
    }

    [Fact]
    public void Terminal_ui_still_references_business_only()
    {
        var csproj = ReadRepoFile("src/Pilldue.UI/Pilldue.UI.csproj");
        Assert.Contains("Pilldue.Business", csproj, StringComparison.Ordinal);
        Assert.Contains("Spectre.Console", csproj, StringComparison.Ordinal);
        Assert.DoesNotContain("Pilldue.Data", csproj, StringComparison.Ordinal);
    }

    private static string ReadRepoFile(string relativePath) =>
        File.ReadAllText(RepoPath(relativePath));

    private static string RepoPath(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Pilldue.slnx")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return Path.Combine(dir.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
