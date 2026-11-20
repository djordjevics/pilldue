using Pilldue.Business;
using Pilldue.Data;
using Pilldue.UI.Localization;
using Pilldue.UI.Mobile;

namespace Pilldue.UI.MobileHost;

public partial class App : Application
{
    private readonly object _sessionGate = new();
    private PilldueSession? _session;
    private bool _closing;

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

        PilldueSession? session = null;
        var handedOff = false;
        try
        {
            var appData = Microsoft.Maui.Storage.FileSystem.Current.AppDataDirectory;
            var databasePath = Path.Combine(appData, SqliteDatabasePaths.FileName);
            var configPath = Path.Combine(appData, SqliteDatabasePaths.ConfigFileName);
            session = await PilldueComposition.CreateSessionAsync(databasePath, configPath);
            var config = await session.App.GetConfigAsync();

            lock (_sessionGate)
            {
                if (!_closing)
                {
                    _session = session;
                    handedOff = true;
                }
            }

            if (!handedOff)
            {
                await session.DisposeAsync();
                session = null;
                return;
            }

            UiLocalizer.Apply(config);
            window.Page = new StartupPage(session.App);
            session = null;
        }
        catch (Exception ex)
        {
            if (session is not null && !handedOff)
            {
                await session.DisposeAsync();
            }

            if (IsClosing)
            {
                return;
            }

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

    private bool IsClosing
    {
        get
        {
            lock (_sessionGate)
            {
                return _closing;
            }
        }
    }

    private async void OnWindowDestroying(object? sender, EventArgs e)
    {
        PilldueSession? session;
        lock (_sessionGate)
        {
            _closing = true;
            session = _session;
            _session = null;
        }

        if (session is not null)
        {
            await session.DisposeAsync();
        }
    }
}
