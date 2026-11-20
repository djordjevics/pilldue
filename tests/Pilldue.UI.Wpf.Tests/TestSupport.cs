using Pilldue.Business;
using Pilldue.UI.Wpf;

namespace Pilldue.UI.Wpf.Tests;

internal sealed class TestSession
{
    public TestSession()
    {
        Medications = new InMemoryMedicationRepository();
        Refills = new InMemoryRefillEventRepository();
        Skips = new InMemorySkipDoseEventRepository();
        Missed = new InMemoryMissedDoseEventRepository();
        Config = new InMemoryAppConfigStore(new AppConfig
        {
            UiLanguage = AppConfig.EnglishLanguage,
            UiTheme = AppConfig.ThemeLight,
        });
        App = new PilldueApp(Medications, Refills, Skips, Missed, Config);
    }

    public InMemoryMedicationRepository Medications { get; }

    public InMemoryRefillEventRepository Refills { get; }

    public InMemorySkipDoseEventRepository Skips { get; }

    public InMemoryMissedDoseEventRepository Missed { get; }

    public InMemoryAppConfigStore Config { get; }

    public PilldueApp App { get; }

    public static DateOnly Today { get; } = new(2026, 5, 5);

    public static Medication Sample(string name = "Aspirin") => new()
    {
        Name = name,
        PackageSizePills = 28,
        PrescribedPackageCount = 1,
        DailyDosagePills = 1,
        DoseIntervalDays = 1,
        CurrentStockPills = 10,
        PrescriptionStartDate = new DateOnly(2026, 1, 5),
        PrescriptionDurationMonths = 6,
    };
}

internal sealed class FakeUiInteraction : IUiInteraction
{
    public List<(string Message, string Title, UiMessageKind Kind)> Messages { get; } = new();

    public bool ConfirmResult { get; set; }

    public string? LastConfirmMessage { get; private set; }

    public int ConfirmCalls { get; private set; }

    public Func<AddMedicationViewModel, bool>? Add { get; set; }

    public Func<RefillViewModel, bool>? Refill { get; set; }

    public Func<SkipDoseViewModel, bool>? Skip { get; set; }

    public Func<FlagMissedDoseViewModel, bool>? Flag { get; set; }

    public Func<SettingsViewModel, bool>? Settings { get; set; }

    public Action<MissedDoseViewModel>? Missed { get; set; }

    public StockCoverageViewModel? CoverageShown { get; private set; }

    public CalendarViewModel? CalendarShown { get; private set; }

    public MissedDoseViewModel? MissedShown { get; private set; }

    public void ShowMessage(string message, string title, UiMessageKind kind) =>
        Messages.Add((message, title, kind));

    public bool Confirm(string message, string title)
    {
        ConfirmCalls++;
        LastConfirmMessage = message;
        return ConfirmResult;
    }

    public bool ShowAddMedication(AddMedicationViewModel viewModel) =>
        Add?.Invoke(viewModel) ?? false;

    public bool ShowRefill(RefillViewModel viewModel) =>
        Refill?.Invoke(viewModel) ?? false;

    public bool ShowSkipDose(SkipDoseViewModel viewModel) =>
        Skip?.Invoke(viewModel) ?? false;

    public void ShowStockCoverage(StockCoverageViewModel viewModel) =>
        CoverageShown = viewModel;

    public void ShowCalendar(CalendarViewModel viewModel) =>
        CalendarShown = viewModel;

    public void ShowMissedDoses(MissedDoseViewModel viewModel)
    {
        MissedShown = viewModel;
        Missed?.Invoke(viewModel);
    }

    public bool ShowFlagMissedDose(FlagMissedDoseViewModel viewModel) =>
        Flag?.Invoke(viewModel) ?? false;

    public bool ShowSettings(SettingsViewModel viewModel) =>
        Settings?.Invoke(viewModel) ?? false;
}
