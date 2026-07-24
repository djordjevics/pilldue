using Pilldue.Business;

namespace Pilldue.IntegrationTests;

/// <summary>
/// E2: 28 pills vs 31-day May→June gap — shortfall and packages-to-buy.
/// </summary>
public class ShortBeforeRefillScenarios
{
    [Fact]
    public async Task Twenty_eight_pills_vs_thirty_one_day_gap_is_short_with_package_suggestion()
    {
        var medications = new InMemoryMedicationRepository();
        var app = new PilldueApp(
            medications,
            new InMemoryRefillEventRepository(),
            new InMemorySkipDoseEventRepository(),
            new InMemoryAppConfigStore());

        // Documented example: refill day 5, stock 28, dosage 1, package 28.
        // 5 May → 5 June = 31 days → 3 pills short → ceil(3/28)=1 package to close the gap
        // (full gap from empty needs ceil(31/28)=2 packages for the cycle).
        await app.AddMedicationAsync(new Medication
        {
            Name = "ExampleMed",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 28,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
        });

        var asOf = new DateOnly(2026, 5, 5);
        var shortList = await app.ListShortBeforeNextRefillAsync(asOf);
        var result = Assert.Single(shortList);

        Assert.Equal(new DateOnly(2026, 6, 5), result.NextRefillDate);
        Assert.Equal(3, result.PillsShort);
        Assert.Equal(1, result.PackagesToBuy);
        Assert.Equal(new DateOnly(2026, 6, 1), result.LastCoveredDate);
        Assert.False(result.CoversUntilNextRefill);

        // Full packages for a 31-day empty cycle (docs: suggest buying 2).
        Assert.Equal(2, RefillCalendarRules.PackagesToBuy(31, 28));
        Assert.Equal(2, result.PackagesToBuy + result.Medication.PrescribedPackageCount);
    }
}
