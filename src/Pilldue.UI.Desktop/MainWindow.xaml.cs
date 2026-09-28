using System.Windows;
using Pilldue.Business;

namespace Pilldue.UI.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await RefreshAsync();
    }

    private async void OnRefreshClick(object sender, RoutedEventArgs e)
    {
        await RefreshAsync();
    }

    private async void OnAddClick(object sender, RoutedEventArgs e)
    {
        var dialog = new AddMedicationWindow { Owner = this };
        if (dialog.ShowDialog() != true || dialog.Result is null)
        {
            return;
        }

        try
        {
            await App.Pilldue.AddMedicationAsync(dialog.Result);
            StatusText.Text = $"Added {dialog.Result.Name}.";
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Add medication", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void OnRefillClick(object sender, RoutedEventArgs e)
    {
        if (MedicationsGrid.SelectedItem is not Medication selected)
        {
            MessageBox.Show(this, "Select a medication first.", "Refill", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var packages = selected.PrescribedPackageCount > 0 ? selected.PrescribedPackageCount : 1;
        var confirm = MessageBox.Show(
            this,
            $"Log refill of {packages} package(s) for {selected.Name} today?",
            "Refill",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await App.Pilldue.RefillAsync(selected.Id, packages, DateOnly.FromDateTime(DateTime.Today));
            StatusText.Text = $"Refilled {selected.Name} (+{packages} package(s)).";
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Refill", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void OnRemoveClick(object sender, RoutedEventArgs e)
    {
        if (MedicationsGrid.SelectedItem is not Medication selected)
        {
            MessageBox.Show(this, "Select a medication first.", "Remove", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var confirm = MessageBox.Show(
            this,
            $"Remove {selected.Name}? This also deletes refill and skip history.",
            "Remove",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await App.Pilldue.RemoveMedicationAsync(selected.Id);
            StatusText.Text = $"Removed {selected.Name}.";
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Remove", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task RefreshAsync()
    {
        try
        {
            var medications = await App.Pilldue.ListMedicationsAsync();
            MedicationsGrid.ItemsSource = medications;
            StatusText.Text = medications.Count == 0
                ? "No medications yet. Click Add to create one."
                : $"{medications.Count} medication(s).";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Refresh", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
