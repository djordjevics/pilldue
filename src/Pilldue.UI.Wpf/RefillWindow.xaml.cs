using System.Windows;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public partial class RefillWindow : Window
{
    public int PackageCount { get; private set; }

    public RefillWindow(Medication medication)
    {
        ArgumentNullException.ThrowIfNull(medication);
        InitializeComponent();
        MedicationText.Text = medication.Name;
        if (medication.PrescribedPackageCount > 0)
        {
            PackageCountBox.Text = medication.PrescribedPackageCount.ToString();
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(PackageCountBox.Text.Trim(), out var count) || count < 1)
        {
            MessageBox.Show(
                this,
                UiLocalizer.Get("Common.MustBeAtLeast1"),
                UiLocalizer.Get("Refill.Title"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        PackageCount = count;
        DialogResult = true;
        Close();
    }
}
