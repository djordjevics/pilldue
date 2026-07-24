using Spectre.Console;
using Pilldue.Business;
using Pilldue.UI.Localization;

namespace Pilldue.UI;

internal static class LanguageScreen
{
    public static async Task RunAsync(IPilldueApp app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        AnsiConsole.MarkupLine($"[bold]{UiLocalizer.Get("Lang.Title").EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();

        var english = UiLocalizer.Get("Lang.English");
        var serbian = UiLocalizer.Get("Lang.Serbian");
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(UiLocalizer.Get("Lang.Prompt"))
                .AddChoices(english, serbian));

        var language = choice == serbian ? AppConfig.SerbianLanguage : AppConfig.EnglishLanguage;
        var config = await app.GetConfigAsync(cancellationToken).ConfigureAwait(false);
        config.UiLanguage = language;
        await app.SaveConfigAsync(config, cancellationToken).ConfigureAwait(false);
        UiLocalizer.Apply(config);

        AnsiConsole.MarkupLine(
            $"[green]{UiLocalizer.Format("Lang.Saved", language).EscapeMarkup()}[/]");
    }
}
