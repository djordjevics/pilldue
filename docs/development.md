# Development

## Working with Cursor

This repository is set up for AI-assisted development in Cursor.

- **Rules** — `.cursor/rules/` holds persistent project guidance. `project-overview.mdc` always applies.
- **Skills** — `.cursor/skills/` holds optional, task-specific skills (`skill-name/SKILL.md`). None exist yet; add them when workflows are stable.

See [.cursor/skills/README.md](../.cursor/skills/README.md) for how to add skills.

## Documentation

When structure or decisions change, update the relevant files under `docs/` and keep links in the root [README](../README.md) accurate.

## Layering

**UI → Business → Data**. Do not reference Data from UI, or UI from Business/Data. Persistence (SQLite) stays inside Data.

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
