# Architecture

## Scene Flow

The current implemented flow is:

```text
Bootstrap
  -> Menu
      -> Main
          -> Docked preparation
              -> ShopInterior (additive, entered from either shore)
              -> Ferry console starts crossing
                  -> Augment draft
                      -> Docked preparation at opposite shore
          -> Shop overlay while docked
          -> GameOver overlay
          -> Pause overlay via Cancel/Esc
```

`Assets/Scenes/Bootstrap.unity` exists as the stable first scene for builds.
`Assets/Scenes/Menu.unity` has New Game, Settings, and Quit.
`Assets/Scenes/Main.unity` remains loaded as the authoritative gameplay scene.
The shared `ShopInterior` scene loads additively, so run state, HUD, player,
audio, and managers remain owned by Main.
Its complete room hierarchy lives below `Shop Interior Root` at an isolated
world position outside the terrain bounds. This prevents exterior TerrainData
from intersecting the indoor floor while preserving the additive state flow.

## Entity Hierarchy

Conceptual hierarchy from the pre-project PDF:

```text
Character
  Player
  Enemy
    VampireEnemy
    BatEnemy
    BirdEnemy
  NPC
    CivilianNPC
```

Implementation guidance:

- Do not overbuild inheritance early.
- Use shared components such as `HealthSystem` where possible.
- Add abstract base classes only when two or more concrete classes genuinely share behavior.

## Management Layer

Planned managers:

- `GameManager`: owns game state and coordinates other managers.
- `RoundManager`: handles round start, crossing complete, difficulty scaling if this outgrows `GameManager`.
- `ScoreManager`: tracks money, score, kills, and rewards if this outgrows `GameManager`.
- `UIManager`: controls menu, HUD, shop, and game over screens.
- `SpawnManager`: spawns enemies by round rules.
- `ShopManager`: presents and applies upgrades.

Prototype rule:

Start simple. A manager can be a MonoBehaviour in the game scene. Convert to persistent singleton only if the system must survive scene loads.

## Core Systems

Planned systems and responsibilities:

- `SceneFlow`: tiny static scene loader for bootstrap, menu, main, and quit.
- `BootstrapLoader`: redirects the bootstrap scene to the menu scene.
- `MainMenuController`: handles menu buttons and menu cancel input.
- `GameplayMenuInput`: owns the in-game pause overlay. Cancel/Esc closes an
  open shop first, opens or resumes pause, and navigates back from pause settings.
