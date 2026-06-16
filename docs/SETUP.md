# Setup

## Requirements

- Unity `6000.3.6f1`
- Git
- Git LFS
- Wwise `2025.1.5 Build 9095`
- Wwise Unity Integration `2025.1.5.4090`

## Clone

```powershell
git lfs install
git clone https://github.com/kornti-debug/Rollfaehren_Fury.git
cd Rollfaehren_Fury
git lfs pull
```

## Open in Unity

1. Open Unity Hub.
2. Add the cloned project folder.
3. Open with Unity `6000.3.6f1`.
4. Let Unity rebuild `Library/`.
5. Check the Console for compile/import errors.

Do not commit `Library/`, `Logs/`, `UserSettings/`, generated `.csproj` files, or `.sln` files.

## Play the Current MVP

1. Pull the `codex/playable-mvp` branch.
2. Open `Assets/Scenes/Main.unity`.
3. Press Play.
4. Test the loop:
   - `WASD` moves.
   - Mouse looks.
   - Left click shoots.
   - Enemies approach the ferry.
   - Ferry health drops when enemies touch it.
   - Killing enemies grants money.
   - Crossing completion opens the shop.
   - Ferry health reaching zero opens game over.

If scene objects are missing or broken, run `Rollfaehren Fury > Build Prototype Scene`. This rebuilds the prototype player, ferry damage trigger, enemy prefab reference, spawn points, HUD, shop panel, game over panel, and Wwise hook component.

When tuning, start with Inspector values on:

- `Prototype Game Manager`
- `EnemySpawner`
- `PrototypeEnemy` prefab
- `Prototype Player`
- `Ferry` / `Ferry Damage Target`

## Wwise First Run

1. Install Wwise `2025.1.5 Build 9095`.
2. Open `Rollfaehren_Fury_WwiseProject/Rollfaehren_Fury_WwiseProject.wproj` in Wwise.
3. Open Unity after Wwise is installed.
4. If Unity reports missing soundbanks, generate them from Wwise.
5. See [WWISE.md](WWISE.md) for details.

## Git Hooks (one-time)

The repo ships a soft documentation reminder in `.githooks/`. Enable it once per
clone so it warns you when gameplay code is committed without doc updates (see
the Agent Workflow section in [AGENTS.md](../AGENTS.md)):

```powershell
git config core.hooksPath .githooks
```

The hook never blocks a commit; it only prints a reminder.

## Before Committing

Run:

```powershell
git status --ignored
```

Check that these are ignored:

- `Library/`
- `Logs/`
- `UserSettings/`
- generated `.csproj` files
- `Rollfaehren_Fury.sln`
- Wwise `.cache/`
- Wwise validation cache files
- Wwise setup logs and installer zips
- Wwise debug symbols such as `.pdb`

Commit `.meta` files together with any Unity asset they belong to.

## Fresh Clone Acceptance Check

A teammate should be able to:

- Clone the repo.
- Pull LFS assets.
- Open the project in Unity.
- Open the Wwise project.
- Generate soundbanks if needed.
- Enter Play Mode without compile errors.
