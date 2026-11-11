using System.Windows;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public partial class AddMedicationWindow : Window
{
    private readonly Medication? _existing;

    public Medication? Result { get; private set; }

    public AddMedicationWindow(Medication? existing = null)
    {
        _existing = existing;
        InitializeComponent();
        if (existing is null)
        {
            RxStartBox.Text = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");
            return;
        }

        Title = UiLocalizer.Get("Med.EditTitle");
        NameBox.Text = existing.Name;
        PackageSizeBox.Text = existing.PackageSizePills.ToString();
        PrescribedBox.Text = existing.PrescribedPackageCount.ToString();
        DosageBox.Text = existing.DailyDosagePills.ToString();
        IntervalBox.Text = existing.DoseIntervalDays.ToString();
        StockBox.Text = existing.CurrentStockPills.ToString();
        RxStartBox.Text = existing.PrescriptionStartDate.ToString("yyyy-MM-dd");
        DurationBox.Text = existing.PrescriptionDurationMonths.ToString();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        var title = _existing is null
            ? UiLocalizer.Get("Med.AddTitle")
            : UiLocalizer.Get("Med.EditTitle");

        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show(
                this,
                UiLocalizer.Get("Common.NameRequired"),
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(PackageSizeBox.Text.Trim(), out var packageSize) || packageSize < 1
            || !int.TryParse(PrescribedBox.Text.Trim(), out var prescribed) || prescribed < 1
            || !int.TryParse(DosageBox.Text.Trim(), out var dosage) || dosage < 1
            || !int.TryParse(IntervalBox.Text.Trim(), out var interval) || interval < 1
            || !int.TryParse(StockBox.Text.Trim(), out var stock) || stock < 0
            || !int.TryParse(DurationBox.Text.Trim(), out var duration) || duration < 1
            || !DateOnly.TryParseExact(RxStartBox.Text.Trim(), "yyyy-MM-dd", out var rxStart))
        {
            MessageBox.Show(
                this,
                UiLocalizer.Get("Desktop.AddInvalid"),
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var saved = new Medication
        {
            Name = NameBox.Text.Trim(),
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
        DialogResult = true;
        Close();
    }
}
