using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public partial class SkipDoseViewModel : DialogViewModel
{
    private readonly IUiInteraction _interaction;

    public SkipDoseViewModel(Medication medication, IUiInteraction interaction, Func<DateOnly> today)
    {
        ArgumentNullException.ThrowIfNull(medication);
        ArgumentNullException.ThrowIfNull(interaction);
        ArgumentNullException.ThrowIfNull(today);
        _interaction = interaction;
        MedicationName = medication.Name;
        PillsText = medication.DailyDosagePills > 0
            ? medication.DailyDosagePills.ToString()
            : string.Empty;
        DateText = today().ToString("yyyy-MM-dd");
    }

    public string MedicationName { get; }

    public int PillsReturned { get; private set; }

    public DateOnly Date { get; private set; }

    [ObservableProperty]
    private string _pillsText = string.Empty;

    [ObservableProperty]
    private string _dateText = string.Empty;

    [RelayCommand]
    private void Confirm()
    {
        if (!int.TryParse(PillsText.Trim(), out var count) || count < 1)
        {
            _interaction.ShowMessage(
                UiLocalizer.Get("Common.MustBeAtLeast1"),
                UiLocalizer.Get("Skip.Title"),
                UiMessageKind.Warning);
            return;
        }

        if (!DateOnly.TryParseExact(DateText.Trim(), "yyyy-MM-dd", out var date))
        {
            _interaction.ShowMessage(
                UiLocalizer.Get("Common.UseDateFormat"),
                UiLocalizer.Get("Skip.Title"),
                UiMessageKind.Warning);
            return;
        }

        PillsReturned = count;
        Date = date;
        CloseResult = true;
    }
}
