using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI.Mobile;

public partial class StartupPage : ContentPage
{
    private readonly IPilldueApp _app;

    public StartupPage(IPilldueApp app)
    {
        _app = app ?? throw new ArgumentNullException(nameof(app));
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var config = await _app.GetConfigAsync();
            UiLocalizer.Apply(config);
            StatusLabel.Text = UiLocalizer.Get("Mobile.Ready");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = UiLocalizer.Format("Mobile.StartupFailed", ex.Message);
        }
    }
}
