using Pilldue.Business;

namespace Pilldue.Business.Tests.Planning;

public class ExtraPackagesQueryTests
{
    private static readonly AppConfig DefaultConfig = new() { DefaultRefillDayOfMonth = 5 };

    private static PilldueApp CreateApp()
    {
        return new PilldueApp(
            new InMemoryMedicationRepository(),
            new InMemoryRefillEventRepository(),
            new InMemorySkipDoseEventRepository(),
            new InMemoryAppConfigStore());
    }

    [Fact]
    public void Evaluate_28_pills_on_May5_needs_extra_for_July5()
    {
        // May 5 → July 5 = 61 days; stock 28 @ 1/day → 33 short → ceil(33/28)=2 packages;
        // prescribed 1 → 1 extra. Formula: packagesNeeded = ceil(pillsShort / packageSize).
        var medication = new Medication
        {
            Name = "Short",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 28,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
        };

        var result = ExtraPackagesQuery.Evaluate(medication, DefaultConfig, new DateOnly(2026, 5, 5));

        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2026, 7, 5), result.SecondRefillDate);
        Assert.Equal(2, result.PackagesNeeded);
        Assert.Equal(1, result.PrescribedPackageCount);
        Assert.Equal(1, result.ExtraPackages);
    }

    [Fact]
    public void Evaluate_returns_null_when_prescribed_packages_suffice()
    {
        var medication = new Medication
        {
            Name = "Covered",
            PackageSizePills = 28,
            PrescribedPackageCount = 2,
            DailyDosagePills = 1,
            CurrentStockPills = 28,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
        };

        var result = ExtraPackagesQuery.Evaluate(medication, DefaultConfig, new DateOnly(2026, 5, 5));

        Assert.Null(result);
    }

    [Fact]
    public async Task ListNeedExtraForSecondRefillAsync_filters_to_meds_needing_extra()
    {
        var app = CreateApp();
        await app.AddMedicationAsync(new Medication
        {
            Name = "NeedsExtra",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 28,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
        });
        await app.AddMedicationAsync(new Medication
        {
            Name = "Ok",
            PackageSizePills = 28,
            PrescribedPackageCount = 3,
            DailyDosagePills = 1,
            CurrentStockPills = 60,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
        });

        var results = await app.ListNeedExtraForSecondRefillAsync(new DateOnly(2026, 5, 5));

        var single = Assert.Single(results);
        Assert.Equal("NeedsExtra", single.Medication.Name);
        Assert.True(single.ExtraPackages > 0);
    }
}
