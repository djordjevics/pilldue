using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public sealed class MissedDoseRow
{
    public required string Date { get; init; }

    public required string Medication { get; init; }
}

public partial class MissedDoseViewModel : ObservableObject
{
    private readonly IPilldueApp _app;
    private readonly IUiInteraction _interaction;
    private readonly Medication? _medication;
    private readonly Func<DateOnly> _today;

    public MissedDoseViewModel(
        IPilldueApp app,
        IUiInteraction interaction,
        Medication? medication,
        Func<DateOnly> today)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(interaction);
        ArgumentNullException.ThrowIfNull(today);
        _app = app;
        _interaction = interaction;
        _medication = medication;
        _today = today;
        Loaded = LoadAsync();
    }

    public Task Loaded { get; }

    public bool Flagged { get; private set; }

    public DateOnly? FlaggedDate { get; private set; }

    [ObservableProperty]
    private IReadOnlyList<MissedDoseRow> _rows = Array.Empty<MissedDoseRow>();

    [ObservableProperty]
    private bool _isEmpty;

    [RelayCommand]
    private async Task FlagAsync()
    {
        if (_medication is null)
        {
            _interaction.ShowMessage(
                UiLocalizer.Get("Desktop.SelectFirst"),
                UiLocalizer.Get("Missed.Flag"),
                UiMessageKind.Information);
            return;
        }

        var flag = new FlagMissedDoseViewModel(_medication, _interaction, _today);
        if (!_interaction.ShowFlagMissedDose(flag))
        {
            return;
        }

        try
        {
            await _app.FlagMissedDoseAsync(_medication.Id, flag.Date);
            Flagged = true;
            FlaggedDate = flag.Date;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _interaction.ShowMessage(ex.Message, UiLocalizer.Get("Missed.Title"), UiMessageKind.Error);
        }
    }

    private async Task LoadAsync()
    {
        try
        {
            var missed = await _app.ListMissedDosesAsync();
            var medications = await _app.ListMedicationsAsync();
            var names = medications.ToDictionary(medication => medication.Id, medication => medication.Name);
            Rows = missed
                .Select(item => new MissedDoseRow
                {
                    Date = item.Date.ToString("yyyy-MM-dd"),
                    Medication = names.TryGetValue(item.MedicationId, out var name)
                        ? name
                        : item.MedicationId.ToString(),
                })
                .ToList();
            IsEmpty = missed.Count == 0;
        }
        catch (Exception ex)
        {
            _interaction.ShowMessage(ex.Message, UiLocalizer.Get("Missed.Title"), UiMessageKind.Error);
        }
    }
}
