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
- [x] Add additive shared-shop transition state and reusable shore-house portals.
- [x] Build and validate the shared `ShopInterior.unity` scene.
- [x] Isolate the additive shop interior outside the exterior terrain without rebuilding manual room edits.
- [x] Prevent the two shore portals from fighting over the shared interaction prompt.
- [ ] Verify both doors show `Press E - Enter shop` and return the player to the correct entrance.
- [ ] Verify entering, buying, exiting, and returning to both doors in Play Mode.
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
- [x] Replace Flamethrower with an Assault Rifle (hitscan, ~600 RPM, tracer); widen weapon ranges.
- [x] HUD rework: compact top-left status, money top-right, weapon panel bottom-right, fire rate in RPM, animated HP/crossing bars, swarm warning banner, floating "Start Crossing" billboard.
- [x] Tracer originates from the crosshair; widen Shotgun spread; add Flamethrower (spread).
- [x] Add `Projectile` fire mode + `Projectile` (gravity parabola, raycast hit, trail); Harpoon throws an arcing projectile.
- [x] Weapon switching: mouse scroll + keys `1`–`4` (1 Harpoon, 2 Pistol, 3 Shotgun, 4 Flamethrower).
- [x] Magazine & reload system: Pistol 6, Assault Rifle 20, Shotgun 4, Harpoon unlimited. Empty magazine (or `R`) starts a timed reload; firing the last round auto-reloads. HUD shows ammo in the weapon panel + a centered reload progress bar while reloading.
- [x] Reload pauses while a weapon is holstered (only the equipped weapon's reload advances), so switching weapons no longer skips the reload wait.
- [x] Max-ammo / reserve magazines: Pistol 8, Assault Rifle 6, Shotgun 8 spare mags (Harpoon unlimited). Empty mag + empty reserve = that weapon is dry; HUD shows `Ammo m/n   Reserve r`. Ammo refills to full at the start of a run and otherwise only via the shop.
- [x] Fire modes: Assault Rifle stays automatic (hold to fire); Pistol/Shotgun/Harpoon are semi-auto — one shot per press.
- [x] Widen Shotgun range (85 → 150) so it reaches the spawn arc.
- [ ] Verify projectiles in Unity: re-run `Build Prototype Scene`, Harpoon arcs and hits.
- [ ] Verify reload + ammo in Unity: weapon empties, reload bar fills, reserve drains; switching mid-reload pauses it; running fully dry leaves only the Harpoon.

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
- [x] Reject spawn points behind the ferry after hierarchy/model changes.
- [x] Lock fish spawning and movement to world Y `7`.
- [x] Pace enemy spawns across 5%-90% of ferry crossing progress.
- [x] Raise fish/pigeon speed moderately for the moving-ferry encounter.
- [x] Attach the looping flight Animator to the pigeon prefab.
- [ ] Verify round 1 fish-only spawning and round 2 weighted pigeon spawning.
- [x] Swarm flocking (`SwarmMovement`, boids) auto-attached to enemies; cluster spawns with varied size.
- [x] Intercept-lead spawn: swarms spawn ahead + to the side of the moving ferry (computed lead) so they reach its flank instead of trailing behind; spawns constrained to the water-surface bounds.
- [x] Single absolute enemy speed (`enemyBaseSpeed`); continuous swarm stream is now the normal spawn behavior (crossing-paced/flood-test path removed).
- [x] Removed the adaptive-escalation / big-swarm-warning mechanic; swarms are plain random-size waves.
- [x] Progressive per-round difficulty: swarm size grows (`baseSwarmMin/Max` + `swarmSizePerRound`, capped) and the spawn interval shrinks (`baseSwarmInterval` − `intervalStepPerRound`, floored) each round; round 1 stays small + slow (`firstRoundIntervalFactor`) so it is beatable with harpoon/pistol.
- [x] More round-end augments: Sluggish Tide (slower enemies), Bounty (+kill reward), War Chest (instant cash), Reinforced Hull (+ferry max HP).
- [ ] Verify swarm feel in Unity. (`testFerryMaxHealth` in GameManager still forces 100 ferry HP — drop if undesired.)
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
- [x] Catalog-driven shop: `ShopManager` builds one button per catalog entry at runtime (clones the first button as a template), so upgrades can be added without scene/builder work.
- [x] Weapon-ammo upgrades in the shop: Bigger Magazine (+rounds), Extra Ammo (+reserve mags), Faster Reload (shorter reload), Resupply Ammo (refill mags + reserve). Added to the catalog at runtime with tunable cost/amount on `ShopManager` (~15–20 each, coherent with the base upgrades).
- [x] Weapon Damage upgrade is now a **percentage** (+25%/buy) instead of flat +10, so it scales every weapon evenly (no more per-pellet blow-up on the shotgun). Harpoon base damage 120 → 140 (one-shots fish through ~round 6).
- [x] Node-tree shop: `ShopManager` now builds a per-weapon upgrade tree at runtime (click a weapon node → lines branch to its upgrade nodes). Per weapon: Damage (+25%/level, escalating cost), Fire Rate; ammo weapons also Faster Reload + Refill Ammo (tops magazine + reserve to current cap); Harpoon gets Ricochet (wired into the projectile). Replaces the flat catalog shop; the old `UpgradeDefinition` assets are now dormant.
- [x] Lower income to match the simpler shop: kill reward 6 → 3 (tunable via `killRewardScale`).
- [ ] Verify the node-tree shop in Unity: weapon nodes + branching lines render; per-weapon purchases apply; Refill greys out when full; Harpoon ricochet chains. Tune node spacing/line constants in `ShopManager` if the layout looks off.
- [ ] Later: more master upgrades (knockback, fuel); prune the dormant `UpgradeDefinition` assets/scripts.
- [x] Move shop access from the ferry into the shared shore-house interior.
- [ ] Verify indoor shop interaction in Unity: enter with `E`, buy, close, and leave.
- [x] Shop: Close/Exit button in the automat overlay; each upgrade max 3 buys, Querschläger 1.
- [x] Track C: round-end augment draft (1 of 3) replaces the round-end shop popup; picking advances the round.
- [x] Track C augments v1: Tailwind, Repair Kit, The Swarm, Bruisers (+ EnemySpawner count/health multipliers, crossing speedup, per-round heal, reset on new game).
- [x] Track C augments v2 (added to the pool at runtime): Bilge Pump (heal per kill), Reload Fury (+50% damage 10s after each reload), Rapid Reload (−30% reload on all weapons), Adrenaline (+40% move speed 5s every 5th kill). Kill-triggered effects route through `GameManager.RegisterEnemyKilled`.
- [ ] Verify augments in Unity: survive a round → draft shows the new augments → pick → effect applies (heal-on-kill, post-reload damage, faster reload, kill-streak speed).
- [ ] Later: mechanic-heavy augments (mines, gulls, oil slick, shield...) + the spec's master weapon upgrades.
- [ ] Track C: round-end augment draft (1 of 3) — replaces the round-end shop popup. (Next.)
- [ ] Later: add cargo survival reward.

## Art and Scene Props

- [x] Low-poly ferry model.
- [x] Restore Fraunz as the visible player model at the correct ferry-deck height.
- [x] Keep the player model static while the team creates its own animation work.
- [ ] Later: connect team-authored idle/walking animation.
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
- [ ] Optional: restore the vending-machine model as non-interactive ferry decoration.
- [ ] Optional civilian NPCs.

## Audio and Wwise

- [x] Confirm Wwise project opens.
- [x] Import the authored Wwise containers, events, SoundBank definition, and source audio.
- [x] Import Istvan's `wwise_2` ferry, enemy, weapon, and expanded wood-footstep work into the current audio branch.
- [x] Add the sound collection, Wwise hierarchy, event naming, and attribution checklist.
- [x] Add guarded `Play_Steps` footsteps to the current player.
- [x] Generate and test the initial Windows SoundBanks locally.
- [x] Enable `WwiseGlobal` in the shared gameplay scene.
- [x] Keep ferry/round-flow changes independent from local generated SoundBanks.
- [x] Hook shooting event in Unity.
- [x] Hook enemy death event.
- [x] Hook ferry damage event.
- [ ] Hook UI confirm/cancel events.
- [ ] Add ambience for river/ferry.
- [ ] Complete creator, source URL, license, and edit fields in `docs/AUDIO_SOURCES.csv`.
- [x] Collect and import gravel and grass footsteps.
- [x] Import initial UI hover/click and shop-door sounds.
- [ ] Collect and import UI purchase, back, and error sounds.
- [x] Import assault-rifle fire, enemy/ferry contact, background, shop,
  victory, and defeat audio.
- [ ] Collect and import missing pistol-reload, ferry-damage/docking,
  enemy-death, and ambience audio.
- [ ] Split Wwise authoring into category Work Units and child SFX busses.
- [x] Build the `SurfaceType` footstep Switch Container.
- [ ] Build the `Ferry_Loop` Blend Container driven by `BoatSpeed`.
- [ ] Add enemy playback limits and final weapon/UI/ambience event names.
- [x] Create the initial `GameState`/`CombatIntensity` switch-driven music
  system.
- [ ] Tune music transitions and connect game-state changes from Unity.
- [x] Create the initial three-bank layout: `MainSoundBank`,
  `OutdoorSoundBank`, and `IndoorSoundBank`.
- [x] Generate and validate all three Windows SoundBanks after integrating the
  latest music and combat audio.
- [x] Finalize first-pass Wwise routing, 3D attenuation, ferry Stop Events, and
  background-music bus assignment.
- [ ] Finalize all three SoundBanks, then generate and validate them locally.

## Presentation

- [x] Add controls section to README when controls exist.
- [x] Rewrite the GitHub Pages ePortfolio for Assignment 2.
- [x] Add updated feature, system-design, system-infrastructure, requirements, and team sections.
- [x] Synchronize the ePortfolio with the merged swarm movement and HUD work.
- [ ] Add four current gameplay screenshots to `docs/assets/assignment2/`.
- [x] Add the Assignment 2 time-plan and task-distribution workbook.
- [ ] Add screenshots or GIFs when the prototype is visible.
- [ ] Update GitHub Pages with current prototype status.
- [ ] Prepare final demo build instructions.
