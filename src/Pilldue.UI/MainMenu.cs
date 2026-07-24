using Spectre.Console;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI;

/// <summary>
/// Spectre main menu shell for v1 flows. Screens call <see cref="IPilldueApp"/> only.
/// </summary>
internal static class MainMenu
{
    private const string IdList = "list";
    private const string IdPlanning = "planning";
    private const string IdAdd = "add";
    private const string IdEdit = "edit";
    private const string IdRefill = "refill";
    private const string IdSkip = "skip";
    private const string IdCalendar = "calendar";
    private const string IdLanguage = "language";
    private const string IdExit = "exit";

    private sealed record MenuItem(string Id, string Label);

    public static async Task RunAsync(IPilldueApp app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        while (!cancellationToken.IsCancellationRequested)
        {
            AnsiConsole.Clear();
            await WriteHeaderAsync(app, cancellationToken).ConfigureAwait(false);

            var items = BuildMenu();
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<MenuItem>()
                    .Title($"[bold]{UiLocalizer.Get("Menu.Title").EscapeMarkup()}[/]")
                    .PageSize(12)
                    .UseConverter(i => i.Label)
                    .AddChoices(items));

            if (choice.Id == IdExit)
            {
                AnsiConsole.MarkupLine($"[grey]{UiLocalizer.Get("Common.Goodbye").EscapeMarkup()}[/]");
                return;
            }

            await HandleAsync(app, choice.Id, cancellationToken).ConfigureAwait(false);
            Pause();
        }
    }

    private static List<MenuItem> BuildMenu() =>
    [
        new(IdList, UiLocalizer.Get("Menu.List")),
        new(IdPlanning, UiLocalizer.Get("Menu.Planning")),
        new(IdAdd, UiLocalizer.Get("Menu.Add")),
        new(IdEdit, UiLocalizer.Get("Menu.Edit")),
        new(IdRefill, UiLocalizer.Get("Menu.Refill")),
        new(IdSkip, UiLocalizer.Get("Menu.Skip")),
        new(IdCalendar, UiLocalizer.Get("Menu.Calendar")),
        new(IdLanguage, UiLocalizer.Get("Menu.Language")),
        new(IdExit, UiLocalizer.Get("Menu.Exit")),
    ];

    private static async Task WriteHeaderAsync(IPilldueApp app, CancellationToken cancellationToken)
    {
        var settings = await app.GetConfigAsync(cancellationToken).ConfigureAwait(false);
        AnsiConsole.MarkupLine(
            $"[bold blue]Pilldue[/] — {UiLocalizer.Get("App.Tagline").EscapeMarkup()}");
        AnsiConsole.MarkupLine(
            $"[grey]{UiLocalizer.Format("App.HeaderMeta", settings.DefaultRefillDayOfMonth, UiLocalizer.Language).EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();
    }

    private static async Task HandleAsync(
        IPilldueApp app,
        string choiceId,
        CancellationToken cancellationToken)
    {
        switch (choiceId)
        {
            case IdList:
                await ShowMedicationListAsync(app, cancellationToken).ConfigureAwait(false);
                break;
            case IdPlanning:
                await PlanningQueriesScreen.RunAsync(app, cancellationToken).ConfigureAwait(false);
                break;
            case IdAdd:
                await MedicationForm.AddAsync(app, cancellationToken).ConfigureAwait(false);
                break;
            case IdEdit:
                await MedicationForm.EditAsync(app, cancellationToken).ConfigureAwait(false);
                break;
            case IdRefill:
                await RefillForm.RunAsync(app, cancellationToken).ConfigureAwait(false);
                break;
            case IdSkip:
                await SkipDoseForm.RunAsync(app, cancellationToken).ConfigureAwait(false);
                break;
            case IdCalendar:
                await CalendarScreen.RunAsync(app, cancellationToken).ConfigureAwait(false);
                break;
            case IdLanguage:
                await LanguageScreen.RunAsync(app, cancellationToken).ConfigureAwait(false);
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
            AnsiConsole.MarkupLine($"[yellow]{UiLocalizer.Get("List.Empty").EscapeMarkup()}[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(UiLocalizer.Get("List.ColName"))
            .AddColumn(UiLocalizer.Get("List.ColStock"))
            .AddColumn(UiLocalizer.Get("List.ColDaily"))
            .AddColumn(UiLocalizer.Get("List.ColPackage"));

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

    private static void Pause()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[grey]{UiLocalizer.Get("Common.PressEnterMenu").EscapeMarkup()}[/]");
        Console.ReadLine();
    }
}
