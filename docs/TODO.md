# Todo

This list is the working task board for the prototype. Keep it practical and update it as tasks move into code, art, audio, or design work.

## Repository and Setup

- [x] Create Unity URP project.
- [x] Connect GitHub repository.
- [x] Add `.gitignore` and `.gitattributes`.
- [x] Integrate Wwise.
- [x] Add project docs.
- [x] Add first playable MVP branch: `codex/playable-mvp`.
- [x] Track the binary TerrainData asset through Git LFS for fresh clones.
- [ ] Verify a fresh clone on another machine.
- [x] Confirm Unity batchmode compiles the prototype scene builder.
- [ ] Confirm Unity Play Mode has no Console errors on a teammate machine.
- [ ] Decide final team branch rules.

## Game Flow

- [x] Create bootstrap/menu flow.
- [x] Add New Game button.
- [x] Add Settings placeholder.
- [x] Add Quit button.
- [x] Add initial gameplay Cancel/Esc navigation.
- [x] Replace direct gameplay Esc return with a pause-controller flow.
- [ ] Verify Resume, New Game, Settings, Main Menu, and Quit pause buttons.
- [x] Create one-scene game flow in `Assets/Scenes/Main.unity`.
- [x] Add game over state.
- [x] Add shop/upgrade state.
- [x] Add round restart/next round behavior.
- [x] Add docked preparation state and shared `E` interaction for the ferry console/shop.
- [ ] Verify the ferry house console prompt and manual round start in Unity.

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
- [x] Add physical dock-to-dock ferry controller and arrival-driven round completion.
- [x] Add alternating Dock A/Dock B route support.
- [x] Replace sideways linear travel with a curved bow-first route and dock turns.
- [x] Correct Dock B to remain on the river side of the opposite jetty.
- [ ] Verify ferry/player motion is visually stable without shaking.
- [ ] Verify the player remains aboard during a full crossing.
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
- [x] Align enemy spawn points with the terrain river surface.
- [x] Add separate fish and pigeon enemy profiles with surface/flying movement.
- [x] Add weighted round unlocks: fish from round 1, pigeons from round 2.
- [x] Replace full spawn rings with ferry-relative forward attack arcs.
- [x] Pace enemy spawns across 5%-90% of ferry crossing progress.
- [x] Raise fish/pigeon speed moderately for the moving-ferry encounter.
- [x] Attach the looping flight Animator to the pigeon prefab.
- [ ] Verify round 1 fish-only spawning and round 2 weighted pigeon spawning.
- [ ] Later: add boss fish or boss pigeon variants.

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
- [ ] Verify automat in Unity: walk to the machine, press `E`, buy, then close it.
- [x] Shop: Close/Exit button in the automat overlay; each upgrade max 3 buys, Querschläger 1.
- [x] Track C: round-end augment draft (1 of 3) replaces the round-end shop popup; picking advances the round.
- [x] Track C augments v1: Tailwind, Repair Kit, The Swarm, Bruisers (+ EnemySpawner count/health multipliers, crossing speedup, per-round heal, reset on new game).
- [ ] Verify augments in Unity: re-run `Build Prototype Scene`, survive a round → 3-augment draft → pick → effect applies.
- [ ] Later: mechanic-heavy augments (mines, gulls, oil slick, shield...) + the spec's master weapon upgrades.
- [ ] Track C: round-end augment draft (1 of 3) — replaces the round-end shop popup. (Next.)
- [ ] Later: add cargo survival reward.

## Art and Scene Props

- [x] Low-poly ferry model.
- [x] Import the revised Fraunz rig, walk-cycle controller, and character prefab.
- [x] Replace the current player visual with the revised Fraunz prefab and walking animation.
- [ ] Verify the revised Fraunz idle/walking animation in Play Mode.
- [x] Prototype shore placeholders.
- [x] Start environment terrain branch.
- [x] Add URP-safe temporary river water material.
- [x] Add simple animated river water scrolling.
- [x] Integrate terrain, water, environment props, ferry/player placement, and spawn points into the latest gameplay scene; keep the scene builder aligned with that layout.
- [ ] Later: replace temporary water with a tuned Shader Graph if needed.
- [ ] Later: cargo crates.
- [ ] Weapon model.
- [x] First enemy model: animated carp prefab.
- [x] Import the fish explosion animation asset.
- [x] Play the fish explosion animation when it reaches and damages the ferry.
- [ ] Verify fish still approach the ferry and the contact explosion timing in Play Mode.
- [x] Vending-machine shop prop.
- [ ] Optional civilian NPCs.

## Audio and Wwise

- [x] Confirm Wwise project opens.
- [x] Import the authored Wwise containers, events, SoundBank definition, and source audio.
- [x] Add guarded `Play_Steps` footsteps to the current player.
- [x] Generate and test the initial Windows SoundBanks locally.
- [x] Enable `WwiseGlobal` in the shared gameplay scene.
- [x] Keep ferry/round-flow changes independent from local generated SoundBanks.
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
