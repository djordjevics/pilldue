using System.Windows;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public partial class CalendarWindow : Window
{
    private readonly IPilldueApp _app;

    public CalendarWindow(IPilldueApp app)
    {
        _app = app ?? throw new ArgumentNullException(nameof(app));
        InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        try
        {
            var calendar = await _app.GetCalendarAsync(today);
            RangeText.Text = UiLocalizer.Format(
                "Cal.RangeLabel",
                calendar.RangeStart.ToString("yyyy-MM-dd"),
                calendar.RangeEnd.ToString("yyyy-MM-dd"));
            CalendarGrid.ItemsSource = calendar.Entries.Select(ToRow).ToList();
            EmptyText.Visibility = calendar.Entries.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, UiLocalizer.Get("Cal.Title"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static CalendarRow ToRow(MedicationCalendarEntry entry) => new()
    {
        Name = entry.Medication.Name,
        FirstRefill = entry.FirstRefillDate.ToString("yyyy-MM-dd"),
        SecondRefill = entry.SecondRefillDate.ToString("yyyy-MM-dd"),
        StockOut = string.Join(", ", entry.StockOutDates.Select(date => date.ToString("yyyy-MM-dd"))),
        PrescriptionEnd = entry.PrescriptionEndDate.ToString("yyyy-MM-dd"),
    };

    private sealed class CalendarRow
    {
        public required string Name { get; init; }
        public required string FirstRefill { get; init; }
        public required string SecondRefill { get; init; }
        public required string StockOut { get; init; }
        public required string PrescriptionEnd { get; init; }
    }
}
