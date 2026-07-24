namespace Pilldue.Business.Tests;

/// <summary>
/// Refill-by date and stock math. Enable as domain services are implemented.
/// </summary>
public class RefillCalculationTests
{
    [Fact(Skip = "Pending: refill-by calculation not implemented yet")]
    public void Refill_by_date_uses_stock_dose_size_and_schedule()
    {
        // Given stock, dose size, and frequency → expect refill-by date
    }

    [Fact(Skip = "Pending: skip-dose adjustment not implemented yet")]
    public void Flagging_skipped_dose_increases_pills_left_by_dose_size()
    {
        // Inventory correction only — not a reminder
    }

    [Fact(Skip = "Pending: refill logging not implemented yet")]
    public void Logging_refill_increases_stock_by_pack_quantity()
    {
    }
}
