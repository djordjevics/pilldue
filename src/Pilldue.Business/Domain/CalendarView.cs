namespace Pilldue.Business;

/// <summary>
/// Flow 3 calendar: from as-of through the second upcoming config refill day,
/// with per-med stock-out dates assuming prescribed restock at the first refill.
/// </summary>
public sealed class CalendarView
{
    public required DateOnly RangeStart { get; init; }

    public required DateOnly RangeEnd { get; init; }

    public required IReadOnlyList<MedicationCalendarEntry> Entries { get; init; }

    /// <summary>Distinct stock-out days across all medications (for red highlighting).</summary>
    public IReadOnlyList<DateOnly> AllStockOutDates =>
        Entries
            .SelectMany(e => e.StockOutDates)
            .Distinct()
            .OrderBy(d => d)
            .ToList();
}
