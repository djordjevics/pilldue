using Pilldue.Business;
using Pilldue.UI.Localization;
using Pilldue.UI.Wpf;

namespace Pilldue.UI.Wpf.Tests;

public sealed class MainViewModelTests : IDisposable
{
    public MainViewModelTests()
    {
        UiLocalizer.Apply(new AppConfig { UiLanguage = AppConfig.EnglishLanguage });
    }

    public void Dispose()
    {
        UiLocalizer.Apply(new AppConfig { UiLanguage = AppConfig.EnglishLanguage });
    }

    [Fact]
    public async Task Refresh_lists_medications_and_reports_the_count()
    {
        var session = new TestSession();
        await session.App.AddMedicationAsync(TestSession.Sample());
        var main = new MainViewModel(session.App, new FakeUiInteraction(), () => TestSession.Today);

        await main.Loaded;

        var listed = Assert.Single(main.Medications);
        Assert.Equal("Aspirin", listed.Name);
        Assert.Equal(UiLocalizer.Format("Desktop.Count", 1), main.StatusText);

        var empty = new MainViewModel(new TestSession().App, new FakeUiInteraction(), () => TestSession.Today);
        await empty.Loaded;
        Assert.Empty(empty.Medications);
        Assert.Equal(UiLocalizer.Get("Desktop.Empty"), empty.StatusText);
    }

    [Fact]
    public async Task Add_saves_and_cancel_does_not()
    {
        var session = new TestSession();
        var interaction = new FakeUiInteraction();
        var main = new MainViewModel(session.App, interaction, () => TestSession.Today);
        await main.Loaded;

        interaction.Add = vm =>
        {
            vm.Name = "Aspirin";
            vm.CancelCommand.Execute(null);
            return vm.CloseResult == true;
        };
        await main.AddCommand.ExecuteAsync(null);
        Assert.Empty(await session.App.ListMedicationsAsync());

        interaction.Add = vm =>
        {
            vm.Name = "Aspirin";
            vm.SaveCommand.Execute(null);
            return vm.CloseResult == true;
        };
        await main.AddCommand.ExecuteAsync(null);

        var saved = Assert.Single(await session.App.ListMedicationsAsync());
        Assert.Equal("Aspirin", saved.Name);
        Assert.Equal(28, saved.PackageSizePills);
        Assert.Equal(0, saved.CurrentStockPills);
        Assert.Equal(TestSession.Today, saved.PrescriptionStartDate);
    }

    [Fact]
    public async Task Edit_keeps_the_selected_medication_id()
    {
        var session = new TestSession();
        var original = TestSession.Sample();
        original.RefillDayOfMonthOverride = 9;
        await session.App.AddMedicationAsync(original);
        var interaction = new FakeUiInteraction();
        var main = new MainViewModel(session.App, interaction, () => TestSession.Today);
        await main.Loaded;
        main.SelectedMedication = Assert.Single(main.Medications);

        interaction.Add = vm =>
        {
            Assert.Equal("Aspirin", vm.Name);
            vm.Name = "Renamed";
            vm.SaveCommand.Execute(null);
            Assert.NotNull(vm.Result);
            Assert.Equal(original.Id, vm.Result.Id);
            Assert.Equal(9, vm.Result.RefillDayOfMonthOverride);
            return vm.CloseResult == true;
        };
        await main.EditCommand.ExecuteAsync(null);

        var updated = Assert.Single(await session.App.ListMedicationsAsync());
        Assert.Equal(original.Id, updated.Id);
        Assert.Equal("Renamed", updated.Name);
        Assert.Equal(9, updated.RefillDayOfMonthOverride);
        Assert.Equal(10, updated.CurrentStockPills);
    }

    [Fact]
    public async Task Refill_cancel_does_not_write()
    {
        var (session, interaction, main) = await SelectedAsync();
        interaction.Refill = vm =>
        {
            vm.CancelCommand.Execute(null);
            return vm.CloseResult == true;
        };

        await main.RefillCommand.ExecuteAsync(null);

        var stored = Assert.Single(await session.App.ListMedicationsAsync());
        Assert.Equal(10, stored.CurrentStockPills);
        Assert.Empty(await session.Refills.ListByMedicationAsync(stored.Id));
    }