- `HealthSystem`: max health, current health, damage, death event.
- `WeaponSystem`: implemented (Track A) — owns the player's weapons and the firing input (`Player/Attack`), switches the active weapon (digit keys / scroll), reloads on `R`, and forwards fire/hit events so HUD and audio do not care which weapon is active. Automatic weapons keep firing while the button is held; semi-auto weapons fire one shot per press. Exposes ammo / reload state for the HUD.
- `Weapon`: implemented — data-driven runtime weapon. Reads a `WeaponDefinition` and fires by fire mode (hitscan / spread / projectile). Keeps runtime copies of the stats, so upgrades never mutate the shared asset. Owns the magazine + reserve: each shot consumes a round, an empty magazine (or pressing `R`) starts a timed reload that draws from the reserve, and firing the last round auto-reloads. The reload only advances while the weapon is equipped, so switching weapons pauses it instead of finishing it in the background. Magazine size 0 means unlimited ammo (no reload, no reserve). `RefillAmmo` restores magazine + reserve; runtime upgrade hooks adjust magazine size, reserve, and reload time.
- `WeaponDefinition`: implemented — ScriptableObject of weapon stats (fire mode, damage, range, cooldown, aim assist, pellets, spread angle, automatic flag, magazine size, reload time, reserve magazines). Assets live in `Assets/Weapons/` (Pistol — semi-auto, 6 rds + 8 spare mags; Shotgun — semi-auto, 4 rds + 8; Harpoon — unlimited; Assault Rifle — automatic, 20 rds + 6; the rifle still uses the `Flamethrower.asset` file/GUID to preserve scene references). Ammo refills to full at the start of a run and otherwise only through the shop.
- `WeaponTracer`: implemented — placeholder shot visual. Pooled `LineRenderer`s draw a brief muzzle→hit line so hitscan/spread shots are visible while there are no weapon/projectile assets. The HUD also shows the active weapon name + slot, updated on switch.
- `Projectile`: implemented — a thrown projectile that flies a gravity parabola, raycasts its own path to hit `Health`, then despawns (placeholder cube + trail). Spawned by `Weapon` for `WeaponFireMode.Projectile` (the Harpoon).
- `SwarmMovement`: implemented — boids flocking auto-attached to each `SimpleEnemy` at runtime; blends separation/alignment/cohesion with a seek toward the ferry (2D for Surface enemies, 3D for Flying). `EnemySpawner` spawns continuous swarms with an intercept-lead origin ahead of and beside the moving ferry, constrained to the water bounds, and ramps difficulty per round: swarm size grows (`baseSwarmMin/Max` + `swarmSizePerRound`, capped) while the spawn interval shrinks (`baseSwarmInterval` − `intervalStepPerRound`, floored at `minSwarmInterval`); round 1 is extra-gentle (`firstRoundIntervalFactor`) so it is beatable with the harpoon/pistol. `SimpleHUD` shows the bottom-right weapon panel (RPM + ammo), animated HP/crossing bars, and a centered reload bar that appears only while reloading; `RoundStartConsole` shows a billboard "Start Crossing" label above the lever.
- Fire modes: `Hitscan`, `Spread`, `Projectile`. A new weapon is still just a `WeaponDefinition` asset; `Projectile` reuses the existing `Projectile` script.
- `UpgradeDefinition` (Track B): implemented — polymorphic ScriptableObject upgrade; subclasses define the effect via `Apply(UpgradeContext)`: `WeaponDamageUpgrade`, `FireRateUpgrade`, `FerryHealthUpgrade`, the master `RicochetUpgrade`, and the weapon-ammo set `MagazineUpgrade` / `ReserveAmmoUpgrade` / `ReloadSpeedUpgrade` / `ResupplyUpgrade`. Weapon upgrades route through `WeaponSystem` to the active weapon. `InitRuntime` lets the shop create configured instances from code.
- `ShopManager` (Track B): implemented — holds a catalog of `UpgradeDefinition` entries and builds one UI button per catalog entry at runtime by cloning the first serialized button as a template (so the catalog is the single source of truth and upgrades can be added without scene work). The ammo/reload weapon upgrades are appended to the catalog at startup as runtime instances (tunable cost/amount on the `ShopManager`). Purchases go through `GameManager.TryPurchase`; one-off "master" upgrades are tracked per run.
- `ShopInteractable` (Track C): vending-machine interaction available while
  inside the shared shop during `Preparation`; it uses the shared
  `Player/Interact` action.
- `ShopScenePortal`: reusable `E` interaction placed on both shore-house door
  triggers. Each portal carries an unused `shopId` for a possible later catalog
  split, while both currently load `ShopInterior`. The portal containing the
  player owns the shared HUD prompt so the distant portal cannot hide it.
- `ShopSceneCoordinator`: saves the exterior player pose, loads/unloads the
  shop additively, teleports the existing player safely, and restores the exact
  entrance position on exit.
- `ShopInteriorExit`: returns the player through the same exterior portal.
- The ferry vending-machine model is optional decoration and must not own a
  `ShopInteractable`; purchases happen through the NPC inside `ShopInterior`.
- `RoundStartConsole`: ferry-house interaction available only during
  `Preparation`; `Player/Interact` starts the next crossing.
- `AugmentSystem` / `AugmentDefinition` (Track C): implemented — round-end draft. At each round end the player picks 1 of 3 random augments (polymorphic `Apply(AugmentContext)`); picking advances the round. Pooled augments: Tailwind (faster crossing), Repair Kit (per-round heal), The Swarm (2× count / ½ HP), Bruisers (½ count / 2× HP), plus runtime-added Bilge Pump (heal per kill), Reload Fury (timed damage boost after each reload), Rapid Reload (−30% reload on all weapons), Adrenaline (+move speed for 5 s every 5th kill). `InitRuntime` configures runtime-created augments; the kill-triggered ones route through `GameManager.RegisterEnemyKilled`. The shop popup no longer appears at round end — shopping is the automat, round end is the augment draft.
- `FerryController`: moves a kinematic ferry between two dock transforms,
  follows a sampled cubic route aligned to each dock's forward direction,
  carries the player through matching translation and rotation, reports
  distance-based progress, and signals arrival to `GameManager`.
