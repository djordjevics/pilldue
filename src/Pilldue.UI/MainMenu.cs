using Spectre.Console;
using Pilldue.Business;

namespace Pilldue.UI;

/// <summary>
/// Spectre main menu shell for v1 flows. Screens call <see cref="IPilldueApp"/> only;
/// unfinished flows show a placeholder (no domain math in UI).
/// </summary>
internal static class MainMenu
{
    private const string ListMedications = "List medications";
    private const string PlanningQueries = "Planning queries (stock vs refill days)";
    private const string AddMedication = "Add medication";
    private const string EditMedication = "Edit medication";
    private const string LogRefill = "Log refill";
    private const string SkipDose = "Skip dose";
    private const string Calendar = "Calendar";
    private const string Exit = "Exit";

    public static async Task RunAsync(IPilldueApp app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        while (!cancellationToken.IsCancellationRequested)
        {
            AnsiConsole.Clear();
            await WriteHeaderAsync(app, cancellationToken).ConfigureAwait(false);

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Main menu[/]")
                    .PageSize(10)
                    .AddChoices(
                        ListMedications,
                        PlanningQueries,
                        AddMedication,
                        EditMedication,
                        LogRefill,
                        SkipDose,
                        Calendar,
                        Exit));

            if (choice == Exit)
            {
                AnsiConsole.MarkupLine("[grey]Goodbye.[/]");
                return;
            }

            await HandleAsync(app, choice, cancellationToken).ConfigureAwait(false);
            Pause();
        }
    }

    private static async Task WriteHeaderAsync(IPilldueApp app, CancellationToken cancellationToken)
    {
        var settings = await app.GetConfigAsync(cancellationToken).ConfigureAwait(false);
        AnsiConsole.MarkupLine("[bold blue]Pilldue[/] — medication refill tracker");
        AnsiConsole.MarkupLine(
            $"[grey]Default refill day: {settings.DefaultRefillDayOfMonth}. In-memory store.[/]");
        AnsiConsole.WriteLine();
    }

    private static async Task HandleAsync(
        IPilldueApp app,
        string choice,
        CancellationToken cancellationToken)
    {
        switch (choice)
        {
            case ListMedications:
                await ShowMedicationListAsync(app, cancellationToken).ConfigureAwait(false);
                break;
            case PlanningQueries:
                await PlanningQueriesScreen.RunAsync(app, cancellationToken).ConfigureAwait(false);
                break;
            case AddMedication:
                await MedicationForm.AddAsync(app, cancellationToken).ConfigureAwait(false);
                break;
            case EditMedication:
                await MedicationForm.EditAsync(app, cancellationToken).ConfigureAwait(false);
                break;
            case LogRefill:
                await RefillForm.RunAsync(app, cancellationToken).ConfigureAwait(false);
                break;
            case SkipDose:
                await SkipDoseForm.RunAsync(app, cancellationToken).ConfigureAwait(false);
                break;
            case Calendar:
                await CalendarScreen.RunAsync(app, cancellationToken).ConfigureAwait(false);
                break;
        }
    }

    private static async Task ShowMedicationListAsync(
        IPilldueApp app,
        CancellationToken cancellationToken)
    {
        var medications = await app.ListMedicationsAsync(cancellationToken).ConfigureAwait(false);
        if (medications.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No medications yet.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Name")
            .AddColumn("Stock")
            .AddColumn("Daily dose")
            .AddColumn("Package size");

        foreach (var med in medications)
        {
            table.AddRow(
                med.Name.EscapeMarkup(),
                med.CurrentStockPills.ToString(),
                med.DailyDosagePills.ToString(),
                med.PackageSizePills.ToString());
        }

        AnsiConsole.Write(table);
    }

    private static void ShowNotImplemented(string screen)
    {
        AnsiConsole.MarkupLine($"[yellow]Not implemented yet:[/] {screen.EscapeMarkup()}");
    }

    private static void Pause()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press Enter to return to the menu…[/]");
        Console.ReadLine();
    }
}
