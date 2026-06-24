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
audio setup. `WwiseAudioRuntime` loads Main and Outdoor after Wwise
initialization. Indoor loads only while the additive shop visit is active and
unloads after returning outside.
`PrototypeAudioEvents.postEvents` is enabled.

Authored content currently tracked in the repository:

- A `SurfaceType` Switch Group with `Wood`, `Gravel`, and `Grass`
- A `SC_Footsteps` Switch Container with randomized material-specific
  footstep containers
- Random/sequence containers for ferry, fish, pigeons, weapons, doors, and UI
- Events for footsteps, voice, ferry loops, fish/pigeon movement and hits,
  harpoon, pistol, separate shotgun fire/reload, shared gun reload, doors, and UI
- Ferry-contact effects for fish and pigeons
- A `BackgroundMusic` Music Switch Container driven by `GameState`, with
  `CombatIntensity` selecting the moving-ferry music
- Dedicated victory and defeat Music Segments
- A looping `TitleScreenMusic_Loop` playlist controlled by dedicated Play/Stop
  Events
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

`PlayerFootsteps` is attached to the player and references `Play_Steps`.
`WwiseGlobal` is enabled and owns runtime loading for the Main and Outdoor
banks. All gameplay posts use guarded helpers, so missing local banks disable
the authored gameplay audio without blocking the game loop.

`Play_HaraldKrullSpeaking` plays once after a completed crossing.
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
- `Play_ShotgunFired`
- `Play_ShotgunReload`
- `Play_GunReload`
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
- `Play_TitleScreenMusic` / `Stop_TitleScreenMusic`

The retired combined `Play_ShotgunFiredAndReload` Event has been removed.

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
loops use the existing long-distance ferry attenuation. Shotgun fire and
reload are separate Events; `Play_GunReload` is shared by the Pistol and
Assault Rifle until dedicated reload recordings are added.

The first in-game listening pass confirmed the main weapon, ferry, enemy,
voice, and UI Events. Its latest authored level adjustments are kept as a
separate checkpoint before footstep, lifecycle, and music-transition fixes.
The final tested mix raises the AK47 to `-16 dB` and applies the `-16 dB`
background-music adjustment at the parent Music Switch Container so Docked,
Moving, Intense, and Shop music share the same baseline.

Runtime ownership:

- `WwiseGlobal/WwiseAudioRuntime` loads `MainSoundBank` and
  `OutdoorSoundBank`; the previous `AkBank` component is removed to prevent
  duplicate loading.
- `WwiseGlobal` is the non-spatial music emitter.
- `Ferry_Root/FerryAudio` owns standing water, moving wake, engine, steering,
  and `BoatSpeed`.
- `PlayerFootsteps` sets `SurfaceType` before posting `Play_Steps`.
- `PrototypeAudioEvents` maps weapon fire, enemy hits, ferry contact, and the
  round-complete Harald line to authored Event names.
- `EnemyMovementAudio` owns per-enemy fish/pigeon movement loops.
- `WwiseUIButtonAudio` posts non-spatial hover and click feedback from
  gameplay, pause, augment, game-over, and shop buttons. Runtime-created shop
  nodes inherit it from their button template.
- `ShopScenePortal` and `ShopInteriorExit` post `Play_RC_Door_Open` only when
  their additive transition is accepted. Door-close audio remains unwired.
- `IndoorSoundBank` remains empty, but its shop-entry load and shop-exit unload
  lifecycle is wired for later room tone, dialogue, and reverb content.

`PrototypeAudioEvents.postEvents` is enabled in `Main.unity`. All posts are
guarded by `WwiseAudioRuntime.IsReady`, so a missing local bank prevents audio
without breaking gameplay.

The footstep Switch Container explicitly maps `Wood`, `Gravel`, and `Grass`
to their matching Random Containers. Unity currently uses the `Wood` tag or
the ferry/jetty/shop hierarchy names for Wood and falls back to Gravel for all
other walkable surfaces. Grass remains authored for possible later use.

Music switch changes use a short restart of the active background-music
playing ID. This makes Docked, Moving, Shop, Mid, and Intense changes audible
immediately instead of waiting for the end cue of a multi-minute music segment.

## First Functional Play Mode Check

1. Generate all three Windows SoundBanks locally.
2. Start from `Bootstrap.unity` or `Menu.unity`, then enter Main.
3. Confirm docked water and docked music begin.
4. Test Wood, Gravel, and Grass footsteps.
5. Start a crossing and confirm engine, wake, weapons, enemy movement/hits,
   contact feedback, and the Mid-to-Intense music switch.
6. Enter and leave either shop door and confirm door audio plus Shop music.
7. Hover and click gameplay, pause, augment, and shop buttons.
8. Trigger Game Over and confirm background music stops before defeat music.
