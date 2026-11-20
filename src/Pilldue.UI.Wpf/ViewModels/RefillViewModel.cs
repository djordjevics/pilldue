using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public partial class RefillViewModel : DialogViewModel
{
    private readonly IUiInteraction _interaction;

    public RefillViewModel(Medication medication, IUiInteraction interaction)
    {
        ArgumentNullException.ThrowIfNull(medication);
        ArgumentNullException.ThrowIfNull(interaction);
        _interaction = interaction;
        MedicationName = medication.Name;
        PackageCountText = medication.PrescribedPackageCount > 0
            ? medication.PrescribedPackageCount.ToString()
            : string.Empty;
    }

    public string MedicationName { get; }

    public int PackageCount { get; private set; }

    [ObservableProperty]
    private string _packageCountText = string.Empty;

    [RelayCommand]
    private void Confirm()
    {
        if (!int.TryParse(PackageCountText.Trim(), out var count) || count < 1)
        {
            _interaction.ShowMessage(
                UiLocalizer.Get("Common.MustBeAtLeast1"),
                UiLocalizer.Get("Refill.Title"),
                UiMessageKind.Warning);
            return;
        }

        PackageCount = count;
        CloseResult = true;
    }
}
