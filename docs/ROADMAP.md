# Roadmap

A technical, dependency-ordered build plan. Each item notes what it **unlocks**
and what it **depends on**, so work can be picked up in an order that avoids
rework. This file is about *build order*; the live task checklist lives in
[TODO.md](TODO.md).

## Where we are now

The playable vertical slice already exists and now runs through a real scene
flow — `Assets/Scenes/Bootstrap.unity` → `Assets/Scenes/Menu.unity` →
`Assets/Scenes/Main.unity`, with `Assets/Scenes/ShopInterior.unity` loaded
additively from either shore during preparation. Scenes are rebuildable via
`Rollfaehren Fury > Build Prototype Scene`,
`Rollfaehren Fury > Build Bootstrap And Menu Scenes`, and
`Rollfaehren Fury > Build Shared Shop Interior`.

- `GameManager` — docked preparation, crossing, augment, and game-over states
- `Health` — reusable health/damage component with events (target-agnostic)
- `WeaponSystem` + `Weapon` + `WeaponDefinition` — data-driven weapons (Track A): the active weapon fires, weapons switch, upgrades hit the active weapon
- `SimpleEnemy` + `EnemySpawner` — weighted fish/pigeon profiles, round unlocks, surface/flying movement, and contact damage
- `FerryDamageTarget` — the ferry as the protected/damageable object
- `SimpleHUD` — HUD + shop panel + game over panel
- `SceneFlow` / `BootstrapLoader` / `MainMenuController` / `GameplayMenuInput` — bootstrap → menu → main flow plus an in-game pause overlay
- `FerryController` / `RoundStartConsole` — alternating physical crossings started manually from the ferry house
- Project-wide **Input System** layer (`PrototypeInputActions` + `Assets/InputSystem_Actions.inputactions`) — gameplay and menu read `InputAction` callbacks instead of polling devices
- `PrototypeAudioEvents` — Wwise hook points (shoot / hit / enemy death / ferry damage)
- Static Fraunz player visual + a non-interactive vending-machine decoration
- Shared indoor shop scene with additive enter/exit transitions

**The core loop is functionally complete:** start from the menu, move, shoot,
kill for money, survive the crossing, buy one of three upgrades, face a harder
round, game over on ferry death, Esc back to the menu.

### Known shortcuts in the current code (these drive the order below)

- Upgrades and their costs are **hardcoded in `GameManager`** — not data-driven (Track B).
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
- Tuning pass: enemy mix/speed, spawn timing, ferry speed/health, weapon damage, and shop prices.
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

**Track A — weapons — implemented (pending Unity verification)**

Built data-driven (per the chosen design) instead of an inheritance tree:

- **A1. `WeaponDefinition` (ScriptableObject) + data-driven `Weapon`** — replaces the single hitscan weapon. `Weapon` reads a definition and fires by `WeaponFireMode` (hitscan / spread); it keeps runtime copies of the stats so upgrades never mutate the shared asset.
- **A2. `WeaponSystem`** — owns the firing input (`Player/Attack`), holds the weapon list, switches the active weapon (`Player/Next` / `Player/Previous`), and forwards fire/hit events to HUD + audio.
- Plus a second weapon (Shotgun, spread) to prove the abstraction end-to-end. New weapons are now just a `WeaponDefinition` asset under `Assets/Weapons/`.

Remaining: verify in Unity — run `Build Prototype Scene`, confirm no compile errors and that both weapons fire and switch.

**Track B — shop & upgrades — implemented (pending Unity verification)**

- **B1. `UpgradeDefinition` (polymorphic SO) + effects** — abstract `Apply(UpgradeContext)`; `WeaponDamageUpgrade` / `FireRateUpgrade` / `FerryHealthUpgrade` migrate the old hardcoded upgrades to assets. Hardcoded upgrade logic is out of `GameManager`.
- **B2. `ShopManager`** — catalog of `UpgradeDefinition` assets + UI buttons; purchases go through `GameManager.TryPurchase`; one-off master upgrades tracked per run.
- Plus the first **master upgrade**: Pistol **Querschläger** (ricochet to the nearest enemy) — proves the polymorphic model carries exotic effects.

Remaining: verify in Unity. Per-weapon base upgrades + more master upgrades need new mechanics (magazine/reload, knockback, fuel).

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
