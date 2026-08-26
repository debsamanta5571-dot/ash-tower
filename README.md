# Ash Tower

A totally original deckbuilding roguelike.

![Ash Tower](Docs/preview.gif)

Someone once told me you aren't a real game developer until you've made a roguelike deckbuilder. Tell me if you can guess the inspiration.

You are the Cinder Knight. Climb the tower, collect cards and relics, and fight your way to the final boss.

## Play

[Download for Windows](https://github.com/debsamanta5571-dot/ash-tower/releases/download/v0.1/AshTower-Windows.zip)

Unzip and run `AshTower.exe`.

## Gameplay Features

- Card combat with intent, block, status, and relic hooks
- Procedural climb with a map
- Shop, rest, and reward screens

## What’s in the game

- 54 cards
- 9 enemies
- 29 relics
- 10 potions
- 6 events

## Technical Mechanics

### Architecture

Unity 6.3 draws the screens. The rules are not scattered across buttons. One run object owns the climb: deck, relics, potions, gold, HP, and the map. One combat object owns the fight: energy, piles, enemies, turns, and damage. Title, map, combat, shop, rest, events, and rewards read those objects. They do not keep a second copy of the rules.

The reason for that split is the same reason you would keep a rules library out of a renderer. A new screen can be a view. A new card should not require a new combat class.

### Deckbuilding

Cards live in a catalog. Each one is a pile of numbers (cost, damage, block, targeting) and, if it needs something special, a short extra effect. Upgrade flags and temporary discounts sit on the copy you actually play, so the catalog itself does not get rewritten mid-run.

The run holds your real deck. A fight clones it into draw, hand, discard, and exhaust. Every play goes through the same steps. Pay energy, apply the numbers, then run the extra effect if there is one. Rest, shop, and events only change the run deck. Adding a card is a catalog row, not a new combat class. Exhaust takes a card out of that fight. Ethereal cards leave at the end of the turn the same way.

### Energy

Energy exists only during a fight. You get a fresh pool at the start of your turn. One function decides what a card costs, so relics, statuses, spend-all cards, and free plays cannot disagree. You spend when you play. Whatever you do not spend stays until the turn ends. Relics that care about leftover energy can read that remainder without inventing a second resource.

Spend-all cards take whatever is left and turn that into repeats. Free plays and discounts go through the same cost function, so a relic that zeros the first card of a fight does not bypass the rest of the rules.

### Relics

Relics are not a giant switch statement inside combat. Each one can hook a moment: fight start, turn start, turn end, a card being played, losing HP, shuffling, picking the relic up, or leaving a fight. Combat just calls those moments. A new relic is another catalog entry and a function that runs at the right time. You can only hold one of each, so the same relic cannot stack by accident.

Because the hooks sit on the same fight, a relic that refunds energy, a relic that pays you for shuffling, and a relic that reacts to HP loss all compose. They do not each write their own turn order.

### Combat

One rules engine runs the fight. Enemies pick a move and show an intent, so the UI is reading the next action, not guessing. All damage goes through one function. Block first, then HP, then status reactions. Cards, relics, and enemy moves all call that same damage function, so nothing gets to invent its own HP math.

Statuses hang on the fighter as stacks and update at turn boundaries.

- Heft burns enemies at the end of your turn.
- Brittle makes the next hit ignore block.
- Dulled cuts outgoing damage.
- Unsteady punishes ending a turn at zero block.
- Nails bite back when block gets chipped. Block still applies.

A new status is a name, a stack, and a rule at a turn boundary. It is not a new combat class.

### Climb

The map is generated for the run. Fight, elite, rest, shop, event, chest, and boss nodes only change run data, then hand off to combat when a fight starts. Rest heals or upgrades a card already in the deck. The shop sells catalog rows. Events run a small list of choices that mutate the run, then return you to the map. Rewards pick from the same catalogs combat already uses.

### Build

Unity 6.3 LTS (`6000.3.0f1`). Playable Windows zip is on the release. If you clone or unzip the GitHub source, open the folder that contains `Assets`, `Packages`, and `ProjectSettings` together. After a GitHub zip that is the inner `ash-tower-main` folder, not the wrapper around it. Open `Assets/Scenes/AshTower.unity` and press Play. The UI is built in code when the fight starts, so the scene looks sparse until then.
