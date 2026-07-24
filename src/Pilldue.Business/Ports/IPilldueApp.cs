namespace Pilldue.Business;

/// <summary>
/// Application facade for UI and integration tests (flows 1–4).
/// Query/orchestration bodies are filled in later business issues; signatures are locked here.
/// </summary>
public interface IPilldueApp
{
    Task<AppConfig> GetConfigAsync(CancellationToken cancellationToken = default);

    Task SaveConfigAsync(AppConfig config, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Medication>> ListMedicationsAsync(CancellationToken cancellationToken = default);

    Task<Medication> AddMedicationAsync(Medication medication, CancellationToken cancellationToken = default);

    Task<Medication> UpdateMedicationAsync(Medication medication, CancellationToken cancellationToken = default);

    /// <summary>Flow 1.1 / 1.2: coverage vs next refill day, including pills short and packages to buy.</summary>
    Task<IReadOnlyList<StockCoverageResult>> GetStockCoverageAsync(
        DateOnly asOfDate,
        CancellationToken cancellationToken = default);

    /// <summary>Flow 1.2: meds that run out before the next refill day.</summary>
    Task<IReadOnlyList<StockCoverageResult>> ListShortBeforeNextRefillAsync(
        DateOnly asOfDate,
        CancellationToken cancellationToken = default);

    /// <summary>Flow 1.3: meds needing more than prescribed packages to reach the second refill day.</summary>
    Task<IReadOnlyList<ExtraPackagesNeeded>> ListNeedExtraForSecondRefillAsync(
        DateOnly asOfDate,
        CancellationToken cancellationToken = default);

    /// <summary>Flow 2: add N packages to stock and record a refill event.</summary>
    Task RefillAsync(
        Guid medicationId,
        int packageCount,
        DateOnly date,
        CancellationToken cancellationToken = default);

    /// <summary>Flow 4: return pills to stock (skipped dose) and record the event.</summary>
    Task SkipDoseAsync(
        Guid medicationId,
        int pillsReturned,
        DateOnly date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Flow 3: calendar from <paramref name="asOfDate"/> through the second config refill day,
    /// with stock-out dates assuming prescribed restock at each med's first refill.
    /// </summary>
    Task<CalendarView> GetCalendarAsync(
        DateOnly asOfDate,
        CancellationToken cancellationToken = default);
}
