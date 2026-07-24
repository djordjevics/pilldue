using Spectre.Console;
using Pilldue.Business;

namespace Pilldue.UI;

/// <summary>
/// Spectre prompts for medication definition fields (use-cases.md). Calls <see cref="IPilldueApp"/> only.
/// </summary>
internal static class MedicationForm
{
    public static async Task AddAsync(IPilldueApp app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        AnsiConsole.MarkupLine("[bold]Add medication[/]");
        AnsiConsole.WriteLine();

        var medication = PromptFields(existing: null);
        try
        {
            var saved = await app.AddMedicationAsync(medication, cancellationToken).ConfigureAwait(false);
            AnsiConsole.MarkupLine(
                $"[green]Added[/] {saved.Name.EscapeMarkup()} ([grey]{saved.Id}[/]).");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Could not add medication:[/] {ex.Message.EscapeMarkup()}");
        }
    }

    public static async Task EditAsync(IPilldueApp app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        AnsiConsole.MarkupLine("[bold]Edit medication[/]");
        AnsiConsole.WriteLine();

        var medications = await app.ListMedicationsAsync(cancellationToken).ConfigureAwait(false);
        if (medications.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No medications to edit.[/]");
            return;
        }

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<Medication>()
                .Title("Select a medication")
                .PageSize(10)
                .UseConverter(m => m.Name)
                .AddChoices(medications));

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"Editing [bold]{selected.Name.EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();

        var updated = PromptFields(selected);
        try
        {
            var saved = await app.UpdateMedicationAsync(updated, cancellationToken).ConfigureAwait(false);
            AnsiConsole.MarkupLine(
                $"[green]Updated[/] {saved.Name.EscapeMarkup()} ([grey]{saved.Id}[/]).");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Could not update medication:[/] {ex.Message.EscapeMarkup()}");
        }
    }

    private static Medication PromptFields(Medication? existing)
    {
        var name = AnsiConsole.Prompt(
            new TextPrompt<string>("Name:")
                .DefaultValue(existing?.Name ?? string.Empty)
                .Validate(value =>
                    string.IsNullOrWhiteSpace(value)
                        ? ValidationResult.Error("Name is required.")
                        : ValidationResult.Success()));

        var packageSize = AnsiConsole.Prompt(
            new TextPrompt<int>("Package size (pills per package):")
                .DefaultValue(existing?.PackageSizePills is > 0 ? existing.PackageSizePills : 28)
                .Validate(v =>
                    v > 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Must be greater than 0.")));

        var prescribedPackages = AnsiConsole.Prompt(
            new TextPrompt<int>("Prescribed package count (usual packages per refill):")
                .DefaultValue(existing?.PrescribedPackageCount is > 0 ? existing.PrescribedPackageCount : 1)
                .Validate(v =>
                    v > 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Must be greater than 0.")));

        var dailyDosage = AnsiConsole.Prompt(
            new TextPrompt<int>("Daily dosage (pills per day):")
                .DefaultValue(existing?.DailyDosagePills is > 0 ? existing.DailyDosagePills : 1)
                .Validate(v =>
                    v > 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Must be greater than 0.")));

        var currentStock = AnsiConsole.Prompt(
            new TextPrompt<int>("Current stock (pills on hand):")
                .DefaultValue(existing?.CurrentStockPills ?? 0)
                .Validate(v =>
                    v >= 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Must be 0 or greater.")));

        var refillOverrideRaw = AnsiConsole.Prompt(
            new TextPrompt<string>("Refill day override (1–31, or blank to inherit config default):")
                .AllowEmpty()
                .DefaultValue(existing?.RefillDayOfMonthOverride?.ToString() ?? string.Empty)
                .Validate(value =>
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return ValidationResult.Success();
                    }

                    return int.TryParse(value.Trim(), out var day) && day is >= 1 and <= 31
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Enter a day 1–31, or leave blank.");
                }));

        int? refillOverride = string.IsNullOrWhiteSpace(refillOverrideRaw)
            ? null
            : int.Parse(refillOverrideRaw.Trim());

        var defaultStart = existing?.PrescriptionStartDate ?? DateOnly.FromDateTime(DateTime.Today);
        var prescriptionStart = AnsiConsole.Prompt(
            new TextPrompt<string>("Prescription start date (yyyy-MM-dd):")
                .DefaultValue(defaultStart.ToString("yyyy-MM-dd"))
                .Validate(value =>
                    DateOnly.TryParseExact(value.Trim(), "yyyy-MM-dd", out _)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Use yyyy-MM-dd.")));

        var durationMonths = AnsiConsole.Prompt(
            new TextPrompt<int>("Prescription duration (months):")
                .DefaultValue(existing?.PrescriptionDurationMonths is > 0
                    ? existing.PrescriptionDurationMonths
                    : 6)
                .Validate(v =>
                    v > 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Must be greater than 0.")));

        return new Medication
        {
            Id = existing?.Id ?? Guid.NewGuid(),
            Name = name.Trim(),
            PackageSizePills = packageSize,
            PrescribedPackageCount = prescribedPackages,
            DailyDosagePills = dailyDosage,
            CurrentStockPills = currentStock,
            RefillDayOfMonthOverride = refillOverride,
            PrescriptionStartDate = DateOnly.ParseExact(prescriptionStart.Trim(), "yyyy-MM-dd"),
            PrescriptionDurationMonths = durationMonths,
        };
    }
}
