using Spectre.Console;
using Pilldue.Business;
using Pilldue.Data;
using Pilldue.UI;

// Composition root (in-memory ports until repository issues land).
IMedicationRepository medications = new InMemoryMedicationRepository();
IRefillEventRepository refills = new InMemoryRefillEventRepository();
ISkipDoseEventRepository skips = new InMemorySkipDoseEventRepository();
IAppConfigStore config = new InMemoryAppConfigStore();
IPilldueApp app = new PilldueApp(medications, refills, skips, config);

try
{
    var dbPath = SqliteDatabasePaths.GetDefaultDatabasePath();
    await PilldueDbBootstrap.MigrateAsync(dbPath);

    await MainMenu.RunAsync(app);
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
    Environment.ExitCode = 1;
}
