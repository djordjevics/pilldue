using System.Windows.Markup;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Wpf;

/// <summary>XAML markup for a catalog key: <c>{wpf:Loc Menu.Add}</c>.</summary>
public sealed class LocExtension : MarkupExtension
{
    public LocExtension()
    {
    }

    public LocExtension(string key) => Key = key;

    public string Key { get; set; } = "";

    public override object ProvideValue(IServiceProvider serviceProvider) => UiLocalizer.Get(Key);
}
