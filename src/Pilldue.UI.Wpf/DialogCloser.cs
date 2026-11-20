using System.ComponentModel;
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

    public static readonly DependencyProperty CancelCloseProperty = DependencyProperty.RegisterAttached(
        "CancelClose",
        typeof(bool),
        typeof(DialogCloser),
        new PropertyMetadata(false, OnCancelCloseChanged));

    public static void SetCancelClose(DependencyObject element, bool value) =>
        element.SetValue(CancelCloseProperty, value);

    public static bool GetCancelClose(DependencyObject element) =>
        (bool)element.GetValue(CancelCloseProperty);

    private static void OnCancelCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window window)
        {
            return;
        }

        window.Closing -= OnWindowClosing;
        if (e.NewValue is true)
        {
            window.Closing += OnWindowClosing;
        }
    }

    private static void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        if (sender is Window window && GetCancelClose(window))
        {
            e.Cancel = true;
        }
    }

    private static void OnCloseResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Window window && e.NewValue is bool result && !GetCancelClose(window))
        {
            window.DialogResult = result;
        }
    }
}
