using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public partial class MainViewModel : ObservableObject
{
    private readonly IPilldueApp _app;
    private readonly IUiInteraction _interaction;
    private readonly Func<DateOnly> _today;

    public MainViewModel(IPilldueApp app, IUiInteraction interaction, Func<DateOnly>? today = null)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(interaction);
        _app = app;
        _interaction = interaction;
        _today = today ?? (() => DateOnly.FromDateTime(DateTime.Today));
        ApplyCaptions();
        Loaded = RefreshAsync();
    }

    public Task Loaded { get; }

    [ObservableProperty]
    private IReadOnlyList<Medication> _medications = Array.Empty<Medication>();

    [ObservableProperty]
    private Medication? _selectedMedication;

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    private string _refreshCaption = string.Empty;

    [ObservableProperty]
    private string _addCaption = string.Empty;

    [ObservableProperty]
    private string _editCaption = string.Empty;

    [ObservableProperty]
    private string _refillCaption = string.Empty;

    [ObservableProperty]
    private string _removeCaption = string.Empty;

    [ObservableProperty]
    private string _coverageCaption = string.Empty;

    [ObservableProperty]
    private string _skipCaption = string.Empty;

    [ObservableProperty]
    private string _missedCaption = string.Empty;

    [ObservableProperty]
    private string _calendarCaption = string.Empty;

    [ObservableProperty]
    private string _settingsCaption = string.Empty;

    [ObservableProperty]
    private string _nameHeader = string.Empty;

    [ObservableProperty]
    private string _stockHeader = string.Empty;

    [ObservableProperty]
    private string _doseHeader = string.Empty;

    [ObservableProperty]
    private string _intervalHeader = string.Empty;

    [ObservableProperty]
    private string _packageHeader = string.Empty;

    [RelayCommand]
    private Task Refresh() => RefreshAsync();

    [RelayCommand]
    private async Task AddAsync()
    {
        var dialog = new AddMedicationViewModel(_interaction, _today);
        if (!_interaction.ShowAddMedication(dialog) || dialog.Result is null)
        {
            return;
        }

        try
        {
            await _app.AddMedicationAsync(dialog.Result);
            StatusText = UiLocalizer.Format("Desktop.Added", dialog.Result.Name);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _interaction.ShowMessage(ex.Message, UiLocalizer.Get("Med.AddTitle"), UiMessageKind.Error);
        }
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        if (SelectedMedication is not Medication selected)
        {
            _interaction.ShowMessage(
                UiLocalizer.Get("Desktop.SelectFirst"),
                UiLocalizer.Get("Med.EditTitle"),
                UiMessageKind.Information);
            return;
        }

        var dialog = new AddMedicationViewModel(_interaction, _today, selected);
        if (!_interaction.ShowAddMedication(dialog) || dialog.Result is null)
        {
            return;
        }

        try
        {
            await _app.UpdateMedicationAsync(dialog.Result);
            StatusText = UiLocalizer.Format("Desktop.Updated", dialog.Result.Name);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _interaction.ShowMessage(ex.Message, UiLocalizer.Get("Med.EditTitle"), UiMessageKind.Error);
        }
    }

    [RelayCommand]
    private async Task RefillAsync()
    {
        if (SelectedMedication is not Medication selected)
        {
            _interaction.ShowMessage(
                UiLocalizer.Get("Desktop.SelectFirst"),
                UiLocalizer.Get("Refill.Title"),
                UiMessageKind.Information);
            return;
        }

        var dialog = new RefillViewModel(selected, _interaction);
        if (!_interaction.ShowRefill(dialog) || dialog.PackageCount < 1)
        {
            return;
        }

        try
        {
            await _app.RefillAsync(selected.Id, dialog.PackageCount, _today());
            await RefreshAsync();
            StatusText = UiLocalizer.Format("Desktop.Refilled", selected.Name, dialog.PackageCount);
        }
        catch (Exception ex)
        {
            _interaction.ShowMessage(ex.Message, UiLocalizer.Get("Refill.Title"), UiMessageKind.Error);
        }
    }

    [RelayCommand]
    private async Task RemoveAsync()
    {
        if (SelectedMedication is not Medication selected)
        {
            _interaction.ShowMessage(
                UiLocalizer.Get("Desktop.SelectFirst"),
                UiLocalizer.Get("Med.RemoveTitle"),
                UiMessageKind.Information);
            return;
        }

        if (!_interaction.Confirm(
                UiLocalizer.Format("Med.RemoveConfirm", selected.Name),
                UiLocalizer.Get("Med.RemoveTitle")))
        {
            return;
        }

        try
        {
            await _app.RemoveMedicationAsync(selected.Id);
            StatusText = UiLocalizer.Format("Desktop.Removed", selected.Name);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _interaction.ShowMessage(ex.Message, UiLocalizer.Get("Med.RemoveTitle"), UiMessageKind.Error);
        }
    }

    [RelayCommand]
    private void Coverage()
    {
        var dialog = new StockCoverageViewModel(_app, _interaction, _today);
        _interaction.ShowStockCoverage(dialog);
    }

    [RelayCommand]
    private async Task SkipAsync()
    {
        if (SelectedMedication is not Medication selected)
        {
            _interaction.ShowMessage(
                UiLocalizer.Get("Desktop.SelectFirst"),
                UiLocalizer.Get("Skip.Title"),
                UiMessageKind.Information);
            return;
        }

        var dialog = new SkipDoseViewModel(selected, _interaction, _today);
        if (!_interaction.ShowSkipDose(dialog) || dialog.PillsReturned < 1)
        {
            return;
        }

        try
        {
            var before = selected.CurrentStockPills;
            await _app.SkipDoseAsync(selected.Id, dialog.PillsReturned, dialog.Date);
            await RefreshAsync();
            var after = Medications.FirstOrDefault(medication => medication.Id == selected.Id)?.CurrentStockPills ?? before;
            StatusText = UiLocalizer.Format("Skip.Done", selected.Name, before, after, dialog.PillsReturned);
        }
        catch (Exception ex)
        {
            _interaction.ShowMessage(ex.Message, UiLocalizer.Get("Skip.Title"), UiMessageKind.Error);
        }
    }

    [RelayCommand]
    private async Task MissedAsync()
    {
        var selected = SelectedMedication;
        var dialog = new MissedDoseViewModel(_app, _interaction, selected, _today);
        _interaction.ShowMissedDoses(dialog);
        if (!dialog.Flagged || selected is null || dialog.FlaggedDate is null)
        {
            return;
        }

        try
        {
            await RefreshAsync();
            StatusText = UiLocalizer.Format(
                "Missed.Flagged",
                selected.Name,
                dialog.FlaggedDate.Value.ToString("yyyy-MM-dd"));
        }
        catch (Exception ex)
        {
            _interaction.ShowMessage(ex.Message, UiLocalizer.Get("Missed.Title"), UiMessageKind.Error);
        }
    }

    [RelayCommand]
    private void Calendar()
    {
        var dialog = new CalendarViewModel(_app, _interaction, _today);
        _interaction.ShowCalendar(dialog);
    }

    [RelayCommand]
    private async Task SettingsAsync()
    {
        try
        {
            var config = await _app.GetConfigAsync();
            var dialog = new SettingsViewModel(_app, _interaction, config);
            if (!_interaction.ShowSettings(dialog))
            {
                return;
            }

            ApplyCaptions();
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _interaction.ShowMessage(ex.Message, UiLocalizer.Get("Desktop.Settings"), UiMessageKind.Error);
        }
    }

    private void ApplyCaptions()
    {
        RefreshCaption = UiLocalizer.Get("Desktop.Refresh");
        AddCaption = UiLocalizer.Get("Desktop.Add");
        EditCaption = UiLocalizer.Get("Desktop.Edit");
        RefillCaption = UiLocalizer.Get("Desktop.Refill");
        RemoveCaption = UiLocalizer.Get("Desktop.Remove");
        CoverageCaption = UiLocalizer.Get("Desktop.Coverage");
        SkipCaption = UiLocalizer.Get("Desktop.Skip");
        MissedCaption = UiLocalizer.Get("Desktop.Missed");
        CalendarCaption = UiLocalizer.Get("Desktop.Calendar");
        SettingsCaption = UiLocalizer.Get("Desktop.Settings");
        NameHeader = UiLocalizer.Get("List.ColName");
        StockHeader = UiLocalizer.Get("List.ColStock");
        DoseHeader = UiLocalizer.Get("Desktop.ColDose");
        IntervalHeader = UiLocalizer.Get("Desktop.ColInterval");
        PackageHeader = UiLocalizer.Get("List.ColPackage");
    }

    private async Task RefreshAsync()
    {
        try
        {
            var medications = await _app.ListMedicationsAsync();
            Medications = medications;
            StatusText = medications.Count == 0
                ? UiLocalizer.Get("Desktop.Empty")
                : UiLocalizer.Format("Desktop.Count", medications.Count);
        }
        catch (Exception ex)
        {
            _interaction.ShowMessage(ex.Message, UiLocalizer.Get("Desktop.Refresh"), UiMessageKind.Error);
        }
    }
}
