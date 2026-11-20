using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public partial class AddMedicationViewModel : DialogViewModel
{
    private readonly IUiInteraction _interaction;
    private readonly Medication? _existing;

    public AddMedicationViewModel(IUiInteraction interaction, Func<DateOnly> today, Medication? existing = null)
    {
        ArgumentNullException.ThrowIfNull(interaction);
        ArgumentNullException.ThrowIfNull(today);
        _interaction = interaction;
        _existing = existing;
        Title = UiLocalizer.Get(existing is null ? "Med.AddTitle" : "Med.EditTitle");
        PackageSize = "28";
        Prescribed = "1";
        Dosage = "1";
        Interval = "1";
        Stock = "0";
        Duration = "6";
        RxStart = today().ToString("yyyy-MM-dd");
        if (existing is null)
        {
            return;
        }

        Name = existing.Name;
        PackageSize = existing.PackageSizePills.ToString();
        Prescribed = existing.PrescribedPackageCount.ToString();
        Dosage = existing.DailyDosagePills.ToString();
        Interval = existing.DoseIntervalDays.ToString();
        Stock = existing.CurrentStockPills.ToString();
        RxStart = existing.PrescriptionStartDate.ToString("yyyy-MM-dd");
        Duration = existing.PrescriptionDurationMonths.ToString();
    }

    public string Title { get; }

    public Medication? Result { get; private set; }

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _packageSize = string.Empty;

    [ObservableProperty]
    private string _prescribed = string.Empty;

    [ObservableProperty]
    private string _dosage = string.Empty;

    [ObservableProperty]
    private string _interval = string.Empty;

    [ObservableProperty]
    private string _stock = string.Empty;

    [ObservableProperty]
    private string _rxStart = string.Empty;

    [ObservableProperty]
    private string _duration = string.Empty;

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            _interaction.ShowMessage(UiLocalizer.Get("Common.NameRequired"), Title, UiMessageKind.Warning);
            return;
        }

        if (!TryCount(PackageSize, out var packageSize)
            || !TryCount(Prescribed, out var prescribed)
            || !TryCount(Dosage, out var dosage)
            || !TryCount(Interval, out var interval)
            || !TryStock(Stock, out var stock)
            || !TryCount(Duration, out var duration)
            || !TryDate(RxStart, out var rxStart))
        {
            _interaction.ShowMessage(UiLocalizer.Get("Desktop.AddInvalid"), Title, UiMessageKind.Warning);
            return;
        }

        var saved = new Medication
        {
            Name = Name.Trim(),
            PackageSizePills = packageSize,
            PrescribedPackageCount = prescribed,
            DailyDosagePills = dosage,
            DoseIntervalDays = interval,
            CurrentStockPills = stock,
            PrescriptionStartDate = rxStart,
            PrescriptionDurationMonths = duration,
        };
        if (_existing is not null)
        {
            saved.Id = _existing.Id;
            saved.RefillDayOfMonthOverride = _existing.RefillDayOfMonthOverride;
        }

        Result = saved;
        CloseResult = true;
    }

    private static bool TryCount(string? text, out int value)
    {
        value = 0;
        return !string.IsNullOrWhiteSpace(text)
            && int.TryParse(text.Trim(), out value)
            && value >= 1;
    }

    private static bool TryStock(string? text, out int value)
    {
        value = 0;
        return !string.IsNullOrWhiteSpace(text)
            && int.TryParse(text.Trim(), out value)
            && value >= 0;
    }

    private static bool TryDate(string? text, out DateOnly value)
    {
        value = default;
        return !string.IsNullOrWhiteSpace(text)
            && DateOnly.TryParseExact(text.Trim(), "yyyy-MM-dd", out value);
    }
}
