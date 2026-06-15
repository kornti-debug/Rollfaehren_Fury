# Architecture

## Scene Flow

The intended full game flow from the pre-project design is:

```text
MainMenu
  -> GameScene
      -> ShopScene or ShopOverlay
          -> GameScene with harder round
      -> GameOver
```

The current MVP uses one scene, `Assets/Scenes/Main.unity`. Shop and game over are UI overlays inside that scene. Separate scenes can be introduced later if they make the workflow cleaner.

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

- `HealthSystem`: max health, current health, damage, death event.
- `WeaponSystem`: selected weapon, fire input, ammo/cooldown if needed.
- `Weapon`: base weapon behavior.
- `Gun`: first simple weapon implementation.
- `Flamethrower`: later weapon option.
- `UpgradeSystem`: applies upgrades to player, weapons, ferry, and later cargo rewards.
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
2. One-scene UI flow for gameplay, shop, and game over.
3. Timed crossing progress.
4. Shared `Health`.
5. Player controller and hitscan weapon.
6. One enemy prefab and `EnemySpawner`.
7. Money rewards inside `GameManager`.
8. Basic shop/upgrade UI.
9. Wwise events for confirmed gameplay actions.
