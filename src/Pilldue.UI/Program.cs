using Spectre.Console;
using Pilldue.Business;
using Pilldue.Data;
using Pilldue.UI;
using Pilldue.UI.Localization;

var dbPath = SqliteDatabasePaths.GetDefaultDatabasePath();
var options = PilldueDbBootstrap.CreateOptions(dbPath);
await PilldueDbBootstrap.MigrateAsync(dbPath);

await using var db = new PilldueDbContext(options);
IMedicationRepository medications = new EfMedicationRepository(db);
IRefillEventRepository refills = new EfRefillEventRepository(db);
ISkipDoseEventRepository skips = new EfSkipDoseEventRepository(options);
IAppConfigStore configStore = new FileAppConfigStore(SqliteDatabasePaths.GetDefaultConfigPath());
IPilldueApp app = new PilldueApp(medications, refills, skips, configStore);

try
{
    var config = await app.GetConfigAsync();
    UiLocalizer.Apply(config);

    await MainMenu.RunAsync(app);
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
    Environment.ExitCode = 1;
}
