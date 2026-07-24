# pilldue

Terminal medication tracker: log your therapy schedule and know when each med needs a refill.

## Status

**.NET 10** + **Spectre.Console** + **EF Core / SQLite** bootstrap (schema + migrations; repository CRUD still landing). Layers: **Data / Business / UI**. Default DB: `%LocalAppData%/Pilldue/pilldue.db`.

**v1:** monthly refill day (default 5th), package-based stock, prescription end ~6 months, skip-dose stock bump, calendar. See [use cases](docs/use-cases.md).

```bash
dotnet test Pilldue.slnx
dotnet run --project src/Pilldue.UI
```

Docs: [Use cases](docs/use-cases.md) · [Architecture](docs/architecture.md) · [Getting started](docs/getting-started.md) · [Development](docs/development.md) · [todo.md](todo.md)
