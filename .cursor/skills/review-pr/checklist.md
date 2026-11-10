# Standards checklist

Use after path routing in `SKILL.md`. Read the linked rule for details.

Mark each applicable item pass, fail, or N/A.

## Always

| Check | Rule |
|-------|------|
| Screen projects (`Pilldue.UI`, `Pilldue.UI.Wpf`) do not reference Data, EF, or SQL. `Pilldue.UI.Wpf` references Business only and does not reference Spectre | `layer-structure` |
| Business does not reference `Pilldue.Data` or EF | `layer-structure` |
| Refill math stays in Business | `layer-structure`, `model-separation` |
| PR base is `main` | `create-pr` |
| Branch is `<issue>-<slug>` (no `feature/` prefix) | `project-context` |
| `Closes #N` only if the PR finishes that issue | `github-ticket-flow` |

## Domain and persistence

| Check | Rule |
|-------|------|
| New persisted field is on a Business domain type, mapped in `PilldueDbContext` | `model-separation` |
| Schema change has an EF migration; snapshot was not hand-written | `migrations` |
| Rename or drop is safe for an existing `%LocalAppData%/Pilldue/pilldue.db` | `migrations` |
| No Dapper or hand-rolled ADO | `project-context` |

## UI copy

| Check | Rule |
|-------|------|
| New key exists in both English and Serbian catalogs in `UiLocalizer` | `localization-both-resx` |
| Serbian values are Serbian Latin | `localization-both-resx` |
| Screens use `UiLocalizer.Get` / `Format` | `localization-both-resx` |

## Tests

| Check | Rule |
|-------|------|
| Domain behavior covered in `Pilldue.Business.Tests` | `testing` |
| New persistence covered in Data or Integration tests | `testing` |
| Integration tests do not drive Spectre | `testing` |
| UI string parity test still applies | `testing`, `localization-both-resx` |
