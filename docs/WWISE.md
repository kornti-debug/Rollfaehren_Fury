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

- Random/sequence containers: `Steps`, `HaraldKrullSpeaking`
- Events: `Play_Steps`, `Play_HaraldKrullSpeaking`
- User-defined SoundBank: `MainSoundBank`
- Original footstep and voice WAV files under `Rollfaehren_Fury_WwiseProject/Originals/`
- Unity Event and SoundBank reference assets under `Assets/Wwise/ScriptableObjects/`

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

## Starter Event Ideas

- `Play_Weapon_Shoot`
- `Play_Enemy_Hit`
- `Play_Enemy_Death`
- `Play_Ferry_Damage`
- `Play_Round_Complete`
- `Play_Game_Over`
- `Play_UI_Upgrade`
- `Play_UI_Select`
- `Play_UI_Confirm`

The playable MVP branch has a `PrototypeAudioEvents` component that can post the first seven event names above. Missing events and missing banks must not block gameplay; create the matching Wwise events and generate soundbanks when audio work begins.

For the current prototype, `PrototypeAudioEvents.postEvents` is disabled by default so missing Wwise events do not spam the Unity Console. Enable it in the Inspector after the matching Wwise events exist and the soundbanks have been regenerated.

The ferry crossing and enemy-type work does not enable Wwise automatically.
Generate banks locally and verify authored events only after the gameplay scene
passes without audio.
