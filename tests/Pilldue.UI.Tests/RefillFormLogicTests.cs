using Pilldue.UI;

namespace Pilldue.UI.Tests;

public class RefillFormLogicTests
{
    [Fact]
    public void IsCancelSelection_true_when_label_matches()
    {
        Assert.True(RefillFormLogic.IsCancelSelection("Cancel", "Cancel"));
        Assert.True(RefillFormLogic.IsCancelSelection("Ponisti", "Ponisti"));
    }

    [Fact]
    public void IsCancelSelection_false_for_medication_label()
    {
        Assert.False(RefillFormLogic.IsCancelSelection("Aspirin (stock: 10)", "Cancel"));
        Assert.False(RefillFormLogic.IsCancelSelection("yes", "Cancel"));
    }

    [Fact]
    public void IsCancelSelection_is_ordinal_and_rejects_null()
    {
        Assert.False(RefillFormLogic.IsCancelSelection(null, "Cancel"));
        Assert.False(RefillFormLogic.IsCancelSelection("cancel", "Cancel"));
    }
}