    [Fact]
    public async Task Remove_deletes_only_after_confirm()
    {
        var (session, interaction, main) = await SelectedAsync();
        var selected = main.SelectedMedication!;

        main.SelectedMedication = null;
        await main.RemoveCommand.ExecuteAsync(null);
        Assert.Equal(0, interaction.ConfirmCalls);
        Assert.Single(await session.App.ListMedicationsAsync());

        main.SelectedMedication = selected;
        interaction.ConfirmResult = false;
        await main.RemoveCommand.ExecuteAsync(null);
        Assert.Equal(UiLocalizer.Format("Med.RemoveConfirm", "Aspirin"), interaction.LastConfirmMessage);
        Assert.Single(await session.App.ListMedicationsAsync());

        interaction.ConfirmResult = true;
        await main.RemoveCommand.ExecuteAsync(null);
        Assert.Empty(await session.App.ListMedicationsAsync());
    }

    [Fact]
    public async Task Skip_returns_pills_on_the_chosen_date()
    {
        var (session, interaction, main) = await SelectedAsync();
        interaction.Skip = vm =>
        {
            vm.CancelCommand.Execute(null);
            return vm.CloseResult == true;
        };
        await main.SkipCommand.ExecuteAsync(null);
        Assert.Empty(await session.Skips.ListByMedicationAsync(main.SelectedMedication!.Id));

        interaction.Skip = vm =>
        {
            vm.PillsText = "2";
            vm.DateText = "2026-05-04";
            vm.ConfirmCommand.Execute(null);
            return vm.CloseResult == true;
        };
        await main.SkipCommand.ExecuteAsync(null);

        var stored = Assert.Single(await session.App.ListMedicationsAsync());
        Assert.Equal(12, stored.CurrentStockPills);
        var skip = Assert.Single(await session.Skips.ListByMedicationAsync(stored.Id));
        Assert.Equal(2, skip.PillsReturned);
        Assert.Equal(new DateOnly(2026, 5, 4), skip.Date);
        Assert.Equal(UiLocalizer.Format("Skip.Done", "Aspirin", 10, 12, 2), main.StatusText);
    }

    [Fact]
    public async Task Settings_save_refreshes_open_captions_and_cancel_does_not()
    {
        var session = new TestSession();
        var interaction = new FakeUiInteraction();
        var main = new MainViewModel(session.App, interaction, () => TestSession.Today);
        await main.Loaded;
        Assert.Equal("Refresh", main.RefreshCaption);
        Assert.Equal("Name", main.NameHeader);

        interaction.Settings = vm =>
        {
            vm.SelectedLanguage = vm.Languages.Single(choice => choice.Value == AppConfig.SerbianLanguage);
            vm.SelectedTheme = vm.Themes.Single(choice => choice.Value == AppConfig.ThemeDark);
            vm.CancelCommand.Execute(null);
            return vm.CloseResult == true;
        };
        await main.SettingsCommand.ExecuteAsync(null);
        Assert.Equal("Refresh", main.RefreshCaption);
        Assert.Equal("Name", main.NameHeader);
        Assert.Equal(AppConfig.EnglishLanguage, UiLocalizer.Language);
        var unchanged = await session.App.GetConfigAsync();
        Assert.Equal(AppConfig.EnglishLanguage, unchanged.UiLanguage);
        Assert.Equal(AppConfig.ThemeLight, unchanged.UiTheme);

        interaction.Settings = vm =>
        {
            vm.SelectedLanguage = vm.Languages.Single(choice => choice.Value == AppConfig.SerbianLanguage);
            vm.SelectedTheme = vm.Themes.Single(choice => choice.Value == AppConfig.ThemeDark);
            vm.SaveCommand.ExecuteAsync(null).GetAwaiter().GetResult();
            return vm.CloseResult == true;
        };
        await main.SettingsCommand.ExecuteAsync(null);

        Assert.Equal("Osvezi", main.RefreshCaption);
        Assert.Equal("Naziv", main.NameHeader);
        Assert.Equal("Doza", main.DoseHeader);
        Assert.Equal(AppConfig.SerbianLanguage, UiLocalizer.Language);
        var saved = await session.App.GetConfigAsync();
        Assert.Equal(AppConfig.SerbianLanguage, saved.UiLanguage);
        Assert.Equal(AppConfig.ThemeDark, saved.UiTheme);
    }

    private static async Task<(TestSession Session, FakeUiInteraction Interaction, MainViewModel Main)> SelectedAsync()
    {
        var session = new TestSession();
        await session.App.AddMedicationAsync(TestSession.Sample());
        var interaction = new FakeUiInteraction();
        var main = new MainViewModel(session.App, interaction, () => TestSession.Today);
        await main.Loaded;
        main.SelectedMedication = Assert.Single(main.Medications);
        return (session, interaction, main);
    }
}
