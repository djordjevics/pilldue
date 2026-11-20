using System.Windows;
using System.Windows.Controls;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public partial class BulkRefillWindow : Window
{
    private readonly IPilldueApp _app;
    private readonly List<BulkRefillRow> _rows = new();
    private bool _confirming;

    public bool Changed { get; private set; }

    public int RefilledCount { get; private set; }

    public BulkRefillWindow(IPilldueApp app)
    {
        _app = app ?? throw new ArgumentNullException(nameof(app));
        InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var medications = await _app.ListMedicationsAsync();
            _rows.Clear();
            _rows.AddRange(medications.Select(medication => new BulkRefillRow
            {
                MedicationId = medication.Id,
                Name = medication.Name,
                PackageCountText = BulkRefillPass.StartingPackageCount(medication).ToString(),
            }));
            MedicationsGrid.ItemsSource = _rows;
            EmptyText.Visibility = _rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, UiLocalizer.Get("BulkRefill.Title"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        if (_confirming)
        {
            return;
        }

        MedicationsGrid.CommitEdit(DataGridEditingUnit.Cell, true);
        MedicationsGrid.CommitEdit(DataGridEditingUnit.Row, true);

        if (!TryReadLines(out var lines))
        {
            MessageBox.Show(
                this,
                UiLocalizer.Get("Common.MustBeZeroOrGreater"),
                UiLocalizer.Get("BulkRefill.Title"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        _confirming = true;
        try
        {
            var saved = await BulkRefillPass.ApplyAsync(_app, lines);
            RefilledCount = saved;
            Changed = saved > 0;
            DialogResult = true;
            Close();
        }
        catch (ArgumentOutOfRangeException)
        {
            MessageBox.Show(
                this,
                UiLocalizer.Get("Common.MustBeZeroOrGreater"),
                UiLocalizer.Get("BulkRefill.Title"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            Changed = lines.Any(line => line.PackageCount >= 1);
            MessageBox.Show(this, ex.Message, UiLocalizer.Get("BulkRefill.Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            DialogResult = false;
            Close();
        }
        finally
        {
            _confirming = false;
        }
    }

    private bool TryReadLines(out List<BulkRefillLine> lines)
    {
        lines = new List<BulkRefillLine>(_rows.Count);
        foreach (var row in _rows)
        {
            var text = row.PackageCountText?.Trim() ?? "";
            if (!int.TryParse(text, out var count) || count < 0)
            {
                lines.Clear();
                return false;
            }

            lines.Add(new BulkRefillLine
            {
                MedicationId = row.MedicationId,
                PackageCount = count,
            });
        }

        return true;
    }

    private sealed class BulkRefillRow
    {
        public Guid MedicationId { get; init; }

        public required string Name { get; init; }

        public string? PackageCountText { get; set; }
    }
}
