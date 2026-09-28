namespace Pilldue.UI;

/// <summary>
/// Pure helpers for medication add/edit prompts — unit-tested without Spectre I/O.
/// </summary>
public static class MedicationFormLogic
{
    /// <summary>Empty or whitespace name abandons the form before any save.</summary>
    public static bool IsCancelledName(string? name) => string.IsNullOrWhiteSpace(name);

    /// <summary>
    /// Resolves a medication list choice. Returns null when Cancel is selected;
    /// otherwise the index into the medication list (not including the Cancel row).
    /// </summary>
    public static int? ResolveMedicationSelection(
        IReadOnlyList<string> labels,
        string? selectedLabel,
        string cancelLabel)
    {
        ArgumentNullException.ThrowIfNull(labels);
        ArgumentException.ThrowIfNullOrWhiteSpace(cancelLabel);

        if (RefillFormLogic.IsCancelSelection(selectedLabel, cancelLabel))
        {
            return null;
        }

        for (var i = 0; i < labels.Count; i++)
        {
            if (string.Equals(labels[i], selectedLabel, StringComparison.Ordinal))
            {
                // Last row is Cancel; medication indices are 0..Count-2.
                return i < labels.Count - 1 ? i : null;
            }
        }

        return null;
    }
}
