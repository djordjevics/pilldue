# Development

## Working with Cursor

This repository is set up for AI-assisted development in Cursor.

- **Rules** — `.cursor/rules/` holds persistent project guidance. `project-overview.mdc` always applies.
- **Skills** — `.cursor/skills/` holds optional, task-specific skills (`skill-name/SKILL.md`). None exist yet; add them when workflows are stable.

See [.cursor/skills/README.md](../.cursor/skills/README.md) for how to add skills.

## Documentation

When structure or decisions change, update the relevant files under `docs/` and keep links in the root [README](../README.md) accurate.

## Git workflow

- Branch from `main` using the issue id as prefix: `3-sqlite-bootstrap`, `11-shortfall-query`.
- Open a PR into `main` (do not push commits directly to `main`).
- Prefer one issue per PR when practical.

## Layering

DIP: **UI → Business**, **UI → Data**, **Data → Business**. Persistence stays inside Data.

**SQLite access:** EF Core only (migrations for schema). No Dapper/ADO in v1. Config file I/O is separate from EF.

In-memory port implementations live in Business for tests and early UI composition.

## Tests

| Project | Role |
|---------|------|
| `tests/Pilldue.Data.Tests` | Unit tests for SQLite / persistence |
| `tests/Pilldue.Business.Tests` | Unit tests for domain / application services |
| `tests/Pilldue.IntegrationTests` | Multi-step scenarios (actions → assert results), no UI |

```bash
dotnet test Pilldue.slnx
```

Integration tests drive Business (+ Data) like a scripted session: add med, skip dose, log refill, then assert stock / refill-by / history. They do not use Spectre.Console.

Pending feature tests use `[Fact(Skip = "...")]` until implemented so CI stays green.

## Build and run

```bash
dotnet build Pilldue.slnx
dotnet run --project src/Pilldue.UI
```

Publish a small exe when ready to distribute:

```bash
dotnet publish src/Pilldue.UI -c Release -o ./publish
```
