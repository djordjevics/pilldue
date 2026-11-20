using CommunityToolkit.Mvvm.ComponentModel;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public sealed class CoverageRow
{
    public required string Name { get; init; }

    public required string NextRefill { get; init; }

    public required string LastCovered { get; init; }

    public required string Covers { get; init; }

    public int PillsShort { get; init; }

    public int PackagesToBuy { get; init; }
}

public sealed class ExtraPackagesRow
{
    public required string Name { get; init; }

    public required string SecondRefill { get; init; }

    public int PackagesNeeded { get; init; }

    public int Prescribed { get; init; }

    public int ExtraPackages { get; init; }
}

public partial class StockCoverageViewModel : ObservableObject
{
    private readonly IPilldueApp _app;
    private readonly IUiInteraction _interaction;
    private readonly Func<DateOnly> _today;

    public StockCoverageViewModel(IPilldueApp app, IUiInteraction interaction, Func<DateOnly> today)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(interaction);
        ArgumentNullException.ThrowIfNull(today);
        _app = app;
        _interaction = interaction;
        _today = today;
        Loaded = LoadAsync();
    }

    public Task Loaded { get; }

    [ObservableProperty]
    private string _asOfText = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<CoverageRow> _coverageRows = Array.Empty<CoverageRow>();

    [ObservableProperty]
    private IReadOnlyList<CoverageRow> _shortRows = Array.Empty<CoverageRow>();

    [ObservableProperty]
    private IReadOnlyList<ExtraPackagesRow> _extraRows = Array.Empty<ExtraPackagesRow>();

    [ObservableProperty]
    private bool _isCoverageEmpty;

    [ObservableProperty]
    private bool _isShortEmpty;

    [ObservableProperty]
    private bool _isExtraEmpty;

    private async Task LoadAsync()
    {
        var today = _today();
        AsOfText = UiLocalizer.Format("Plan.CoverageTitle", today.ToString("yyyy-MM-dd"));

        try
        {
            var coverage = await _app.GetStockCoverageAsync(today);
            var shortList = await _app.ListShortBeforeNextRefillAsync(today);
            var extra = await _app.ListNeedExtraForSecondRefillAsync(today);
            CoverageRows = coverage.Select(ToCoverageRow).ToList();
            ShortRows = shortList.Select(ToCoverageRow).ToList();
            ExtraRows = extra.Select(ToExtraRow).ToList();
            IsCoverageEmpty = coverage.Count == 0;
            IsShortEmpty = shortList.Count == 0;
            IsExtraEmpty = extra.Count == 0;
        }
        catch (Exception ex)
        {
            _interaction.ShowMessage(ex.Message, UiLocalizer.Get("Desktop.Coverage"), UiMessageKind.Error);
        }
    }

    private static CoverageRow ToCoverageRow(StockCoverageResult result) => new()
    {
        Name = result.Medication.Name,
        NextRefill = result.NextRefillDate.ToString("yyyy-MM-dd"),
        LastCovered = result.LastCoveredDate?.ToString("yyyy-MM-dd") ?? "",
        Covers = UiLocalizer.Get(result.CoversUntilNextRefill ? "Common.Yes" : "Common.No"),
        PillsShort = result.PillsShort,
        PackagesToBuy = result.PackagesToBuy,
    };

    private static ExtraPackagesRow ToExtraRow(ExtraPackagesNeeded result) => new()
    {
        Name = result.Medication.Name,
        SecondRefill = result.SecondRefillDate.ToString("yyyy-MM-dd"),
        PackagesNeeded = result.PackagesNeeded,
        Prescribed = result.PrescribedPackageCount,
        ExtraPackages = result.ExtraPackages,
    };
}
