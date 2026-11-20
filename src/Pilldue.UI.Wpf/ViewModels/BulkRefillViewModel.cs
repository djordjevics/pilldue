using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

public partial class BulkRefillViewModel : DialogViewModel
{
    private readonly IPilldueApp _app;
    private readonly IUiInteraction _interaction;

    public BulkRefillViewModel(IPilldueApp app, IUiInteraction interaction)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(interaction);
        _app = app;
        _interaction = interaction;
    }

    public ObservableCollection<BulkRefillRowViewModel> Rows { get; } = new();

    public int RefilledCount { get; private set; }

    public bool Changed { get; private set; }

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isEmpty;

    public async Task LoadAsync()
    {
        var medications = await _app.ListMedicationsAsync();
        Rows.Clear();
        foreach (var medication in medications)
        {
            Rows.Add(new BulkRefillRowViewModel
            {
                MedicationId = medication.Id,
                Name = medication.Name,
                PackageCountText = BulkRefillPass.StartingPackageCount(medication).ToString(),
            });
        }

        IsEmpty = Rows.Count == 0;
    }

    [RelayCommand]
    private void Dismiss()
    {
        if (IsBusy)
        {
            return;
        }

        CloseResult = null;
        CloseResult = false;
    }

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        if (IsBusy || !TryReadLines(out var lines))
        {
            if (!IsBusy)
            {
                _interaction.ShowMessage(
                    UiLocalizer.Get("Common.MustBeZeroOrGreater"),
                    UiLocalizer.Get("BulkRefill.Title"),
                    UiMessageKind.Warning);
            }

            return;
        }

        IsBusy = true;
        try
        {
            var saved = await BulkRefillPass.ApplyAsync(_app, lines);
            RefilledCount += saved;
            Changed = RefilledCount > 0;
            IsBusy = false;
            CloseResult = true;
        }
        catch (BulkRefillInterruptedException ex)
        {
            RefilledCount += ex.SavedCount;
            Changed = true;
            ClearSavedPackageCounts(lines, ex.SavedCount);
            IsBusy = false;
            _interaction.ShowMessage(
                UiLocalizer.Format("BulkRefill.Partial", ex.SavedCount, ex.Message),
                UiLocalizer.Get("BulkRefill.Title"),
                UiMessageKind.Warning);
        }
        catch (ArgumentOutOfRangeException)
        {
            IsBusy = false;
            _interaction.ShowMessage(
                UiLocalizer.Get("Common.MustBeZeroOrGreater"),
                UiLocalizer.Get("BulkRefill.Title"),
                UiMessageKind.Warning);
        }
        catch (Exception ex)
        {
            IsBusy = false;
            _interaction.ShowMessage(ex.Message, UiLocalizer.Get("BulkRefill.Title"), UiMessageKind.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool TryReadLines(out List<BulkRefillLine> lines)
    {
        lines = new List<BulkRefillLine>(Rows.Count);
        foreach (var row in Rows)
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

    private void ClearSavedPackageCounts(IReadOnlyList<BulkRefillLine> submitted, int savedCount)
    {
        var savedIds = new HashSet<Guid>();
        foreach (var line in submitted)
        {
            if (savedIds.Count == savedCount)
            {
                break;
            }

            if (line.PackageCount >= 1)
            {
                savedIds.Add(line.MedicationId);
            }
        }

        foreach (var row in Rows)
        {
            if (savedIds.Contains(row.MedicationId))
            {
                row.PackageCountText = "0";
            }
        }
    }
}

public partial class BulkRefillRowViewModel : ObservableObject
{
    public Guid MedicationId { get; init; }

    public required string Name { get; init; }

    [ObservableProperty]
    private string? _packageCountText;
}
