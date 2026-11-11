using System.Windows;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public partial class MainWindow : Window
{
    private readonly IPilldueApp _app;

    public MainWindow(IPilldueApp app)
    {
        _app = app ?? throw new ArgumentNullException(nameof(app));
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
            await _app.AddMedicationAsync(dialog.Result);
            StatusText.Text = UiLocalizer.Format("Desktop.Added", dialog.Result.Name);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, UiLocalizer.Get("Med.AddTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void OnEditClick(object sender, RoutedEventArgs e)
    {
        if (MedicationsGrid.SelectedItem is not Medication selected)
        {
            MessageBox.Show(
                this,
                UiLocalizer.Get("Desktop.SelectFirst"),
                UiLocalizer.Get("Med.EditTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var dialog = new AddMedicationWindow(selected) { Owner = this };
        if (dialog.ShowDialog() != true || dialog.Result is null)
        {
            return;
        }

        try
        {
            await _app.UpdateMedicationAsync(dialog.Result);
            StatusText.Text = UiLocalizer.Format("Desktop.Updated", dialog.Result.Name);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, UiLocalizer.Get("Med.EditTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void OnRefillClick(object sender, RoutedEventArgs e)
    {
        if (MedicationsGrid.SelectedItem is not Medication selected)
        {
            MessageBox.Show(
                this,
                UiLocalizer.Get("Desktop.SelectFirst"),
                UiLocalizer.Get("Refill.Title"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var packages = selected.PrescribedPackageCount > 0 ? selected.PrescribedPackageCount : 1;
        var confirm = MessageBox.Show(
            this,
            UiLocalizer.Format("Desktop.RefillConfirm", packages, selected.Name),
            UiLocalizer.Get("Refill.Title"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _app.RefillAsync(selected.Id, packages, DateOnly.FromDateTime(DateTime.Today));
            StatusText.Text = UiLocalizer.Format("Desktop.Refilled", selected.Name, packages);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, UiLocalizer.Get("Refill.Title"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void OnRemoveClick(object sender, RoutedEventArgs e)
    {
        if (MedicationsGrid.SelectedItem is not Medication selected)
        {
            MessageBox.Show(
                this,
                UiLocalizer.Get("Desktop.SelectFirst"),
                UiLocalizer.Get("Med.RemoveTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var confirm = MessageBox.Show(
            this,
            UiLocalizer.Format("Med.RemoveConfirm", selected.Name),
            UiLocalizer.Get("Med.RemoveTitle"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _app.RemoveMedicationAsync(selected.Id);
            StatusText.Text = UiLocalizer.Format("Desktop.Removed", selected.Name);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, UiLocalizer.Get("Med.RemoveTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task RefreshAsync()
    {
        try
        {
            var medications = await _app.ListMedicationsAsync();
            MedicationsGrid.ItemsSource = medications;
            StatusText.Text = medications.Count == 0
                ? UiLocalizer.Get("Desktop.Empty")
                : UiLocalizer.Format("Desktop.Count", medications.Count);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, UiLocalizer.Get("Desktop.Refresh"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
