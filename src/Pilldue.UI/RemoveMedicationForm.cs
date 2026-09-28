using Spectre.Console;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI;

/// <summary>
/// Spectre screen: pick a medication, confirm, call <see cref="IPilldueApp.RemoveMedicationAsync"/>.
/// </summary>
internal static class RemoveMedicationForm
{
    public static async Task RunAsync(IPilldueApp app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        AnsiConsole.MarkupLine($"[bold]{UiLocalizer.Get("Med.RemoveTitle").EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();

        var medications = await app.ListMedicationsAsync(cancellationToken).ConfigureAwait(false);
        if (medications.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]{UiLocalizer.Get("Med.RemoveNone").EscapeMarkup()}[/]");
            return;
        }

        var cancelLabel = UiLocalizer.Get("Common.Cancel");
        var labels = medications.Select(m => m.Name).Append(cancelLabel).ToList();

        var selectedLabel = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(UiLocalizer.Get("Common.SelectMedication"))
                .PageSize(12)
                .AddChoices(labels));

        var selectedIndex = MedicationFormLogic.ResolveMedicationSelection(labels, selectedLabel, cancelLabel);
        if (selectedIndex is null)
        {
            AnsiConsole.MarkupLine($"[grey]{UiLocalizer.Get("Med.RemoveCancelled").EscapeMarkup()}[/]");
            return;
        }

        var selected = medications[selectedIndex.Value];

        var confirm = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(UiLocalizer.Format("Med.RemoveConfirm", selected.Name))
                .AddChoices(UiLocalizer.Get("Common.Yes"), cancelLabel));

        if (RefillFormLogic.IsCancelSelection(confirm, cancelLabel))
        {
            AnsiConsole.MarkupLine($"[grey]{UiLocalizer.Get("Med.RemoveCancelled").EscapeMarkup()}[/]");
            return;
        }

        try
        {
            await app.RemoveMedicationAsync(selected.Id, cancellationToken).ConfigureAwait(false);
            AnsiConsole.MarkupLine(
                $"[green]{UiLocalizer.Format("Med.Removed", selected.Name).EscapeMarkup()}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(
                $"[red]{UiLocalizer.Format("Med.RemoveFailed", ex.Message).EscapeMarkup()}[/]");
        }
    }
}
