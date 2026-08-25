# Ash Tower

A deckbuilding roguelike.

![Ash Tower](Docs/preview.gif)

Someone once told me you aren't a real game developer until you've made a roguelike deckbuilder. Tell me if you can guess the inspiration.

You are the Cinder Knight. Climb the tower, collect cards and relics, and fight your way to the final boss.

## Contents

- 54 cards
- 9 enemies
- 29 relics
- 10 potions
- 6 events

## Mechanics

**Deckbuilding**

Cards are data plus an optional play callback, not a subclass per card. A definition holds cost, type, targeting, and numbers. Runtime state (upgrade, cost mods) lives on a wrapper so the catalog stays immutable. The run keeps the persistent deck. Combat clones it into draw, hand, discard, and exhaust. Play always goes through one path: pay energy, apply the numbers, then run the callback if there is one. New cards are catalog entries. Rest, shop, and events only mutate the run deck.

**Energy**

Energy is combat-scoped and reset at the start of each player turn. Cost is resolved in one function so relics, statuses, X-cost, and free-play flags cannot disagree. Spend happens at play time. Whatever is left stays on the combatant until turn end, which is how leftover-energy relics work without a second currency.

**Relics**

Relics are hook lists, not a switch on relic id inside combat. Each relic can subscribe to fight start, turn start, turn end, card play, HP loss, shuffle, pickup, or after combat. Combat broadcasts those moments. Adding a relic is a catalog row and a lambda. Pickup is unique by id so the same relic cannot stack twice by accident.

**Combat**

Combat is a single rules engine. Enemies expose an intent so the UI is a view of the next move, not a separate AI. Damage always enters one function: block, then HP, then status reactions (Brittle, Nails, Heft, Dulled, Unsteady). Cards, relics, and enemy moves call that function instead of writing their own HP math. Statuses are a stack map on the combatant, ticked at turn boundaries, so a new status is data and a tick rule rather than a new combat class.

## Play

[Download for Windows](https://github.com/debsamanta5571-dot/ash-tower/releases/download/v0.1/AshTower-Windows.zip)

Unzip and run `AshTower.exe`.

## Features

- Card combat with intent, block, status, and relic hooks
- Procedural climb with a map
- Cards, relics, potions, and events
- Shop, rest, and reward screens

Made with Unity 6.3 LTS (`6000.3.0f1`).
