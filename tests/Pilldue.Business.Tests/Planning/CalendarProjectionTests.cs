using Pilldue.Business;

namespace Pilldue.Business.Tests.Planning;

public class CalendarProjectionTests
{
    [Fact]
    public void Evaluate_includes_med_when_last_covered_in_range()
    {
        var asOf = new DateOnly(2026, 5, 1);
        var medication = new Medication
        {
            Name = "Covered",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 28,
            PrescriptionStartDate = new DateOnly(2025, 1, 1),
            PrescriptionDurationMonths = 6,
        };

        var entry = CalendarProjection.Evaluate(
            medication,
            rangeStart: new DateOnly(2026, 5, 1),
            rangeEnd: new DateOnly(2026, 5, 31),
            asOfDate: asOf);

        Assert.NotNull(entry);
        Assert.Equal(new DateOnly(2026, 5, 28), entry.LastCoveredDate);
        Assert.Equal(new DateOnly(2025, 7, 1), entry.PrescriptionEndDate);
    }

    [Fact]
    public void Evaluate_includes_med_when_prescription_end_in_range()
    {
        var medication = new Medication
        {
            Name = "RxEnd",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 100,
            PrescriptionStartDate = new DateOnly(2026, 1, 1),
            PrescriptionDurationMonths = 6,
        };

        var entry = CalendarProjection.Evaluate(
            medication,
            rangeStart: new DateOnly(2026, 6, 1),
            rangeEnd: new DateOnly(2026, 7, 31),
            asOfDate: new DateOnly(2026, 5, 1));

        Assert.NotNull(entry);
        Assert.Equal(new DateOnly(2026, 7, 1), entry.PrescriptionEndDate);
    }

    [Fact]
    public void Evaluate_returns_null_when_neither_date_in_range()
    {
        var medication = new Medication
        {
            Name = "Out",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 5,
            PrescriptionStartDate = new DateOnly(2025, 1, 1),
            PrescriptionDurationMonths = 6,
        };

        var entry = CalendarProjection.Evaluate(
            medication,
            rangeStart: new DateOnly(2026, 8, 1),
            rangeEnd: new DateOnly(2026, 8, 31),
            asOfDate: new DateOnly(2026, 5, 1));

        Assert.Null(entry);
    }

    [Fact]
    public async Task GetCalendarAsync_returns_overlapping_entries_only()
    {
        var app = new PilldueApp(
            new InMemoryMedicationRepository(),
            new InMemoryRefillEventRepository(),
            new InMemorySkipDoseEventRepository(),
            new InMemoryAppConfigStore());

        await app.AddMedicationAsync(new Medication
        {
            Name = "InRange",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 10,
            PrescriptionStartDate = new DateOnly(2025, 1, 1),
        });
        await app.AddMedicationAsync(new Medication
        {
            Name = "OutOfRange",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 200,
            PrescriptionStartDate = new DateOnly(2024, 1, 1),
        });

        var entries = await app.GetCalendarAsync(
            rangeStart: new DateOnly(2026, 5, 1),
            rangeEnd: new DateOnly(2026, 5, 31),
            asOfDate: new DateOnly(2026, 5, 1));

        var entry = Assert.Single(entries);
        Assert.Equal("InRange", entry.Medication.Name);
        Assert.Equal(new DateOnly(2026, 5, 10), entry.LastCoveredDate);
    }
}
