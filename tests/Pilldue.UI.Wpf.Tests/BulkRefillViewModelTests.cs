using Pilldue.Business;
using Pilldue.UI.Localization;
using Pilldue.UI.Wpf;

namespace Pilldue.UI.Wpf.Tests;

public class BulkRefillViewModelTests
{
    [Fact]
    public async Task Partial_failure_clears_the_medications_that_were_saved()
    {
        var session = new TestSession();
        await session.App.AddMedicationAsync(TestSession.Sample("Alpha"));
        await session.App.AddMedicationAsync(TestSession.Sample("Beta"));
        await session.App.AddMedicationAsync(TestSession.Sample("Gamma"));
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var interaction = new FakeUiInteraction();
        var vm = new BulkRefillViewModel(new GatedRefillApp(session.App, started, release, failOnCall: 2), interaction);
        await vm.LoadAsync();
        var skipped = vm.Rows[0];
        var saved = vm.Rows[1];
        var failed = vm.Rows[2];

        skipped.PackageCountText = "0";
        saved.PackageCountText = "2";
        failed.PackageCountText = "1";

        var confirm = vm.ConfirmCommand.ExecuteAsync(null);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        skipped.PackageCountText = "5";
        saved.PackageCountText = "9";
        failed.PackageCountText = "3";
        release.TrySetResult();
        await confirm.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal("5", skipped.PackageCountText);
        Assert.Equal("0", saved.PackageCountText);
        Assert.Equal("3", failed.PackageCountText);
        Assert.Equal(1, vm.RefilledCount);
        Assert.True(vm.Changed);
        Assert.False(vm.IsBusy);
        Assert.Null(vm.CloseResult);
        Assert.Contains(interaction.Messages, message => message.Kind == UiMessageKind.Warning
            && message.Message == UiLocalizer.Format("BulkRefill.Partial", 1, "later refill failed"));

        var loaded = await session.App.ListMedicationsAsync();
        Assert.Equal(10, Assert.Single(loaded, medication => medication.Id == skipped.MedicationId).CurrentStockPills);
        Assert.Equal(10 + 2 * 28, Assert.Single(loaded, medication => medication.Id == saved.MedicationId).CurrentStockPills);
        Assert.Equal(10, Assert.Single(loaded, medication => medication.Id == failed.MedicationId).CurrentStockPills);
    }

    private sealed class GatedRefillApp : IPilldueApp
    {
        private readonly PilldueApp _inner;
        private readonly TaskCompletionSource _started;
        private readonly TaskCompletionSource _release;
        private readonly int _failOnCall;
        private int _calls;

        public GatedRefillApp(
            PilldueApp inner,
            TaskCompletionSource started,
            TaskCompletionSource release,
            int failOnCall)
        {
            _inner = inner;
            _started = started;
            _release = release;
            _failOnCall = failOnCall;
        }

        public async Task RefillAsync(
            Guid medicationId,
            int packageCount,
            DateOnly date,
            CancellationToken cancellationToken = default)
        {
            var call = Interlocked.Increment(ref _calls);
            if (call == 1)
            {
                _started.TrySetResult();
                await _release.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            }

            if (call == _failOnCall)
            {
                throw new InvalidOperationException("later refill failed");
            }

            await _inner.RefillAsync(medicationId, packageCount, date, cancellationToken).ConfigureAwait(false);
        }

        public Task<AppConfig> GetConfigAsync(CancellationToken cancellationToken = default)
            => _inner.GetConfigAsync(cancellationToken);

        public Task SaveConfigAsync(AppConfig config, CancellationToken cancellationToken = default)
            => _inner.SaveConfigAsync(config, cancellationToken);

        public Task<IReadOnlyList<Medication>> ListMedicationsAsync(CancellationToken cancellationToken = default)
            => _inner.ListMedicationsAsync(cancellationToken);

        public Task<Medication> AddMedicationAsync(Medication medication, CancellationToken cancellationToken = default)
            => _inner.AddMedicationAsync(medication, cancellationToken);

        public Task<Medication> UpdateMedicationAsync(Medication medication, CancellationToken cancellationToken = default)
            => _inner.UpdateMedicationAsync(medication, cancellationToken);

        public Task RemoveMedicationAsync(Guid medicationId, CancellationToken cancellationToken = default)
            => _inner.RemoveMedicationAsync(medicationId, cancellationToken);

        public Task<IReadOnlyList<StockCoverageResult>> GetStockCoverageAsync(
            DateOnly asOfDate,
            CancellationToken cancellationToken = default)
            => _inner.GetStockCoverageAsync(asOfDate, cancellationToken);

        public Task<IReadOnlyList<StockCoverageResult>> ListShortBeforeNextRefillAsync(
            DateOnly asOfDate,
            CancellationToken cancellationToken = default)
            => _inner.ListShortBeforeNextRefillAsync(asOfDate, cancellationToken);

        public Task<IReadOnlyList<ExtraPackagesNeeded>> ListNeedExtraForSecondRefillAsync(
            DateOnly asOfDate,
            CancellationToken cancellationToken = default)
            => _inner.ListNeedExtraForSecondRefillAsync(asOfDate, cancellationToken);

        public Task SkipDoseAsync(
            Guid medicationId,
            int pillsReturned,
            DateOnly date,
            CancellationToken cancellationToken = default)
            => _inner.SkipDoseAsync(medicationId, pillsReturned, date, cancellationToken);

        public Task FlagMissedDoseAsync(
            Guid medicationId,
            DateOnly date,
            CancellationToken cancellationToken = default)
            => _inner.FlagMissedDoseAsync(medicationId, date, cancellationToken);

        public Task<IReadOnlyList<MissedDoseEvent>> ListMissedDosesAsync(CancellationToken cancellationToken = default)
            => _inner.ListMissedDosesAsync(cancellationToken);

        public Task<CalendarView> GetCalendarAsync(DateOnly asOfDate, CancellationToken cancellationToken = default)
            => _inner.GetCalendarAsync(asOfDate, cancellationToken);
    }
}
