# Game Design

## Core Idea

Rollfaehren Fury is a first-person ferry-defense shooter. The player protects a cable ferry while it crosses a river. Enemies attack the ferry, cargo, and player. The player earns money by killing enemies and preserving cargo, then spends that money on upgrades between rounds.

The first target is a prototype, not a content-heavy finished game. Placeholder low-poly shapes are preferred until the loop is fun and reliable.

## Core Loop

1. Start from the main menu.
2. Enter the game scene on the ferry.
3. Ferry begins crossing from shore 1 to shore 2.
4. Enemies spawn and attack.
5. Player shoots enemies.
6. Kills grant money.
7. Surviving cargo grants extra money at the end of the crossing.
8. Player reaches shop/upgrade phase.
9. Player buys upgrades.
10. Next round begins with harder enemies and/or more cargo.
11. Ferry destruction triggers game over.

## Scenes and States

Minimum flow:

```text
Main Menu -> Game Scene -> Shop/Upgrade -> Game Scene
                         -> Game Over
```

Implementation can use separate scenes or UI overlays. For the prototype, separate `MainMenu` and `Game` scenes are enough if shop and game over are implemented as UI panels.

## Player Fantasy

The player is a chaotic ferry defender: a low-poly sailor/captain standing on the ferry, using weapons to keep the ferry alive. The GitHub Pages flavor names Captain Fraunz and gives the setting a playful Danube rogue-lite tone. The prototype should keep that tone without blocking gameplay implementation.

## Protected Objects

Primary protected object:

- Ferry hull/structure

Secondary protected objects:

- Cargo crates
- Optional civilian NPCs later

Failure condition:

- Ferry health reaches zero.

Reward condition:

- Each surviving cargo item pays money at the end of the round.

## Enemies

Prototype enemy priority:

1. One simple flying enemy that approaches and damages ferry/cargo/player.
2. One variation with more health or faster movement.
3. Later: birds, bats, vampires, swimming enemies, or small boats.

Enemy behavior should be simple:

- Spawn outside the ferry area.
- Move toward a target.
- Attack or collide.
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
- Surviving cargo at round end

Upgrade examples:

- More weapon damage
- Faster fire rate
- More ferry health
- More cargo reward
- Faster player movement
- Faster repair or passive recovery

Prototype shop:

- Show three upgrade choices or a small fixed list.
- Allow buying one or more upgrades with money.
- Start next round after purchase/continue.

## MVP Boundaries

Must exist for the first vertical slice:

- Main menu
- Ferry deck
- Player movement/look
- Shooting
- One enemy
- Ferry/cargo health
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
