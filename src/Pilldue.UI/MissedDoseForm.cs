using Spectre.Console;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI;

/// <summary>
/// Spectre screen: flag a missed dose (no stock change) or list past flags.
/// </summary>
internal static class MissedDoseForm
{
    private const string ActionFlag = "flag";
    private const string ActionList = "list";
    private const string ActionCancel = "cancel";

    private sealed record ActionItem(string Id, string Label);

    public static async Task RunAsync(IPilldueApp app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        AnsiConsole.MarkupLine($"[bold]{UiLocalizer.Get("Missed.Title").EscapeMarkup()}[/]");
        AnsiConsole.MarkupLine($"[grey]{UiLocalizer.Get("Missed.Hint").EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();

        var cancelLabel = UiLocalizer.Get("Common.Cancel");
        var action = AnsiConsole.Prompt(
            new SelectionPrompt<ActionItem>()
                .Title(UiLocalizer.Get("Missed.ChooseAction"))
                .UseConverter(i => i.Label)
                .AddChoices(
                    new ActionItem(ActionFlag, UiLocalizer.Get("Missed.Flag")),
                    new ActionItem(ActionList, UiLocalizer.Get("Missed.List")),
                    new ActionItem(ActionCancel, cancelLabel)));

        if (action.Id == ActionCancel)
        {
            AnsiConsole.MarkupLine($"[grey]{UiLocalizer.Get("Missed.Cancelled").EscapeMarkup()}[/]");
            return;
        }

        if (action.Id == ActionList)
        {
            await ListAsync(app, cancellationToken).ConfigureAwait(false);
            return;
        }

        await FlagAsync(app, cancelLabel, cancellationToken).ConfigureAwait(false);
    }

    private static async Task FlagAsync(
        IPilldueApp app,
        string cancelLabel,
        CancellationToken cancellationToken)
    {
        var medications = await app.ListMedicationsAsync(cancellationToken).ConfigureAwait(false);
        if (medications.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]{UiLocalizer.Get("Missed.Empty").EscapeMarkup()}[/]");
            return;
        }

        var labels = medications.Select(m => m.Name).Append(cancelLabel).ToList();
        var selectedLabel = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(UiLocalizer.Get("Common.SelectMedication"))
                .PageSize(12)
                .AddChoices(labels));

        var selectedIndex = MedicationFormLogic.ResolveMedicationSelection(labels, selectedLabel, cancelLabel);
        if (selectedIndex is null)
        {
            AnsiConsole.MarkupLine($"[grey]{UiLocalizer.Get("Missed.Cancelled").EscapeMarkup()}[/]");
            return;
        }

        var selected = medications[selectedIndex.Value];
        var today = DateOnly.FromDateTime(DateTime.Today);
        var dateRaw = AnsiConsole.Prompt(
            new TextPrompt<string>(UiLocalizer.Get("Missed.Date"))
                .DefaultValue(today.ToString("yyyy-MM-dd"))
                .Validate(value =>
                    DateOnly.TryParseExact(value.Trim(), "yyyy-MM-dd", out _)
                        ? ValidationResult.Success()
                        : ValidationResult.Error(UiLocalizer.Get("Common.UseDateFormat"))));
        var date = DateOnly.ParseExact(dateRaw.Trim(), "yyyy-MM-dd");

        try
        {
            var stockBefore = selected.CurrentStockPills;
            await app.FlagMissedDoseAsync(selected.Id, date, cancellationToken).ConfigureAwait(false);
            var after = (await app.ListMedicationsAsync(cancellationToken).ConfigureAwait(false))
                .First(m => m.Id == selected.Id);
            AnsiConsole.MarkupLine(
                $"[green]{UiLocalizer.Format("Missed.Flagged", selected.Name, date).EscapeMarkup()}[/]");
            AnsiConsole.MarkupLine(
                $"[grey]{UiLocalizer.Format("Missed.StockUnchanged", stockBefore, after.CurrentStockPills).EscapeMarkup()}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(
                $"[red]{UiLocalizer.Format("Missed.Failed", ex.Message).EscapeMarkup()}[/]");
        }
    }

    private static async Task ListAsync(IPilldueApp app, CancellationToken cancellationToken)
    {
        var events = await app.ListMissedDosesAsync(cancellationToken).ConfigureAwait(false);
        if (events.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]{UiLocalizer.Get("Missed.ListEmpty").EscapeMarkup()}[/]");
            return;
        }

        var medications = (await app.ListMedicationsAsync(cancellationToken).ConfigureAwait(false))
            .ToDictionary(m => m.Id);

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(UiLocalizer.Get("Missed.ColDate"))
            .AddColumn(UiLocalizer.Get("Missed.ColMed"));

        foreach (var entry in events)
        {
            var name = medications.TryGetValue(entry.MedicationId, out var med)
                ? med.Name
                : entry.MedicationId.ToString();
            table.AddRow(entry.Date.ToString("yyyy-MM-dd"), name.EscapeMarkup());
        }

        AnsiConsole.Write(table);
    }
}
