using Spectre.Console;
using Pilldue.Data;
using Pilldue.UI;
using Pilldue.UI.Localization;

await using var session = await PilldueComposition.CreateDefaultSessionAsync();

try
{
    var config = await session.App.GetConfigAsync();
    UiLocalizer.Apply(config);

    await MainMenu.RunAsync(session.App);
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
    Environment.ExitCode = 1;
}
