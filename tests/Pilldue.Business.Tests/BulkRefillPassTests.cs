using Pilldue.Business;

namespace Pilldue.Business.Tests;

/// <summary>Bulk refill pass rules, through <see cref="IPilldueApp"/> and in-memory ports.</summary>
public class BulkRefillPassTests
{
    [Fact]
    public async Task ApplyAsync_calls_RefillAsync_for_each_included_medication_today()
    {
        var (app, refills, spy) = CreateApp();
        var first = await app.AddMedicationAsync(NewMedication("Alpha", packageSize: 28, stock: 5));
        var second = await app.AddMedicationAsync(NewMedication("Beta", packageSize: 30, stock: 10));
        var today = DateOnly.FromDateTime(DateTime.Today);

        var saved = await BulkRefillPass.ApplyAsync(spy, new[]
        {
            new BulkRefillLine { MedicationId = first.Id, PackageCount = 2 },
            new BulkRefillLine { MedicationId = second.Id, PackageCount = 1 },
        });

        Assert.Equal(2, saved);
        Assert.Equal(2, spy.Refills.Count);
        Assert.Equal((first.Id, 2, today), spy.Refills[0]);
        Assert.Equal((second.Id, 1, today), spy.Refills[1]);

        var loaded = await app.ListMedicationsAsync();
        Assert.Equal(5 + 2 * 28, Assert.Single(loaded, medication => medication.Id == first.Id).CurrentStockPills);
        Assert.Equal(10 + 1 * 30, Assert.Single(loaded, medication => medication.Id == second.Id).CurrentStockPills);

        var firstEvent = Assert.Single(await refills.ListByMedicationAsync(first.Id));
        Assert.Equal(2, firstEvent.PackageCount);
        Assert.Equal(today, firstEvent.Date);
        var secondEvent = Assert.Single(await refills.ListByMedicationAsync(second.Id));
        Assert.Equal(1, secondEvent.PackageCount);
        Assert.Equal(today, secondEvent.Date);
    }

    [Fact]
    public async Task ApplyAsync_skips_a_count_of_zero()
    {
        var (app, refills, spy) = CreateApp();
        var skipped = await app.AddMedicationAsync(NewMedication("Skip", packageSize: 28, stock: 4));
        var included = await app.AddMedicationAsync(NewMedication("Keep", packageSize: 10, stock: 3));
        var today = DateOnly.FromDateTime(DateTime.Today);

        var saved = await BulkRefillPass.ApplyAsync(spy, new[]
        {
            new BulkRefillLine { MedicationId = skipped.Id, PackageCount = 0 },
            new BulkRefillLine { MedicationId = included.Id, PackageCount = 3 },
        });

        Assert.Equal(1, saved);
        var call = Assert.Single(spy.Refills);
        Assert.Equal((included.Id, 3, today), call);

        var loaded = await app.ListMedicationsAsync();
        Assert.Equal(4, Assert.Single(loaded, medication => medication.Id == skipped.Id).CurrentStockPills);
        Assert.Equal(3 + 3 * 10, Assert.Single(loaded, medication => medication.Id == included.Id).CurrentStockPills);
        Assert.Empty(await refills.ListByMedicationAsync(skipped.Id));
        Assert.Single(await refills.ListByMedicationAsync(included.Id));
    }

    [Fact]
    public async Task ApplyAsync_writes_nothing_when_every_count_is_zero()
    {
        var (app, refills, spy) = CreateApp();
        var first = await app.AddMedicationAsync(NewMedication("Alpha", packageSize: 28, stock: 5));
        var second = await app.AddMedicationAsync(NewMedication("Beta", packageSize: 30, stock: 8));

        var saved = await BulkRefillPass.ApplyAsync(spy, new[]
        {
            new BulkRefillLine { MedicationId = first.Id, PackageCount = 0 },
            new BulkRefillLine { MedicationId = second.Id, PackageCount = 0 },
        });

        Assert.Equal(0, saved);
        Assert.Empty(spy.Refills);
        var loaded = await app.ListMedicationsAsync();
        Assert.Equal(5, Assert.Single(loaded, medication => medication.Id == first.Id).CurrentStockPills);
        Assert.Equal(8, Assert.Single(loaded, medication => medication.Id == second.Id).CurrentStockPills);
        Assert.Empty(await refills.ListByMedicationAsync(first.Id));
        Assert.Empty(await refills.ListByMedicationAsync(second.Id));
    }

