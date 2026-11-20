using CommunityToolkit.Mvvm.ComponentModel;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public sealed class CalendarRow
{
    public required string Name { get; init; }

    public required string FirstRefill { get; init; }

    public required string SecondRefill { get; init; }

    public required string StockOut { get; init; }

    public required string PrescriptionEnd { get; init; }
}

public partial class CalendarViewModel : ObservableObject
{
    private readonly IPilldueApp _app;
    private readonly IUiInteraction _interaction;
    private readonly Func<DateOnly> _today;

    public CalendarViewModel(IPilldueApp app, IUiInteraction interaction, Func<DateOnly> today)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(interaction);
        ArgumentNullException.ThrowIfNull(today);
        _app = app;
        _interaction = interaction;
        _today = today;
        Loaded = LoadAsync();
    }

    public Task Loaded { get; }

    [ObservableProperty]
    private string _rangeText = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<CalendarRow> _rows = Array.Empty<CalendarRow>();

    [ObservableProperty]
    private bool _isEmpty;

    private async Task LoadAsync()
    {
        var today = _today();
        try
        {
            var calendar = await _app.GetCalendarAsync(today);
            RangeText = UiLocalizer.Format(
                "Cal.RangeLabel",
                calendar.RangeStart.ToString("yyyy-MM-dd"),
                calendar.RangeEnd.ToString("yyyy-MM-dd"));
            Rows = calendar.Entries.Select(ToRow).ToList();
            IsEmpty = calendar.Entries.Count == 0;
        }
        catch (Exception ex)
        {
            _interaction.ShowMessage(ex.Message, UiLocalizer.Get("Cal.Title"), UiMessageKind.Error);
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
}
