using System.Windows;
using Pilldue.Business;
using Pilldue.Data;

namespace Pilldue.UI.Desktop;

public partial class App : Application
{
    private PilldueSession? _session;

    public static IPilldueApp Pilldue { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            _session = await PilldueComposition.CreateDefaultSessionAsync();
            Pilldue = _session.App;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not open the Pilldue database:\n{ex.Message}",
                "Pilldue",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_session is not null)
        {
            await _session.DisposeAsync();
        }

        base.OnExit(e);
    }
}
