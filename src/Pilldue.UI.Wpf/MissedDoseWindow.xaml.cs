using System.Windows;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public partial class MissedDoseWindow : Window
{
    private readonly IPilldueApp _app;
    private readonly Medication? _medication;

    public bool Flagged { get; private set; }

    public DateOnly? FlaggedDate { get; private set; }

    public MissedDoseWindow(IPilldueApp app, Medication? medication)
    {
        _app = app ?? throw new ArgumentNullException(nameof(app));
        _medication = medication;
        InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, UiLocalizer.Get("Missed.Title"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void OnFlagClick(object sender, RoutedEventArgs e)
    {
        if (_medication is null)
        {
            MessageBox.Show(
                this,
                UiLocalizer.Get("Desktop.SelectFirst"),
                UiLocalizer.Get("Missed.Flag"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var flag = new FlagMissedDoseWindow(_medication) { Owner = this };
        if (flag.ShowDialog() != true)
        {
            return;
        }

        try
        {
            await _app.FlagMissedDoseAsync(_medication.Id, flag.Date);
            Flagged = true;
            FlaggedDate = flag.Date;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, UiLocalizer.Get("Missed.Title"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadAsync()
    {
        var missed = await _app.ListMissedDosesAsync();
        var medications = await _app.ListMedicationsAsync();
        var names = medications.ToDictionary(medication => medication.Id, medication => medication.Name);

        MissedGrid.ItemsSource = missed
            .Select(item => new MissedRow
            {
                Date = item.Date.ToString("yyyy-MM-dd"),
                Medication = names.TryGetValue(item.MedicationId, out var name)
                    ? name
                    : item.MedicationId.ToString(),
            })
            .ToList();
        EmptyText.Visibility = missed.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private sealed class MissedRow
    {
        public required string Date { get; init; }

        public required string Medication { get; init; }
    }
}