- `PlayerFootsteps`: reads the controller's movement, grounded, and sprint
  state and posts the authored Wwise `Play_Steps` event at walk/sprint
  intervals. It posts only while the Wwise engine is initialized, so missing
  local SoundBanks do not block gameplay.
- The player's hidden controller capsule owns the camera, movement, weapons,
  and collisions. Its child `Fraunz Visual` now uses `Assets/Animations/FraunzAnimator.controller` to drive the idle/walk loop from `SimpleFPSController`, so the captain animates during movement instead of remaining in a static bind pose.
- `Cargo`: later destructible cargo with reward value.

`EnemySpawner` uses weighted `EnemySpawnProfile` entries. Each profile owns its
prefab, spawn-point pool, first eligible round, weight, and optional fixed spawn
height. `SimpleEnemy` remains shared and selects either planar `Surface`
movement or full 3D `Flying` movement from the prefab. Spawn points are
ferry-relative forward attack arcs, while spawn timing is distributed across
configured ferry-progress thresholds so enemies do not all appear at departure.
The spawner ignores points behind the ferry and uses a forward fallback arc.
The fish profile is fixed to world Y `7`, preventing ferry hierarchy offsets
from lifting surface enemies above the river.
The pigeon prefab owns an `AlwaysAnimate` Animator using
`PigeonAnimator.controller`; movement remains script-driven with root motion off.
The moving fish keeps the proven `CarpAnimator.controller` on its gameplay
root. On contact, `SimpleEnemy` applies ferry damage once, spawns the temporary
`FishContactExplosion` visual, then immediately removes the gameplay fish.
Keeping the second FBX animation on a separate effect prevents animation
root-transform curves from interfering with scripted enemy navigation.

## Key Relationships

From the pre-project design:

- `Character` has a `HealthSystem`.
- `Player` has a `WeaponSystem`.
- `WeaponSystem` contains one or more `Weapon` instances.
- `SpawnManager` creates enemies.
- `ShopManager` uses `UpgradeSystem`.
- `UpgradeSystem` modifies player, weapon, ferry, or later cargo values.
- Current MVP `Enemy` attacks only the ferry.
- Later `FerryController` can hold cargo and optional civilian NPCs.
- Current MVP rewards enemy kills and crossing completion.

## Input Layer

The prototype uses the project-wide `Assets/InputSystem_Actions.inputactions` asset. Gameplay scripts subscribe to `InputAction` callbacks and store local input state instead of polling `Keyboard.current` or `Mouse.current` directly.

Current action usage:

- `Player/Move`: movement vector.
- `Player/Look`: mouse/controller look delta.
- `Player/Jump`: queued jump.
- `Player/Sprint`: sprint held state.
- `Player/Attack`: hold-to-fire weapon input and cursor relock.
- `Player/Interact`: context interaction for the vending machine and ferry console.
- `UI/Cancel`: menu back action and gameplay pause/resume navigation.

## Suggested Project Structure

```text
Assets/Scripts/Core/
Assets/Scripts/Characters/
Assets/Scripts/Enemies/
Assets/Scripts/Ferry/
Assets/Scripts/Weapons/
Assets/Scripts/Upgrades/
Assets/Scripts/UI/
Assets/Prefabs/
Assets/Scenes/
```

Create folders when the first script or prefab needs them. Empty folders are not important.

## Bootstrap Implementation Order

1. `GameManager` with basic game states.
2. Bootstrap/menu scene flow plus gameplay, shop, and game over overlays.
3. Physical ferry crossing progress and dock arrival.
4. Shared `Health`.
5. Player controller and hitscan weapon.
6. One enemy prefab and `EnemySpawner`.
7. Money rewards inside `GameManager`.
8. Basic shop/upgrade UI.
9. Wwise events for confirmed gameplay actions.
