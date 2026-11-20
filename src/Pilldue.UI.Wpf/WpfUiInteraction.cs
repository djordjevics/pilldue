using System.Windows;

namespace Pilldue.UI.Wpf;

public sealed class WpfUiInteraction : IUiInteraction
{
    private Window? _owner;

    public void SetOwner(Window owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        _owner = owner;
    }

    public void ShowMessage(string message, string title, UiMessageKind kind)
    {
        var image = ToImage(kind);
        var owner = PromptOwner(null);
        if (owner is null)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, image);
            return;
        }

        MessageBox.Show(owner, message, title, MessageBoxButton.OK, image);
    }

    public bool Confirm(string message, string title)
    {
        var owner = PromptOwner(null);
        var result = owner is null
            ? MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning)
            : MessageBox.Show(owner, message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        return result == MessageBoxResult.Yes;
    }

    public bool ShowAddMedication(AddMedicationViewModel viewModel) =>
        Show(new AddMedicationWindow(), viewModel) == true;

    public bool ShowRefill(RefillViewModel viewModel) =>
        Show(new RefillWindow(), viewModel) == true;

    public bool ShowBulkRefill(BulkRefillViewModel viewModel) =>
        Show(new BulkRefillWindow(), viewModel) == true;

    public bool ShowSkipDose(SkipDoseViewModel viewModel) =>
        Show(new SkipDoseWindow(), viewModel) == true;

    public void ShowStockCoverage(StockCoverageViewModel viewModel) =>
        Show(new StockCoverageWindow(), viewModel);

    public void ShowCalendar(CalendarViewModel viewModel) =>
        Show(new CalendarWindow(), viewModel);

    public void ShowMissedDoses(MissedDoseViewModel viewModel) =>
        Show(new MissedDoseWindow(), viewModel);

    public bool ShowFlagMissedDose(FlagMissedDoseViewModel viewModel) =>
        Show(new FlagMissedDoseWindow(), viewModel) == true;

    public bool ShowSettings(SettingsViewModel viewModel) =>
        Show(new SettingsWindow(), viewModel) == true;

    private bool? Show(Window window, object viewModel)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(viewModel);
        window.DataContext = viewModel;
        var owner = PromptOwner(window);
        if (owner is not null)
        {
            window.Owner = owner;
        }

        return window.ShowDialog();
    }

    /// <summary>
    /// A prompt opened while another dialog is active is owned by that window,
    /// so it stays in front of the modal. The main window is the fallback.
    /// </summary>
    private Window? PromptOwner(Window? dialog) => ActiveWindow(dialog) ?? _owner;

    private static Window? ActiveWindow(Window? dialog)
    {
        var application = Application.Current;
        if (application is null)
        {
            return null;
        }

        foreach (Window open in application.Windows)
        {
            if (open.IsActive && !ReferenceEquals(open, dialog))
            {
                return open;
            }
        }

        return null;
    }

    private static MessageBoxImage ToImage(UiMessageKind kind) => kind switch
    {
        UiMessageKind.Warning => MessageBoxImage.Warning,
        UiMessageKind.Error => MessageBoxImage.Error,
        _ => MessageBoxImage.Information,
    };
}
