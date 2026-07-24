using Pilldue.Business;

namespace Pilldue.IntegrationTests;

/// <summary>
/// End-to-end scenarios: perform several Business actions, then assert outcomes.
/// No Spectre.Console / UI — drive <see cref="PilldueApp"/> like a scripted user session.
/// Uses in-memory ports (EF repositories land in B3–B5); SQLite schema is covered elsewhere.
/// </summary>
public class RefillWorkflowScenarios
{
    private static readonly DateOnly AsOf = new(2026, 5, 1);
    private static readonly DateOnly RefillDate = new(2026, 5, 5);
    private static readonly DateOnly SkipDate = new(2026, 5, 6);

    [Fact]
    public async Task Add_medication_then_see_refill_by_date()
    {
        var (app, _, _, _) = CreateApp();

        var med = await app.AddMedicationAsync(CreateMed(stock: 10));

        var listed = Assert.Single(await app.ListMedicationsAsync());
        Assert.Equal(med.Id, listed.Id);
        Assert.Equal("Atorvastatin", listed.Name);

        var config = await app.GetConfigAsync();
        var day = RefillCalendarRules.EffectiveRefillDayOfMonth(listed, config);
        var (next, _) = RefillCalendarRules.NextAndSecondRefillDates(AsOf, day);
        Assert.Equal(new DateOnly(2026, 5, 5), next);
    }

    [Fact]
    public async Task Skip_dose_then_stock_and_last_covered_move_out()
    {
        var (app, _, _, skips) = CreateApp();
        var med = await app.AddMedicationAsync(CreateMed(stock: 10));

        await app.SkipDoseAsync(med.Id, pillsReturned: med.DailyDosagePills, date: SkipDate);

        var loaded = Assert.Single(await app.ListMedicationsAsync());
        Assert.Equal(11, loaded.CurrentStockPills);

        var skip = Assert.Single(await skips.ListByMedicationAsync(med.Id));
        Assert.Equal(SkipDate, skip.Date);
        Assert.Equal(1, skip.PillsReturned);

        var lastCovered = RefillCalendarRules.LastCoveredDate(
            AsOf,
            loaded.CurrentStockPills,
            loaded.DailyDosagePills);
        Assert.Equal(new DateOnly(2026, 5, 11), lastCovered);
    }

    [Fact]
    public async Task Log_refill_then_history_and_stock_reflect_it()
    {
        var (app, _, refills, _) = CreateApp();
        var med = await app.AddMedicationAsync(CreateMed(stock: 0));

        await app.RefillAsync(med.Id, packageCount: 1, date: RefillDate);

        var loaded = Assert.Single(await app.ListMedicationsAsync());
        Assert.Equal(28, loaded.CurrentStockPills);

        var refill = Assert.Single(await refills.ListByMedicationAsync(med.Id));
        Assert.Equal(RefillDate, refill.Date);
        Assert.Equal(1, refill.PackageCount);

        var lastCovered = RefillCalendarRules.LastCoveredDate(
            AsOf,
            loaded.CurrentStockPills,
            loaded.DailyDosagePills);
        Assert.Equal(new DateOnly(2026, 5, 28), lastCovered);
    }

    [Fact]
    public async Task Full_path_add_refill_skip_matches_expected_stock_history_and_coverage()
    {
        var (app, _, refills, skips) = CreateApp();

        // 1. Add med (stock 10, dose 1, package 28)
        var med = await app.AddMedicationAsync(CreateMed(stock: 10));

        // 2. Log refill (+1 pack → +28)
        await app.RefillAsync(med.Id, packageCount: 1, date: RefillDate);

        // 3. Flag skipped dose (+1)
        await app.SkipDoseAsync(med.Id, pillsReturned: med.DailyDosagePills, date: SkipDate);

        var loaded = Assert.Single(await app.ListMedicationsAsync());
        Assert.Equal(39, loaded.CurrentStockPills);

        var refill = Assert.Single(await refills.ListByMedicationAsync(med.Id));
        Assert.Equal(1, refill.PackageCount);
        Assert.Equal(RefillDate, refill.Date);

        var skip = Assert.Single(await skips.ListByMedicationAsync(med.Id));
        Assert.Equal(1, skip.PillsReturned);
        Assert.Equal(SkipDate, skip.Date);

        // Inclusive last-covered: floor(39/1)=39 → asOf + 38 days = 8 Jun
        var lastCovered = RefillCalendarRules.LastCoveredDate(
            AsOf,
            loaded.CurrentStockPills,
            loaded.DailyDosagePills);
        Assert.Equal(new DateOnly(2026, 6, 8), lastCovered);
    }

    private static (
        PilldueApp App,
        InMemoryMedicationRepository Medications,
        InMemoryRefillEventRepository Refills,
        InMemorySkipDoseEventRepository Skips) CreateApp()
    {
        var medications = new InMemoryMedicationRepository();
        var refills = new InMemoryRefillEventRepository();
        var skips = new InMemorySkipDoseEventRepository();
        var config = new InMemoryAppConfigStore();
        var app = new PilldueApp(medications, refills, skips, config);
        return (app, medications, refills, skips);
    }

    private static Medication CreateMed(int stock) => new()
    {
        Name = "Atorvastatin",
        PackageSizePills = 28,
        PrescribedPackageCount = 1,
        DailyDosagePills = 1,
        CurrentStockPills = stock,
        PrescriptionStartDate = new DateOnly(2026, 1, 1),
    };
}
