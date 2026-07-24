using Spectre.Console;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI;

/// <summary>
/// Spectre screen: select a medication, enter package count, call <see cref="IPilldueApp.RefillAsync"/>.
/// User can cancel from the medication list or at the final confirmation.
/// </summary>
internal static class RefillForm
{
    public static async Task RunAsync(IPilldueApp app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        AnsiConsole.MarkupLine($"[bold]{UiLocalizer.Get("Refill.Title").EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();

        var medications = await app.ListMedicationsAsync(cancellationToken).ConfigureAwait(false);
        if (medications.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]{UiLocalizer.Get("Refill.Empty").EscapeMarkup()}[/]");
            return;
        }

        var cancelLabel = UiLocalizer.Get("Common.Cancel");
        var labels = medications
            .Select(m => UiLocalizer.Format("Common.StockSuffix", m.Name, m.CurrentStockPills))
            .Append(cancelLabel)
            .ToList();

        var selectedLabel = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(UiLocalizer.Get("Common.SelectMedication"))
                .PageSize(12)
                .AddChoices(labels));

        if (RefillFormLogic.IsCancelSelection(selectedLabel, cancelLabel))
        {
            AnsiConsole.MarkupLine($"[grey]{UiLocalizer.Get("Refill.Cancelled").EscapeMarkup()}[/]");
            return;
        }

        var selectedIndex = labels.IndexOf(selectedLabel);
        var selected = medications[selectedIndex];

        var packageCount = AnsiConsole.Prompt(
            new TextPrompt<int>(UiLocalizer.Get("Refill.Packages"))
                .DefaultValue(selected.PrescribedPackageCount > 0 ? selected.PrescribedPackageCount : 1)
                .Validate(v =>
                    v > 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error(UiLocalizer.Get("Common.MustBeAtLeast1"))));

        var today = DateOnly.FromDateTime(DateTime.Today);
        var dateRaw = AnsiConsole.Prompt(
            new TextPrompt<string>(UiLocalizer.Get("Refill.Date"))
                .DefaultValue(today.ToString("yyyy-MM-dd"))
                .Validate(value =>
                    DateOnly.TryParseExact(value.Trim(), "yyyy-MM-dd", out _)
                        ? ValidationResult.Success()
                        : ValidationResult.Error(UiLocalizer.Get("Common.UseDateFormat"))));
        var date = DateOnly.ParseExact(dateRaw.Trim(), "yyyy-MM-dd");

        var confirm = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(UiLocalizer.Format("Refill.Confirm", selected.Name, packageCount, date.ToString("yyyy-MM-dd")))
                .AddChoices(UiLocalizer.Get("Common.Yes"), cancelLabel));

        if (RefillFormLogic.IsCancelSelection(confirm, cancelLabel))
        {
            AnsiConsole.MarkupLine($"[grey]{UiLocalizer.Get("Refill.Cancelled").EscapeMarkup()}[/]");
            return;
        }

        try
        {
            await app.RefillAsync(selected.Id, packageCount, date, cancellationToken).ConfigureAwait(false);
            var updated = (await app.ListMedicationsAsync(cancellationToken).ConfigureAwait(false))
                .First(m => m.Id == selected.Id);
            AnsiConsole.MarkupLine(
                $"[green]{UiLocalizer.Format(
                    "Refill.Done",
                    updated.Name,
                    selected.CurrentStockPills,
                    updated.CurrentStockPills,
                    packageCount,
                    selected.PackageSizePills).EscapeMarkup()}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(
                $"[red]{UiLocalizer.Format("Refill.Failed", ex.Message).EscapeMarkup()}[/]");
        }
    }
}
