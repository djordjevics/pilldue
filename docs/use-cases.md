# Pilldue v1 use cases

Personal local refill tracker. One instance per person. No multi-user, no dose reminders, no pharmacy contact fields.

## Config

| Key | Default | Storage |
|-----|---------|---------|
| Default refill day of month | **6** | Config file next to the app |

Medications inherit this day unless overridden.

## Medication definition

| Field | Meaning |
|-------|---------|
| Name | Display name |
| Package size | Pills per package (e.g. 12, 28, 30, 60) |
| Prescribed package count | Usual packages obtained each refill |
| Daily dosage | Pills consumed per day |
| Current stock | Pills on hand |
| Refill day override | Nullable 1–31; `null` = use config default (6) |
| Prescription start | Start of current prescription validity |
| Prescription duration | Default **6 months** (or explicit end date) |

**Effective refill day** = override if set, else config default.

**Edge case:** if day-of-month is invalid for a month (e.g. 31), clamp to last day of that month.

## Flow 1 — Refill-day planning queries

Using each med’s effective refill day and current stock.

**Calendar months matter.** Days until the next refill day are the real calendar span between consecutive refill-day dates (28–31), not a fixed “30 days.”

Example: refill day = 6th, stock = 28 pills, daily dosage = 1. From 6 May → 6 June is **31** days → stock covers 28 days → **3 pills short**. If `packageSize` = 28, packages needed for that gap is `ceil(31 / 28) = 2` (or `ceil(3 / 28) = 1` extra on top of one usual package) → **suggest buying 2 packages**.

Queries:

1. **Covers until next refill?** — Does stock last until the next occurrence of that day-of-month (using actual day count)?
2. **Short before next refill** — List meds whose stock runs out *before* the next refill day; include **pills short** and **packages to buy** (`ceil(pillsShort / packageSize)`, minimum packages to close the gap).
3. **Need extra for second refill** — List meds where `prescribedPackageCount` packages are not enough for stock to last until the *second* upcoming refill day (user should buy more than usual). Same calendar-accurate day counts.

## Flow 2 — Refill by packages

User logs a refill with **N packages** →  
`currentStock += N × packageSize`.

Record a refill history entry (date, med, package count).

## Flow 3 — Calendar

From **today** through the **second** upcoming config refill day:

- Spectre month calendars with **stock-out days in red** (days when stock is below daily dosage)
- Simulation **assumes the usual prescribed packages are obtained at the first refill** before that day’s dose
- Notes list medications that run out in the window; table shows first/second refill, stock-out days, prescription end

## Flow 4 — Skipped dose

User flags a skipped/missed dose → increase stock by the dose amount (typically one day of `dailyDosage`) so last covered day moves later. Inventory correction only — not a reminder system.

## Derived values (Business)

- Next refill date / second refill date from “today” + day-of-month rule (clamp invalid days to month end)
- **Days in gap** = calendar days from one refill date to the next (month length sensitive)
- **Last covered date (inclusive):** `asOfDate + floor(stock / dailyDosage) - 1` days when floor > 0; otherwise none. Locked in `RefillCalendarRules`.
- **Prescription end:** `PrescriptionStartDate.AddMonths(PrescriptionDurationMonths)` (default 6); locked in `RefillCalendarRules`
- **Pills short** / **packages to buy** = `ceil(pillsShort / packageSize)` for flow 1 shortfalls (see 31-day / 28-pill example above)

## Out of scope (v1)

- Dose reminders / push notifications
- Pharmacy / doctor / Rx number fields
- Multi-device sync / accounts
- Cloud hosting
