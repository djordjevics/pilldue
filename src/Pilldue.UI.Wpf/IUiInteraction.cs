namespace Pilldue.UI.Wpf;

public enum UiMessageKind
{
    Information,
    Warning,
    Error,
}

/// <summary>
/// Dialogs and message boxes for view models. Implementations may reference windows; view models do not.
/// </summary>
public interface IUiInteraction
{
    void ShowMessage(string message, string title, UiMessageKind kind);

    bool Confirm(string message, string title);

    bool ShowAddMedication(AddMedicationViewModel viewModel);

    bool ShowRefill(RefillViewModel viewModel);

    bool ShowBulkRefill(BulkRefillViewModel viewModel);

    bool ShowSkipDose(SkipDoseViewModel viewModel);

    void ShowStockCoverage(StockCoverageViewModel viewModel);

    void ShowCalendar(CalendarViewModel viewModel);

    void ShowMissedDoses(MissedDoseViewModel viewModel);

    bool ShowFlagMissedDose(FlagMissedDoseViewModel viewModel);

    bool ShowSettings(SettingsViewModel viewModel);
}
