# Todo

This list is the working task board for the prototype. Keep it practical and update it as tasks move into code, art, audio, or design work.

## Repository and Setup

- [x] Create Unity URP project.
- [x] Connect GitHub repository.
- [x] Add `.gitignore` and `.gitattributes`.
- [x] Integrate Wwise.
- [x] Add project docs.
- [x] Add first playable MVP branch: `codex/playable-mvp`.
- [x] Track the binary TerrainData asset through Git LFS for fresh clones.
- [ ] Verify a fresh clone on another machine.
- [x] Confirm Unity batchmode compiles the prototype scene builder.
- [ ] Confirm Unity Play Mode has no Console errors on a teammate machine.
- [ ] Decide final team branch rules.

## Game Flow

- [x] Create bootstrap/menu flow.
- [x] Add New Game button.
- [x] Add Settings placeholder.
- [x] Add Quit button.
- [x] Add gameplay Cancel/Esc return to menu.
- [x] Create one-scene game flow in `Assets/Scenes/Main.unity`.
- [x] Add game over state.
- [x] Add shop/upgrade state.
- [x] Add round restart/next round behavior.

## Player and Weapons

- [x] Add FPS movement and camera look.
- [x] Refactor player input to subscribed InputAction callbacks.
- [x] Add basic crosshair.
- [x] Add first weapon.
- [x] Refactor shooting input to subscribed InputAction callbacks.
- [x] Add enemy hit detection.
- [x] Add weapon cooldown.
- [x] Add Wwise hook for shooting.
- [x] Add Wwise hook for hit confirmation.

## Ferry and Cargo

- [x] Create placeholder ferry deck.
- [x] Create river/shore placeholder layout.
- [x] Implement ferry crossing progress.
- [x] Add ferry health.
- [ ] Add visible damage or warning feedback.
- [ ] Later: add cargo prefab.
- [ ] Later: add cargo health.
- [ ] Later: add cargo survival reward.

## Enemies

- [x] Create first enemy prefab.
- [x] Add enemy health.
- [x] Add movement toward ferry.
- [x] Add contact damage behavior.
- [x] Add `EnemySpawner`.
- [x] Add round-based spawn scaling.
- [ ] Add second enemy variant if time allows.

## Score, Money, and Upgrades

- [x] Add kill reward.
- [x] Add round completion reward.
- [x] Add money display.
- [x] Add shop UI.
- [x] Add first upgrade: weapon damage.
- [x] Add second upgrade: ferry health.
- [x] Add third upgrade: fire rate.
- [x] Add next-round difficulty increase.
- [ ] Later: add cargo survival reward.

## Art and Scene Props

- [ ] Low-poly ferry model.
- [x] Prototype shore placeholders.
- [x] Start environment terrain branch.
- [x] Add URP-safe temporary river water material.
- [x] Add simple animated river water scrolling.
- [ ] Later: replace temporary water with a tuned Shader Graph if needed.
- [ ] Later: cargo crates.
- [ ] Weapon model.
- [ ] First enemy model.
- [ ] Optional shop NPC or vending machine.
- [ ] Optional civilian NPCs.

## Audio and Wwise

- [ ] Confirm Wwise project opens.
- [ ] Create first test event.
- [ ] Generate initial soundbanks.
- [x] Hook shooting event in Unity.
- [x] Hook enemy death event.
- [x] Hook ferry damage event.
- [ ] Hook UI confirm/cancel events.
- [ ] Add ambience for river/ferry.

## Presentation

- [x] Add controls section to README when controls exist.
- [ ] Add screenshots or GIFs when the prototype is visible.
- [ ] Update GitHub Pages with current prototype status.
- [ ] Prepare final demo build instructions.
