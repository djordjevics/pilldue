using System.Windows;
using System.Windows.Controls;

namespace Pilldue.UI.Wpf;

/// <summary>
/// Pushes observable captions into the main medication grid.
/// Column order is name, stock, dose, interval, package.
/// </summary>
public static class MedicationColumnHeaders
{
    public static readonly DependencyProperty NameProperty = Register("Name");

    public static readonly DependencyProperty StockProperty = Register("Stock");

    public static readonly DependencyProperty DoseProperty = Register("Dose");

    public static readonly DependencyProperty IntervalProperty = Register("Interval");

    public static readonly DependencyProperty PackageProperty = Register("Package");

    public static void SetName(DependencyObject element, string? value) => element.SetValue(NameProperty, value);

    public static string? GetName(DependencyObject element) => (string?)element.GetValue(NameProperty);

    public static void SetStock(DependencyObject element, string? value) => element.SetValue(StockProperty, value);

    public static string? GetStock(DependencyObject element) => (string?)element.GetValue(StockProperty);

    public static void SetDose(DependencyObject element, string? value) => element.SetValue(DoseProperty, value);

    public static string? GetDose(DependencyObject element) => (string?)element.GetValue(DoseProperty);

    public static void SetInterval(DependencyObject element, string? value) => element.SetValue(IntervalProperty, value);

    public static string? GetInterval(DependencyObject element) => (string?)element.GetValue(IntervalProperty);

    public static void SetPackage(DependencyObject element, string? value) => element.SetValue(PackageProperty, value);

    public static string? GetPackage(DependencyObject element) => (string?)element.GetValue(PackageProperty);

    private static DependencyProperty Register(string name) =>
        DependencyProperty.RegisterAttached(
            name,
            typeof(string),
            typeof(MedicationColumnHeaders),
            new PropertyMetadata(null, OnChanged));

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid grid)
        {
            return;
        }

        if (grid.IsInitialized)
        {
            Apply(grid);
            return;
        }

        grid.Initialized += OnInitialized;
    }

    private static void OnInitialized(object? sender, EventArgs e)
    {
        if (sender is not DataGrid grid)
        {
            return;
        }

        grid.Initialized -= OnInitialized;
        Apply(grid);
    }

    private static void Apply(DataGrid grid)
    {
        Set(grid, 0, GetName(grid));
        Set(grid, 1, GetStock(grid));
        Set(grid, 2, GetDose(grid));
        Set(grid, 3, GetInterval(grid));
        Set(grid, 4, GetPackage(grid));
    }

    private static void Set(DataGrid grid, int index, string? header)
    {
        if (header is null || grid.Columns.Count <= index)
        {
            return;
        }

        grid.Columns[index].Header = header;
    }
}
