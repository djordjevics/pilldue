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
| `docs/` | Project documentation |
| `.cursor/` | Cursor rules and project skills |
| `todo.md` | Decision questionnaire / notes |
