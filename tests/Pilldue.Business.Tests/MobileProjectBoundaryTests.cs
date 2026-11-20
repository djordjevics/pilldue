namespace Pilldue.Business.Tests;

public class MobileProjectBoundaryTests
{
    [Fact]
    public void Mobile_screens_reference_business_only()
    {
        var csproj = ReadRepoFile("src/Pilldue.UI.Mobile/Pilldue.UI.Mobile.csproj");
        Assert.Contains("net10.0-android", csproj, StringComparison.Ordinal);
        Assert.Contains("net10.0-ios", csproj, StringComparison.Ordinal);
        Assert.Contains("Pilldue.Business", csproj, StringComparison.Ordinal);
        Assert.DoesNotContain("Pilldue.Data", csproj, StringComparison.Ordinal);
        Assert.DoesNotContain("EntityFramework", csproj, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQLite", csproj, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("System.Data", csproj, StringComparison.Ordinal);
        Assert.DoesNotContain("SqlClient", csproj, StringComparison.OrdinalIgnoreCase);

        foreach (var file in SourceFiles("src/Pilldue.UI.Mobile"))
        {
            var text = File.ReadAllText(file);
            Assert.DoesNotContain("Pilldue.Data", text, StringComparison.Ordinal);
            Assert.DoesNotContain("EntityFramework", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("SQLite", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Microsoft.Data.Sqlite", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("System.Data", text, StringComparison.Ordinal);
            Assert.DoesNotContain("SqlClient", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("RefillCalendarRules", text, StringComparison.Ordinal);
        }

        var screen = ReadRepoFile("src/Pilldue.UI.Mobile/StartupPage.xaml.cs");
        Assert.Contains("StartupPage(IPilldueApp app)", screen, StringComparison.Ordinal);
        Assert.Contains("GetConfigAsync", screen, StringComparison.Ordinal);
    }

    [Fact]
    public void Mobile_host_opens_an_app_data_session_and_passes_the_app()
    {
        var csproj = ReadRepoFile("src/Pilldue.UI.MobileHost/Pilldue.UI.MobileHost.csproj");
        Assert.Contains("net10.0-android", csproj, StringComparison.Ordinal);
        Assert.Contains("net10.0-ios", csproj, StringComparison.Ordinal);
        Assert.Contains("Pilldue.Data", csproj, StringComparison.Ordinal);
        Assert.Contains("Pilldue.UI.Mobile", csproj, StringComparison.Ordinal);
        Assert.Contains("Pilldue.Business", csproj, StringComparison.Ordinal);

        var combined = string.Join(
            '\n',
            SourceFiles("src/Pilldue.UI.MobileHost").Select(File.ReadAllText));
        Assert.Contains("PilldueComposition.CreateSessionAsync", combined, StringComparison.Ordinal);
        Assert.Contains("Microsoft.Maui.Storage.FileSystem.Current.AppDataDirectory", combined, StringComparison.Ordinal);
        Assert.Contains("SqliteDatabasePaths.FileName", combined, StringComparison.Ordinal);
        Assert.Contains("SqliteDatabasePaths.ConfigFileName", combined, StringComparison.Ordinal);
        Assert.Contains("new StartupPage(session.App)", combined, StringComparison.Ordinal);
        Assert.Contains("_closing", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("CreateDefaultSessionAsync", combined, StringComparison.Ordinal);
        var sceneDelegate = ReadRepoFile("src/Pilldue.UI.MobileHost/Platforms/iOS/SceneDelegate.cs");
        Assert.Contains("[Register(\"SceneDelegate\")]", sceneDelegate, StringComparison.Ordinal);
        Assert.Contains("MauiUISceneDelegate", sceneDelegate, StringComparison.Ordinal);
        var plist = ReadRepoFile("src/Pilldue.UI.MobileHost/Platforms/iOS/Info.plist");
        Assert.Contains("SceneDelegate", plist, StringComparison.Ordinal);
        Assert.DoesNotContain("UserProfile", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("LocalApplicationData", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("Environment.SpecialFolder", combined, StringComparison.Ordinal);
    }

    private static IEnumerable<string> SourceFiles(string relativeDir)
    {
        foreach (var file in Directory.EnumerateFiles(RepoPath(relativeDir), "*", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                || file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            yield return file;
        }
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
