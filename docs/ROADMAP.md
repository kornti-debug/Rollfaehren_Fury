# Roadmap

## Phase 0: Repository and Tooling

Status: in progress.

- Unity project created.
- GitHub repo connected.
- Git LFS configured.
- Wwise integrated.
- Project documentation added.
- `codex/playable-mvp` branch contains the first generated playable slice.

Exit criteria:

- Teammates can clone, open Unity, and see no missing package or Wwise setup errors.

## Phase 1: Bootstrap Playable Flow

Goal: prove the whole game loop with placeholder geometry.

Tasks:

- Use one-scene flow first; add menu later if time allows.
- Create placeholder ferry deck and river crossing space.
- Add FPS player movement and look.
- Add one weapon.
- Add one enemy.
- Add ferry health.
- Add score/money.
- Add game over.
- Add simple shop/upgrade phase.

Exit criteria:

- The player can start a game, fight enemies, complete or fail a crossing, spend money, and start the next harder round.

## Phase 2: Vertical Slice

Goal: make the prototype feel like Rollfaehren Fury, not just a test scene.

Tasks:

- Replace key placeholders with low-poly ferry, shore, weapon, and enemy assets.
- Add cargo only after the ferry-only loop is stable.
- Add at least two enemy types or variants.
- Add basic UI styling.
- Add Wwise events for major gameplay feedback.
- Add round balancing values.
- Add simple animations where they clarify behavior.

Exit criteria:

- A new player can understand the objective, lose, win a round, and buy upgrades without explanation.

## Phase 3: Content and Polish

Goal: improve replay value and presentation.

Tasks:

- Add more upgrades.
- Add better enemy waves.
- Add civilian NPC panic behavior if time allows.
- Add improved sound mix and music.
- Improve lighting, water, and low-poly art direction.
- Add final menu/game over polish.

Exit criteria:

- The project is presentation-ready for class.

## Phase 4: Presentation Build

Goal: stabilize and package.

Tasks:

- Freeze feature scope.
- Fix bugs.
- Confirm no missing assets or Wwise banks.
- Create a Windows build.
- Prepare short demo script.
- Update docs with final controls and known issues.

Exit criteria:

- The team can run the final build and present the core loop reliably.
