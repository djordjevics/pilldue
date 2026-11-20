using System.Text.RegularExpressions;

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
    public void Window_code_behind_only_calls_InitializeComponent()
    {
        var windows = WindowCodeBehindFiles();
        Assert.Equal(
            new[]
            {
                "AddMedicationWindow.xaml.cs",
                "BulkRefillWindow.xaml.cs",
                "CalendarWindow.xaml.cs",
                "FlagMissedDoseWindow.xaml.cs",
                "MainWindow.xaml.cs",
                "MissedDoseWindow.xaml.cs",
                "RefillWindow.xaml.cs",
                "SettingsWindow.xaml.cs",
                "SkipDoseWindow.xaml.cs",
                "StockCoverageWindow.xaml.cs",
            },
            windows.Select(Path.GetFileName).OrderBy(name => name, StringComparer.Ordinal).ToArray());

        foreach (var file in windows)
        {
            var text = File.ReadAllText(file);
            var xaml = File.ReadAllText(file[..^3]);
            Assert.Matches(ConstructorOnly, text);
            Assert.Equal(2, text.Count(character => character == '{'));
            Assert.DoesNotContain("IPilldueApp", text, StringComparison.Ordinal);
            Assert.DoesNotContain("MessageBox", text, StringComparison.Ordinal);
            Assert.DoesNotContain("RoutedEventArgs", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Click=", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Loaded=", xaml, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Desktop_host_builds_the_main_view_model_and_does_not_own_screen_logic()
    {
        var host = ReadRepoFile("src/Pilldue.UI.Desktop/App.xaml.cs");
        Assert.Contains("PilldueComposition.CreateDefaultSessionAsync", host, StringComparison.Ordinal);
        Assert.Contains("UiLocalizer.Apply", host, StringComparison.Ordinal);
        Assert.Contains("WpfTheme.Apply", host, StringComparison.Ordinal);
        Assert.Contains("new WpfUiInteraction()", host, StringComparison.Ordinal);
        Assert.Contains("new MainViewModel(_session.App, interaction)", host, StringComparison.Ordinal);
        Assert.Contains("new MainWindow", host, StringComparison.Ordinal);
        Assert.DoesNotContain("ListMedicationsAsync", host, StringComparison.Ordinal);
        Assert.DoesNotContain("AddMedicationAsync", host, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveConfigAsync", host, StringComparison.Ordinal);
        Assert.DoesNotContain("RefillAsync", host, StringComparison.Ordinal);

        var theme = ReadRepoFile("src/Pilldue.UI.Wpf/WpfTheme.cs");
        Assert.Contains("ThemeMode.Light", theme, StringComparison.Ordinal);
        Assert.Contains("ThemeMode.Dark", theme, StringComparison.Ordinal);
        Assert.Contains("ThemeMode.System", theme, StringComparison.Ordinal);

        var appXaml = ReadRepoFile("src/Pilldue.UI.Desktop/App.xaml");
        Assert.Contains("ThemeMode=\"System\"", appXaml, StringComparison.Ordinal);
    }

    [Fact]
    public void Mvvm_toolkit_is_referenced_only_from_the_wpf_screen_project()
    {
        var repo = RepoRoot();
        var projects = Directory.EnumerateFiles(repo, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !IsBuildOutput(path))
            .ToList();
        Assert.Contains(projects, path => path.EndsWith("Pilldue.UI.Wpf.csproj", StringComparison.OrdinalIgnoreCase));

        foreach (var project in projects)
        {
            var text = File.ReadAllText(project);
            if (project.EndsWith("Pilldue.UI.Wpf.csproj", StringComparison.OrdinalIgnoreCase))
            {
                Assert.Contains("CommunityToolkit.Mvvm", text, StringComparison.Ordinal);
                continue;
            }

            Assert.DoesNotContain("CommunityToolkit.Mvvm", text, StringComparison.Ordinal);
        }

        foreach (var relative in new[] { "src/Pilldue.Business", "src/Pilldue.Data" })
        {
            foreach (var file in Directory.EnumerateFiles(RepoPath(relative), "*", SearchOption.AllDirectories))
            {
                if (IsBuildOutput(file))
                {
                    continue;
                }

                var text = File.ReadAllText(file);
                Assert.DoesNotContain("CommunityToolkit", text, StringComparison.Ordinal);
                Assert.DoesNotContain("UseWPF", text, StringComparison.Ordinal);
                Assert.DoesNotContain("PresentationFramework", text, StringComparison.Ordinal);
                Assert.DoesNotContain("System.Windows", text, StringComparison.Ordinal);
            }
        }
    }

    private static List<string> WindowCodeBehindFiles() =>
        Directory.EnumerateFiles(RepoPath("src/Pilldue.UI.Wpf"), "*Window.xaml.cs", SearchOption.AllDirectories)
            .Where(path => !IsBuildOutput(path))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();

    private static bool IsBuildOutput(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);

    private static readonly Regex ConstructorOnly = new(
        @"public \w+\(\)\s*\{\s*InitializeComponent\(\);\s*\}",
        RegexOptions.CultureInvariant);

    private static string ReadRepoFile(string relativePath) =>
        File.ReadAllText(RepoPath(relativePath));

    private static string RepoPath(string relativePath) =>
        Path.Combine(RepoRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar));

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Pilldue.slnx")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return dir.FullName;
    }
}
