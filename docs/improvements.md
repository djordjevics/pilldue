# Improvements

Ideas to pick up later. Nothing here is a decision yet.

When an idea is chosen, record the outcome in [architecture.md](architecture.md) or [use-cases.md](use-cases.md) and check it off below.

Status key: `[ ]` open · `[~]` in progress · `[x]` done

Add the next idea as a new numbered section at the bottom.

---

## 1. Extract the data layer from the terminal host

- [ ] Open

`Pilldue.Data` already implements the Business ports (EF Core + SQLite, config file). `Pilldue.UI` is also the composition root: it references Data and wires the database at startup.

Move that wiring out of the terminal project. A small host opens SQLite, runs migrations, and passes `IPilldueApp` into the UI. Terminal screens then depend on Business only, and a second UI can reuse the same host.

## 2. Terminal and desktop UI on one business layer

- [ ] Open

Keep one shared business layer and add a WPF desktop UI beside the Spectre terminal. Both call `IPilldueApp` and the planning code in `Pilldue.Business`. Persistence stays in `Pilldue.Data` (see idea 1 for who wires it).

```
Pilldue.UI            Spectre terminal (existing)
Pilldue.UI.Desktop    WPF
Pilldue.Business      domain, ports, planning, app service
Pilldue.Data          EF Core + SQLite + config file
```

The desktop app is Windows-only. WPF stays in its own project so the terminal UI keeps working on its own.

## 3. Doses that are not every day

- [ ] Open

`DailyDosagePills` and the planning math (last covered day, shortfall, extra packages, calendar stock-outs) assume one dose every calendar day.

Some medications are taken every 2 days, every 3 days, or weekly. Store an interval in days (`1` daily, `2`, `3`, `7` weekly) and consume stock only on dose days.

Open point: weekly as every 7 days from the prescription start, or on a chosen weekday.

## 4. Flag a missed dose

- [ ] Open

When a dose is missed, record it so it can be seen later: medication, date, and that it was missed.

The current Skip dose screen (`SkipDoseForm` → `SkipDoseAsync`) returns pills to stock and stores a skip-dose event. That corrects inventory. This idea is the flag itself — a mark that the dose was missed, visible afterward.

Open point: same action as skip-dose (stock goes up), or a separate mark that only records the miss.

## 5. Bug: Add medication has no way back

- [x] Done (empty name cancels; confirm before save — #73)

Starting **Add medication** walks every field (name through prescription duration) and then saves. There is no Back or Cancel, so the only way out is to finish the form.

Refill already offers Cancel before it writes. Edit medication uses the same field prompts, so the same gap is there too.

Fix: abandon Add (and Edit) before any save and return to the main menu.

## 6. Android app on the shared business/data layer

- [ ] Open

Yes, this is doable. Keep `Pilldue.Business` and `Pilldue.Data` (EF Core + SQLite) as the shared core, and add a .NET MAUI Android UI that calls `IPilldueApp` the same way the terminal and a future WPF desktop would.

```
Pilldue.UI            Spectre terminal (existing)
Pilldue.UI.Desktop    WPF (idea 2)
Pilldue.UI.Mobile     .NET MAUI (Android)
Pilldue.Business      domain, ports, planning, app service
Pilldue.Data          EF Core + SQLite + config file
```

Spectre.Console does not run on Android, so the mobile shell is a separate project. SQLite still lives on the device; install via sideload or a store later. Same personal-local product: one DB per phone, no cloud sync in this idea.

Depends on extracting data wiring from the terminal host (idea 1) so every UI shares one composition path. Open point: Android only first, or MAUI for Android and iOS together.
