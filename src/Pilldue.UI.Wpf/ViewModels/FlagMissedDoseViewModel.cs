using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public partial class FlagMissedDoseViewModel : DialogViewModel
{
    private readonly IUiInteraction _interaction;

    public FlagMissedDoseViewModel(Medication medication, IUiInteraction interaction, Func<DateOnly> today)
    {
        ArgumentNullException.ThrowIfNull(medication);
        ArgumentNullException.ThrowIfNull(interaction);
        ArgumentNullException.ThrowIfNull(today);
        _interaction = interaction;
        MedicationName = medication.Name;
        DateText = today().ToString("yyyy-MM-dd");
    }

    public string MedicationName { get; }

    public DateOnly Date { get; private set; }

    [ObservableProperty]
    private string _dateText = string.Empty;

    [RelayCommand]
    private void Confirm()
    {
        if (!DateOnly.TryParseExact(DateText.Trim(), "yyyy-MM-dd", out var date))
        {
            _interaction.ShowMessage(
                UiLocalizer.Get("Common.UseDateFormat"),
                UiLocalizer.Get("Missed.Flag"),
                UiMessageKind.Warning);
            return;
        }

        Date = date;
        CloseResult = true;
    }
}
