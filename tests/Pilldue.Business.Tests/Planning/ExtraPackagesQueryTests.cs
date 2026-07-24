using Pilldue.Business;

namespace Pilldue.Business.Tests.Planning;

public class ExtraPackagesQueryTests
{
        [Fact]
    public void Evaluate_on_refill_day_empty_stock_needs_2_packages_for_31_day_gap()
    {
        var today = new DateOnly(2026, 5, 5);
        var medication = new Medication
        {
            Name = "Example",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 0,
            PrescriptionStartDate = new DateOnly(2026, 1, 5),
        };

        var result = ExtraPackagesQuery.Evaluate(medication, today);

        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2026, 6, 5), result.SecondRefillDate);
        Assert.Equal(2, result.PackagesNeeded);
        Assert.Equal(1, result.PrescribedPackageCount);
        Assert.Equal(1, result.ExtraPackages);
    }

    [Fact]
    public void Evaluate_returns_null_when_prescribed_packages_suffice()
    {
        var today = new DateOnly(2026, 5, 5);
        var medication = new Medication
        {
            Name = "Enough",
            PackageSizePills = 28,
            PrescribedPackageCount = 2,
            DailyDosagePills = 1,
            CurrentStockPills = 0,
            PrescriptionStartDate = new DateOnly(2026, 1, 5),
        };

        Assert.Null(ExtraPackagesQuery.Evaluate(medication, today));
    }

    [Fact]
    public async Task ListNeedExtraForSecondRefillAsync_only_returns_meds_needing_extra()
    {
        var app = new PilldueApp(
            new InMemoryMedicationRepository(),
            new InMemoryRefillEventRepository(),
            new InMemorySkipDoseEventRepository(),
            new InMemoryAppConfigStore());

        await app.AddMedicationAsync(new Medication
        {
            Name = "NeedsExtra",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 0,
            PrescriptionStartDate = new DateOnly(2026, 1, 5),
        });
        await app.AddMedicationAsync(new Medication
        {
            Name = "Ok",
            PackageSizePills = 28,
            PrescribedPackageCount = 2,
            DailyDosagePills = 1,
            CurrentStockPills = 0,
            PrescriptionStartDate = new DateOnly(2026, 1, 5),
        });

        var results = await app.ListNeedExtraForSecondRefillAsync(new DateOnly(2026, 5, 5));

        var result = Assert.Single(results);
        Assert.Equal("NeedsExtra", result.Medication.Name);
        Assert.Equal(2, result.PackagesNeeded);
        Assert.Equal(1, result.ExtraPackages);
    }
}
