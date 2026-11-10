# Development

## Working with Cursor

This repository is set up for AI-assisted development in Cursor.

- **Rules** — `.cursor/rules/` holds persistent project guidance. `project-context.mdc` always applies. The index is `implementation-rules-index.mdc`.
- **Skills** — `.cursor/skills/` holds task skills: `github-ticket-flow` and `review-pr` (`@review-pr`).

See [.cursor/skills/README.md](../.cursor/skills/README.md) for how to add skills.

## Documentation

When structure or decisions change, update the relevant files under `docs/` and keep links in the root [README](../README.md) accurate.

## Git workflow

- Branch from `main` using the issue id as prefix: `3-sqlite-bootstrap`, `11-shortfall-query`.
- Open a PR into `main` (do not push commits directly to `main`).
- Prefer one issue per PR when practical.
- Track that work on GitHub Issues and one Project. Status is Backlog, Ready, In Progress, Done. See `.cursor/rules/issue-tracking.mdc`.

## Parallel agents

- Prefer **cheaper/faster** models for pure helpers, thin UI, config, and well-scoped CRUD.
- Prefer **stronger** models for EF migrations, facade/cross-layer work, and hard rebases.
- If a cheap agent fails twice on the same issue, retry with a stronger model.
- Details: `.cursor/rules/project-context.mdc` (Parallel agents). One issue per worktree: `.cursor/rules/ticket-worktree.mdc`.

## Layering

DIP: **UI → Business**, **Data → Business**. Hosts (`Pilldue.App`, `Pilldue.UI.Desktop`) reference Data to compose the app. Persistence stays inside Data.

**SQLite access:** EF Core only (migrations for schema). No Dapper/ADO in v1. Config file I/O is separate from EF.

Default DB path: `%UserProfile%/Pilldue/pilldue.db`, with `config.json` beside it (`SqliteDatabasePaths`). Hosts reach it through `PilldueComposition`. Apply schema with `PilldueDbBootstrap.MigrateAsync` (also used from UI startup). A database left at `%LocalAppData%/Pilldue/pilldue.db` is moved on startup when the profile folder has none.

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

## UI strings (i18n)

User-facing Spectre copy lives in `src/Pilldue.UI/Localization/UiLocalizer.cs` (English + Serbian Latin catalogs).

1. Add the same key to **both** `English` and `Serbian` dictionaries.
2. Call `UiLocalizer.Get("Your.Key")` / `Format(...)` from UI screens (never hardcode English in menus/prompts).
3. Language is stored as `AppConfig.UiLanguage` (`en` / `sr`, empty = detect OS) via the JSON config file (`%UserProfile%/Pilldue/config.json`). Changing language in the menu applies immediately; no reinstall needed.

`tests/Pilldue.UI.Tests` asserts every English key exists in Serbian.

## Build and run

```bash
dotnet build Pilldue.slnx
dotnet run --project src/Pilldue.App
```

Publish a small exe when ready to distribute:

```bash
dotnet publish src/Pilldue.App -c Release -o ./publish
```
