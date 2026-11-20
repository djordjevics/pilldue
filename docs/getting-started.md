# Getting started

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows (the WPF host)
- .NET MAUI workload, to build `Pilldue.UI.MobileHost` for Android and iOS

## Clone

```bash
git clone <repository-url>
cd pilldue
```

## Build and run

```bash
dotnet build Pilldue.slnx
dotnet test Pilldue.slnx
dotnet run --project src/Pilldue.UI.Desktop
```

## Local files

The WPF database and the settings config live in one profile folder:

`%UserProfile%/Pilldue/`

- `pilldue.db` — medications and history
- `config.json` — UI language and theme

On Windows that is `C:\Users\<you>\Pilldue\`. The WPF host uses this folder through `PilldueComposition.CreateDefaultSessionAsync` (`SqliteDatabasePaths` is the Windows default). Schema is applied with EF Core migrations (`PilldueDbBootstrap.MigrateAsync`).

The MAUI host does not read or write that folder. It creates `pilldue.db` and `config.json` under the device app data directory.

`dotnet ef` uses a design-time `pilldue-design.db` in the working directory, not in this folder.

If `%UserProfile%/Pilldue/pilldue.db` is missing and an older database exists at `%LocalAppData%/Pilldue/pilldue.db`, startup moves that database and `config.json` into the profile folder.

## Repository layout

| Path | Purpose |
|------|---------|
| `Pilldue.slnx` | Solution |
| `src/Pilldue.Data` | Persistence (SQLite) + composition helpers |
| `src/Pilldue.Business` | Domain and application services |
| `src/Pilldue.UI.Wpf` | WPF screens (Business only) |
| `src/Pilldue.UI.Desktop` | WPF desktop host (Windows) |
| `src/Pilldue.UI.Mobile` | MAUI screens (Business only; Android and iOS) |
| `src/Pilldue.UI.MobileHost` | MAUI host (Android and iOS) |
| `tests/Pilldue.Data.Tests` | Data unit tests |
| `tests/Pilldue.Business.Tests` | Business unit tests |
| `tests/Pilldue.IntegrationTests` | Multi-step scenario tests (no UI) |
| `docs/` | Project documentation ([use cases](use-cases.md)) |
| `.cursor/` | Cursor rules and project skills |
| `todo.md` | Decision questionnaire / notes |
