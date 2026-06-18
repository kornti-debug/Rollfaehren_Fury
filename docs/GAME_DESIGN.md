# Game Design

## Core Idea

Rollfaehren Fury is a first-person ferry-defense shooter. The player protects a cable ferry while it crosses a river. In the first MVP, enemies attack only the ferry. The player earns money by killing enemies and surviving the crossing, then spends that money on upgrades between rounds.

The first target is a prototype, not a content-heavy finished game. Placeholder low-poly shapes are preferred until the loop is fun and reliable.

## Core Loop

1. Start from the main menu.
2. Enter the game scene on the ferry.
3. Player starts the ferry from its house console.
4. Ferry physically crosses from its current shore to the opposite jetty.
5. Enemies spawn and attack.
6. Player shoots enemies.
7. Kills grant money.
8. Docking grants a small round reward.
9. Player chooses an augment and prepares at the opposite shore.
10. Next round crosses back with harder enemies.
11. Ferry destruction triggers game over.

## Scenes and States

Minimum flow:

```text
Bootstrap -> Main Menu -> Game Scene -> Shop/Upgrade -> Game Scene
                                  -> Game Over
Game Scene -> Pause overlay -> Resume / New Game / Settings / Main Menu / Quit
```

Each round begins in a docked preparation state. The player may use the vending
machine, then walks into the ferry house and presses `E` at the start console.
Enemies and ferry movement begin only after this interaction.

The current MVP uses `Assets/Scenes/Bootstrap.unity` as the first build scene, `Assets/Scenes/Menu.unity` for New Game, Settings, and Quit, and `Assets/Scenes/Main.unity` for gameplay. Preparation, shop, pause, augment, and game over are states or overlays inside the gameplay scene.

## Player Fantasy

The player is a chaotic ferry defender: a low-poly sailor/captain standing on the ferry, using weapons to keep the ferry alive. The GitHub Pages flavor names Captain Fraunz and gives the setting a playful Danube rogue-lite tone. The prototype should keep that tone without blocking gameplay implementation.

## Protected Objects

Primary protected object:

- Ferry hull/structure

Postponed protected objects:

- Cargo crates
- Optional civilian NPCs

Failure condition:

- Ferry health reaches zero.

Reward condition:

- Kills pay money immediately.
- Crossing completion pays a small round reward.

## Enemies

Current enemy types:

1. Fish spawn at river-surface points and approach horizontally from round 1.
2. Pigeons spawn at aerial points, dive in full 3D, and join the mix from round 2.
3. Later: boss fish, boss pigeons, bats, vampires, or small boats.

Enemy behavior should be simple:

- Spawn outside the ferry area.
- Move toward a target.
- Collide with the ferry.
- Take damage.
- Die and reward money.

## Weapons

Prototype weapon:

- One hitscan or projectile gun.

Possible later weapons:

- Flamethrower
- Stronger gun
- Wider spread weapon
- Faster reload weapon

Weapon rules:

- Keep the first weapon reliable and easy to tune.
- Weapon feedback should be clear: fire, hit, enemy death, reload or cooldown.
- Wwise events should be connected after the gameplay event exists.

## Economy and Upgrades

Money sources:

- Enemy kills
- Round completion reward

Upgrade examples:

- More weapon damage
- Faster fire rate
- More ferry health
- Ferry repair or max health
- Faster player movement
- Faster repair or passive recovery

Prototype shop:

- Show three upgrade choices or a small fixed list.
- Allow buying one or more upgrades with money.
- Start next round after purchase/continue.

## MVP Boundaries

Must exist for the first vertical slice:

- Ferry deck
- Player movement/look
- Shooting
- One enemy
- Ferry health
- Money/score
- Round end
- One upgrade
- Game over

Do not block the MVP on:

- Final character models
- Final ferry model
- Complex enemy animation
- Perfect balancing
- Full UI polish
- More than one weapon
- Cargo, until the ferry-only loop is stable
