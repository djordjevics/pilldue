using Spectre.Console;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI;

/// <summary>
/// Spectre screen: flag a skipped dose, return pills to stock via <see cref="IPilldueApp.SkipDoseAsync"/>.
/// </summary>
internal static class SkipDoseForm
{
    public static async Task RunAsync(IPilldueApp app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        AnsiConsole.MarkupLine($"[bold]{UiLocalizer.Get("Skip.Title").EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();

        var medications = await app.ListMedicationsAsync(cancellationToken).ConfigureAwait(false);
        if (medications.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]{UiLocalizer.Get("Skip.Empty").EscapeMarkup()}[/]");
            return;
        }

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<Medication>()
                .Title(UiLocalizer.Get("Common.SelectMedication"))
                .PageSize(10)
                .UseConverter(m => UiLocalizer.Format("Common.StockSuffix", m.Name, m.CurrentStockPills))
                .AddChoices(medications));

        var pillsReturned = AnsiConsole.Prompt(
            new TextPrompt<int>(UiLocalizer.Get("Skip.Pills"))
                .DefaultValue(selected.DailyDosagePills > 0 ? selected.DailyDosagePills : 1)
                .Validate(v =>
                    v > 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error(UiLocalizer.Get("Common.MustBeAtLeast1"))));

        var today = DateOnly.FromDateTime(DateTime.Today);
        var dateRaw = AnsiConsole.Prompt(
            new TextPrompt<string>(UiLocalizer.Get("Skip.Date"))
                .DefaultValue(today.ToString("yyyy-MM-dd"))
                .Validate(value =>
                    DateOnly.TryParseExact(value.Trim(), "yyyy-MM-dd", out _)
                        ? ValidationResult.Success()
                        : ValidationResult.Error(UiLocalizer.Get("Common.UseDateFormat"))));
        var date = DateOnly.ParseExact(dateRaw.Trim(), "yyyy-MM-dd");

        try
        {
            await app.SkipDoseAsync(selected.Id, pillsReturned, date, cancellationToken).ConfigureAwait(false);
            var updated = (await app.ListMedicationsAsync(cancellationToken).ConfigureAwait(false))
                .First(m => m.Id == selected.Id);
            AnsiConsole.MarkupLine(
                $"[green]{UiLocalizer.Format(
                    "Skip.Done",
                    updated.Name,
                    selected.CurrentStockPills,
                    updated.CurrentStockPills,
                    pillsReturned).EscapeMarkup()}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(
                $"[red]{UiLocalizer.Format("Skip.Failed", ex.Message).EscapeMarkup()}[/]");
        }
    }
}
