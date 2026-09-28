using System.Windows;
using Pilldue.Business;

namespace Pilldue.UI.Desktop;

public partial class AddMedicationWindow : Window
{
    public Medication? Result { get; private set; }

    public AddMedicationWindow()
    {
        InitializeComponent();
        RxStartBox.Text = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show(this, "Name is required.", "Add medication", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                "Check numeric fields and use yyyy-MM-dd for the prescription start.",
                "Add medication",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        Result = new Medication
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
        DialogResult = true;
        Close();
    }
}
