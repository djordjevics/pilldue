# Getting started

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows (the app is WPF)

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

The database and the language config live in one profile folder:

`%UserProfile%/Pilldue/`

- `pilldue.db` — medications and history
- `config.json` — UI language

On Windows that is `C:\Users\<you>\Pilldue\`. The WPF host uses this folder through `PilldueComposition` (`SqliteDatabasePaths` is the only default). Schema is applied with EF Core migrations (`PilldueDbBootstrap.MigrateAsync`).

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
| `tests/Pilldue.Data.Tests` | Data unit tests |
| `tests/Pilldue.Business.Tests` | Business unit tests |
| `tests/Pilldue.IntegrationTests` | Multi-step scenario tests (no UI) |
| `docs/` | Project documentation ([use cases](use-cases.md)) |
| `.cursor/` | Cursor rules and project skills |
| `todo.md` | Decision questionnaire / notes |
