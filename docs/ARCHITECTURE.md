# Architecture

## Scene Flow

The intended game flow from the pre-project design is:

```text
MainMenu
  -> GameScene
      -> ShopScene or ShopOverlay
          -> GameScene with harder round
      -> GameOver
```

For the prototype, `ShopScene` and `GameOver` can be UI overlays inside the game scene. Separate scenes can be introduced later if they make the workflow cleaner.

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
- `RoundManager`: handles round start, crossing complete, difficulty scaling.
- `ScoreManager`: tracks money, score, kills, cargo rewards.
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
- `UpgradeSystem`: applies upgrades to player, weapons, ferry, and cargo rewards.
- `FerryController`: ferry movement and crossing progress.
- `Cargo`: destructible cargo with reward value.

## Key Relationships

From the pre-project design:

- `Character` has a `HealthSystem`.
- `Player` has a `WeaponSystem`.
- `WeaponSystem` contains one or more `Weapon` instances.
- `SpawnManager` creates enemies.
- `ShopManager` uses `UpgradeSystem`.
- `UpgradeSystem` modifies player, weapon, ferry, or cargo values.
- `Enemy` attacks ferry, cargo, or player.
- `FerryController` holds cargo and optional civilian NPCs.
- `ScoreManager` pays rewards for enemy kills and surviving cargo.

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
2. Scene loading or UI flow for main menu and game scene.
3. `FerryController` with crossing progress.
4. `HealthSystem`.
5. Player controller and `WeaponSystem`.
6. One enemy prefab and `SpawnManager`.
7. `ScoreManager`.
8. Basic shop/upgrade UI.
9. Wwise events for confirmed gameplay actions.
