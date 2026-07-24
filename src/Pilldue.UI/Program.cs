using Spectre.Console;
using Pilldue.Business;

// Composition root (in-memory until EF/SQLite issues land).
IMedicationRepository medications = new InMemoryMedicationRepository();
IRefillEventRepository refills = new InMemoryRefillEventRepository();
ISkipDoseEventRepository skips = new InMemorySkipDoseEventRepository();
IAppConfigStore config = new InMemoryAppConfigStore();
IPilldueApp app = new PilldueApp(medications, refills, skips, config);

var settings = await app.GetConfigAsync();
AnsiConsole.MarkupLine("[bold blue]Pilldue[/] — medication refill tracker");
AnsiConsole.MarkupLine(
    $"[grey]Contracts ready (DIP). Default refill day: {settings.DefaultRefillDayOfMonth}. In-memory store.[/]");
