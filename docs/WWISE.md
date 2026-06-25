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
audio setup. Unity copies these locally generated banks into builds during the
pre-build step; bank generation itself remains manual. Do not build until the
Windows banks have been generated successfully. `WwiseAudioRuntime` loads Main and Outdoor after Wwise
initialization. Indoor loads only while the additive shop visit is active and
unloads after returning outside.
`PrototypeAudioEvents.postEvents` is enabled.

`MasterVolume`, `MusicVolume`, and `SFXVolume` use a `0` to `100` range and
must have shared initial values of `100`. A value of `0` maps to `-96 dB` and
mutes the corresponding bus. If Wwise Authoring remains silent after pulling
the corrected defaults, close Wwise and delete the ignored
`<project>.<username>.wsettings` file; Wwise recreates it from the shared
defaults. This local file affects Authoring preview only, not Unity builds.

`Assets/Wwise/ScriptableObjects/AkWwiseInitializationSettings.asset` must stay
in Unity Player Settings under **Preloaded Assets**. Standalone players cannot
discover this asset through the Editor `AssetDatabase`; preloading it registers
the serialized Windows platform settings before the runtime-created
`AkInitializer` starts. Without it, the player reports that no platform
settings exist and Wwise initialization fails before any SoundBank can load.

Authored content currently tracked in the repository:

- A `SurfaceType` Switch Group with `Wood`, `Gravel`, and `Grass`
- A `FishProximity` Switch Group with `Far`, `Medium`, and `Close`
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
- A non-spatial `EnemyFishRadioactive` detector loop, a spatial pigeon
  `EnemyBirdStuka` one-shot, and non-spatial enemy-kill feedback
- Replacement fish/pigeon ferry-contact recordings with
  `Attn_Medium_40m`
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

Local build checklist:

1. Generate all three Windows SoundBanks in Wwise.
2. Confirm `MainSoundBank.bnk`, `OutdoorSoundBank.bnk`, and
   `IndoorSoundBank.bnk` exist under the Wwise `GeneratedSoundBanks/Windows`
   output.
3. Build from Unity. The Wwise pre-build step copies the generated banks into
   the application automatically.
4. Test title music, UI audio, footsteps, weapons, ferry loops, enemies, and
   shop audio in the executable.
5. If the executable is silent, inspect
   `%USERPROFILE%\AppData\LocalLow\DefaultCompany\Rollfaehren_Fury\Player.log`.
   There should be no Wwise initialization exception and no warning that
   platform-specific settings could not be created.

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
- `Play_EnemyFishRadioactive`
- `Play_EnemyBirdStuka`
- `Play_EnemyKilled`
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
- `FishProximity`: `Far`, `Medium`, `Close`
- `BoatSpeed`: RTPC from `0` to `100`

Weapons and enemy sounds use 3D positioning with `Attn_Medium_40m`. Ferry
loops use the existing long-distance ferry attenuation. Shotgun fire and
reload are separate Events; `Play_GunReload` is shared by the Pistol and
Assault Rifle until dedicated reload recordings are added.

The ferry standing water, moving wake, and engine containers currently start
at `-12 dB`. The radioactive detector is intentionally non-spatial and has no
long-range attenuation because Unity will run only one nearest-fish loop on
the player-side emitter.

The first in-game listening pass confirmed the main weapon, ferry, enemy,
voice, and UI Events. Its latest authored level adjustments are kept as a
separate checkpoint before footstep, lifecycle, and music-transition fixes.
The final tested mix raises the AK47 to `-16 dB` and applies the `-16 dB`
background-music adjustment at the parent Music Switch Container so Docked,
Moving, Intense, and Shop music share the same baseline.

Runtime ownership:

- `WwiseInitializerRuntime` guarantees one persistent `Wwise Runtime` object
  across Bootstrap, Menu, and Main. It owns both `AkInitializer` and the
  registered non-spatial emitter used by title music and menu UI.
- `Menu Wwise Audio/MenuWwiseAudio` loads `MainSoundBank`, plays/stops the
  looping title music, and manages menu bank lifetime without owning a Wwise
  emitter or initializer.
- `Menu Camera` owns the default `AkAudioListener`, so title music and UI
  Events are audible before gameplay creates its player-camera listener.
- `WwiseGlobal/WwiseAudioRuntime` loads `MainSoundBank` and
  `OutdoorSoundBank`; the previous `AkBank` component is removed to prevent
  duplicate loading.
- `WwiseGlobal` is the non-spatial music emitter.
- `Ferry_Root/FerryAudio` owns standing water, moving wake, engine, steering,
  and `BoatSpeed`.
- `PlayerFootsteps` sets `SurfaceType` before posting `Play_Steps`.
- `PrototypeAudioEvents` maps weapon fire, active-weapon reload start, enemy
  hits, ferry contact, and the round-complete Harald line to authored Event
  names. Shotgun reload uses its dedicated Event; Pistol and Assault Rifle use
  the shared gun-reload Event.
- `EnemyMovementAudio` owns per-enemy fish/pigeon movement loops.
- `WwiseUIButtonAudio` posts non-spatial hover and click feedback from
  gameplay, pause, augment, game-over, and shop buttons. Runtime-created shop
  nodes inherit it from their button template.
- `ShopSceneCoordinator` sequences spatial door audio around additive
  transitions: open at the current door, delay briefly, teleport/load or
  unload, wait briefly, then close at the destination door.
- `IndoorSoundBank` remains empty, but its shop-entry load and shop-exit unload
  lifecycle is wired for later room tone, dialogue, and reverb content.

`PrototypeAudioEvents.postEvents` is enabled in `Main.unity`. All posts are
guarded by `WwiseAudioRuntime.IsReady`, so a missing local bank prevents audio
without breaking gameplay.

The footstep Switch Container explicitly maps `Wood`, `Gravel`, and `Grass`
to their matching Random Containers. Unity uses the `Wood` tag or the
ferry/jetty/shop hierarchy names for Wood, the dominant terrain layer named
`NewLayer 4` for Grass, and Gravel for every other terrain or unknown surface.

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
