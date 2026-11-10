using System.Windows;
using Pilldue.Business;
using Pilldue.Data;
using Pilldue.UI.Localization;
using Pilldue.UI.Wpf;

namespace Pilldue.UI.Desktop;

public partial class App : Application
{
    private PilldueSession? _session;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        UiLocalizer.Apply(new AppConfig());

        try
        {
            _session = await PilldueComposition.CreateDefaultSessionAsync();
            UiLocalizer.Apply(await _session.App.GetConfigAsync());

            var window = new MainWindow(_session.App);
            MainWindow = window;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                UiLocalizer.Format("Desktop.StartupFailed", ex.Message),
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
