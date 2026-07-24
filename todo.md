# Pilldue — decisions before we build

Purpose of this branch: answer enough of these to know how to proceed.
Work through at your pace. Mark choices as you go; leave notes or links under each item.

Status key: `[ ]` open · `[~]` investigating · `[x]` decided

---

## 1. Product shape

- [x] **Who is this for?**
  - Only you (personal tool)
  - Household / family sharing
  - Others later (open source, friends)
  - Notes: **Personal only.** Anyone else in the household runs their own local instance. Never multi-tenant / multi-user.

- [x] **Core job (v1)**
  - Track meds + when refill is due
  - Also dose reminders / adherence
  - Also inventory (pills left in bottle)
  - Also prescriptions / pharmacy info
  - Notes:
    - **In:** meds + schedule, pills left → refill-by date, log refill, refill history, **flag a skipped/missed dose** (increases pills left; not a reminder system).
    - **Out:** pharmacy / Rx / doctor fields; dose reminders / “take now” prompts.

- [x] **Must-have for first usable version**
  - List 3–5 things that make it “worth opening”:
  1. See which meds need refill soon (and by when)
  2. Add/edit meds with schedule + current stock
  3. Log a refill (reset / add stock)
  4. Flag a skipped dose so stock goes back up
  5. Keep simple refill history

---

## 2. Client / UI

- [x] **Primary interface**
  - Console / rich TUI (Spectre.Console, Terminal.Gui, …)
  - Desktop GUI (WPF, WinUI, Avalonia, …)
  - Web app (browser)
  - Hybrid later (e.g. CLI + web)
  - Notes: **Small local TUI for v1.** Extra UIs can be added later via a separate project on the same business layer.

- [ ] **Platforms you care about**
  - Windows only
  - Windows + macOS / Linux
  - Phone (later or never)
  - Notes:

- [x] **Offline-first?**
  - Must work fully offline
  - Online OK if sync is nice-to-have
  - Always online is fine
  - Notes: **Local exe, no server required for v1.**

---

## 3. Hosting & where data lives

- [x] **Where does the app run?**
  - Local only (on your machine)
  - Free cloud (Railway, Fly.io, Render, Azure free tier, Cloudflare, …)
  - Self-hosted (home server / VPS)
  - Packaged desktop install; no server
  - Notes: **Small published exe, local only for v1.**

- [x] **Where is data stored?**
  - Local files next to the app
  - Dropbox / OneDrive / Google Drive synced folder
  - Cloud database / API you host
  - Something else
  - Notes: **On the machine with the app** (exact format under §4).

- [ ] **Backup & portability**
  - Manual export (JSON/CSV) enough?
  - Auto sync to cloud folder?
  - Version history / recover mistaken deletes?
  - Notes:

- [x] **Multi-device**
  - One machine is enough for v1
  - Need same data on laptop + desktop (+ phone later)
  - Notes: **One machine / one local DB per person for v1.** No sync product.

---

## 4. Database & persistence

- [x] **Storage style**
  - Flat files (JSON / YAML / SQLite file)
  - Embedded DB (SQLite)
  - Server DB (PostgreSQL, etc.) if you host something
  - Notes: **SQLite for v1.** JSON/txt not the primary store; export later if useful. All access via Data project.

- [ ] **If files: sync folder (Dropbox etc.) vs pure local**
  - Investigate: conflict if two devices edit at once
  - Investigate: encryption at rest for health-ish data
  - Notes:

- [ ] **Schema ownership**
  - Simple enough to edit by hand in a text editor?
  - Only via the app (safer)
  - Notes:

---

## 5. Privacy, safety, trust

- [ ] **Sensitivity**
  - Treat as personal health data → careful defaults
  - Lightweight personal list is fine for v1
  - Notes:

- [ ] **No medical claims**
  - App is a tracker/reminder, not advice — OK as product framing?
  - Notes:

- [x] **Secrets / accounts**
  - No login for v1
  - Optional account if cloud later
  - Notes: **No accounts.** Single local user implied by the machine/instance.

---

## 6. Tech stack (.NET)

- [x] **UI tech** (depends on §2)
  - Spectre.Console / Terminal.Gui / …
  - WPF / Avalonia / …
  - ASP.NET (Blazor, MVC, minimal APIs + SPA)
  - Notes: **Spectre.Console** for v1 TUI. Terminal.Gui only if we outgrow forms later.

- [x] **.NET version / TFM**
  - Latest LTS vs newest
  - Notes: **.NET 10 (`net10.0`)** — current latest and LTS (as of 2026).

- [x] **Solution layout**
  - Single project under `src/`
  - Multiple projects (domain / UI / persistence) from day one
  - Notes: **Three projects from the start:** Data (persistence), Business (domain/services), UI (TUI host). Goal: add another UI later without rewriting business/data.

---

## 7. Distribution & install

- [x] **How do you run it day to day?**
  - `dotnet run` from repo
  - Published single-file exe
  - Installer / winget later
  - Web URL
  - Notes: **Small exe** for day-to-day; `dotnet run` fine while developing.

- [ ] **Updates**
  - Pull git / rebuild
  - Auto-update (later)
  - Notes:

---

## 8. Nice-to-investigate (interesting, not blocking)

Use these as research rabbit holes when you have energy; none are required for day one.

- [ ] **Refill math**
  - Inputs: quantity, dose frequency, start date, leftover count
  - Edge cases: as-needed (PRN), tapering, pauses, dual pharmacies
  - Notes:

- [ ] **Notifications**
  - Terminal only vs OS notifications vs email/push
  - Notes:

- [ ] **Imports**
  - Pharmacy PDFs / photos of labels (OCR) — later?
  - Notes:

- [ ] **Calendar export**
  - ICS for “refill by” dates
  - Notes:

- [ ] **Open source**
  - Private forever vs public repo later
  - License if public
  - Notes:

- [x] **Testing strategy**
  - Domain logic unit tests first; UI later
  - Notes: **Data + Business unit tests; IntegrationTests for multi-step action→assert scenarios (no UI).**

- [ ] **i18n / locale**
  - English only vs dates/units localized
  - Notes:

---

## 9. Process for this branch

When a decision is made:

1. Mark it `[x]` and write the choice in Notes.
2. Mirror lasting decisions in `docs/architecture.md` (and getting-started / development if relevant).
3. Update `.cursor/rules/project-overview.mdc` if agents should stop treating UI/stack as undecided.
4. Only then scaffold the real project under `src/`.

### Minimum to leave this “decision” phase

Enough to answer:

1. **UI:** console / desktop GUI / web — **done: small TUI (Spectre.Console lean)**
2. **Where app runs:** local / cloud / both — **done: local exe**
3. **Where data lives:** files / SQLite / hosted DB (+ sync story if multi-device) — **done: lean SQLite (local)**
4. **v1 scope:** what’s in / out — **done (see §1)**

Still soft before scaffold: none for stack — ready to scaffold when you want.

---

## Scratch / next session

Freeform notes:

```
Date: 2026-07-24
Decided today:
  - Personal local instances only (never multi-user)
  - v1: refill tracking + stock + refill log/history + skipped-dose stock bump
  - Out: pharmacy/Rx/doctor, dose reminders
  - Local exe + Spectre.Console TUI; Data/Business/UI; SQLite; .NET 10
Still unsure:
  - (none blocking)
Next step:
  - Merge this branch to main
  - Design phase, then multi-agent implementation
```
