# Improvements

Ideas to pick up later. Nothing here is a decision yet.

When an idea is chosen, record the outcome in [architecture.md](architecture.md) or [use-cases.md](use-cases.md) and check it off below.

Status key: `[ ]` open · `[~]` in progress · `[x]` done

Add the next idea as a new numbered section at the bottom.

---

## 1. Extract the data layer from the terminal host

- [x] Done (`Pilldue.App` host + `PilldueComposition` — #69)

`Pilldue.Data` implements the Business ports. Composition goes through `PilldueComposition`. #107 removed the terminal host and screens. The WPF host is the only UI.

## 2. Terminal and desktop UI on one business layer

- [x] Done (WPF `Pilldue.UI.Desktop` — #70)

Keep one shared business layer and add a WPF desktop UI beside the Spectre terminal. Both call `IPilldueApp` via `PilldueComposition`. Persistence stays in `Pilldue.Data`.

```
Pilldue.UI.Desktop    WPF host (Windows-only)
Pilldue.UI.Wpf        WPF screens
Pilldue.Business      domain, ports, planning, app service
Pilldue.Data          EF Core + SQLite + config file + composition
```

The desktop app is Windows-only (`net10.0-windows`). #107 removed `Pilldue.App` and `Pilldue.UI`. The WPF host is the only UI.

## 3. Doses that are not every day

- [x] Done (interval days from Rx start; weekly = every 7 days — #71)

`DailyDosagePills` is pills per dose. `DoseIntervalDays` (`1` daily, `2`, `3`, `7` weekly) controls which calendar days consume stock, counted from the prescription start.

Decision: weekly is every 7 days from the prescription start, not a chosen weekday.

## 4. Flag a missed dose

- [x] Done (separate flag, no stock change — #72)

When a dose is missed, record it so it can be seen later: medication, date, and that it was missed.

The current Skip dose screen (`SkipDoseForm` → `SkipDoseAsync`) returns pills to stock and stores a skip-dose event. That corrects inventory. This idea is the flag itself — a mark that the dose was missed, visible afterward.

Decision: separate mark that only records the miss (stock unchanged).

## 5. Bug: Add medication has no way back

- [x] Done (empty name cancels; confirm before save — #73)

Starting **Add medication** walks every field (name through prescription duration) and then saves. There is no Back or Cancel, so the only way out is to finish the form.

Refill already offers Cancel before it writes. Edit medication uses the same field prompts, so the same gap is there too.

Fix: abandon Add (and Edit) before any save and return to the main menu.

## 6. Android app on the shared business/data layer

- [ ] Open

Yes, this is doable. Keep `Pilldue.Business` and `Pilldue.Data` (EF Core + SQLite) as the shared core, and add a .NET MAUI app for Android and iOS that calls `IPilldueApp` the same way the WPF host does.

```
Pilldue.UI.Desktop    WPF host (existing)
Pilldue.UI.Wpf        WPF screens (existing)
Pilldue.UI.Mobile     .NET MAUI (Android and iOS)
Pilldue.Business      domain, ports, planning, app service
Pilldue.Data          EF Core + SQLite + config file
```

The mobile shell is a separate project. SQLite still lives on the device; install via sideload or a store later. Same personal-local product: one DB per phone, no cloud sync in this idea.

Every UI shares `PilldueComposition`. Android and iOS ship together (#76).
