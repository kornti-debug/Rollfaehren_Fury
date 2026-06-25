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
- [x] Add Settings menu with Master/Music/SFX volume sliders and mouse sensitivity.
- [x] Add Quit button.
- [x] Add initial gameplay Cancel/Esc navigation.
- [x] Replace direct gameplay Esc return with a pause-controller flow.
- [ ] Verify Resume, New Game, Settings, Main Menu, and Quit pause buttons.
- [ ] Verify Main Menu remains clickable after using Pause -> Main Menu.
- [ ] Verify menu and pause settings persist through `PlayerPrefs` and apply live.
- [x] Create one-scene game flow in `Assets/Scenes/Main.unity`.
- [x] Add game over state.
- [x] Falling off the ferry into the river is instant game over (`SimpleFPSController.fallDeathHeight` → `GameManager.TriggerGameOver`).
- [x] Add shop/upgrade state.
- [x] Add round restart/next round behavior.
- [x] Add docked preparation state and shared `E` interaction for the ferry console/shop.
- [x] Add additive shared-shop transition state and reusable shore-house portals.
- [x] Build and validate the shared `ShopInterior.unity` scene.
- [x] Isolate the additive shop interior outside the exterior terrain without rebuilding manual room edits.
- [x] Preserve the decorated shop while applying the taller walls, ceiling,
  counter, NPC scale, and URP light data from the latest gameplay scene.
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
- [x] Add per-run weapon ownership to `WeaponSystem`, including locked hotkeys,
  scroll skipping, immediate equip on unlock, and Harpoon-only reset support.
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
- [x] Max-ammo / reserve magazines: Pistol 4, Assault Rifle 2, Shotgun 4 spare mags (Harpoon unlimited). Empty mag + empty reserve = that weapon is dry; HUD shows `Ammo m/n   Reserve r`. Ammo refills to full at the start of a run and otherwise only via the shop.
- [x] Fire modes: Assault Rifle stays automatic (hold to fire); Pistol/Shotgun/Harpoon are semi-auto — one shot per press.
- [x] Widen Shotgun range (85 → 150) so it reaches the spawn arc.
- [x] Rebalance the Shotgun as a close-range crowd weapon: `20` damage per
  pellet, ten pellets in a circular `9` degree cone, `90` range, and four
  spare magazines.
- [ ] Verify the rebalanced Shotgun in Unity: a close blast kills a round-six
  fish when most pellets connect, can hit clustered enemies, and is unreliable
  at long range.
- [ ] Verify projectiles in Unity: re-run `Build Prototype Scene`, Harpoon arcs and hits.
- [ ] Verify reload + ammo in Unity: weapon empties, reload bar fills, reserve drains; switching mid-reload pauses it; running fully dry leaves only the Harpoon.
- [x] First-person weapon viewmodels: imported Easy Weapons gun models (Pistol/Shotgun/M4) shown via `WeaponVisuals` (muzzle flash, recoil, hit FX, fire/reload sounds, procedural reload dip), driven by existing `Weapon` events. `Tools > Rollfaehren Fury > Setup Weapon Viewmodels` wires them (forces the textured URP material; cosmetic-only). Harpoon has no gun model. (Easy Weapons scripts + demo removed; materials converted to URP.)
- [ ] Verify/tune viewmodels in Unity: right gun shows per active weapon, textured, muzzle/recoil/reload play; dial in size/position + recoil & reload-dip feel.

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
- [x] Weighted enemy profiles via per-profile `firstRound`; both fish and pigeons now spawn from round 1 with equal `spawnWeight` (0.5/0.5, so a 50/50 random mix). (Pigeons were round-2; moved to round 1 since per-round difficulty now comes from swarm-size growth, not enemy variety.)
- [x] Replace full spawn rings with ferry-relative forward attack arcs.
- [x] Reject spawn points behind the ferry after hierarchy/model changes.
- [x] Lock fish spawning and movement to world Y `7`.
- [x] Pace enemy spawns across 5%-90% of ferry crossing progress.
- [x] Raise fish/pigeon speed moderately for the moving-ferry encounter.
- [x] Attach the looping flight Animator to the pigeon prefab.
- [ ] Verify round 1 spawns a mixed fish/pigeon stream (~50/50, random), single enemies per swarm, and that birds dive onto the ferry.
- [x] Swarm flocking (`SwarmMovement`, boids) auto-attached to enemies; cluster spawns with varied size.
- [x] Intercept-lead spawn: swarms spawn ahead + to the side of the moving ferry (computed lead) so they reach its flank instead of trailing behind; spawns constrained to the water-surface bounds.
- [x] Single absolute enemy speed (`enemyBaseSpeed`); continuous swarm stream is now the normal spawn behavior (crossing-paced/flood-test path removed).
- [x] Removed the adaptive-escalation / big-swarm-warning mechanic; swarms are plain random-size waves.
- [x] Progressive per-round difficulty: the spawn interval shrinks (`baseSwarmInterval` − `intervalStepPerRound`, floored) each round; round 1 stays slow (`firstRoundIntervalFactor`) so it is beatable with harpoon/pistol.
- [x] Swarm size scales with the round: max size = `swarmSizeRound1Max` + `swarmSizeGrowthPerRound`·(round−1), capped at `swarmSizeCap`; each swarm rolls a random count in `[max − swarmSizeRange, max]` (floored at 1). Defaults give round 1 = 1, round 2 = 1-2, round 3 = 2-3, … (replaces the old `baseSwarmMin/Max` + `swarmSizePerRound`).
- [x] Birds cruise high then dive: flying enemies hold `birdCruiseAltitude` (raised from the old `flyingSpawnHeight`) and approach level at the same speed as the fish, then commit to a plunge onto the ferry once within `diveRange` (horizontal), descending a touch faster (`diveSpeedMultiplier`). Dive/impact animation + explosion are a later pass.
- [x] More round-end augments: Sluggish Tide (slower enemies), Bounty (+kill reward), War Chest (instant cash), Reinforced Hull (+ferry max HP).
- [ ] Verify swarm feel in Unity. (`testFerryMaxHealth` in GameManager still forces 100 ferry HP — drop if undesired.)
- [ ] Verify the new balancing in Unity: round 1 spawns single enemies and swarm size grows each round; birds fly higher, cruise in level, then dive onto the ferry; birds match fish speed; walking off the ferry into the water ends the run.
- [ ] Later: add boss fish or boss pigeon variants.

