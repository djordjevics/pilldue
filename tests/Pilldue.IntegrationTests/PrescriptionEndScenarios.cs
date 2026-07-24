using Pilldue.Business;

namespace Pilldue.IntegrationTests;

/// <summary>
/// E3: prescription start → derived end (~6 months) available for calendar/planning.
/// </summary>
public class PrescriptionEndScenarios
{
    [Fact]
    public async Task Add_medication_exposes_prescription_end_on_calendar()
    {
        var app = new PilldueApp(
            new InMemoryMedicationRepository(),
            new InMemoryRefillEventRepository(),
            new InMemorySkipDoseEventRepository(),
            new InMemoryAppConfigStore(new AppConfig { DefaultRefillDayOfMonth = 6 }));

        var start = new DateOnly(2026, 1, 15);
        var med = await app.AddMedicationAsync(new Medication
        {
            Name = "RxMed",
            PackageSizePills = 28,
            PrescribedPackageCount = 1,
            DailyDosagePills = 1,
            CurrentStockPills = 10,
            PrescriptionStartDate = start,
            PrescriptionDurationMonths = 6,
        });

        var expectedEnd = RefillCalendarRules.PrescriptionEndDate(med);
        Assert.Equal(new DateOnly(2026, 7, 15), expectedEnd);

        var asOf = new DateOnly(2026, 5, 1);
        var view = await app.GetCalendarAsync(asOf);

        var entry = Assert.Single(view.Entries);
        Assert.Equal(med.Id, entry.Medication.Id);
        Assert.Equal(expectedEnd, entry.PrescriptionEndDate);
        Assert.Equal(new DateOnly(2026, 5, 15), entry.FirstRefillDate);
    }
}
