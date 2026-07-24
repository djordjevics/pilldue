using Spectre.Console;
using Pilldue.Business;

namespace Pilldue.UI;

/// <summary>
/// Spectre screen: flag a skipped dose, return pills to stock via <see cref="IPilldueApp.SkipDoseAsync"/>.
/// </summary>
internal static class SkipDoseForm
{
    public static async Task RunAsync(IPilldueApp app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        AnsiConsole.MarkupLine("[bold]Skip dose[/]");
        AnsiConsole.WriteLine();

        var medications = await app.ListMedicationsAsync(cancellationToken).ConfigureAwait(false);
        if (medications.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No medications. Add one first.[/]");
            return;
        }

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<Medication>()
                .Title("Select a medication")
                .PageSize(10)
                .UseConverter(m => $"{m.Name} (stock: {m.CurrentStockPills})")
                .AddChoices(medications));

        var pillsReturned = AnsiConsole.Prompt(
            new TextPrompt<int>("Pills returned to stock (usually daily dosage):")
                .DefaultValue(selected.DailyDosagePills > 0 ? selected.DailyDosagePills : 1)
                .Validate(v =>
                    v > 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Must be at least 1.")));

        var today = DateOnly.FromDateTime(DateTime.Today);
        var dateRaw = AnsiConsole.Prompt(
            new TextPrompt<string>("Skip date (yyyy-MM-dd):")
                .DefaultValue(today.ToString("yyyy-MM-dd"))
                .Validate(value =>
                    DateOnly.TryParseExact(value.Trim(), "yyyy-MM-dd", out _)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Use yyyy-MM-dd.")));
        var date = DateOnly.ParseExact(dateRaw.Trim(), "yyyy-MM-dd");

        try
        {
            await app.SkipDoseAsync(selected.Id, pillsReturned, date, cancellationToken).ConfigureAwait(false);
            var updated = (await app.ListMedicationsAsync(cancellationToken).ConfigureAwait(false))
                .First(m => m.Id == selected.Id);
            AnsiConsole.MarkupLine(
                $"[green]Skipped dose[/] for {updated.Name.EscapeMarkup()}: " +
                $"stock [bold]{selected.CurrentStockPills}[/] → [bold]{updated.CurrentStockPills}[/] " +
                $"([grey]+{pillsReturned}[/]).");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Could not record skip:[/] {ex.Message.EscapeMarkup()}");
        }
    }
}
