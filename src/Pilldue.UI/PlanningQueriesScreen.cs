using Spectre.Console;
using Pilldue.Business;

namespace Pilldue.UI;

/// <summary>
/// Spectre screens for flow 1 planning queries (coverage / short / need-extra).
/// </summary>
internal static class PlanningQueriesScreen
{
    private const string Coverage = "Stock covers until next refill?";
    private const string ShortBefore = "Short before next refill";
    private const string NeedExtra = "Need extra packages for second refill";
    private const string Back = "Back";

    public static async Task RunAsync(IPilldueApp app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        while (!cancellationToken.IsCancellationRequested)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold]Planning queries[/]");
            AnsiConsole.WriteLine();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose a query")
                    .AddChoices(Coverage, ShortBefore, NeedExtra, Back));

            if (choice == Back)
            {
                return;
            }

            var asOf = PromptAsOfDate();
            AnsiConsole.WriteLine();

            switch (choice)
            {
                case Coverage:
                    await ShowCoverageAsync(app, asOf, cancellationToken).ConfigureAwait(false);
                    break;
                case ShortBefore:
                    await ShowShortBeforeAsync(app, asOf, cancellationToken).ConfigureAwait(false);
                    break;
                case NeedExtra:
                    await ShowNeedExtraAsync(app, asOf, cancellationToken).ConfigureAwait(false);
                    break;
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press Enter to continue…[/]");
            Console.ReadLine();
        }
    }

    private static DateOnly PromptAsOfDate()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var raw = AnsiConsole.Prompt(
            new TextPrompt<string>("As-of date (yyyy-MM-dd):")
                .DefaultValue(today.ToString("yyyy-MM-dd"))
                .Validate(value =>
                    DateOnly.TryParseExact(value.Trim(), "yyyy-MM-dd", out _)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Use yyyy-MM-dd.")));
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
            AnsiConsole.MarkupLine("[yellow]No medications.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title($"Stock coverage as of {asOf:yyyy-MM-dd}")
            .AddColumn("Name")
            .AddColumn("Next refill")
            .AddColumn("Last covered")
            .AddColumn("Covers?")
            .AddColumn("Pills short")
            .AddColumn("Packages to buy");

        foreach (var r in results)
        {
            table.AddRow(
                r.Medication.Name.EscapeMarkup(),
                r.NextRefillDate.ToString("yyyy-MM-dd"),
                r.LastCoveredDate?.ToString("yyyy-MM-dd") ?? "—",
                r.CoversUntilNextRefill ? "[green]yes[/]" : "[red]no[/]",
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
            AnsiConsole.MarkupLine("[green]No medications are short before the next refill.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title($"Short before next refill (as of {asOf:yyyy-MM-dd})")
            .AddColumn("Name")
            .AddColumn("Next refill")
            .AddColumn("Last covered")
            .AddColumn("Pills short")
            .AddColumn("Packages to buy");

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
            AnsiConsole.MarkupLine("[green]No medications need extra packages for the second refill day.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title($"Need extra for second refill (as of {asOf:yyyy-MM-dd})")
            .AddColumn("Name")
            .AddColumn("Second refill")
            .AddColumn("Packages needed")
            .AddColumn("Prescribed")
            .AddColumn("Extra");

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
