using System.Windows;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public partial class FlagMissedDoseWindow : Window
{
    public DateOnly Date { get; private set; }

    public FlagMissedDoseWindow(Medication medication)
    {
        ArgumentNullException.ThrowIfNull(medication);
        InitializeComponent();
        MedicationText.Text = medication.Name;
        DateBox.Text = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        if (!DateOnly.TryParseExact(DateBox.Text.Trim(), "yyyy-MM-dd", out var date))
        {
            MessageBox.Show(
                this,
                UiLocalizer.Get("Common.UseDateFormat"),
                UiLocalizer.Get("Missed.Flag"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        Date = date;
        DialogResult = true;
        Close();
    }
}
