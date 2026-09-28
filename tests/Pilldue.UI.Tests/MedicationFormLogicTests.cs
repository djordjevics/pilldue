using Pilldue.UI;

namespace Pilldue.UI.Tests;

public class MedicationFormLogicTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void IsCancelledName_true_when_empty_or_whitespace(string? name)
    {
        Assert.True(MedicationFormLogic.IsCancelledName(name));
    }

    [Theory]
    [InlineData("Aspirin")]
    [InlineData(" a ")]
    public void IsCancelledName_false_when_name_has_content(string name)
    {
        Assert.False(MedicationFormLogic.IsCancelledName(name));
    }
}
