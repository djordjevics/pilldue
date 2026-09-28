namespace Pilldue.UI;

/// <summary>
/// Pure helpers for medication add/edit prompts — unit-tested without Spectre I/O.
/// </summary>
public static class MedicationFormLogic
{
    /// <summary>Empty or whitespace name abandons the form before any save.</summary>
    public static bool IsCancelledName(string? name) => string.IsNullOrWhiteSpace(name);
}
