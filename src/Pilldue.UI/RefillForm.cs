using Spectre.Console;
using Pilldue.Business;

namespace Pilldue.UI;

/// <summary>
/// Spectre screen: select a medication, enter package count, call <see cref="IPilldueApp.RefillAsync"/>.
/// </summary>
internal static class RefillForm
{
    public static async Task RunAsync(IPilldueApp app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        AnsiConsole.MarkupLine("[bold]Log refill[/]");
        AnsiConsole.WriteLine();

        var medications = await app.ListMedicationsAsync(cancellationToken).ConfigureAwait(false);
        if (medications.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No medications to refill. Add one first.[/]");
            return;
        }

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<Medication>()
                .Title("Select a medication")
                .PageSize(10)
                .UseConverter(m => $"{m.Name} (stock: {m.CurrentStockPills})")
                .AddChoices(medications));

        var packageCount = AnsiConsole.Prompt(
            new TextPrompt<int>("Packages obtained:")
                .DefaultValue(selected.PrescribedPackageCount > 0 ? selected.PrescribedPackageCount : 1)
                .Validate(v =>
                    v > 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Must be at least 1.")));

        var today = DateOnly.FromDateTime(DateTime.Today);
        var dateRaw = AnsiConsole.Prompt(
            new TextPrompt<string>("Refill date (yyyy-MM-dd):")
                .DefaultValue(today.ToString("yyyy-MM-dd"))
                .Validate(value =>
                    DateOnly.TryParseExact(value.Trim(), "yyyy-MM-dd", out _)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Use yyyy-MM-dd.")));
        var date = DateOnly.ParseExact(dateRaw.Trim(), "yyyy-MM-dd");

        try
        {
            await app.RefillAsync(selected.Id, packageCount, date, cancellationToken).ConfigureAwait(false);
            var updated = (await app.ListMedicationsAsync(cancellationToken).ConfigureAwait(false))
                .First(m => m.Id == selected.Id);
            AnsiConsole.MarkupLine(
                $"[green]Refilled[/] {updated.Name.EscapeMarkup()}: " +
                $"stock [bold]{selected.CurrentStockPills}[/] → [bold]{updated.CurrentStockPills}[/] " +
                $"([grey]+{packageCount} × {selected.PackageSizePills}[/]).");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Could not log refill:[/] {ex.Message.EscapeMarkup()}");
        }
    }
}
