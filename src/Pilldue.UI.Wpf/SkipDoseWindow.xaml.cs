using System.Windows;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public partial class SkipDoseWindow : Window
{
    public int PillsReturned { get; private set; }

    public DateOnly Date { get; private set; }

    public SkipDoseWindow(Medication medication)
    {
        ArgumentNullException.ThrowIfNull(medication);
        InitializeComponent();
        MedicationText.Text = medication.Name;
        if (medication.DailyDosagePills > 0)
        {
            PillsBox.Text = medication.DailyDosagePills.ToString();
        }

        DateBox.Text = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(PillsBox.Text.Trim(), out var count) || count < 1)
        {
            MessageBox.Show(
                this,
                UiLocalizer.Get("Common.MustBeAtLeast1"),
                UiLocalizer.Get("Skip.Title"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (!DateOnly.TryParseExact(DateBox.Text.Trim(), "yyyy-MM-dd", out var date))
        {
            MessageBox.Show(
                this,
                UiLocalizer.Get("Common.UseDateFormat"),
                UiLocalizer.Get("Skip.Title"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        PillsReturned = count;
        Date = date;
        DialogResult = true;
        Close();
    }
}
