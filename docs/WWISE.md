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

Suggested first workflow:

1. Create a test event in Wwise, for example `Play_Test_Click`.
2. Assign a simple test sound.
3. Generate soundbanks.
4. In Unity, add a temporary object with an `AkEvent` or call the event from a test script.
5. Confirm the sound plays in Play Mode.
6. Commit Wwise project changes and any required generated soundbank files.

Do not build gameplay around audio before the gameplay event exists. First create the gameplay action, then connect the Wwise event.

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

The playable MVP branch has a `PrototypeAudioEvents` component that already posts the first seven event names above. Missing events should not block gameplay; create the matching Wwise events and generate soundbanks when audio work begins.

For the current prototype, `PrototypeAudioEvents.postEvents` is disabled by default so missing Wwise events do not spam the Unity Console. Enable it in the Inspector after the matching Wwise events exist and the soundbanks have been regenerated.
