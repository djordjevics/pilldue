namespace Pilldue.Business.Tests;

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
        Assert.Contains("UpdateMedicationAsync", screen, StringComparison.Ordinal);
        Assert.Contains("RefillAsync", screen, StringComparison.Ordinal);
        Assert.Contains("RemoveMedicationAsync", screen, StringComparison.Ordinal);
    }

    [Fact]
    public void Edit_keeps_the_selected_medication_id_and_rejects_invalid_input()
    {
        var toolbar = ReadRepoFile("src/Pilldue.UI.Wpf/MainWindow.xaml");
        Assert.Contains("Desktop.Edit", toolbar, StringComparison.Ordinal);
        Assert.Contains("OnEditClick", toolbar, StringComparison.Ordinal);

        var screen = ReadRepoFile("src/Pilldue.UI.Wpf/MainWindow.xaml.cs");
        Assert.Contains("new AddMedicationWindow(selected)", screen, StringComparison.Ordinal);
        Assert.Contains("dialog.ShowDialog() != true", screen, StringComparison.Ordinal);
        Assert.Contains("UpdateMedicationAsync", screen, StringComparison.Ordinal);

        var editor = ReadRepoFile("src/Pilldue.UI.Wpf/AddMedicationWindow.xaml.cs");
        Assert.Contains("saved.Id = _existing.Id", editor, StringComparison.Ordinal);
        Assert.Contains("DialogResult = false", editor, StringComparison.Ordinal);
        Assert.Contains("Desktop.AddInvalid", editor, StringComparison.Ordinal);
        Assert.DoesNotContain("IPilldueApp", editor, StringComparison.Ordinal);
    }

    [Fact]
    public void Refill_asks_for_a_package_count_and_cancels_without_writing()
    {
        var screen = ReadRepoFile("src/Pilldue.UI.Wpf/MainWindow.xaml.cs");
        Assert.Contains("new RefillWindow(selected)", screen, StringComparison.Ordinal);
        Assert.Contains("dialog.ShowDialog() != true || dialog.PackageCount < 1", screen, StringComparison.Ordinal);
        Assert.Contains(
            "RefillAsync(selected.Id, dialog.PackageCount, DateOnly.FromDateTime(DateTime.Today))",
            screen,
            StringComparison.Ordinal);
        Assert.DoesNotContain("PrescribedPackageCount > 0 ? selected.PrescribedPackageCount : 1", screen, StringComparison.Ordinal);

        var dialog = ReadRepoFile("src/Pilldue.UI.Wpf/RefillWindow.xaml.cs");
        Assert.Contains("count < 1", dialog, StringComparison.Ordinal);
        Assert.Contains("DialogResult = false", dialog, StringComparison.Ordinal);
        Assert.DoesNotContain("IPilldueApp", dialog, StringComparison.Ordinal);
        Assert.DoesNotContain("RefillAsync", dialog, StringComparison.Ordinal);
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
