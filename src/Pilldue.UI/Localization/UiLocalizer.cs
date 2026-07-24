using System.Globalization;
using Pilldue.Business;

namespace Pilldue.UI.Localization;

/// <summary>
/// English / Serbian (Latin) UI strings. Add keys to both catalogs when introducing copy.
/// </summary>
public static class UiLocalizer
{
    private static string _language = AppConfig.EnglishLanguage;

    public static string Language => _language;

    public static string ResolveLanguage(AppConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        if (!string.IsNullOrWhiteSpace(config.UiLanguage))
        {
            return Normalize(config.UiLanguage);
        }

        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            .Equals(AppConfig.SerbianLanguage, StringComparison.OrdinalIgnoreCase)
            ? AppConfig.SerbianLanguage
            : AppConfig.EnglishLanguage;
    }

    public static void Apply(AppConfig config)
    {
        _language = ResolveLanguage(config);
        var culture = _language == AppConfig.SerbianLanguage
            ? CultureInfo.GetCultureInfo("sr-Latn")
            : CultureInfo.GetCultureInfo("en");
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
    }

    public static string Normalize(string language) =>
        language.Trim().Equals(AppConfig.SerbianLanguage, StringComparison.OrdinalIgnoreCase)
            ? AppConfig.SerbianLanguage
            : AppConfig.EnglishLanguage;

    public static string Get(string key)
    {
        var catalog = _language == AppConfig.SerbianLanguage ? Serbian : English;
        if (catalog.TryGetValue(key, out var value))
        {
            return value;
        }

        if (English.TryGetValue(key, out var fallback))
        {
            return fallback;
        }

        return key;
    }

    public static string Format(string key, params object[] args) =>
        string.Format(CultureInfo.CurrentUICulture, Get(key), args);

    public static IReadOnlyCollection<string> RequiredKeys => English.Keys;

    public static bool HasKey(string language, string key)
    {
        var catalog = Normalize(language) == AppConfig.SerbianLanguage ? Serbian : English;
        return catalog.ContainsKey(key);
    }

