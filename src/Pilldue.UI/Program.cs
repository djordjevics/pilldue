using Spectre.Console;
using Pilldue.Business;
using Pilldue.UI;

// Composition root (in-memory until EF/SQLite issues land).
IMedicationRepository medications = new InMemoryMedicationRepository();
IRefillEventRepository refills = new InMemoryRefillEventRepository();
ISkipDoseEventRepository skips = new InMemorySkipDoseEventRepository();
IAppConfigStore config = new InMemoryAppConfigStore();
IPilldueApp app = new PilldueApp(medications, refills, skips, config);

try
{
    await MainMenu.RunAsync(app);
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
    Environment.ExitCode = 1;
}
