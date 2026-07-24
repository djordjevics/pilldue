using Spectre.Console;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI;

/// <summary>
/// Spectre screen: calendar from today through the second refill day.
/// Stock-out days are highlighted red; notes list meds that run out (restock assumed at first refill).
/// </summary>
internal static class CalendarScreen
{
    public static async Task RunAsync(IPilldueApp app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        AnsiConsole.MarkupLine($"[bold]{UiLocalizer.Get("Cal.Title").EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();

        var today = DateOnly.FromDateTime(DateTime.Today);

        try
        {
            var view = await app.GetCalendarAsync(today, cancellationToken).ConfigureAwait(false);

            AnsiConsole.MarkupLine(
                $"[grey]{UiLocalizer.Format(
                    "Cal.RangeLabel",
                    view.RangeStart.ToString("yyyy-MM-dd"),
                    view.RangeEnd.ToString("yyyy-MM-dd")).EscapeMarkup()}[/]");
            AnsiConsole.MarkupLine($"[grey]{UiLocalizer.Get("Cal.RestockNote").EscapeMarkup()}[/]");
            AnsiConsole.WriteLine();

            if (view.Entries.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{UiLocalizer.Get("Cal.Empty").EscapeMarkup()}[/]");
                return;
            }

            WriteMonthCalendars(view);
            WriteNotes(view);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(
                $"[red]{UiLocalizer.Format("Cal.Failed", ex.Message).EscapeMarkup()}[/]");
        }
    }

    private static void WriteMonthCalendars(CalendarView view)
    {
        var stockOut = view.AllStockOutDates.ToHashSet();
        var cursor = new DateOnly(view.RangeStart.Year, view.RangeStart.Month, 1);
        var lastMonth = new DateOnly(view.RangeEnd.Year, view.RangeEnd.Month, 1);

        while (cursor <= lastMonth)
        {
            var calendar = new Calendar(cursor.Year, cursor.Month)
            {
                HighlightStyle = new Style(Color.White, Color.Red),
                HeaderStyle = new Style(Color.Blue),
            };

            foreach (var day in stockOut.Where(d => d.Year == cursor.Year && d.Month == cursor.Month))
            {
                calendar.AddCalendarEvent(day.ToDateTime(TimeOnly.MinValue));
            }

            AnsiConsole.Write(calendar);
            AnsiConsole.WriteLine();
            cursor = cursor.AddMonths(1);
        }
    }

    private static void WriteNotes(CalendarView view)
    {
        AnsiConsole.MarkupLine($"[bold]{UiLocalizer.Get("Cal.NotesTitle").EscapeMarkup()}[/]");

        var withStockOut = view.Entries.Where(e => e.StockOutDates.Count > 0).ToList();
        if (withStockOut.Count == 0)
        {
            AnsiConsole.MarkupLine($"[green]{UiLocalizer.Get("Cal.NotesNone").EscapeMarkup()}[/]");
        }
        else
        {
            foreach (var entry in withStockOut)
            {
                var days = string.Join(", ", entry.StockOutDates.Select(d => d.ToString("yyyy-MM-dd")));
                AnsiConsole.MarkupLine(
                    $"[red]{UiLocalizer.Format(
                        "Cal.NoteStockOut",
                        entry.Medication.Name,
                        days).EscapeMarkup()}[/]");
            }
        }

        AnsiConsole.WriteLine();
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(UiLocalizer.Get("List.ColName"))
            .AddColumn(UiLocalizer.Get("Cal.ColFirstRefill"))
            .AddColumn(UiLocalizer.Get("Cal.ColSecondRefill"))
            .AddColumn(UiLocalizer.Get("Cal.ColStockOut"))
            .AddColumn(UiLocalizer.Get("Cal.ColRxEnd"));

        foreach (var entry in view.Entries)
        {
            var stockOut = entry.StockOutDates.Count == 0
                ? "—"
                : string.Join(", ", entry.StockOutDates.Select(d => d.ToString("yyyy-MM-dd")));
            table.AddRow(
                entry.Medication.Name.EscapeMarkup(),
                entry.FirstRefillDate.ToString("yyyy-MM-dd"),
                entry.SecondRefillDate.ToString("yyyy-MM-dd"),
                stockOut.EscapeMarkup(),
                entry.PrescriptionEndDate.ToString("yyyy-MM-dd"));
        }

        AnsiConsole.Write(table);
    }
}