## Score, Money, and Upgrades

- [x] Add kill reward.
- [x] Add round completion reward.
- [x] Add money display.
- [x] Add shop UI.
- [x] Add visible chained shop unlocks: Harpoon -> Pistol -> Shotgun ->
  Assault Rifle, with predecessor, round, and price gates.
- [ ] Verify weapon unlock progression in Play Mode: Harpoon-only start,
  locked hotkeys/scroll, round 2/3/4 gates, exact prices, immediate equip, and
  new-run reset.
- [x] Add first upgrade: weapon damage.
- [x] Add second upgrade: ferry health.
- [x] Add third upgrade: fire rate.
- [x] Add next-round difficulty increase.
- [x] Track B: data-driven `UpgradeSystem` (polymorphic `UpgradeDefinition`) + `ShopManager`; the 3 base upgrades are now assets.
- [x] Track B: first master upgrade — Pistol Querschläger (ricochet to nearest enemy).
- [x] Authored shop UI: `ShopManager` binds serialized weapon tabs and upgrade
  cards instead of generating the visible shop at runtime.
- [x] Simplify ammo progression to one `Extra Magazine +1` upgrade, capped at
  three levels. Loaded-magazine growth remains available as a dormant runtime
  API but is no longer exposed in the shop.
- [x] Weapon Damage upgrade is percentage-based (`+20%` per purchase) instead
  of flat damage, so it scales every weapon consistently without excessive
  per-pellet growth on the Shotgun.
- [x] Card-based weapon shop: owned weapons show persistent Damage, Fire Rate,
  Reload, Extra Magazine, and Harpoon Ricochet cards as applicable; locked
  weapons show one unlock card with predecessor, round, and price requirements.
  Damage, Fire Rate, and Faster Reload reach level 5; Extra Magazine reaches
  level 3; Harpoon Ricochet reaches level 1; Refill is repeatable.
- [x] Structure shop cards into icon/title, current-to-next value, level, and
  amber cost regions instead of one plain text label.
- [x] Preview the affected weapon stat in the center summary when an available
  upgrade card is hovered or selected.
- [ ] Verify ammo purchases in Unity: locked weapons expose no upgrades,
  Extra Magazine adds one complete spare reload, refill restores the upgraded
  cap, and New Game restores definition defaults.
- [x] Lower income to match the simpler shop: kill reward 6 → 3 (tunable via `killRewardScale`).
- [ ] Verify the card shop in Unity: weapon tabs switch correctly, locked
  requirements update, per-weapon purchases apply, Refill greys out when full,
  and Harpoon ricochet chains.
