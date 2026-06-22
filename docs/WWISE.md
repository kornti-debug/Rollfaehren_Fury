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
`Main.unity`, so each teammate must generate `MainSoundBank` locally before
entering Play Mode. `PrototypeAudioEvents.postEvents` stays disabled until the
remaining general gameplay events exist.

Authored content currently tracked in the repository:

- Random/sequence containers for wood footsteps, ferry, fish, pigeons, and
  weapon sounds
- Events for footsteps, voice, ferry loops, fish/pigeon movement and hits,
  harpoon, pistol, and shotgun
- `BoatSpeed` game parameter for ferry engine pitch
- Shared distance-volume/low-pass attenuation
- User-defined SoundBanks: `MainSoundBank` and `IndoorSoundBank`
- Original footstep and voice WAV files under `Rollfaehren_Fury_WwiseProject/Originals/`
- Unity Event and SoundBank reference assets under `Assets/Wwise/ScriptableObjects/`

The detailed collection list, final event naming, container configuration, and
two-bank layout are documented in
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

These names remain available while authoring. Before Unity wiring, split
combined actions such as `Play_ShotgunFiredAndReload` and adopt the final event
names in `AUDIO_COLLECTION.md`.

For the current prototype, `PrototypeAudioEvents.postEvents` is disabled by
default so missing Wwise events do not spam the Unity Console. Enable it only
after the final events exist and SoundBanks have been regenerated.

The ferry crossing and enemy-type work does not enable Wwise automatically.
Generate banks locally and verify authored events only after the gameplay scene
passes without audio.