    [Fact]
    public async Task ApplyAsync_writes_nothing_when_a_count_is_negative()
    {
        var (app, refills, spy) = CreateApp();
        var first = await app.AddMedicationAsync(NewMedication("Alpha", packageSize: 28, stock: 5));
        var second = await app.AddMedicationAsync(NewMedication("Beta", packageSize: 30, stock: 8));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => BulkRefillPass.ApplyAsync(spy, new[]
        {
            new BulkRefillLine { MedicationId = first.Id, PackageCount = 2 },
            new BulkRefillLine { MedicationId = second.Id, PackageCount = -1 },
        }));

        Assert.Empty(spy.Refills);
        var loaded = await app.ListMedicationsAsync();
        Assert.Equal(5, Assert.Single(loaded, medication => medication.Id == first.Id).CurrentStockPills);
        Assert.Equal(8, Assert.Single(loaded, medication => medication.Id == second.Id).CurrentStockPills);
        Assert.Empty(await refills.ListByMedicationAsync(first.Id));
        Assert.Empty(await refills.ListByMedicationAsync(second.Id));
    }

    [Fact]
    public async Task ApplyAsync_keeps_earlier_refills_when_a_later_call_fails()
    {
        var (app, refills, spy) = CreateApp(failOnCall: 2);
        var first = await app.AddMedicationAsync(NewMedication("Alpha", packageSize: 28, stock: 5));
        var second = await app.AddMedicationAsync(NewMedication("Beta", packageSize: 30, stock: 8));
        var today = DateOnly.FromDateTime(DateTime.Today);

        await Assert.ThrowsAsync<InvalidOperationException>(() => BulkRefillPass.ApplyAsync(spy, new[]
        {
            new BulkRefillLine { MedicationId = first.Id, PackageCount = 2 },
            new BulkRefillLine { MedicationId = second.Id, PackageCount = 1 },
        }));

        var call = Assert.Single(spy.Refills);
        Assert.Equal((first.Id, 2, today), call);

        var loaded = await app.ListMedicationsAsync();
        Assert.Equal(5 + 2 * 28, Assert.Single(loaded, medication => medication.Id == first.Id).CurrentStockPills);
        Assert.Equal(8, Assert.Single(loaded, medication => medication.Id == second.Id).CurrentStockPills);

        var firstEvent = Assert.Single(await refills.ListByMedicationAsync(first.Id));
        Assert.Equal(2, firstEvent.PackageCount);
        Assert.Equal(today, firstEvent.Date);
        Assert.Empty(await refills.ListByMedicationAsync(second.Id));
    }

    [Fact]
    public void StartingPackageCount_uses_prescribed_count_or_one()
    {
        Assert.Equal(3, BulkRefillPass.StartingPackageCount(new Medication { PrescribedPackageCount = 3 }));
        Assert.Equal(1, BulkRefillPass.StartingPackageCount(new Medication { PrescribedPackageCount = 0 }));
    }

    private static (PilldueApp App, InMemoryRefillEventRepository Refills, RefillSpy Spy) CreateApp(int? failOnCall = null)
    {
        var refills = new InMemoryRefillEventRepository();
        var app = new PilldueApp(
            new InMemoryMedicationRepository(),
            refills,
            new InMemorySkipDoseEventRepository(),
            new InMemoryMissedDoseEventRepository(),
            new InMemoryAppConfigStore());
        return (app, refills, new RefillSpy(app, failOnCall));
    }

    private static Medication NewMedication(string name, int packageSize, int stock) => new()
    {
        Name = name,
        PackageSizePills = packageSize,
        PrescribedPackageCount = 1,
        DailyDosagePills = 1,
        CurrentStockPills = stock,
        PrescriptionStartDate = new DateOnly(2026, 1, 6),
    };

    private sealed class RefillSpy : IPilldueApp
    {
        private readonly PilldueApp _inner;
        private readonly int? _failOnCall;
        private int _calls;

        public RefillSpy(PilldueApp inner, int? failOnCall)
        {
            _inner = inner;
            _failOnCall = failOnCall;
        }

        public List<(Guid MedicationId, int PackageCount, DateOnly Date)> Refills { get; } = new();

        public Task RefillAsync(
            Guid medicationId,
            int packageCount,
            DateOnly date,
            CancellationToken cancellationToken = default)
        {
            _calls++;
            if (_failOnCall == _calls)
            {
                throw new InvalidOperationException("later refill failed");
            }

            Refills.Add((medicationId, packageCount, date));
            return _inner.RefillAsync(medicationId, packageCount, date, cancellationToken);
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
