using Spectre.Console;
using Pilldue.Business;

namespace Pilldue.UI;

/// <summary>
/// Spectre screen: date range with last covered day and prescription end per medication.
/// </summary>
internal static class CalendarScreen
{
    public static async Task RunAsync(IPilldueApp app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        AnsiConsole.MarkupLine("[bold]Calendar[/]");
        AnsiConsole.WriteLine();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var rangeStart = PromptDate("Range start (yyyy-MM-dd):", today.AddDays(1 - today.Day));
        var rangeEnd = PromptDate("Range end (yyyy-MM-dd):", rangeStart.AddMonths(1).AddDays(-1));
        if (rangeEnd < rangeStart)
        {
            AnsiConsole.MarkupLine("[red]Range end must be on or after range start.[/]");
            return;
        }

        var asOf = PromptDate("As-of date for stock coverage (yyyy-MM-dd):", today);
        AnsiConsole.WriteLine();

        try
        {
            var entries = await app.GetCalendarAsync(rangeStart, rangeEnd, asOf, cancellationToken)
                .ConfigureAwait(false);

            if (entries.Count == 0)
            {
                AnsiConsole.MarkupLine(
                    $"[yellow]No medications with last-covered or prescription end in {rangeStart:yyyy-MM-dd}…{rangeEnd:yyyy-MM-dd}.[/]");
                return;
            }

            var table = new Table()
                .Border(TableBorder.Rounded)
                .Title($"Calendar {rangeStart:yyyy-MM-dd} → {rangeEnd:yyyy-MM-dd} (as of {asOf:yyyy-MM-dd})")
                .AddColumn("Name")
                .AddColumn("Last covered")
                .AddColumn("Prescription end");

            foreach (var entry in entries)
            {
                table.AddRow(
                    entry.Medication.Name.EscapeMarkup(),
                    entry.LastCoveredDate?.ToString("yyyy-MM-dd") ?? "—",
                    entry.PrescriptionEndDate.ToString("yyyy-MM-dd"));
            }

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Could not load calendar:[/] {ex.Message.EscapeMarkup()}");
        }
    }

    private static DateOnly PromptDate(string label, DateOnly defaultValue)
    {
        var raw = AnsiConsole.Prompt(
            new TextPrompt<string>(label)
                .DefaultValue(defaultValue.ToString("yyyy-MM-dd"))
                .Validate(value =>
                    DateOnly.TryParseExact(value.Trim(), "yyyy-MM-dd", out _)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Use yyyy-MM-dd.")));
        return DateOnly.ParseExact(raw.Trim(), "yyyy-MM-dd");
    }
}
