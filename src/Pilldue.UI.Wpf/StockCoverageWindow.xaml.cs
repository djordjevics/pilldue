using System.Windows;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public partial class StockCoverageWindow : Window
{
    private readonly IPilldueApp _app;

    public StockCoverageWindow(IPilldueApp app)
    {
        _app = app ?? throw new ArgumentNullException(nameof(app));
        InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        AsOfText.Text = UiLocalizer.Format("Plan.CoverageTitle", today.ToString("yyyy-MM-dd"));

        try
        {
            var coverage = await _app.GetStockCoverageAsync(today);
            var shortList = await _app.ListShortBeforeNextRefillAsync(today);
            var extra = await _app.ListNeedExtraForSecondRefillAsync(today);

            CoverageGrid.ItemsSource = coverage.Select(ToCoverageRow).ToList();
            ShortGrid.ItemsSource = shortList.Select(ToCoverageRow).ToList();
            ExtraGrid.ItemsSource = extra.Select(ToExtraRow).ToList();

            CoverageEmpty.Visibility = coverage.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            ShortEmpty.Visibility = shortList.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            ExtraEmpty.Visibility = extra.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, UiLocalizer.Get("Desktop.Coverage"), MessageBoxButton.OK, MessageBoxImage.Error);
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

    private static ExtraRow ToExtraRow(ExtraPackagesNeeded result) => new()
    {
        Name = result.Medication.Name,
        SecondRefill = result.SecondRefillDate.ToString("yyyy-MM-dd"),
        PackagesNeeded = result.PackagesNeeded,
        Prescribed = result.PrescribedPackageCount,
        ExtraPackages = result.ExtraPackages,
    };

    private sealed class CoverageRow
    {
        public required string Name { get; init; }
        public required string NextRefill { get; init; }
        public required string LastCovered { get; init; }
        public required string Covers { get; init; }
        public int PillsShort { get; init; }
        public int PackagesToBuy { get; init; }
    }

    private sealed class ExtraRow
    {
        public required string Name { get; init; }
        public required string SecondRefill { get; init; }
        public int PackagesNeeded { get; init; }
        public int Prescribed { get; init; }
        public int ExtraPackages { get; init; }
    }
}
