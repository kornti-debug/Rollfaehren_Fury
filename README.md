# Rollfaehren Fury

Rollfaehren Fury is a student Unity project: a stylized low-poly ferry defense FPS where the player protects a ferry and its cargo during river crossings, earns money, and upgrades between rounds.

## Project Setup

- Unity version: `6000.3.6f1`
- Render pipeline: Universal Render Pipeline (URP)
- Git LFS: required before the first commit and before cloning assets
- Wwise: planned, not integrated yet

Before opening or committing the project, install Git LFS:

```powershell
git lfs install
```

The important Unity project folders are:

- `Assets/`
- `Packages/`
- `ProjectSettings/`

Generated Unity folders such as `Library/`, `Temp/`, `Logs/`, and `UserSettings/` must not be committed.

## First Git Setup

From the project root:

```powershell
git init
git lfs install
git add .
git status
git commit -m "Initial Unity URP project setup"
```

Before committing, check that `Library/`, `Temp/`, `Logs/`, and `UserSettings/` do not appear in `git status`.

## Team Workflow

- Work from `main` only when starting new branches.
- Use feature branches such as `feature/player-controller`, `feature/ferry-prototype`, or `feature/wwise-audio`.
- Commit all `.meta` files together with their assets.
- Avoid multiple people editing the same Unity scene at the same time.
- Prefer prefabs for shared work like the ferry, enemies, weapons, cargo, and NPCs.

## Wwise Notes

When Wwise is integrated, use the same Wwise version for the whole team. Add Wwise in a separate commit after the clean Unity base commit, then verify that Unity opens without audio integration errors and soundbanks can be generated.
