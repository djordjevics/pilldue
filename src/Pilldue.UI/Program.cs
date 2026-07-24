using Spectre.Console;
using Pilldue.Business;
using Pilldue.Data;
using Pilldue.UI;
using Pilldue.UI.Localization;

IMedicationRepository medications = new InMemoryMedicationRepository();
IRefillEventRepository refills = new InMemoryRefillEventRepository();
ISkipDoseEventRepository skips = new InMemorySkipDoseEventRepository();
IAppConfigStore configStore = new FileAppConfigStore(SqliteDatabasePaths.GetDefaultConfigPath());
IPilldueApp app = new PilldueApp(medications, refills, skips, configStore);

try
{
    var config = await app.GetConfigAsync();
    UiLocalizer.Apply(config);

    var dbPath = SqliteDatabasePaths.GetDefaultDatabasePath();
    await PilldueDbBootstrap.MigrateAsync(dbPath);

    await MainMenu.RunAsync(app);
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
    Environment.ExitCode = 1;
}
