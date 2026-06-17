# Todo

This list is the working task board for the prototype. Keep it practical and update it as tasks move into code, art, audio, or design work.

## Repository and Setup

- [x] Create Unity URP project.
- [x] Connect GitHub repository.
- [x] Add `.gitignore` and `.gitattributes`.
- [x] Integrate Wwise.
- [x] Add project docs.
- [x] Add first playable MVP branch: `codex/playable-mvp`.
- [ ] Verify a fresh clone on another machine.
- [x] Confirm Unity batchmode compiles the prototype scene builder.
- [ ] Confirm Unity Play Mode has no Console errors on a teammate machine.
- [ ] Decide final team branch rules.

## Game Flow

- [x] Create bootstrap/menu flow.
- [x] Add New Game button.
- [x] Add Settings placeholder.
- [x] Add Quit button.
- [x] Add gameplay Cancel/Esc return to menu.
- [x] Create one-scene game flow in `Assets/Scenes/Main.unity`.
- [x] Add game over state.
- [x] Add shop/upgrade state.
- [x] Add round restart/next round behavior.

## Player and Weapons

- [x] Add FPS movement and camera look.
- [x] Refactor player input to subscribed InputAction callbacks.
- [x] Add basic crosshair.
- [x] Add first weapon.
- [x] Refactor shooting input to subscribed InputAction callbacks.
- [x] Add enemy hit detection.
- [x] Add weapon cooldown.
- [x] Add Wwise hook for shooting.
- [x] Add Wwise hook for hit confirmation.
- [x] Track A1: extract data-driven `Weapon` + `WeaponDefinition` (ScriptableObject) from the single hitscan weapon.
- [x] Track A2: add `WeaponSystem` that owns firing input and weapon switching.
- [x] Track A: add a second weapon (Shotgun, spread fire mode) to prove the abstraction.
- [x] Add HUD active-weapon indicator (name + slot `[i/n]`, updates on switch).
- [x] Add placeholder shot tracer (`WeaponTracer`, pooled LineRenderer) so hitscan/spread shots are visible.
- [x] Verify Track A + HUD + tracer in Unity: scene built, weapons fire/switch, tracers show.
- [x] Tracer originates from the crosshair; widen Shotgun spread; add Flamethrower (spread).
- [x] Add `Projectile` fire mode + `Projectile` (gravity parabola, raycast hit, trail); Harpoon throws an arcing projectile.
- [x] Weapon switching: mouse scroll + keys `1`–`4` (1 Harpoon, 2 Pistol, 3 Shotgun, 4 Flamethrower).
- [ ] Verify projectiles in Unity: re-run `Build Prototype Scene`, Harpoon arcs and hits.

## Ferry and Cargo

- [x] Create placeholder ferry deck.
- [x] Create river/shore placeholder layout.
- [x] Implement ferry crossing progress.
- [x] Add ferry health.
- [ ] Add visible damage or warning feedback.
- [ ] Later: add cargo prefab.
- [ ] Later: add cargo health.
- [ ] Later: add cargo survival reward.

## Enemies

- [x] Create first enemy prefab.
- [x] Add enemy health.
- [x] Add movement toward ferry.
- [x] Add contact damage behavior.
- [x] Add `EnemySpawner`.
- [x] Add round-based spawn scaling.
- [ ] Add second enemy variant if time allows.

## Score, Money, and Upgrades

- [x] Add kill reward.
- [x] Add round completion reward.
- [x] Add money display.
- [x] Add shop UI.
- [x] Add first upgrade: weapon damage.
- [x] Add second upgrade: ferry health.
- [x] Add third upgrade: fire rate.
- [x] Add next-round difficulty increase.
- [x] Track B: data-driven `UpgradeSystem` (polymorphic `UpgradeDefinition`) + `ShopManager`; the 3 base upgrades are now assets.
- [x] Track B: first master upgrade — Pistol Querschläger (ricochet to nearest enemy).
- [ ] Verify Track B in Unity: re-run `Build Prototype Scene`, shop purchases apply, ricochet works.
- [ ] Later: per-weapon base upgrades + more master upgrades (need magazine/reload, knockback, fuel).
- [ ] Rebalance: upgrade costs lowered to 10/10/10/30 + 100 starting gold (testing values, retune later).
- [x] Track C: vending-machine shop automat (walk up + B opens the shop overlay any time on deck).
- [ ] Verify automat in Unity: re-run `Build Prototype Scene`, walk to the box, press B, buy, B to close.
- [x] Shop: Close/Exit button in the automat overlay; each upgrade max 3 buys, Querschläger 1.
- [ ] Track C: round-end augment draft (1 of 3) — replaces the round-end shop popup. (Next.)
- [ ] Later: add cargo survival reward.

## Art and Scene Props

- [ ] Low-poly ferry model.
- [x] Prototype shore placeholders.
- [ ] Later: cargo crates.
- [ ] Weapon model.
- [ ] First enemy model.
- [ ] Optional shop NPC or vending machine.
- [ ] Optional civilian NPCs.

## Audio and Wwise

- [ ] Confirm Wwise project opens.
- [ ] Create first test event.
- [ ] Generate initial soundbanks.
- [x] Hook shooting event in Unity.
- [x] Hook enemy death event.
- [x] Hook ferry damage event.
- [ ] Hook UI confirm/cancel events.
- [ ] Add ambience for river/ferry.

## Presentation

- [x] Add controls section to README when controls exist.
- [ ] Add screenshots or GIFs when the prototype is visible.
- [ ] Update GitHub Pages with current prototype status.
- [ ] Prepare final demo build instructions.
