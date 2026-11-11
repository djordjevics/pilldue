# pilldue

WPF medication tracker: log your therapy schedule and know when each med needs a refill.

## Status

**.NET 10** + **WPF** + **EF Core / SQLite**. Layers: **Data / Business / WPF**. Default DB: `%UserProfile%/Pilldue/pilldue.db` (`config.json` in the same folder).

**v1:** monthly refill day from prescription start, package-based stock, prescription end ~6 months, skip-dose stock bump, calendar. See [use cases](docs/use-cases.md).

```bash
dotnet test Pilldue.slnx
dotnet run --project src/Pilldue.UI.Desktop
```

Docs: [Use cases](docs/use-cases.md) · [Architecture](docs/architecture.md) · [Getting started](docs/getting-started.md) · [Development](docs/development.md) · [Improvements](docs/improvements.md) · [todo.md](todo.md)
