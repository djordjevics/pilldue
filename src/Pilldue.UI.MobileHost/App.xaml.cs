using Pilldue.Business;
using Pilldue.Data;
using Pilldue.UI.Localization;
using Pilldue.UI.Mobile;

namespace Pilldue.UI.MobileHost;

public partial class App : Application
{
    private PilldueSession? _session;

    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new ContentPage())
        {
            Title = "Pilldue",
        };
        window.Destroying += OnWindowDestroying;
        _ = OpenSessionAsync(window);
        return window;
    }

    private async Task OpenSessionAsync(Window window)
    {
        UiLocalizer.Apply(new AppConfig());

        try
        {
            var appData = Microsoft.Maui.Storage.FileSystem.Current.AppDataDirectory;
            var databasePath = Path.Combine(appData, SqliteDatabasePaths.FileName);
            var configPath = Path.Combine(appData, SqliteDatabasePaths.ConfigFileName);
            _session = await PilldueComposition.CreateSessionAsync(databasePath, configPath);
            var config = await _session.App.GetConfigAsync();
            UiLocalizer.Apply(config);
            window.Page = new StartupPage(_session.App);
        }
        catch (Exception ex)
        {
            window.Page = new ContentPage
            {
                Content = new Label
                {
                    Text = UiLocalizer.Format("Mobile.StartupFailed", ex.Message),
                    Margin = 24,
                },
            };
        }
    }

    private async void OnWindowDestroying(object? sender, EventArgs e)
    {
        if (_session is not null)
        {
            await _session.DisposeAsync();
            _session = null;
        }
    }
}
