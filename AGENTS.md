# AGENTS.md

This file is the source-of-truth briefing for teammates and AI agents working in this repository.

## Agent Workflow (read this first)

This repo is built by AI agents (Claude Code and Codex) plus human teammates.
`AGENTS.md` is the master file — if it ever conflicts with another doc, this file
wins. Follow this loop for every change:

**1. Before implementing — get context.**

- Read this file and the doc(s) relevant to your task (see the map below).
- Check [docs/TODO.md](docs/TODO.md) for the current task state.

**2. After implementing — update the docs in the same change.**

- Tick or add items in [docs/TODO.md](docs/TODO.md).
- Update the matching doc when behaviour, structure, or scope changed.
- Commit doc updates together with the code, not in a separate pass.

### Which doc to update for what

| You changed... | Update... |
| --- | --- |
| Finished or added a task | [docs/TODO.md](docs/TODO.md) |
| Added/renamed a system, manager, or class relationship | [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) |
| Game loop, enemies, economy, or MVP scope | [docs/GAME_DESIGN.md](docs/GAME_DESIGN.md) + `MVP Scope` below |
| Controls, run instructions, public overview | [README.md](README.md) |
| Phase progress or milestone | [docs/ROADMAP.md](docs/ROADMAP.md) |
| Wwise events, banks, or audio workflow | [docs/WWISE.md](docs/WWISE.md) |
| Clone or onboarding steps | [docs/SETUP.md](docs/SETUP.md) |

A `pre-commit` reminder (and, for Claude Code, a `Stop` hook) nudges you when
code changes land without doc updates. See [docs/SETUP.md](docs/SETUP.md) to
enable the git hook.

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
- `Assets/Scripts/`: gameplay scripts grouped by domain (Core, Player,
  Weapons, Enemies, Ferry, Shop, Augments, UI, Audio, Editor) — see
  [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)
- `Assets/Wwise/`: Wwise Unity integration
- `Rollfaehren_Fury_WwiseProject/`: Wwise authoring project
- `Packages/`: Unity package manifest and lock file
- `ProjectSettings/`: Unity project configuration
- `docs/`: project design, architecture, setup, and task docs

Do not commit generated local folders such as `Library/`, `Logs/`, `UserSettings/`, `.cache/`, generated IDE project files, installer zips, setup logs, or debug symbols.

## Game Concept

Core pitch:

The player stands on a ferry in first person and protects it while it crosses the river. Enemies approach the ferry, damage it on contact, and disappear. Killing enemies gives money. Surviving the crossing gives a small round reward. Between rounds, the player buys simple upgrades. Each next round is harder.

Prototype canon:

- Use the pre-project PDF as technical canon.
- Use the GitHub Pages site for tone and flavor.
- Keep scope focused on a vertical slice, not a full roguelite.

## MVP Scope

Current must-have:

- Bootstrap/menu flow: `Assets/Scenes/Bootstrap.unity` loads `Assets/Scenes/Menu.unity`, and New Game loads `Assets/Scenes/Main.unity`
- One playable gameplay scene: `Assets/Scenes/Main.unity`
- Game over state
- Shop or upgrade panel between rounds
- First-person player controller
- One weapon with hit/damage behavior
- Physical ferry crossing between two dock transforms
- Ferry health
- At least one enemy type
- Enemy spawning and contact damage behavior
- Score/money rewards
- Upgrade that affects gameplay

Nice-to-have after MVP:

- Cargo crates and cargo rewards
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
- `RoundManager`: starts, ends, and scales rounds if this outgrows `GameManager`
- `ScoreManager`: tracks money, kills, and rewards if this outgrows `GameManager`
- `UIManager`: menu, HUD, game over, shop UI
- `SpawnManager`: enemy wave spawning
- `ShopManager`: upgrade purchase flow
- `FerryController`: ferry movement, round crossing state, protected-object behavior
- `HealthSystem`: reusable health/damage for player, enemies, ferry, and later cargo
- `WeaponSystem`: player weapon handling
- `UpgradeSystem`: applies upgrades to player, weapons, ferry, or later cargo

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

1. Open `Assets/Scenes/Bootstrap.unity` or `Assets/Scenes/Menu.unity` and press Play.
2. Use `New Game` to enter `Assets/Scenes/Main.unity`.
3. Test `WASD`, mouse look, shooting, `E` interactions, the pause menu, shop, and game over.
4. If needed, run `Rollfaehren Fury > Build Prototype Scene` and `Rollfaehren Fury > Build Bootstrap And Menu Scenes`.
5. Tune fish/pigeon weights, enemy speed, ferry speed, ferry health, weapon damage, and shop prices.
6. Create Wwise events matching the hook names in `PrototypeAudioEvents`.
7. Replace placeholders with team assets after the loop is playable.
