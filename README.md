# pilldue

WPF medication tracker: log your therapy schedule and know when each med needs a refill.

## Status

**.NET 10** + **WPF and .NET MAUI (Android and iOS)** + **EF Core / SQLite**. Layers: **Data / Business / WPF / MAUI**. WPF database: `%UserProfile%/Pilldue/pilldue.db`. The MAUI host stores `pilldue.db` in the device app data directory.

**v1:** monthly refill day from prescription start, package-based stock, prescription end ~6 months, skip-dose stock bump, calendar. See [use cases](docs/use-cases.md).

```bash
dotnet test Pilldue.slnx
dotnet run --project src/Pilldue.UI.Desktop
```

Docs: [Use cases](docs/use-cases.md) · [Architecture](docs/architecture.md) · [Getting started](docs/getting-started.md) · [Development](docs/development.md) · [Improvements](docs/improvements.md) · [todo.md](todo.md)
