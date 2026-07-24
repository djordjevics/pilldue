# Pilldue v1 use cases

Personal local refill tracker. One instance per person. No multi-user, no dose reminders, no pharmacy contact fields.

## Config

| Key | Default | Storage |
|-----|---------|---------|
| Default refill day of month | **5** | Config file next to the app |

Medications inherit this day unless overridden.

## Medication definition

| Field | Meaning |
|-------|---------|
| Name | Display name |
| Package size | Pills per package (e.g. 12, 28, 30, 60) |
| Prescribed package count | Usual packages obtained each refill |
| Daily dosage | Pills consumed per day |
| Current stock | Pills on hand |
| Refill day override | Nullable 1–31; `null` = use config default (5) |
| Prescription start | Start of current prescription validity |
| Prescription duration | Default **6 months** (or explicit end date) |

**Effective refill day** = override if set, else config default.

**Edge case:** if day-of-month is invalid for a month (e.g. 31), clamp to last day of that month.

## Flow 1 — Refill-day planning queries

Using each med’s effective refill day and current stock:

1. **Covers until next refill?** — Does stock last until the next occurrence of that day-of-month?
2. **Short before next refill** — List meds whose stock runs out *before* the next refill day.
3. **Need extra for second refill** — List meds where `prescribedPackageCount` packages are not enough for stock to last until the *second* upcoming refill day (user should buy more than usual).

## Flow 2 — Refill by packages

User logs a refill with **N packages** →  
`currentStock += N × packageSize`.

Record a refill history entry (date, med, package count).

## Flow 3 — Calendar

Show a date range with, per medication:

- **Last covered day** — last calendar day current stock lasts (daily dosage; after skips/refills)
- **Prescription end date** — when the prescription must be renewed (~6 months from start)

## Flow 4 — Skipped dose

User flags a skipped/missed dose → increase stock by the dose amount (typically one day of `dailyDosage`) so last covered day moves later. Inventory correction only — not a reminder system.

## Derived values (Business)

- Next refill date / second refill date from “today” + day-of-month rule
- Last covered date from `floor(stock / dailyDosage)` (define inclusive/exclusive day rule in contracts PR and keep tests consistent)
- Prescription end from start + duration
- Shortfall package counts for flow 1.3

## Out of scope (v1)

- Dose reminders / push notifications
- Pharmacy / doctor / Rx number fields
- Multi-device sync / accounts
- Cloud hosting
