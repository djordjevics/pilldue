using Pilldue.Business;

namespace Pilldue.Business.Tests.Planning;

public class ShortBeforeRefillQueryTests
{
    private static PilldueApp CreateApp()
    {
        return new PilldueApp(
            new InMemoryMedicationRepository(),
            new InMemoryRefillEventRepository(),
            new InMemorySkipDoseEventRepository(),
            new InMemoryAppConfigStore());
    }

    [Fact]
    public async Task ListShortBeforeNextRefillAsync_includes_28_vs_31_example()
    {
        var app = CreateApp();
        await app.AddMedicationAsync(new Medication
        {
            Name = "Short",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 28,
            PrescriptionStartDate = new DateOnly(2026, 1, 5),
        });
        await app.AddMedicationAsync(new Medication
        {
            Name = "Covered",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 31,
            PrescriptionStartDate = new DateOnly(2026, 1, 5),
        });

        var today = new DateOnly(2026, 5, 5);
        var shortMeds = await app.ListShortBeforeNextRefillAsync(today);

        var result = Assert.Single(shortMeds);
        Assert.Equal("Short", result.Medication.Name);
        Assert.Equal(new DateOnly(2026, 6, 5), result.NextRefillDate);
        Assert.Equal(new DateOnly(2026, 6, 1), result.LastCoveredDate);
        Assert.Equal(3, result.PillsShort);
        Assert.Equal(1, result.PackagesToBuy);
    }

    [Fact]
    public async Task ListShortBeforeNextRefillAsync_empty_when_all_cover()
    {
        var app = CreateApp();
        await app.AddMedicationAsync(new Medication
        {
            Name = "Covered",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 31,
            PrescriptionStartDate = new DateOnly(2026, 1, 5),
        });

        var shortMeds = await app.ListShortBeforeNextRefillAsync(new DateOnly(2026, 5, 5));

        Assert.Empty(shortMeds);
    }
}
