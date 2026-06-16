# Roadmap

A technical, dependency-ordered build plan. Each item notes what it **unlocks**
and what it **depends on**, so work can be picked up in an order that avoids
rework. This file is about *build order*; the live task checklist lives in
[TODO.md](TODO.md).

## Where we are now

The playable vertical slice already exists and now runs through a real scene
flow — `Assets/Scenes/Bootstrap.unity` → `Assets/Scenes/Menu.unity` →
`Assets/Scenes/Main.unity` (gameplay; shop and game over are overlays inside the
gameplay scene). Scenes are rebuildable via `Rollfaehren Fury > Build Prototype
Scene` and `Rollfaehren Fury > Build Bootstrap And Menu Scenes`.

- `GameManager` — game states (Idle / Playing / Shop / GameOver), money, rounds, crossing timer
- `Health` — reusable health/damage component with events (target-agnostic)
- `HitscanWeapon` — one weapon: fire, cooldown, damage, aim assist
- `SimpleEnemy` + `EnemySpawner` — one enemy, round-scaled spawning, contact damage
- `FerryDamageTarget` — the ferry as the protected/damageable object
- `SimpleHUD` — HUD + shop panel + game over panel
- `SceneFlow` / `BootstrapLoader` / `MainMenuController` / `GameplayMenuInput` — bootstrap → menu → main flow, New Game / Settings / Quit, Esc/Cancel returns gameplay to the menu
- Project-wide **Input System** layer (`PrototypeInputActions` + `Assets/InputSystem_Actions.inputactions`) — gameplay and menu read `InputAction` callbacks instead of polling devices
- `PrototypeAudioEvents` — Wwise hook points (shoot / hit / enemy death / ferry damage)
- Fraunz player visual + a vending-machine asset (recently merged)

**The core loop is functionally complete:** start from the menu, move, shoot,
kill for money, survive the crossing, buy one of three upgrades, face a harder
round, game over on ferry death, Esc back to the menu.

### Known shortcuts in the current code (these drive the order below)

- Upgrades and their costs are **hardcoded in `GameManager`** — not data-driven.
- There is **one concrete weapon**; no weapon base class or inventory.
- The **Settings panel exists but has no real options yet**.
- Wwise **hooks exist but banks/events are not wired up** (banks are gitignored;
  generate them locally — see [WWISE.md](WWISE.md)).

## Where we want to get to

A full ferry-defense vertical slice: multiple weapons, multiple enemy types,
cargo as a second protected object, a real data-driven shop, working Wwise audio,
low-poly art (ferry / shore / enemy / weapon), and a presentable Windows build.
Scope and MVP boundaries stay as defined in [GAME_DESIGN.md](GAME_DESIGN.md) and
[../AGENTS.md](../AGENTS.md).

## Build order (by dependency)

Because the loop and the scene/menu flow are done, the rule from here is **logic
and systems come before the content that needs them** — exactly so the shop and
weapon code are not rebuilt twice.

### Tier 0 — Stabilize what exists

Cheap, unblocks honest testing. Depends on: nothing.

- Visible ferry damage / low-health feedback (uses existing `Health` events).
- Tuning pass: enemy speed, spawn timing, ferry health, weapon damage, shop prices, crossing duration.
- Confirm a clean fresh clone + Play Mode (through Bootstrap → Menu → Main) on a teammate machine.

### Tier 1 — System foundations (build the logic before the content)

The enablers. These are **not** one linear chain — they are two independent
tracks that can be built in parallel (e.g. one dev each). Within a track the
steps are top-down dependent; between tracks they are not.

```text
Track A (weapons):   Weapon base (A1) ──▶ WeaponSystem (A2)
Track B (shop):      UpgradeSystem (B1) ──▶ ShopManager (B2)
                                   │
                  (glue) weapon upgrades target the active weapon
                         once A2 and B1 both exist
```

**Track A — weapons**

