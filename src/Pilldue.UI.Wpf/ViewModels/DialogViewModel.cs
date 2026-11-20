using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Pilldue.UI.Wpf;

public abstract partial class DialogViewModel : ObservableObject
{
    [ObservableProperty]
    private bool? _closeResult;

    [RelayCommand]
    private void Cancel() => CloseResult = false;
}