- [ ] Later: more master upgrades (knockback, fuel); prune the dormant `UpgradeDefinition` assets/scripts.
- [x] Move shop access from the ferry into the shared shore-house interior.
- [ ] Verify indoor shop interaction in Unity: enter with `E`, buy, close, and leave.
- [x] Shop: Close/Exit button in the automat overlay; core weapon upgrades
  allow 5 buys, ammo capacity 3, and Querschläger/Ricochet 1.
- [x] Track C: round-end augment draft (1 of 3) replaces the round-end shop popup; picking advances the round.
- [x] Track C augments v1: Tailwind, Repair Kit, The Swarm, Bruisers (+ EnemySpawner count/health multipliers, crossing speedup, per-round heal, reset on new game).
- [x] Track C augments v2 (added to the pool at runtime): Bilge Pump (unique;
  heal `0.5` per kill, maximum `10` actual HP per crossing), Reload Fury (+50%
  damage 10s after each reload), Rapid Reload (−30% reload on all weapons),
  Adrenaline (+40% move speed 5s every 5th kill). Augments are repeatable unless
  explicitly unique, and acquired unique choices reset on New Game.
- [ ] Verify specialization and Bilge Pump in Unity: core upgrades stop at
  level 5, ammo capacity stops at level 3, Bilge Pump is not offered twice,
  and it restores no more than 10 HP during one crossing.
- [ ] Verify augments in Unity: survive a round → draft shows the new augments → pick → effect applies (heal-on-kill, post-reload damage, faster reload, kill-streak speed).
- [ ] Later: mechanic-heavy augments (mines, gulls, oil slick, shield...) + the spec's master weapon upgrades.
- [ ] Track C: round-end augment draft (1 of 3) — replaces the round-end shop popup. (Next.)
- [ ] Later: add cargo survival reward.

## Art and Scene Props

- [x] Low-poly ferry model.
- [x] Restore Fraunz as the visible player model at the correct ferry-deck height.
- [x] Keep the player model static while the team creates its own animation work.
- [ ] Later: connect team-authored idle/walking animation.
- [x] Deck mirror for first-person animation visibility: `DeckMirrorSetup` (`Tools > Rollfaehren Fury > Setup Deck Mirror`) builds a selfie-camera → RenderTexture → deck-quad rig so the player can watch their own character animate. `MirrorInteractable` lists the run's active augments on `E` (reads `AugmentSystem.AcquiredAugments`, reset on New Game).
- [ ] Verify the deck mirror in Unity: run the setup tool, position the "Deck Mirror" on the deck, confirm the animated captain shows in it, and that `E` toggles the active-augment list (and the list clears on New Game).
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
- [x] Hook gameplay, pause, augment, game-over, and shop UI hover/click Events.
- [x] Hook accepted exterior-entry and interior-exit interactions to the shop
  door-open Event.
- [ ] Add dedicated UI back, purchase, and error Events.
- [ ] Add ambience for river/ferry.
- [ ] Complete creator, source URL, license, and edit fields in `docs/AUDIO_SOURCES.csv`.
- [x] Collect and import gravel and grass footsteps.
- [x] Import initial UI hover/click and shop-door sounds.
- [x] Author looping title-screen music plus separate shotgun fire, shotgun
  reload, and shared gun-reload Events and assign them to SoundBanks.
- [x] Add menu-local title music and menu UI audio lifecycle.
- [x] Wire separate Shotgun fire/reload and shared Pistol/Rifle reload Events.
- [x] Sequence shop door open/close audio around additive scene transitions.
- [x] Fix Bootstrap/Menu Wwise lifecycle so title music and UI sounds recover
  after returning from gameplay without duplicate `AkInitializer` errors.
- [x] Move title/UI playback to the persistent Wwise emitter and add a default
  listener to the Menu Camera.
- [x] Keep both shop directions ordered as door open, transition, then door
  close at the destination.
- [x] Center the reload progress fill inside its HUD frame.
- [x] Detect `NewLayer 4` as Grass footsteps while Wood-tagged objects remain
  Wood and all other terrain layers use Gravel.
- [ ] Collect and import UI purchase, back, and error sounds.
- [x] Import assault-rifle fire, enemy/ferry contact, background, shop,
  victory, and defeat audio.
- [x] Selectively integrate the Wwise polishing recordings: negative
  fish/pigeon ferry impacts, Stuka dive, nearest-fish Geiger states, and
  enemy-kill feedback without importing the outdated scene or Work Units.
