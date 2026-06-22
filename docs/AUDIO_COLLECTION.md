# Audio Collection and Wwise Authoring

This is the working checklist for the Rollfaehren Fury sound-design pass.
Create and edit source files in SoundQ/Reaper, import them through Wwise, and
settle the event names before Unity wiring begins.

## Collection Priorities

Already present on `codex/audio-design`:

- 14 wood footsteps
- 5 fish swimming splashes
- 3 pigeon wing sounds
- Fish and pigeon hit sounds
- Ferry engine, moving wake, standing water, and steering sounds
- Harpoon, pistol, and shotgun fire
- Shotgun reload
- Harald voice lines

Collect next:

| Priority | Category | Target |
| --- | --- | --- |
| 1 | Footsteps | 6-10 gravel and 6-10 grass variations |
| 2 | UI | 2 hover, 2 click, 2 purchase, and 2 error/back variations |
| 3 | Weapons | 3-5 assault-rifle shots, 1-2 rifle reloads, and 1-2 pistol reloads |
| 4 | Impacts | 2-3 harpoon impacts/explosions and 3 ferry damage impacts |
| 5 | Ferry | 2 docking impacts and one console/lever sound |
| 6 | Enemies | 2-3 additional fish deaths and 2-3 pigeon deaths |
| 7 | Environment | Seamless river and wind loops; optional distant birds |
| 8 | Shop | Seamless indoor room tone |
| 9 | Music | Menu, preparation, combat, shop, and a short game-over sting |

Four looping music tracks and one sting are enough for the prototype.

## Reaper Export

- Use descriptive lowercase names such as `footstep_gravel_01.wav`.
- Export WAV at 48 kHz and preferably 24-bit.
- Use mono for positional effects: footsteps, weapons, enemies, and ferry.
- Use stereo for music, wind, and broad indoor ambience.
- Trim unnecessary silence and add tiny fades to prevent clicks.
- Make ambience and music loops seamless with crossfades.
- Keep peaks below roughly -3 dBFS; perform final balancing in Wwise.
- Record every source and license in [AUDIO_SOURCES.csv](AUDIO_SOURCES.csv).

Prefer CC0 or CC BY material. Do not use files with unclear licensing.

## Wwise Work Units and Busses

Create separate Actor-Mixer Work Units:

```text
SFX_Player
SFX_Weapons
SFX_Ferry
SFX_Enemies
SFX_Environment
SFX_UI
Dialogue
```

Use this bus hierarchy:

```text
Main Audio Bus
|-- Music
|-- SFX
|   |-- Player
|   |-- Weapons
|   |-- Ferry
|   |-- Enemies
|   |-- Environment
|   `-- UI
`-- Dialogue
```

Events target containers, not individual WAV files.

## Footsteps

Create the `SurfaceType` Switch Group with `Wood`, `Gravel`, and `Grass`.

```text
Footsteps [Switch Container]
|-- Wood [Random Container]
|-- Gravel [Random Container]
`-- Grass [Random Container]
```

- Random playback; avoid repeating the last two sounds.
- Randomize volume by about +/-1.5 dB and pitch by about +/-100 cents.
- Use short attenuation around 12-15 m.
- Keep the existing event name `Play_Steps`.
- Unity will set `SurfaceType` later based on the ground below the player.

## Weapons

Create one Random Container per action:

```text
Harpoon_Fire
Harpoon_Impact
Pistol_Fire
Pistol_Reload
Shotgun_Fire
Shotgun_Reload
AssaultRifle_Fire
AssaultRifle_Reload
```

Use these final event names:

```text
Play_Weapon_Harpoon_Fire
Play_Weapon_Harpoon_Impact
Play_Weapon_Pistol_Fire
Play_Weapon_Pistol_Reload
Play_Weapon_Shotgun_Fire
Play_Weapon_Shotgun_Reload
Play_Weapon_AssaultRifle_Fire
Play_Weapon_AssaultRifle_Reload
```

Keep firing and reloading separate. Add subtle pitch/volume randomization to
repeated fire sounds.

## Ferry

Create a `Ferry_Loop` Blend Container driven by the existing `BoatSpeed` RTPC:

```text
Ferry_Loop
|-- Standing_Water
|-- Moving_Wake
`-- Engine
```

- At 0, standing water dominates.
- From 10-40, fade in engine and moving wake.
- From 40-100, emphasize moving wake and engine.
- Increase engine pitch subtly with speed.
- Use large attenuation around 70-90 m.

Events:

```text
Play_Ferry_Loop
Stop_Ferry_Loop
Play_Ferry_Steer
Play_Ferry_Damage
Play_Ferry_Dock
```

## Enemies

Use randomized movement, hit, and death containers for each enemy:

```text
Play_Enemy_Fish_Move
Stop_Enemy_Fish_Move
Play_Enemy_Fish_Hit
Play_Enemy_Fish_Death
Play_Enemy_Pigeon_Move
Stop_Enemy_Pigeon_Move
Play_Enemy_Pigeon_Hit
Play_Enemy_Pigeon_Death
```

Recommended global playback limits:

- Fish movement: 4 voices
- Pigeon movement: 6 voices
- Enemy hit/death sounds: 8 voices
- When the limit is reached, keep the nearest voices.

## UI, Ambience, and Music

UI events are non-spatial and routed to the UI bus:

```text
Play_UI_Hover
Play_UI_Click
Play_UI_Back
Play_UI_Purchase
Play_UI_Error
```

Ambience events:

```text
Play_Ambience_River
Stop_Ambience_River
Play_Ambience_Wind
Stop_Ambience_Wind
Play_Ambience_Shop
Stop_Ambience_Shop
```

Spatialize the river. Wind and broad indoor ambience can remain non-spatial.

Create the `MusicState` State Group:

```text
Menu
Preparation
Combat
Shop
GameOver
```

Use one Music Switch Container driven by `MusicState`, with approximately
1-2 second transitions. Only create `Play_Music` and `Stop_Music`; Unity will
later change the active state.

## SoundBanks

Use two banks:

- `MainSoundBank`: shared UI, footsteps, weapons, ferry, enemies, outdoor
  ambience, music, and shared dialogue.
- `IndoorSoundBank`: shop ambience, shop-specific dialogue, and future
  indoor-only sounds.

`MainSoundBank` remains loaded while the additive shop scene loads
`IndoorSoundBank`.

## Import Checklist

For each category:

1. Create or select the intended Work Unit and container.
2. Use `Project > Import Audio Files`; do not copy files into Originals manually.
3. Assign the correct output bus.
4. Configure randomization, looping, attenuation, and playback limits.
5. Create events targeting containers.
6. Add events to the appropriate SoundBank.
7. Test with Transport or Soundcaster.
8. Generate Windows SoundBanks locally.
9. Commit `.wwu` files and source WAVs, but not generated SoundBanks.

