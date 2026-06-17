# Architecture

## Scene Flow

The current implemented flow is:

```text
Bootstrap
  -> Menu
      -> Main
          -> Shop overlay
              -> Main with harder round
          -> GameOver overlay
          -> Menu via Cancel/Esc
```

`Assets/Scenes/Bootstrap.unity` exists as the stable first scene for builds. `Assets/Scenes/Menu.unity` has New Game, Settings, and Quit. `Assets/Scenes/Main.unity` stays the gameplay scene; shop and game over are UI overlays inside that scene.

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
- `GameplayMenuInput`: listens for UI Cancel/Esc in gameplay and returns to the menu.
- `HealthSystem`: max health, current health, damage, death event.
- `WeaponSystem`: implemented (Track A) — owns the player's weapons and the firing input (`Player/Attack`), switches the active weapon (`Player/Next` / `Player/Previous`), and forwards fire/hit events so HUD and audio do not care which weapon is active.
- `Weapon`: implemented — data-driven runtime weapon. Reads a `WeaponDefinition` and fires by fire mode (hitscan / spread). Keeps runtime copies of the stats, so upgrades never mutate the shared asset.
- `WeaponDefinition`: implemented — ScriptableObject of weapon stats (fire mode, damage, range, cooldown, aim assist, pellets, spread angle). Assets live in `Assets/Weapons/` (Pistol, Shotgun, Harpoon, Flamethrower).
- `WeaponTracer`: implemented — placeholder shot visual. Pooled `LineRenderer`s draw a brief muzzle→hit line so hitscan/spread shots are visible while there are no weapon/projectile assets. The HUD also shows the active weapon name + slot, updated on switch.
- `Projectile`: implemented — a thrown projectile that flies a gravity parabola, raycasts its own path to hit `Health`, then despawns (placeholder cube + trail). Spawned by `Weapon` for `WeaponFireMode.Projectile` (the Harpoon).
- Fire modes: `Hitscan`, `Spread`, `Projectile`. A new weapon is still just a `WeaponDefinition` asset; `Projectile` reuses the existing `Projectile` script.
- `UpgradeSystem`: applies upgrades to player, weapons, ferry, and later cargo rewards. Weapon upgrades currently route through `WeaponSystem` to the active weapon.
- `FerryController`: ferry movement and crossing progress.
- `Cargo`: later destructible cargo with reward value.

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
- `UI/Cancel`: menu back action and gameplay return-to-menu action.

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
3. Timed crossing progress.
4. Shared `Health`.
5. Player controller and hitscan weapon.
6. One enemy prefab and `EnemySpawner`.
7. Money rewards inside `GameManager`.
8. Basic shop/upgrade UI.
9. Wwise events for confirmed gameplay actions.
