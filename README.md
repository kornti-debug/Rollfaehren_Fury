# Rollfaehren Fury

Rollfaehren Fury is a Unity 3D first-person ferry-defense prototype. The player protects a cable ferry while it crosses a river, shoots incoming enemies, earns money, and spends rewards on upgrades before the next harder round.

Project page: https://kornti-debug.github.io/Rollfaehren_Fury/

## Current Prototype Goal

Build a small but complete ferry-only vertical slice with placeholder low-poly assets first:

- One playable scene: `Assets/Scenes/Main.unity`
- FPS player on a prototype ferry deck
- Enemies approaching the ferry
- Enemies damage only the ferry on contact, then disappear
- Weapon/shooting loop with enemy kill rewards
- Timed crossing progress
- Simple shop/upgrades between rounds
- Next round with higher difficulty
- Game over when the ferry is destroyed

The website can keep the more playful Captain Fraunz / Danube rogue-lite flavor. The repo docs use the pre-project PDF as the technical source of truth.

## Tech Stack

- Unity: `6000.3.6f1`
- Render pipeline: Universal Render Pipeline (URP) `17.3.0`
- Audio middleware: Wwise SDK `2025.1.5 Build 9095`
- Wwise Unity Integration: `2025.1.5.4090`
- Version control: Git + Git LFS

## Documentation

- [AGENTS.md](AGENTS.md): source-of-truth context for teammates and AI agents
- [Game Design](docs/GAME_DESIGN.md): core loop, scenes, enemies, scoring, upgrades, MVP scope
- [Architecture](docs/ARCHITECTURE.md): scene flow, class structure, systems, manager layer
- [Roadmap](docs/ROADMAP.md): bootstrap, prototype, vertical slice, polish phases
- [Todo](docs/TODO.md): actionable checklist by discipline
- [Setup](docs/SETUP.md): clone, Git LFS, Unity, Wwise, first-run checklist
- [Wwise](docs/WWISE.md): audio integration notes, commit rules, soundbank workflow

## Repository Layout

```text
Assets/                         Unity project assets
Assets/Scenes/                  Unity scenes
Assets/Settings/                URP and Unity settings assets
Assets/Wwise/                   Wwise Unity integration
Assets/StreamingAssets/         Runtime-loaded files and generated audio output
Packages/                       Unity package manifest and lock file
ProjectSettings/                Unity project settings
Rollfaehren_Fury_WwiseProject/  Wwise authoring project
docs/                           Team documentation
```

Generated folders such as `Library/`, `Logs/`, `UserSettings/`, `.cache/`, generated IDE files, setup logs, and installer zips are intentionally ignored.

## Setup

Install Git LFS before cloning or pulling binary assets:

```powershell
git lfs install
git clone https://github.com/kornti-debug/Rollfaehren_Fury.git
cd Rollfaehren_Fury
git lfs pull
```

Open the folder in Unity `6000.3.6f1`. Unity will rebuild `Library/` locally. Do not copy `Library/` between machines.

For Wwise setup details, see [docs/WWISE.md](docs/WWISE.md).

## Playable MVP Workflow

On the `codex/playable-mvp` branch, open `Assets/Scenes/Main.unity` and press Play.

Controls:

- `WASD`: move
- Mouse: look
- Left click: shoot
- `Shift`: sprint
- `Space`: jump
- `Esc`: unlock mouse cursor

If the scene gets broken while experimenting, run `Rollfaehren Fury > Build Prototype Scene` in the Unity menu to regenerate the prototype player, ferry damage target, enemy spawner, HUD, shop, and Wwise hook component.

## Team Workflow

- Keep `main` working and openable in Unity.
- Use feature branches for gameplay, art, audio, or documentation work.
- Commit Unity `.meta` files together with their assets.
- Avoid multiple people editing the same Unity scene at the same time.
- Prefer prefabs for shared objects such as ferry, enemies, weapons, shop, and NPCs.
- Pull before starting work and before merging.

Useful branch examples:

```text
feature/player-controller
feature/ferry-prototype
feature/enemy-spawner
feature/shop-upgrades
feature/wwise-audio
docs/project-structure
```

## Next Milestone

The next milestone is testing and tuning the playable loop in Unity: enemy speed, spawn timing, ferry health, weapon damage, shop prices, and whether a simple main menu is worth adding before art polish.
