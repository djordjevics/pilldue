namespace Pilldue.IntegrationTests;

/// <summary>
/// End-to-end scenarios: perform several Business (and Data) actions, then assert outcomes.
/// No Spectre.Console / UI — drive the application services like a scripted user session.
/// </summary>
public class RefillWorkflowScenarios
{
    [Fact(Skip = "Pending: application services not implemented yet")]
    public void Add_medication_then_see_refill_by_date()
    {
        // Arrange: empty store
        // Act: add medication with schedule + current stock
        // Assert: medication listed; refill-by date is set
    }

    [Fact(Skip = "Pending: application services not implemented yet")]
    public void Skip_dose_then_stock_and_refill_by_move_out()
    {
        // Act: flag skipped dose
        // Assert: pills left increased by dose size; refill-by date later
    }

    [Fact(Skip = "Pending: application services not implemented yet")]
    public void Log_refill_then_history_and_stock_reflect_it()
    {
        // Act: log refill
        // Assert: stock increased; history has an entry; refill-by recalculated
    }

    [Fact(Skip = "Pending: application services not implemented yet")]
    public void Full_path_add_skip_refill_matches_expected_stock()
    {
        // Act sequence:
        //   1. Add med (stock N, dose D, schedule S)
        //   2. Flag skipped dose
        //   3. Log refill (+pack)
        // Assert: final stock == N + D + pack (modulo any other rules we define)
        // Assert: history contains refill; skip recorded if we store skips
    }
}
