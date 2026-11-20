using System.Windows;

namespace Pilldue.UI.Wpf;

/// <summary>Closes a dialog when the view model sets a bool close result.</summary>
public static class DialogCloser
{
    public static readonly DependencyProperty CloseResultProperty = DependencyProperty.RegisterAttached(
        "CloseResult",
        typeof(bool?),
        typeof(DialogCloser),
        new PropertyMetadata(null, OnCloseResultChanged));

    public static void SetCloseResult(DependencyObject element, bool? value) =>
        element.SetValue(CloseResultProperty, value);

    public static bool? GetCloseResult(DependencyObject element) =>
        (bool?)element.GetValue(CloseResultProperty);

    private static void OnCloseResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Window window && e.NewValue is bool result)
        {
            window.DialogResult = result;
        }
    }
}