    private static readonly Dictionary<string, string> English = new()
    {
        ["App.Tagline"] = "medication refill tracker",
        ["App.HeaderMeta"] = "Default refill day: {0}. Language: {1}.",
        ["Menu.Title"] = "Main menu",
        ["Menu.List"] = "List medications",
        ["Menu.Planning"] = "Planning queries (stock vs refill days)",
        ["Menu.Add"] = "Add medication",
        ["Menu.Edit"] = "Edit medication",
        ["Menu.Refill"] = "Log refill",
        ["Menu.Skip"] = "Skip dose",
        ["Menu.Calendar"] = "Calendar",
        ["Menu.Language"] = "Language",
        ["Menu.Exit"] = "Exit",
        ["Common.Goodbye"] = "Goodbye.",
        ["Common.PressEnterMenu"] = "Press Enter to return to the menu…",
        ["Common.PressEnterContinue"] = "Press Enter to continue…",
        ["Common.SelectMedication"] = "Select a medication",
        ["Common.StockSuffix"] = "{0} (stock: {1})",
        ["Common.MustBeAtLeast1"] = "Must be at least 1.",
        ["Common.MustBeGreaterThan0"] = "Must be greater than 0.",
        ["Common.MustBeZeroOrGreater"] = "Must be 0 or greater.",
        ["Common.UseDateFormat"] = "Use yyyy-MM-dd.",
        ["Common.NameRequired"] = "Name is required.",
        ["Common.Yes"] = "yes",
        ["Common.No"] = "no",
        ["Common.Back"] = "Back",
        ["List.Empty"] = "No medications yet.",
        ["List.ColName"] = "Name",
        ["List.ColStock"] = "Stock",
        ["List.ColDaily"] = "Daily dose",
        ["List.ColPackage"] = "Package size",
        ["Med.AddTitle"] = "Add medication",
        ["Med.EditTitle"] = "Edit medication",
        ["Med.EditNone"] = "No medications to edit.",
        ["Med.Editing"] = "Editing {0}",
        ["Med.Added"] = "Added {0} ({1}).",
        ["Med.Updated"] = "Updated {0} ({1}).",
        ["Med.AddFailed"] = "Could not add medication: {0}",
        ["Med.UpdateFailed"] = "Could not update medication: {0}",
        ["Med.Name"] = "Name:",
        ["Med.PackageSize"] = "Package size (pills per package):",
        ["Med.Prescribed"] = "Prescribed package count (usual packages per refill):",
        ["Med.Daily"] = "Daily dosage (pills per day):",
        ["Med.Stock"] = "Current stock (pills on hand):",
        ["Med.RefillOverride"] = "Refill day override (1–31, or blank to inherit config default):",
        ["Med.RefillOverrideInvalid"] = "Enter a day 1–31, or leave blank.",
        ["Med.RxStart"] = "Prescription start date (yyyy-MM-dd):",
        ["Med.RxDuration"] = "Prescription duration (months):",
        ["Refill.Title"] = "Log refill",
        ["Refill.Empty"] = "No medications to refill. Add one first.",
        ["Refill.Packages"] = "Packages obtained:",
        ["Refill.Date"] = "Refill date (yyyy-MM-dd):",
        ["Refill.Done"] = "Refilled {0}: stock {1} → {2} (+{3} × {4}).",
        ["Refill.Failed"] = "Could not log refill: {0}",
        ["Skip.Title"] = "Skip dose",
        ["Skip.Empty"] = "No medications. Add one first.",
        ["Skip.Pills"] = "Pills returned to stock (usually daily dosage):",
        ["Skip.Date"] = "Skip date (yyyy-MM-dd):",
        ["Skip.Done"] = "Skipped dose for {0}: stock {1} → {2} (+{3}).",
        ["Skip.Failed"] = "Could not record skip: {0}",
        ["Plan.Title"] = "Planning queries",
        ["Plan.Choose"] = "Choose a query",
        ["Plan.Coverage"] = "Stock covers until next refill?",
        ["Plan.Short"] = "Short before next refill",
        ["Plan.Extra"] = "Need extra packages for second refill",
        ["Plan.AsOf"] = "As-of date (yyyy-MM-dd):",
        ["Plan.EmptyMeds"] = "No medications.",
        ["Plan.CoverageTitle"] = "Stock coverage as of {0}",
        ["Plan.ShortTitle"] = "Short before next refill (as of {0})",
        ["Plan.ExtraTitle"] = "Need extra for second refill (as of {0})",
        ["Plan.ShortEmpty"] = "No medications are short before the next refill.",
        ["Plan.ExtraEmpty"] = "No medications need extra packages for the second refill day.",
        ["Plan.ColNext"] = "Next refill",
        ["Plan.ColLast"] = "Last covered",
        ["Plan.ColCovers"] = "Covers?",
        ["Plan.ColPillsShort"] = "Pills short",
        ["Plan.ColPackages"] = "Packages to buy",
        ["Plan.ColSecond"] = "Second refill",
        ["Plan.ColNeeded"] = "Packages needed",
        ["Plan.ColPrescribed"] = "Prescribed",
        ["Plan.ColExtra"] = "Extra",
        ["Cal.Title"] = "Calendar",
        ["Cal.RangeStart"] = "Range start (yyyy-MM-dd):",
        ["Cal.RangeEnd"] = "Range end (yyyy-MM-dd):",
        ["Cal.AsOf"] = "As-of date for stock coverage (yyyy-MM-dd):",
        ["Cal.RangeInvalid"] = "Range end must be on or after range start.",
        ["Cal.Empty"] = "No medications with last-covered or prescription end in {0}…{1}.",
        ["Cal.TableTitle"] = "Calendar {0} → {1} (as of {2})",
        ["Cal.ColLast"] = "Last covered",
        ["Cal.ColRxEnd"] = "Prescription end",
        ["Cal.Failed"] = "Could not load calendar: {0}",
        ["Lang.Title"] = "Language",
        ["Lang.Prompt"] = "Choose UI language",
        ["Lang.English"] = "English",
        ["Lang.Serbian"] = "Serbian (Latin)",
        ["Lang.Saved"] = "Language set to {0}. Menus refresh immediately.",
    };

