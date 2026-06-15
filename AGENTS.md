# AGENTS.md

This file is the source-of-truth briefing for teammates and AI agents working in this repository.

## Project Identity

Rollfaehren Fury is a stand-alone Unity 3D first-person shooter prototype for a university class project. The game is about protecting a cable ferry while it travels from one riverbank to the other.

The playable prototype should be simple, readable, and complete before it becomes visually polished. Use low-poly placeholder geometry first, then replace it with Blender assets and Wwise audio once the loop works.

Reference links:

- GitHub Pages: https://kornti-debug.github.io/Rollfaehren_Fury/
- Main design docs: [docs/GAME_DESIGN.md](docs/GAME_DESIGN.md), [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)

## Technical Baseline

- Unity: `6000.3.6f1`
- URP: `17.3.0`
- Wwise SDK: `2025.1.5 Build 9095`
- Wwise Unity Integration: `2025.1.5.4090`
- Git LFS is required.

Important folders:

- `Assets/`: Unity source assets
- `Assets/Wwise/`: Wwise Unity integration
- `Rollfaehren_Fury_WwiseProject/`: Wwise authoring project
- `Packages/`: Unity package manifest and lock file
- `ProjectSettings/`: Unity project configuration
- `docs/`: project design, architecture, setup, and task docs

Do not commit generated local folders such as `Library/`, `Logs/`, `UserSettings/`, `.cache/`, generated IDE project files, installer zips, setup logs, or debug symbols.

## Game Concept

Core pitch:

The player stands on a ferry in first person and protects it while it crosses the river. Enemies attack from the air, water, and shore. Killing enemies gives money. Surviving cargo gives extra money. Between rounds, the player reaches a shop and buys upgrades. Each next round is harder.

Prototype canon:

- Use the pre-project PDF as technical canon.
- Use the GitHub Pages site for tone and flavor.
- Keep scope focused on a vertical slice, not a full roguelite.

## MVP Scope

Must-have:

- Main menu
- Game scene
- Game over state
- Shop or upgrade state between rounds
- First-person player controller
- One weapon with hit/damage behavior
- Moving or simulated ferry crossing
- Ferry health and cargo health
- At least one enemy type
- Enemy spawning and attack behavior
- Score/money rewards
- Upgrade that affects gameplay

Nice-to-have after MVP:

- Multiple enemy types
- Civilian NPCs reacting on the ferry
- Flamethrower or alternate weapon
- Multiple upgrade choices
- Wwise event integration for shooting, hits, ferry movement, enemies, UI, and game over
- Replaced Blender assets and visual polish

Out of scope until the core loop works:

- Complex AI
- Full asset polish
- Full economy balancing
- Large content variety
- Networking or multiplayer
- VR/AR conversion

## Architecture Direction

Use simple MonoBehaviour-based Unity systems first. Keep systems modular but avoid over-engineering.

Planned high-level systems:

- `GameManager`: controls scene/game state and manager references
- `RoundManager`: starts, ends, and scales rounds
- `ScoreManager`: tracks money, kills, cargo rewards
- `UIManager`: menu, HUD, game over, shop UI
- `SpawnManager`: enemy wave spawning
- `ShopManager`: upgrade purchase flow
- `FerryController`: ferry movement, round crossing state, protected-object behavior
- `HealthSystem`: reusable health/damage for player, enemies, ferry, cargo
- `WeaponSystem`: player weapon handling
- `UpgradeSystem`: applies upgrades to player, weapons, ferry, or cargo

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) before adding new manager-like classes.

## Development Rules

- Prefer clear working prototypes over abstract framework work.
- Keep scene changes small and coordinated.
- Put reusable gameplay objects into prefabs.
- Commit `.meta` files with assets.
- Keep docs updated when gameplay direction changes.
- Before committing, run `git status --ignored` and verify generated files are ignored.
- Use Git LFS for binary assets and Wwise binaries.

## Current Priorities

1. Confirm Wwise integration opens in Unity without Console errors.
2. Create clean scene structure: `MainMenu`, `Game`, `Shop` or shop overlay, `GameOver` or game over overlay.
3. Build bootstrap game flow: menu to game scene to shop/game over.
4. Add first-person controller and placeholder ferry deck.
5. Add one enemy and one weapon.
6. Add ferry/cargo health, money, and one upgrade.
7. Replace placeholders with team assets after the loop is playable.
