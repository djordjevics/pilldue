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

    [Fact]
    public void Coverage_screen_shows_today_query_values()
    {
        var toolbar = ReadRepoFile("src/Pilldue.UI.Wpf/MainWindow.xaml");
        Assert.Contains("Desktop.Coverage", toolbar, StringComparison.Ordinal);
        Assert.Contains("OnCoverageClick", toolbar, StringComparison.Ordinal);

        var main = ReadRepoFile("src/Pilldue.UI.Wpf/MainWindow.xaml.cs");
        Assert.Contains("new StockCoverageWindow(_app)", main, StringComparison.Ordinal);

        var screen = ReadRepoFile("src/Pilldue.UI.Wpf/StockCoverageWindow.xaml.cs");
        Assert.Contains("GetStockCoverageAsync", screen, StringComparison.Ordinal);
        Assert.Contains("ListShortBeforeNextRefillAsync", screen, StringComparison.Ordinal);
        Assert.Contains("ListNeedExtraForSecondRefillAsync", screen, StringComparison.Ordinal);
        Assert.Contains("DateOnly.FromDateTime(DateTime.Today)", screen, StringComparison.Ordinal);
        Assert.Contains("result.PillsShort", screen, StringComparison.Ordinal);
        Assert.Contains("result.PackagesToBuy", screen, StringComparison.Ordinal);
        Assert.Contains("result.ExtraPackages", screen, StringComparison.Ordinal);
        Assert.DoesNotContain("RefillCalendarRules", screen, StringComparison.Ordinal);
        Assert.DoesNotContain("StockCoverageQuery", screen, StringComparison.Ordinal);
        Assert.DoesNotContain("ExtraPackagesQuery", screen, StringComparison.Ordinal);
    }

    [Fact]
    public void Calendar_screen_shows_dates_from_get_calendar()
    {
        var toolbar = ReadRepoFile("src/Pilldue.UI.Wpf/MainWindow.xaml");
        Assert.Contains("Desktop.Calendar", toolbar, StringComparison.Ordinal);
        Assert.Contains("OnCalendarClick", toolbar, StringComparison.Ordinal);

        var main = ReadRepoFile("src/Pilldue.UI.Wpf/MainWindow.xaml.cs");
        Assert.Contains("new CalendarWindow(_app)", main, StringComparison.Ordinal);

        var screen = ReadRepoFile("src/Pilldue.UI.Wpf/CalendarWindow.xaml.cs");
        Assert.Contains("GetCalendarAsync", screen, StringComparison.Ordinal);
        Assert.Contains("DateOnly.FromDateTime(DateTime.Today)", screen, StringComparison.Ordinal);
        Assert.Contains("entry.FirstRefillDate", screen, StringComparison.Ordinal);
        Assert.Contains("entry.SecondRefillDate", screen, StringComparison.Ordinal);
        Assert.Contains("entry.StockOutDates", screen, StringComparison.Ordinal);
        Assert.Contains("entry.PrescriptionEndDate", screen, StringComparison.Ordinal);
        Assert.DoesNotContain("CalendarProjection", screen, StringComparison.Ordinal);
        Assert.DoesNotContain("RefillCalendarRules", screen, StringComparison.Ordinal);
    }

    [Fact]
    public void Skip_and_missed_dose_go_through_the_app()
    {
        var toolbar = ReadRepoFile("src/Pilldue.UI.Wpf/MainWindow.xaml");
        Assert.Contains("Desktop.Skip", toolbar, StringComparison.Ordinal);
        Assert.Contains("OnSkipClick", toolbar, StringComparison.Ordinal);
        Assert.Contains("Desktop.Missed", toolbar, StringComparison.Ordinal);
        Assert.Contains("OnMissedClick", toolbar, StringComparison.Ordinal);

        var main = ReadRepoFile("src/Pilldue.UI.Wpf/MainWindow.xaml.cs");
        Assert.Contains("new SkipDoseWindow(selected)", main, StringComparison.Ordinal);
        Assert.Contains("dialog.ShowDialog() != true || dialog.PillsReturned < 1", main, StringComparison.Ordinal);
        Assert.Contains("SkipDoseAsync(selected.Id, dialog.PillsReturned, dialog.Date)", main, StringComparison.Ordinal);
        Assert.Contains("new MissedDoseWindow(_app, selected)", main, StringComparison.Ordinal);

        var skip = ReadRepoFile("src/Pilldue.UI.Wpf/SkipDoseWindow.xaml.cs");
        Assert.Contains("count < 1", skip, StringComparison.Ordinal);
        Assert.Contains("DialogResult = false", skip, StringComparison.Ordinal);
        Assert.DoesNotContain("IPilldueApp", skip, StringComparison.Ordinal);
        Assert.DoesNotContain("SkipDoseAsync", skip, StringComparison.Ordinal);

        var flag = ReadRepoFile("src/Pilldue.UI.Wpf/FlagMissedDoseWindow.xaml.cs");
        Assert.Contains("DialogResult = false", flag, StringComparison.Ordinal);
        Assert.DoesNotContain("IPilldueApp", flag, StringComparison.Ordinal);
        Assert.DoesNotContain("FlagMissedDoseAsync", flag, StringComparison.Ordinal);

        var list = ReadRepoFile("src/Pilldue.UI.Wpf/MissedDoseWindow.xaml.cs");
        Assert.Contains("ListMissedDosesAsync", list, StringComparison.Ordinal);
        Assert.Contains("FlagMissedDoseAsync", list, StringComparison.Ordinal);
        Assert.Contains("flag.ShowDialog() != true", list, StringComparison.Ordinal);
        Assert.DoesNotContain("CurrentStockPills", list, StringComparison.Ordinal);
    }

    [Fact]
    public void Settings_window_saves_language_and_theme_and_cancel_does_not()
    {
        var toolbar = ReadRepoFile("src/Pilldue.UI.Wpf/MainWindow.xaml");
        Assert.Contains("Desktop.Settings", toolbar, StringComparison.Ordinal);
        Assert.Contains("OnSettingsClick", toolbar, StringComparison.Ordinal);

        var main = ReadRepoFile("src/Pilldue.UI.Wpf/MainWindow.xaml.cs");
        Assert.Contains("new SettingsWindow(_app, config)", main, StringComparison.Ordinal);
        Assert.Contains("dialog.ShowDialog() != true", main, StringComparison.Ordinal);
        Assert.Contains("ApplyLocalizedText()", main, StringComparison.Ordinal);

        var dialog = ReadRepoFile("src/Pilldue.UI.Wpf/SettingsWindow.xaml.cs");
        Assert.Contains("SaveConfigAsync", dialog, StringComparison.Ordinal);
        Assert.Contains("DialogResult = false", dialog, StringComparison.Ordinal);
        Assert.Contains("string.Empty", dialog, StringComparison.Ordinal);
        Assert.Contains("AppConfig.EnglishLanguage", dialog, StringComparison.Ordinal);
        Assert.Contains("AppConfig.SerbianLanguage", dialog, StringComparison.Ordinal);
        Assert.Contains("AppConfig.ThemeLight", dialog, StringComparison.Ordinal);
        Assert.Contains("AppConfig.ThemeDark", dialog, StringComparison.Ordinal);
        Assert.Contains("AppConfig.ThemeSystem", dialog, StringComparison.Ordinal);
        Assert.Contains("UiLocalizer.Apply", dialog, StringComparison.Ordinal);
        Assert.Contains("WpfTheme.Apply", dialog, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveConfigAsync", dialog.Split("OnSaveClick")[0], StringComparison.Ordinal);

        var theme = ReadRepoFile("src/Pilldue.UI.Wpf/WpfTheme.cs");
        Assert.Contains("ThemeMode.Light", theme, StringComparison.Ordinal);
        Assert.Contains("ThemeMode.Dark", theme, StringComparison.Ordinal);
        Assert.Contains("ThemeMode.System", theme, StringComparison.Ordinal);

        var host = ReadRepoFile("src/Pilldue.UI.Desktop/App.xaml.cs");
        Assert.Contains("WpfTheme.Apply", host, StringComparison.Ordinal);
        var appXaml = ReadRepoFile("src/Pilldue.UI.Desktop/App.xaml");
        Assert.Contains("ThemeMode=\"System\"", appXaml, StringComparison.Ordinal);
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
