# Wwise

## Version

- Wwise SDK: `2025.1.5 Build 9095`
- Unity Integration Bundle: `2025.1.5.4090`
- Unity Integration Version: `21`

The Wwise Launcher info is stored in:

```text
Assets/Wwise/LauncherInfo.json
```

## Project Locations

Unity integration:

```text
Assets/Wwise/
Assets/WwiseSettings.xml
Assets/StreamingAssets/
```

Wwise authoring project:

```text
Rollfaehren_Fury_WwiseProject/Rollfaehren_Fury_WwiseProject.wproj
```

## What to Commit

Commit:

- `Assets/Wwise/`
- `Assets/WwiseSettings.xml`
- `Rollfaehren_Fury_WwiseProject/*.wproj`
- `Rollfaehren_Fury_WwiseProject/**/*.wwu`
- Unity `.meta` files for tracked Wwise assets
- Generated soundbanks if the team decides they are needed for play/build reliability

Do not commit:

- Wwise `.cache/`
- `*.validationcache`
- `logRunSetup.txt`
- Wwise installer/source zips
- generated IDE project files
- `.pdb` debug symbols and their `.meta` files

## Git LFS

Wwise binaries and media should use Git LFS:

- `*.dll`
- `*.dylib`
- `*.so`
- `*.bundle`
- `*.bnk`
- `*.wem`
- `*.pck`
- source audio such as `*.wav`, `*.mp3`, `*.ogg`, `*.aiff`

Wwise project files such as `.wproj` and `.wwu` are text files and should remain normal Git files.

## Soundbank Workflow

Generated SoundBanks are ignored by Git. `WwiseGlobal` is enabled in
`Main.unity`, so each teammate must generate `MainSoundBank`,
`OutdoorSoundBank`, and `IndoorSoundBank` locally before testing the complete
audio setup. `PrototypeAudioEvents.postEvents` stays disabled until the
remaining gameplay Events are wired.

Authored content currently tracked in the repository:

- A `SurfaceType` Switch Group with `Wood`, `Gravel`, and `Grass`
- A `SC_Footsteps` Switch Container with randomized material-specific
  footstep containers
- Random/sequence containers for ferry, fish, pigeons, weapons, doors, and UI
- Events for footsteps, voice, ferry loops, fish/pigeon movement and hits,
  harpoon, pistol, shotgun, assault rifle, doors, and UI
- Ferry-contact effects for fish and pigeons
- A `BackgroundMusic` Music Switch Container driven by `GameState`, with
  `CombatIntensity` selecting the moving-ferry music
- Dedicated victory and defeat Music Segments
- `BoatSpeed` game parameter for ferry engine pitch
- Shared distance-volume/low-pass attenuation
- User-defined SoundBanks: `MainSoundBank`, `OutdoorSoundBank`, and
  `IndoorSoundBank`
- Original footstep and voice WAV files under `Rollfaehren_Fury_WwiseProject/Originals/`
- Unity Event and SoundBank reference assets under `Assets/Wwise/ScriptableObjects/`

The detailed collection list, final event naming, container configuration, and
three-bank layout are documented in
[AUDIO_COLLECTION.md](AUDIO_COLLECTION.md). Track source licenses and edits in
[AUDIO_SOURCES.csv](AUDIO_SOURCES.csv).

Local footsteps test:

1. Open `Rollfaehren_Fury_WwiseProject.wproj`.
2. Generate `MainSoundBank` for Windows.
3. Open `Assets/Scenes/Main.unity`.
4. Confirm `WwiseGlobal` is enabled.
5. Enter Play Mode and walk/sprint.
6. Confirm `Play_Steps` plays at different walk and sprint intervals.

`PlayerFootsteps` is already attached to the player and references
`Play_Steps`. `WwiseGlobal` references `MainSoundBank` and is enabled in the
committed scene. The script checks `AkUnitySoundEngine.IsInitialized()` before
posting, but missing local banks will still produce Wwise initialization errors.

`Play_HaraldKrullSpeaking` is preserved but is not connected to gameplay yet.
Do not commit locally generated banks unless the team changes the current
ignore policy.

## Existing Authored Events

- `Play_Steps`
- `Play_HaraldKrullSpeaking`
- `Play_BoatEngine`
- `Play_BoatSteeringScreech`
- `Play_BoatWaveMoving`
- `Play_BoatWaveStanding`
- `Play_BirdFlap` / `Stop_BirdFlap`
- `Play_FishSwimming` / `Stop_FishSwimming`
- `Play_EnemyBirdHit`
- `Play_EnemyFishHit`
- `Play_HarpoonFired`
- `Play_PistolFired`
- `Play_ShotgunFiredAndReload`
- `Play_FallingOffWilhelmScream`
- `Play_RC_UI_Hover`
- `Play_RC_UI_Click`
- `Play_RC_Door_Open`
- `Play_RC_Door_Close`
- `Play_AK47Fired`
- `Play_EnemyFishReachFerry`
- `Play_EnemyBirdReachFerry`
- `Play_BackgroundMusic` / `Stop_BackgroundMusic`
- `Play_VictoryMusic` / `Stop_VictoryMusic`
- `Play_DefeatMusic` / `Stop_DefeatMusic`

These names remain available while authoring. Before Unity wiring, split
combined actions such as `Play_ShotgunFiredAndReload` and adopt the final event
names in `AUDIO_COLLECTION.md`.

The stable Unity-facing footstep event is `Play_Steps`, which targets
`SC_Footsteps`. Unity must set `SurfaceType` before posting it.

Current bank ownership:

- `MainSoundBank`: footsteps, UI, doors, dialogue, background music, victory,
  and defeat
- `OutdoorSoundBank`: weapons, ferry Play/Stop loops, enemies, and enemy/ferry
  contact
- `IndoorSoundBank`: reserved for shop ambience and indoor-only dialogue

The first Unity integration uses these game syncs:

- `SurfaceType`: `Wood`, `Gravel`, `Grass`
- `GameState`: `Docked`, `Shop`, `Moving`
- `CombatIntensity`: `Mid`, `Intense`
- `BoatSpeed`: RTPC from `0` to `100`

Weapons and enemy sounds use 3D positioning with `Attn_Medium_40m`. Ferry
loops use the existing long-distance ferry attenuation. The current combined
`Play_ShotgunFiredAndReload` Event is intentionally retained for the first
functional integration pass.

For the current prototype, `PrototypeAudioEvents.postEvents` is disabled by
default so missing Wwise events do not spam the Unity Console. Enable it only
after the final events exist and SoundBanks have been regenerated.

The ferry crossing and enemy-type work does not enable Wwise automatically.
Generate banks locally and verify authored events only after the gameplay scene
passes without audio.
