# Architecture

## Product intent

Pilldue helps you track medications against a **monthly refill day**, package-based stock, and prescription validity — so you know what to buy before the next (or second) refill and when to renew the prescription.

**Audience:** personal tool only. One local instance per person; never multi-tenant. See [use-cases.md](use-cases.md) for v1 flows.

## v1 scope

### In

- Config default refill **day of month** (5), per-med override
- Med definition: package size, prescribed package count, daily dosage, stock, prescription window (~6 months)
- Queries: stock vs next refill day; short list; need-extra-packages for second refill day
- Refill by package count; skip-dose stock bump; calendar (last covered + prescription end)

### Out

- Pharmacy / Rx numbers / doctor contacts
- Dose reminders / push
- Multi-user, cloud, sync accounts

## Stack

| Topic | Choice |
|-------|--------|
| Delivery | Local small published exe |
| UI | Spectre.Console |
| Persistence | **SQLite via EF Core** (migrations for schema) |
| Config | File (default refill day) — not EF |
| .NET | .NET 10 (`net10.0`) |
| Solution | [Pilldue.slnx](../Pilldue.slnx) |

### Schema and data access

- Use **EF Core** + SQLite in `Pilldue.Data` for tables and repository implementations.
- Manage schema with **EF migrations**; apply on startup (or explicit migrate in tests with temp DB).
- Do **not** use Dapper or hand-rolled ADO for v1.
- Config file stays a simple file store (JSON or similar), separate from EF.

## Solution layout and dependencies

```
Pilldue.slnx
src/
  Pilldue.Business/    # domain, ports, pure logic, app services
  Pilldue.Data/        # EF Core + SQLite (migrations) + config file implementations of ports
  Pilldue.UI/          # Spectre.Console composition root
```

Target dependency direction:

```mermaid
flowchart TB
  UI[Pilldue.UI] --> Business[Pilldue.Business]
  UI --> Data[Pilldue.Data]
  Data --> Business
```

- Ports and entities live in **Business** (plus in-memory fakes for tests/UI until EF lands)
- **Data** implements ports with **EF Core + SQLite** (and config file for app settings)
- **UI** is the composition root and wires implementations

Shared planning formulas live in `RefillCalendarRules` (day clamp, packages-to-buy, inclusive last-covered rule). Full query implementations are tracked in business issues C1–C9.

## Tests

```
tests/
  Pilldue.Data.Tests/
  Pilldue.Business.Tests/
  Pilldue.IntegrationTests/   # multi-step actions → assert; no UI
```

## Docs

- [Use cases](use-cases.md)
- [Getting started](getting-started.md)
- [Development](development.md)
