namespace Pilldue.UI;

/// <summary>
/// Pure helpers for refill prompts (cancel detection) — unit-tested without Spectre I/O.
/// </summary>
public static class RefillFormLogic
{
    public static bool IsCancelSelection(string? selection, string cancelLabel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cancelLabel);
        return string.Equals(selection, cancelLabel, StringComparison.Ordinal);
    }
}
