using Pilldue.Business;
using Pilldue.UI.Localization;
using Pilldue.UI.Wpf;

namespace Pilldue.UI.Wpf.Tests;

public class QueryViewModelTests
{
    [Fact]
    public async Task Coverage_load_maps_query_rows()
    {
        var session = new TestSession();
        await session.App.AddMedicationAsync(ShortMed());
        var vm = new StockCoverageViewModel(session.App, new FakeUiInteraction(), () => TestSession.Today);
        await vm.Loaded;

        var coverage = await session.App.GetStockCoverageAsync(TestSession.Today);
        var shortList = await session.App.ListShortBeforeNextRefillAsync(TestSession.Today);
        var extra = await session.App.ListNeedExtraForSecondRefillAsync(TestSession.Today);

        Assert.Equal(UiLocalizer.Format("Plan.CoverageTitle", "2026-05-05"), vm.AsOfText);
        Assert.Equal(coverage.Count, vm.CoverageRows.Count);
        Assert.Equal(coverage[0].Medication.Name, vm.CoverageRows[0].Name);
        Assert.Equal(coverage[0].NextRefillDate.ToString("yyyy-MM-dd"), vm.CoverageRows[0].NextRefill);
        Assert.Equal(coverage[0].LastCoveredDate?.ToString("yyyy-MM-dd") ?? "", vm.CoverageRows[0].LastCovered);
        Assert.Equal(
            UiLocalizer.Get(coverage[0].CoversUntilNextRefill ? "Common.Yes" : "Common.No"),
            vm.CoverageRows[0].Covers);
        Assert.Equal(coverage[0].PillsShort, vm.CoverageRows[0].PillsShort);
        Assert.Equal(coverage[0].PackagesToBuy, vm.CoverageRows[0].PackagesToBuy);

        Assert.Equal(shortList.Count, vm.ShortRows.Count);
        Assert.Equal(shortList[0].PillsShort, vm.ShortRows[0].PillsShort);
        Assert.Equal(shortList[0].PackagesToBuy, vm.ShortRows[0].PackagesToBuy);
        Assert.Equal(extra.Count, vm.ExtraRows.Count);
        Assert.Equal(extra[0].ExtraPackages, vm.ExtraRows[0].ExtraPackages);
        Assert.Equal(extra[0].SecondRefillDate.ToString("yyyy-MM-dd"), vm.ExtraRows[0].SecondRefill);
        Assert.False(vm.IsCoverageEmpty);

        var empty = new StockCoverageViewModel(new TestSession().App, new FakeUiInteraction(), () => TestSession.Today);
        await empty.Loaded;
        Assert.True(empty.IsCoverageEmpty);
        Assert.True(empty.IsShortEmpty);
        Assert.True(empty.IsExtraEmpty);
    }

    [Fact]
    public async Task Calendar_load_maps_entry_rows()
    {
        var session = new TestSession();
        await session.App.AddMedicationAsync(ShortMed());
        var vm = new CalendarViewModel(session.App, new FakeUiInteraction(), () => TestSession.Today);
        await vm.Loaded;

        var calendar = await session.App.GetCalendarAsync(TestSession.Today);
        var entry = Assert.Single(calendar.Entries);
        var row = Assert.Single(vm.Rows);
        Assert.Equal(
            UiLocalizer.Format(
                "Cal.RangeLabel",
                calendar.RangeStart.ToString("yyyy-MM-dd"),
                calendar.RangeEnd.ToString("yyyy-MM-dd")),
            vm.RangeText);
        Assert.Equal(entry.Medication.Name, row.Name);
        Assert.Equal(entry.FirstRefillDate.ToString("yyyy-MM-dd"), row.FirstRefill);
        Assert.Equal(entry.SecondRefillDate.ToString("yyyy-MM-dd"), row.SecondRefill);
        Assert.Equal(
            string.Join(", ", entry.StockOutDates.Select(date => date.ToString("yyyy-MM-dd"))),
            row.StockOut);
        Assert.Equal(entry.PrescriptionEndDate.ToString("yyyy-MM-dd"), row.PrescriptionEnd);
        Assert.False(vm.IsEmpty);

        var empty = new CalendarViewModel(new TestSession().App, new FakeUiInteraction(), () => TestSession.Today);
        await empty.Loaded;
        Assert.True(empty.IsEmpty);
        Assert.Empty(empty.Rows);
    }

    [Fact]
    public async Task Missed_dose_flag_records_the_date_without_changing_stock()
    {
        var session = new TestSession();
        var med = TestSession.Sample();
        await session.App.AddMedicationAsync(med);
        var interaction = new FakeUiInteraction();
        var vm = new MissedDoseViewModel(session.App, interaction, med, () => TestSession.Today);
        await vm.Loaded;
        Assert.True(vm.IsEmpty);

        interaction.Flag = flag =>
        {
            flag.DateText = "not-a-date";
            flag.ConfirmCommand.Execute(null);
            return flag.CloseResult == true;
        };
        await vm.FlagCommand.ExecuteAsync(null);
        Assert.False(vm.Flagged);
        Assert.Empty(await session.App.ListMissedDosesAsync());

        interaction.Flag = flag =>
        {
            flag.DateText = "2026-05-04";
            flag.ConfirmCommand.Execute(null);
            return flag.CloseResult == true;
        };
        await vm.FlagCommand.ExecuteAsync(null);

        Assert.True(vm.Flagged);
        Assert.Equal(new DateOnly(2026, 5, 4), vm.FlaggedDate);
        var row = Assert.Single(vm.Rows);
        Assert.Equal("Aspirin", row.Medication);
        Assert.Equal("2026-05-04", row.Date);
        var stored = Assert.Single(await session.App.ListMedicationsAsync());
        Assert.Equal(10, stored.CurrentStockPills);
        Assert.False(vm.IsEmpty);

        var unselected = new MissedDoseViewModel(session.App, interaction, null, () => TestSession.Today);
        await unselected.FlagCommand.ExecuteAsync(null);
        Assert.Contains(
            interaction.Messages,
            message => message.Message == UiLocalizer.Get("Desktop.SelectFirst")
                && message.Kind == UiMessageKind.Information);
    }

    private static Medication ShortMed() => new()
    {
        Name = "FacadeMed",
        PackageSizePills = 28,
        PrescribedPackageCount = 1,
        DailyDosagePills = 1,
        DoseIntervalDays = 1,
        CurrentStockPills = 0,
        PrescriptionStartDate = new DateOnly(2026, 1, 5),
        PrescriptionDurationMonths = 6,
    };
}
