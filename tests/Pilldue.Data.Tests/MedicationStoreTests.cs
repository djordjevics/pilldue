namespace Pilldue.Data.Tests;

/// <summary>
/// Persistence unit tests. Enable as SQLite repositories are implemented.
/// </summary>
public class MedicationStoreTests
{
    [Fact(Skip = "Pending: medication persistence not implemented yet")]
    public void Save_and_load_medication_round_trips()
    {
        // Arrange / Act / Assert against Data repositories only
    }

    [Fact(Skip = "Pending: stock persistence not implemented yet")]
    public void Update_stock_persists_new_quantity()
    {
    }

    [Fact(Skip = "Pending: refill history persistence not implemented yet")]
    public void Append_refill_history_entry()
    {
    }
}
