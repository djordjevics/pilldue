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
        if (_owner is null)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, image);
            return;
        }

        MessageBox.Show(_owner, message, title, MessageBoxButton.OK, image);
    }

    public bool Confirm(string message, string title)
    {
        var result = _owner is null
            ? MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning)
            : MessageBox.Show(_owner, message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        return result == MessageBoxResult.Yes;
    }

    public bool ShowAddMedication(AddMedicationViewModel viewModel) =>
        Show(new AddMedicationWindow(), viewModel) == true;

    public bool ShowRefill(RefillViewModel viewModel) =>
        Show(new RefillWindow(), viewModel) == true;

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
        var owner = ActiveWindow(window) ?? _owner;
        if (owner is not null)
        {
            window.Owner = owner;
        }

        return window.ShowDialog();
    }

    /// <summary>
    /// A dialog opened from another dialog (the missed-dose list opening the flag dialog)
    /// is owned by that active window. The main window stays the owner for the first dialog.
    /// </summary>
    private static Window? ActiveWindow(Window dialog)
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
