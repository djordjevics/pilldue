# Getting started

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A terminal that supports ANSI colors (for Spectre.Console)

## Clone

```bash
git clone <repository-url>
cd pilldue
```

## Build and run

```bash
dotnet build Pilldue.slnx
dotnet test Pilldue.slnx
dotnet run --project src/Pilldue.UI
```

## Local SQLite database

By default the app database file is:

`%LocalAppData%/Pilldue/pilldue.db`

(on Windows; equivalent local application data folder elsewhere). Schema is applied with EF Core migrations (`PilldueDbBootstrap.MigrateAsync`). Config (UI language) remains a separate file store, not in SQLite.

## Repository layout

| Path | Purpose |
|------|---------|
| `Pilldue.slnx` | Solution |
| `src/Pilldue.Data` | Persistence (SQLite) |
| `src/Pilldue.Business` | Domain and application services |
| `src/Pilldue.UI` | Spectre.Console TUI entry point |
| `tests/Pilldue.Data.Tests` | Data unit tests |
| `tests/Pilldue.Business.Tests` | Business unit tests |
| `tests/Pilldue.IntegrationTests` | Multi-step scenario tests (no UI) |
| `docs/` | Project documentation ([use cases](use-cases.md)) |
| `.cursor/` | Cursor rules and project skills |
| `todo.md` | Decision questionnaire / notes |
