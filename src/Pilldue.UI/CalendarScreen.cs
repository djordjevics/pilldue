using Spectre.Console;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI;

/// <summary>
/// Spectre screen: date range with last covered day and prescription end per medication.
/// </summary>
internal static class CalendarScreen
{
    public static async Task RunAsync(IPilldueApp app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        AnsiConsole.MarkupLine($"[bold]{UiLocalizer.Get("Cal.Title").EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var rangeStart = PromptDate(UiLocalizer.Get("Cal.RangeStart"), today.AddDays(1 - today.Day));
        var rangeEnd = PromptDate(UiLocalizer.Get("Cal.RangeEnd"), rangeStart.AddMonths(1).AddDays(-1));
        if (rangeEnd < rangeStart)
        {
            AnsiConsole.MarkupLine($"[red]{UiLocalizer.Get("Cal.RangeInvalid").EscapeMarkup()}[/]");
            return;
        }

        var asOf = PromptDate(UiLocalizer.Get("Cal.AsOf"), today);
        AnsiConsole.WriteLine();

        try
        {
            var entries = await app.GetCalendarAsync(rangeStart, rangeEnd, asOf, cancellationToken)
                .ConfigureAwait(false);

            if (entries.Count == 0)
            {
                AnsiConsole.MarkupLine(
                    $"[yellow]{UiLocalizer.Format(
                        "Cal.Empty",
                        rangeStart.ToString("yyyy-MM-dd"),
                        rangeEnd.ToString("yyyy-MM-dd")).EscapeMarkup()}[/]");
                return;
            }

            var table = new Table()
                .Border(TableBorder.Rounded)
                .Title(UiLocalizer.Format(
                    "Cal.TableTitle",
                    rangeStart.ToString("yyyy-MM-dd"),
                    rangeEnd.ToString("yyyy-MM-dd"),
                    asOf.ToString("yyyy-MM-dd")))
                .AddColumn(UiLocalizer.Get("List.ColName"))
                .AddColumn(UiLocalizer.Get("Cal.ColLast"))
                .AddColumn(UiLocalizer.Get("Cal.ColRxEnd"));

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
            AnsiConsole.MarkupLine(
                $"[red]{UiLocalizer.Format("Cal.Failed", ex.Message).EscapeMarkup()}[/]");
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
                        : ValidationResult.Error(UiLocalizer.Get("Common.UseDateFormat"))));
        return DateOnly.ParseExact(raw.Trim(), "yyyy-MM-dd");
    }
}
