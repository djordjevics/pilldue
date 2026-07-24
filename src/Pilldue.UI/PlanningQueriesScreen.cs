using Spectre.Console;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI;

/// <summary>
/// Spectre screens for flow 1 planning queries (coverage / short / need-extra).
/// </summary>
internal static class PlanningQueriesScreen
{
    private const string IdCoverage = "coverage";
    private const string IdShort = "short";
    private const string IdExtra = "extra";
    private const string IdBack = "back";

    private sealed record QueryItem(string Id, string Label);

    public static async Task RunAsync(IPilldueApp app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        while (!cancellationToken.IsCancellationRequested)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[bold]{UiLocalizer.Get("Plan.Title").EscapeMarkup()}[/]");
            AnsiConsole.WriteLine();

            var items = new[]
            {
                new QueryItem(IdCoverage, UiLocalizer.Get("Plan.Coverage")),
                new QueryItem(IdShort, UiLocalizer.Get("Plan.Short")),
                new QueryItem(IdExtra, UiLocalizer.Get("Plan.Extra")),
                new QueryItem(IdBack, UiLocalizer.Get("Common.Back")),
            };

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<QueryItem>()
                    .Title(UiLocalizer.Get("Plan.Choose"))
                    .UseConverter(i => i.Label)
                    .AddChoices(items));

            if (choice.Id == IdBack)
            {
                return;
            }

            var asOf = PromptAsOfDate();
            AnsiConsole.WriteLine();

            switch (choice.Id)
            {
                case IdCoverage:
                    await ShowCoverageAsync(app, asOf, cancellationToken).ConfigureAwait(false);
                    break;
                case IdShort:
                    await ShowShortBeforeAsync(app, asOf, cancellationToken).ConfigureAwait(false);
                    break;
                case IdExtra:
                    await ShowNeedExtraAsync(app, asOf, cancellationToken).ConfigureAwait(false);
                    break;
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[grey]{UiLocalizer.Get("Common.PressEnterContinue").EscapeMarkup()}[/]");
            Console.ReadLine();
        }
    }

    private static DateOnly PromptAsOfDate()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var raw = AnsiConsole.Prompt(
            new TextPrompt<string>(UiLocalizer.Get("Plan.AsOf"))
                .DefaultValue(today.ToString("yyyy-MM-dd"))
                .Validate(value =>
                    DateOnly.TryParseExact(value.Trim(), "yyyy-MM-dd", out _)
                        ? ValidationResult.Success()
                        : ValidationResult.Error(UiLocalizer.Get("Common.UseDateFormat"))));
        return DateOnly.ParseExact(raw.Trim(), "yyyy-MM-dd");
    }

    private static async Task ShowCoverageAsync(
        IPilldueApp app,
        DateOnly asOf,
        CancellationToken cancellationToken)
    {
        var results = await app.GetStockCoverageAsync(asOf, cancellationToken).ConfigureAwait(false);
        if (results.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]{UiLocalizer.Get("Plan.EmptyMeds").EscapeMarkup()}[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title(UiLocalizer.Format("Plan.CoverageTitle", asOf.ToString("yyyy-MM-dd")))
            .AddColumn(UiLocalizer.Get("List.ColName"))
            .AddColumn(UiLocalizer.Get("Plan.ColNext"))
            .AddColumn(UiLocalizer.Get("Plan.ColLast"))
            .AddColumn(UiLocalizer.Get("Plan.ColCovers"))
            .AddColumn(UiLocalizer.Get("Plan.ColPillsShort"))
            .AddColumn(UiLocalizer.Get("Plan.ColPackages"));

        foreach (var r in results)
        {
            table.AddRow(
                r.Medication.Name.EscapeMarkup(),
                r.NextRefillDate.ToString("yyyy-MM-dd"),
                r.LastCoveredDate?.ToString("yyyy-MM-dd") ?? "—",
                r.CoversUntilNextRefill
                    ? $"[green]{UiLocalizer.Get("Common.Yes")}[/]"
                    : $"[red]{UiLocalizer.Get("Common.No")}[/]",
                r.PillsShort.ToString(),
                r.PackagesToBuy.ToString());
        }

        AnsiConsole.Write(table);
    }

    private static async Task ShowShortBeforeAsync(
        IPilldueApp app,
        DateOnly asOf,
        CancellationToken cancellationToken)
    {
        var results = await app.ListShortBeforeNextRefillAsync(asOf, cancellationToken).ConfigureAwait(false);
        if (results.Count == 0)
        {
            AnsiConsole.MarkupLine($"[green]{UiLocalizer.Get("Plan.ShortEmpty").EscapeMarkup()}[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title(UiLocalizer.Format("Plan.ShortTitle", asOf.ToString("yyyy-MM-dd")))
            .AddColumn(UiLocalizer.Get("List.ColName"))
            .AddColumn(UiLocalizer.Get("Plan.ColNext"))
            .AddColumn(UiLocalizer.Get("Plan.ColLast"))
            .AddColumn(UiLocalizer.Get("Plan.ColPillsShort"))
            .AddColumn(UiLocalizer.Get("Plan.ColPackages"));

        foreach (var r in results)
        {
            table.AddRow(
                r.Medication.Name.EscapeMarkup(),
                r.NextRefillDate.ToString("yyyy-MM-dd"),
                r.LastCoveredDate?.ToString("yyyy-MM-dd") ?? "—",
                r.PillsShort.ToString(),
                r.PackagesToBuy.ToString());
        }

        AnsiConsole.Write(table);
    }

    private static async Task ShowNeedExtraAsync(
        IPilldueApp app,
        DateOnly asOf,
        CancellationToken cancellationToken)
    {
        var results = await app.ListNeedExtraForSecondRefillAsync(asOf, cancellationToken).ConfigureAwait(false);
        if (results.Count == 0)
        {
            AnsiConsole.MarkupLine($"[green]{UiLocalizer.Get("Plan.ExtraEmpty").EscapeMarkup()}[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title(UiLocalizer.Format("Plan.ExtraTitle", asOf.ToString("yyyy-MM-dd")))
            .AddColumn(UiLocalizer.Get("List.ColName"))
            .AddColumn(UiLocalizer.Get("Plan.ColSecond"))
            .AddColumn(UiLocalizer.Get("Plan.ColNeeded"))
            .AddColumn(UiLocalizer.Get("Plan.ColPrescribed"))
            .AddColumn(UiLocalizer.Get("Plan.ColExtra"));

        foreach (var r in results)
        {
            table.AddRow(
                r.Medication.Name.EscapeMarkup(),
                r.SecondRefillDate.ToString("yyyy-MM-dd"),
                r.PackagesNeeded.ToString(),
                r.PrescribedPackageCount.ToString(),
                r.ExtraPackages.ToString());
        }

        AnsiConsole.Write(table);
    }
}