    private static readonly Dictionary<string, string> Serbian = new()
    {
        ["App.Tagline"] = "pracenje dopune lekova",
        ["App.HeaderMeta"] = "Podrazumevani dan dopune: {0}. Jezik: {1}.",
        ["Menu.Title"] = "Glavni meni",
        ["Menu.List"] = "Lista lekova",
        ["Menu.Planning"] = "Planiranje (zaliha vs dani dopune)",
        ["Menu.Add"] = "Dodaj lek",
        ["Menu.Edit"] = "Izmeni lek",
        ["Menu.Refill"] = "Evidencija dopune",
        ["Menu.Skip"] = "Preskocena doza",
        ["Menu.Calendar"] = "Kalendar",
        ["Menu.Language"] = "Jezik",
        ["Menu.Exit"] = "Izlaz",
        ["Common.Goodbye"] = "Dovidjenja.",
        ["Common.PressEnterMenu"] = "Pritisnite Enter za povratak u meni…",
        ["Common.PressEnterContinue"] = "Pritisnite Enter za nastavak…",
        ["Common.SelectMedication"] = "Izaberite lek",
        ["Common.StockSuffix"] = "{0} (zaliha: {1})",
        ["Common.MustBeAtLeast1"] = "Mora biti najmanje 1.",
        ["Common.MustBeGreaterThan0"] = "Mora biti vece od 0.",
        ["Common.MustBeZeroOrGreater"] = "Mora biti 0 ili vece.",
        ["Common.UseDateFormat"] = "Koristite yyyy-MM-dd.",
        ["Common.NameRequired"] = "Naziv je obavezan.",
        ["Common.Yes"] = "da",
        ["Common.No"] = "ne",
        ["Common.Back"] = "Nazad",
        ["List.Empty"] = "Jos nema lekova.",
        ["List.ColName"] = "Naziv",
        ["List.ColStock"] = "Zaliha",
        ["List.ColDaily"] = "Dnevna doza",
        ["List.ColPackage"] = "Velicina pakovanja",
        ["Med.AddTitle"] = "Dodaj lek",
        ["Med.EditTitle"] = "Izmeni lek",
        ["Med.EditNone"] = "Nema lekova za izmenu.",
        ["Med.Editing"] = "Izmena {0}",
        ["Med.Added"] = "Dodato {0} ({1}).",
        ["Med.Updated"] = "Azurirano {0} ({1}).",
        ["Med.AddFailed"] = "Nije moguce dodati lek: {0}",
        ["Med.UpdateFailed"] = "Nije moguce azurirati lek: {0}",
        ["Med.Name"] = "Naziv:",
        ["Med.PackageSize"] = "Velicina pakovanja (tablete po pakovanju):",
        ["Med.Prescribed"] = "Propisani broj pakovanja (obicno po dopuni):",
        ["Med.Daily"] = "Dnevna doza (tablete dnevno):",
        ["Med.Stock"] = "Trenutna zaliha (tablete):",
        ["Med.RefillOverride"] = "Dan dopune (1–31, ili prazno za podrazumevano):",
        ["Med.RefillOverrideInvalid"] = "Unesite dan 1–31, ili ostavite prazno.",
        ["Med.RxStart"] = "Pocetak recepta (yyyy-MM-dd):",
        ["Med.RxDuration"] = "Trajanje recepta (meseci):",
        ["Refill.Title"] = "Evidencija dopune",
        ["Refill.Empty"] = "Nema lekova za dopunu. Prvo dodajte lek.",
        ["Refill.Packages"] = "Broj pakovanja:",
        ["Refill.Date"] = "Datum dopune (yyyy-MM-dd):",
        ["Refill.Done"] = "Dopunjeno {0}: zaliha {1} → {2} (+{3} × {4}).",
        ["Refill.Failed"] = "Nije moguce evidentirati dopunu: {0}",
        ["Skip.Title"] = "Preskocena doza",
        ["Skip.Empty"] = "Nema lekova. Prvo dodajte lek.",
        ["Skip.Pills"] = "Tablete vracene u zalihu (obicno dnevna doza):",
        ["Skip.Date"] = "Datum preskakanja (yyyy-MM-dd):",
        ["Skip.Done"] = "Preskocena doza za {0}: zaliha {1} → {2} (+{3}).",
        ["Skip.Failed"] = "Nije moguce evidentirati preskakanje: {0}",
        ["Plan.Title"] = "Planiranje",
        ["Plan.Choose"] = "Izaberite upit",
        ["Plan.Coverage"] = "Da li zaliha traje do sledece dopune?",
        ["Plan.Short"] = "Nedostaje do sledece dopune",
        ["Plan.Extra"] = "Potrebna dodatna pakovanja za drugu dopunu",
        ["Plan.AsOf"] = "Datum stanja (yyyy-MM-dd):",
        ["Plan.EmptyMeds"] = "Nema lekova.",
        ["Plan.CoverageTitle"] = "Pokrice zalihe na dan {0}",
        ["Plan.ShortTitle"] = "Nedostaje do sledece dopune (na dan {0})",
        ["Plan.ExtraTitle"] = "Dodatna pakovanja za drugu dopunu (na dan {0})",
        ["Plan.ShortEmpty"] = "Nijedan lek ne nestaje pre sledece dopune.",
        ["Plan.ExtraEmpty"] = "Nijedan lek ne treba dodatna pakovanja za drugu dopunu.",
        ["Plan.ColNext"] = "Sledeca dopuna",
        ["Plan.ColLast"] = "Poslednji pokriveni dan",
        ["Plan.ColCovers"] = "Pokrica?",
        ["Plan.ColPillsShort"] = "Nedostaje tableta",
        ["Plan.ColPackages"] = "Pakovanja za kupovinu",
        ["Plan.ColSecond"] = "Druga dopuna",
        ["Plan.ColNeeded"] = "Potrebno pakovanja",
        ["Plan.ColPrescribed"] = "Propisano",
        ["Plan.ColExtra"] = "Dodatno",
        ["Cal.Title"] = "Kalendar",
        ["Cal.RangeStart"] = "Pocetak opsega (yyyy-MM-dd):",
        ["Cal.RangeEnd"] = "Kraj opsega (yyyy-MM-dd):",
        ["Cal.AsOf"] = "Datum stanja za zalihu (yyyy-MM-dd):",
        ["Cal.RangeInvalid"] = "Kraj opsega mora biti na ili posle pocetka.",
        ["Cal.Empty"] = "Nema lekova sa poslednjim pokricem ili krajem recepta u {0}…{1}.",
        ["Cal.TableTitle"] = "Kalendar {0} → {1} (stanje {2})",
        ["Cal.ColLast"] = "Poslednji pokriveni dan",
        ["Cal.ColRxEnd"] = "Kraj recepta",
        ["Cal.Failed"] = "Nije moguce ucitati kalendar: {0}",
        ["Lang.Title"] = "Jezik",
        ["Lang.Prompt"] = "Izaberite jezik interfejsa",
        ["Lang.English"] = "Engleski",
        ["Lang.Serbian"] = "Srpski (latinica)",
        ["Lang.Saved"] = "Jezik podesen na {0}. Meniji se osvezavaju odmah.",
    };
}
