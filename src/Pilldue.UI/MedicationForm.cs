using Spectre.Console;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI;

/// <summary>
/// Spectre prompts for medication definition fields (use-cases.md). Calls <see cref="IPilldueApp"/> only.
/// </summary>
internal static class MedicationForm
{
    public static async Task AddAsync(IPilldueApp app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        AnsiConsole.MarkupLine($"[bold]{UiLocalizer.Get("Med.AddTitle").EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();

        var medication = PromptFields(existing: null);
        try
        {
            var saved = await app.AddMedicationAsync(medication, cancellationToken).ConfigureAwait(false);
            AnsiConsole.MarkupLine(
                $"[green]{UiLocalizer.Format("Med.Added", saved.Name, saved.Id).EscapeMarkup()}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(
                $"[red]{UiLocalizer.Format("Med.AddFailed", ex.Message).EscapeMarkup()}[/]");
        }
    }

    public static async Task EditAsync(IPilldueApp app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        AnsiConsole.MarkupLine($"[bold]{UiLocalizer.Get("Med.EditTitle").EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();

        var medications = await app.ListMedicationsAsync(cancellationToken).ConfigureAwait(false);
        if (medications.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]{UiLocalizer.Get("Med.EditNone").EscapeMarkup()}[/]");
            return;
        }

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<Medication>()
                .Title(UiLocalizer.Get("Common.SelectMedication"))
                .PageSize(10)
                .UseConverter(m => m.Name)
                .AddChoices(medications));

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(
            UiLocalizer.Format("Med.Editing", $"[bold]{selected.Name.EscapeMarkup()}[/]"));
        AnsiConsole.WriteLine();

        var updated = PromptFields(selected);
        try
        {
            var saved = await app.UpdateMedicationAsync(updated, cancellationToken).ConfigureAwait(false);
            AnsiConsole.MarkupLine(
                $"[green]{UiLocalizer.Format("Med.Updated", saved.Name, saved.Id).EscapeMarkup()}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(
                $"[red]{UiLocalizer.Format("Med.UpdateFailed", ex.Message).EscapeMarkup()}[/]");
        }
    }

    private static Medication PromptFields(Medication? existing)
    {
        var name = AnsiConsole.Prompt(
            new TextPrompt<string>(UiLocalizer.Get("Med.Name"))
                .DefaultValue(existing?.Name ?? string.Empty)
                .Validate(value =>
                    string.IsNullOrWhiteSpace(value)
                        ? ValidationResult.Error(UiLocalizer.Get("Common.NameRequired"))
                        : ValidationResult.Success()));

        var packageSize = AnsiConsole.Prompt(
            new TextPrompt<int>(UiLocalizer.Get("Med.PackageSize"))
                .DefaultValue(existing?.PackageSizePills is > 0 ? existing.PackageSizePills : 28)
                .Validate(v =>
                    v > 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error(UiLocalizer.Get("Common.MustBeGreaterThan0"))));

        var prescribedPackages = AnsiConsole.Prompt(
            new TextPrompt<int>(UiLocalizer.Get("Med.Prescribed"))
                .DefaultValue(existing?.PrescribedPackageCount is > 0 ? existing.PrescribedPackageCount : 1)
                .Validate(v =>
                    v > 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error(UiLocalizer.Get("Common.MustBeGreaterThan0"))));

        var dailyDosage = AnsiConsole.Prompt(
            new TextPrompt<int>(UiLocalizer.Get("Med.Daily"))
                .DefaultValue(existing?.DailyDosagePills is > 0 ? existing.DailyDosagePills : 1)
                .Validate(v =>
                    v > 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error(UiLocalizer.Get("Common.MustBeGreaterThan0"))));

        var currentStock = AnsiConsole.Prompt(
            new TextPrompt<int>(UiLocalizer.Get("Med.Stock"))
                .DefaultValue(existing?.CurrentStockPills ?? 0)
                .Validate(v =>
                    v >= 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error(UiLocalizer.Get("Common.MustBeZeroOrGreater"))));

        var refillOverrideRaw = AnsiConsole.Prompt(
            new TextPrompt<string>(UiLocalizer.Get("Med.RefillOverride"))
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
                        : ValidationResult.Error(UiLocalizer.Get("Med.RefillOverrideInvalid"));
                }));

        int? refillOverride = string.IsNullOrWhiteSpace(refillOverrideRaw)
            ? null
            : int.Parse(refillOverrideRaw.Trim());

        var defaultStart = existing?.PrescriptionStartDate ?? DateOnly.FromDateTime(DateTime.Today);
        var prescriptionStart = AnsiConsole.Prompt(
            new TextPrompt<string>(UiLocalizer.Get("Med.RxStart"))
                .DefaultValue(defaultStart.ToString("yyyy-MM-dd"))
                .Validate(value =>
                    DateOnly.TryParseExact(value.Trim(), "yyyy-MM-dd", out _)
                        ? ValidationResult.Success()
                        : ValidationResult.Error(UiLocalizer.Get("Common.UseDateFormat"))));

        var durationMonths = AnsiConsole.Prompt(
            new TextPrompt<int>(UiLocalizer.Get("Med.RxDuration"))
                .DefaultValue(existing?.PrescriptionDurationMonths is > 0
                    ? existing.PrescriptionDurationMonths
                    : 6)
                .Validate(v =>
                    v > 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error(UiLocalizer.Get("Common.MustBeGreaterThan0"))));

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
