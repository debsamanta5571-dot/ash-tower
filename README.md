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

The run owns a persistent `Deck` of `CardRuntime` instances. Each card is a `CardDef` (cost, type, targeting, damage, block, exhaust, ethereal, X-cost) plus runtime flags for upgrades, cost modifiers, and free-this-turn. Combat copies that deck into draw, hand, discard, and exhaust piles. Play resolution is data-driven: `CombatState` spends energy, applies the def numbers, then invokes an optional `Extra` callback for card-specific logic. Rest smithing flips `Upgraded`. Shops, rewards, and events call `RunState.AddCard` / `RemoveCard`.

**Energy**

Energy is a combat resource, not a run resource. `EnergyMax` starts at 3 plus relic bonuses. `StartPlayerTurn` resets `Energy = EnergyMax` before drawing. `CardRuntime.GetCost` is the single place cost is computed: base cost, upgrade cost, X-cost (spend remaining energy), relic discounts, and status overrides. Unspent energy is left on the combatant so turn-end relic hooks can read it.

**Relics**

Relics are `RelicDef` objects with optional delegates, not hardcoded if-trees per item. The combat loop invokes them at fixed points: `CombatStart`, `TurnStart`, `TurnEnd`, `OnPlay`, `OnHpLoss`, `OnShuffle`, plus `OnPickup` and `AfterCombat` on the run. Pickup is idempotent by id. That means a new relic is a catalog entry and a lambda, not a new combat branch.

**Combat**

`CombatState` is the rules engine. Enemies pick a `Move` with an `IntentKind` so the UI can telegraph attack, defend, or buff before the enemy turn. `DealDamage` splits into block then HP, with Brittle ignoring block and Nails reflecting through the block chip. Statuses live on the combatant as a `StatusId` to stack map. Heft pulses at end of turn, Dulled scales outgoing damage, Unsteady hits if you end the turn at 0 block. Relic and card hooks run through the same damage and block functions, so they stack instead of bypassing the pipeline.

## Play

[Download for Windows](https://github.com/debsamanta5571-dot/ash-tower/releases/download/v0.1/AshTower-Windows.zip)

Unzip and run `AshTower.exe`.

## Features

- Card combat with intent, block, status, and relic hooks
- Procedural climb with a map
- Cards, relics, potions, and events
- Shop, rest, and reward screens

Made with Unity 6.3 LTS (`6000.3.0f1`).