- **A1. `Weapon` base** — extract the fire / damage / cooldown contract out of `HitscanWeapon` (becomes a subclass). Build on the existing Input System layer rather than re-polling devices.
  - Depends on: nothing (pure refactor, loop keeps working). Unlocks: every additional weapon type.
- **A2. `WeaponSystem`** — owns one or more weapons, handles selection/switching, exposes the active weapon to `GameManager`/HUD.
  - Depends on: A1. Unlocks: multiple weapons, weapon-specific upgrades. (Thin until the second weapon exists — build it right before Tier 2 weapons.)

**Track B — shop & upgrades**

- **B1. `UpgradeSystem`** — data-driven upgrades (ScriptableObject: id, cost, target, effect) applied to player/weapon/ferry. Moves the hardcoded upgrade logic out of `GameManager`.
  - Depends on: nothing — can target the existing `HitscanWeapon` directly. Unlocks: a flexible shop, the "three choices" design, per-weapon upgrades.
- **B2. `ShopManager`** — presents upgrades from `UpgradeSystem`, handles purchase and affordability (replaces the fixed buttons wired through `SimpleHUD`).
  - Depends on: B1.

**Glue (integration, not a prerequisite)**

- Once both A2 and B1 exist, route weapon upgrades (damage, fire rate) through the
  **active weapon** from `WeaponSystem` instead of the single `HitscanWeapon`
  reference. Small wiring commit; do it when both tracks have landed.

### Tier 2 — Content on top of the systems

Each depends on its Tier 1 enabler; otherwise parallelizable.

- **More weapons** (flamethrower, spread, stronger gun) — depends on `Weapon` base + `WeaponSystem`.
- **More / choice-based upgrades** (3-choice shop, weapon upgrades) — depends on `UpgradeSystem` + `ShopManager`.
- **Enemy variants** (fast, tanky; later flying/swimming) — a light extension of `SimpleEnemy` + spawner weighting; depends only on the existing enemy/spawner.
- **Cargo** (second protected object + survival reward) — reuses `Health` plus a `CargoTarget` like `FerryDamageTarget`, slots into round flow + economy. Add only once the ferry-only loop is stable.

### Tier 3 — Game framing & UX

Mostly independent of Tier 1/2; can run in parallel.

- **Main menu + scene flow — done** (Bootstrap → Menu → Main, New Game / Settings / Quit, Esc/Cancel back to menu, project-wide Input System). Remaining: real **Settings** options (audio/sensitivity), and pause/resume polish.
- **Fraunz character** animation states + first-person presence (partially merged).
- **Vending-machine shop** interaction/flavor (asset merged) — depends on `ShopManager`.
- HUD/UI styling pass.

### Tier 4 — Art & presentation

Depends on the systems being stable so art replaces placeholders cleanly.

- Low-poly ferry, shore, weapon, and enemy models replacing the primitives.
- Water, lighting, and low-poly art direction.
- Wwise audio (deferred from Tier 0 — nothing depends on it): generate SoundBanks, wire the events matching `PrototypeAudioEvents`, then mix + river/ferry ambience + music. Until then the `Init.bnk not found` Console errors are expected noise.
- Animations where they clarify behaviour.

### Tier 5 — Ship

- Balancing pass with real values.
- Windows build from `Bootstrap.unity`; verify no missing assets or Wwise banks.
- Demo script; update README controls and known issues.

## Critical path (shortest line to "more than a tech demo")

Tier 0 (damage feedback + tuning) → Track A (A1 `Weapon` base + A2 `WeaponSystem`)
→ Tier 2 (a second weapon + one enemy variant) → Tier 4/5 polish. The main menu
and scene flow are already in place, so framing is no longer on the critical path.

Track B (B1 `UpgradeSystem` + B2 `ShopManager`) only becomes urgent once you want
more than the current three upgrades — do it right before expanding the shop, not
before. Track A and Track B can progress in parallel.