- [x] Validate the polished Wwise authoring with a successful Windows
  SoundBank generation.
- [ ] Collect and import missing pistol-reload, ferry-damage/docking,
  enemy-death, and ambience audio.
- [ ] Split Wwise authoring into category Work Units and child SFX busses.
- [x] Build the `SurfaceType` footstep Switch Container.
- [ ] Build the `Ferry_Loop` Blend Container driven by `BoatSpeed`.
- [ ] Add enemy playback limits and final weapon/UI/ambience event names.
- [x] Create the initial `GameState`/`CombatIntensity` switch-driven music
  system.
- [x] Connect Docked, Moving, Shop, and combat-intensity game syncs from Unity.
- [ ] Tune music transitions and final relative loudness in Wwise.
- [x] Create the initial three-bank layout: `MainSoundBank`,
  `OutdoorSoundBank`, and `IndoorSoundBank`.
- [x] Generate and validate all three Windows SoundBanks after integrating the
  latest music and combat audio.
- [x] Finalize first-pass Wwise routing, 3D attenuation, ferry Stop Events, and
  background-music bus assignment.
- [x] Add explicit Main/Outdoor bank loading and switch-driven gameplay/shop
  music through `WwiseAudioRuntime`.
- [x] Add ferry standing/moving/engine loop transitions and `BoatSpeed` RTPC
  updates through `FerryAudio`.
- [x] Detect Wood, Gravel, and Grass surfaces before posting player footsteps.
- [x] Repair the missing Wwise footstep switch assignments and simplify the
  runtime rule to Wood-tagged ferry/jetty/shop surfaces with Gravel fallback.
- [x] Map Harpoon, Pistol, Shotgun, and Assault Rifle fire to authored Events.
- [x] Add positional fish/pigeon movement loops, hit feedback, and ferry-contact
  feedback without tying contact playback to the destroyed enemy.
- [x] Play a Harald voice line after completing a crossing.
- [x] Add builder-repairable Wwise UI and shop-door components to Main and the
  additive shop scene.
- [x] Stop ferry, enemy, background, and defeat loops by playing ID during
  teardown and refresh persistent audio state across Menu/Main scene loads.
- [x] Force immediate Docked/Moving/Shop and Mid/Intense music changes and
  load `IndoorSoundBank` only during additive shop visits.
- [ ] Run the complete first functional audio Play Mode checklist and tune
  volume, attenuation, loop transitions, and voice limits.
- [ ] Finalize all three SoundBanks, then generate and validate them locally.

## UI and UX

- [x] Add ferry hazard UI palette, Barlow Semi Condensed font assets, and
  reusable UGUI prefabs in `Assets/UI/Prefabs`.
- [x] Replace runtime-generated HUD/shop/menu visuals with editor-authored
  Canvas hierarchies in `Menu.unity` and `Main.unity`.
- [x] Add `UiLayoutMarker` so scene builders repair references without deleting
  manually edited UI layouts.
- [x] Add `GameSettings` and `SettingsPanelController` for persistent volume and
  mouse-sensitivity settings.
- [x] Compact gameplay HUD direction: top-left combines round, ferry health,
  crossing progress, and money; bottom-right stays weapon/ammo-only.
- [x] Use exact-width rectangular HUD bars for ferry health, crossing progress,
  and reload so low percentages read accurately.
- [x] UI builder cleanup direction: `Build Ferry Hazard UI` removes known
  generated roots before rebuilding, preventing duplicate HUD/shop panels.
- [x] Shop close direction: use a top-right `X`, move available funds to the
  centered header area, and keep refill ammo as the fifth compact shop card.
- [x] Center Menu and Pause command columns, subtitles, and buttons.
- [x] Center Augment Draft cards and Game Over command buttons inside their
  frames.
- [x] Add category icons plus separate benefit, drawback, and unique labels to
  augment draft cards.
- [ ] Verify the new UI at `1920x1080`, `1600x900`, and `1280x720`.
- [ ] Verify no duplicate `Gameplay Panel`, `Shop Panel`, or `Close Shop Button`
  remains after rerunning `Build Ferry Hazard UI`.
- [ ] Verify keyboard/controller navigation through menu, pause, shop, and
  augment draft.
- [ ] Verify shop hover/selection previews clear correctly, displayed prices
  match purchases, and all augment benefits/drawbacks fit their cards.
- [ ] Verify Wwise hover/click feedback still fires on all buttons and sliders.
- [ ] Add a gameplay screenshot as the future full-screen menu background.

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
