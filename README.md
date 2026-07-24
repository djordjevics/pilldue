# pilldue

Terminal medication tracker: log your therapy schedule and know when each med needs a refill.

## Status

Scaffolded empty solution on **.NET 10** + **Spectre.Console** + **SQLite** (persistence not implemented yet). Layers: **Data / Business / UI**.

**v1:** meds, stock → refill-by, log refill, history, skipped-dose stock bump. **Out:** pharmacy/Rx/doctor, dose reminders.

```bash
dotnet test Pilldue.slnx
dotnet run --project src/Pilldue.UI
```

Decisions: [todo.md](todo.md) · [Architecture](docs/architecture.md) · [Getting started](docs/getting-started.md) · [Development](docs/development.md)
