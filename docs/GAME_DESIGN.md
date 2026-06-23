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
11. Ferry destruction — or the player falling off the ferry into the river — triggers game over.

## Scenes and States

Minimum flow:

```text
Bootstrap -> Main Menu -> Game Scene -> Shop/Upgrade -> Game Scene
                                  -> Game Over
Game Scene -> Pause overlay -> Resume / New Game / Settings / Main Menu / Quit
```

Each round begins in a docked preparation state. The player may enter either
shore house with `E`; both entrances load the same shop interior and return the
player to the door they used. The player then boards the ferry and presses `E`
at the house console. Enemies and ferry movement begin only after this
interaction.

The ferry departs along its docked heading, turns bow-first across the river,
then curves back into a parallel docking orientation. It should never translate
sideways across the full river or stop beyond a jetty on land.

The current MVP uses `Assets/Scenes/Bootstrap.unity` as the first build scene,
`Assets/Scenes/Menu.unity` for New Game, Settings, and Quit, and
`Assets/Scenes/Main.unity` for gameplay. Settings include Master, Music, SFX,
and mouse-sensitivity sliders shared between Menu and the gameplay pause
screen. The shared `ShopInterior.unity` loads additively during preparation.
Pause, augment, and game over remain overlays or states owned by Main.

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
2. Pigeons spawn high and cruise in level at the fish's speed, then dive onto the ferry once within attack range; they are in the mix from round 1, mixed with the fish by an equal spawn weight (~50/50, random).
3. Later: boss fish, boss pigeons, bats, vampires, or small boats.

During a crossing, enemies spawn ahead and beside the moving ferry rather than
behind it. Fish stay at world Y `7`, just below the visible water surface. The
wave is spread from roughly 5% to 90% crossing progress, keeping combat active
without requiring every enemy to outrun the ferry. Swarm size scales with the
round: round 1 sends single enemies and the maximum grows by one each round
(round 2 = 1-2, round 3 = 2-3, ...), so early crossings stay readable.

Enemy behavior should be simple:

- Spawn outside the ferry area.
- Move toward a target.
- Collide with the ferry.
- Take damage.
- Die and reward money.

Enemy health scales linearly:

```text
health = base health * (1 + 0.35 * (round - 1))
```

Fish start at `50 HP`; pigeons start at `30 HP`. This gives `137.5 / 82.5`
HP in round 6 and `207.5 / 124.5` HP in round 10.

Fish contact has a short visual payoff: the fish damages the ferry, plays its
explosion effect, then disappears. The effect does not delay or repeat the
damage.

## Weapons

Per-run weapon progression:

1. Harpoon is the only starting weapon.
2. Pistol unlocks from round 2 after owning the Harpoon.
3. Shotgun unlocks from round 3 after owning the Pistol.
4. Assault Rifle unlocks from round 4 after owning the Shotgun.

All four weapons remain visible in the shop. Players may spend early money on
Harpoon upgrades instead of immediately saving for the next gun, allowing a
Harpoon-focused build to remain a deliberate option.

Weapon rules:

- Keep the first weapon reliable and easy to tune.
- Harpoon and Shotgun are the deliberate one-shot weapons. The unupgraded
  Harpoon one-shots fish through round 6. A full Shotgun blast deals
  `20 x 10 = 200` theoretical damage and one-shots fish through round 9 when
  most pellets connect.
- Pistol and Assault Rifle are sustained-damage weapons rather than one-shot
  weapons.
- The Shotgun is a close-range crowd weapon: ten pellets use a circular
  nine-degree cone, with enough point-blank damage to defeat a round-six fish
  when most pellets connect. Its short range prevents it replacing precision
  weapons.
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

- Show the chained weapon unlocks and per-weapon upgrade tree.
- Keep locked weapon requirements visible.
- Allow buying multiple affordable upgrades during preparation.
- Damage, fire-rate, and reload can reach five levels, allowing a focused
  single-weapon build. Damage adds `20%` multiplicatively per level.
- Ammo weapons can buy `Extra Magazine +1` up to three times, or pay for a
  repeatable refill to restore the current capacity.
- Use the authored four-tab / four-card UGUI layout rather than the retired
  generated node graph. Harpoon hides unused ammo cards and uses Ricochet as
  its special upgrade.

Augment rules:

- Augments are repeatable unless explicitly marked unique.
- Bilge Pump is unique per run and restores `0.5` ferry HP per kill, capped at
  `10` HP actually restored during each crossing.

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
- Final UI polish beyond the authored prototype layouts
- More than one weapon
- Cargo, until the ferry-only loop is stable
