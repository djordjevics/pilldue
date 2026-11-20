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
        if (_owner is not null)
        {
            window.Owner = _owner;
        }

        return window.ShowDialog();
    }

    private static MessageBoxImage ToImage(UiMessageKind kind) => kind switch
    {
        UiMessageKind.Warning => MessageBoxImage.Warning,
        UiMessageKind.Error => MessageBoxImage.Error,
        _ => MessageBoxImage.Information,
    };
}
