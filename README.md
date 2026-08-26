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

`RunState` is the climb. It owns HP, gold, the persistent deck, relics, potions, and a generated map. `CombatState` is the fight. It owns energy, draw, hand, discard, exhaust, enemies, and the damage pipeline. `AshTowerApp` is the host. It opens screens and calls into those two objects. Title, map, combat, shop, rest, events, and rewards do not keep their own HP or deck.

Catalogs are built once in C# (`CardCatalog`, `EnemyCatalog`, `RelicCatalog`, `PotionCatalog`, `EventCatalog`). There is no JSON pack and no ScriptableObject per card. UI listens to a list of combat floaters (`CombatFx`) after each action. Pressing a button asks combat if the play is legal, then combat mutates state, then the screen redraws.

### Deckbuilding

A `CardDef` is static data: cost, type, target mode, damage, block, draw, exhaust, ethereal, innate, X-cost, and an optional extra effect. A `CardRuntime` is the copy you hold: which def, whether it is upgraded, and flags like free-this-turn.

The starter deck is five Ember Cut, four Ash Guard, one Slag Bash, plus the Kiln Spark relic. `RunState.AddCard` is the only way the climb grows the deck. Rest, shop, rewards, and events call that. A fight does not share those objects. `CombatState.Begin` clones each run card into a new runtime (def and upgrade only), shuffles the draw pile, then starts the first player turn.

Play is one function. `CanPlay` checks hand membership, unplayable, energy, and targeting. `Play` spends energy, removes the card from hand, applies the def numbers (damage, block, draw), then runs the extra effect if the card has one. Attacks, skills, and powers are types on the def, not subclasses. Powers stay in a power list. Exhaust goes to the exhaust pile. Everything else hits discard. Hand size caps at 10. Empty draw pile recycles discard and fires shuffle hooks.

### Energy

Energy is combat-only. `EnergyMax` is 3 plus any run bonus. Each player turn sets `Energy = EnergyMax`, then draws five. Cost is `CardRuntime.GetCost`: unplayable and free-this-turn are 0, X-cost is current energy, First Cut zeros the first play of a fight, otherwise it is base cost plus modifiers. `Play` subtracts that number. X-cost also stores the spent amount as `XValue` so cards like Ashcyclone can hit once per energy spent.

Leftover energy stays on the combat object until the turn ends. Relics that convert remainder into block read it there. They do not need a second resource.

### Relics

A `RelicDef` is a catalog row with optional hooks: fight start, turn start, turn end, card play, player HP loss, shuffle, pickup, and after combat. `CombatState` calls those lists at the matching points. `RunState.AddRelic` refuses a second copy of the same id, then runs pickup.

Because the hooks sit on the same fight, a relic that refunds energy, a relic that pays you for shuffling, and a relic that reacts to HP loss all compose. Combat does not switch on relic id for the turn order. A few relics still special-case inside damage or exhaust (Forge Wedge extra damage, Splinter Bough on exhaust). The rest are hook functions on the def.

### Combat

`DealDamage` is the only place hits become HP. Order is: relic damage riders, then block unless Brittle is piercing, then Nails if block was chipped, then Seal if it would eat the HP, then HP, then on-hit reactions (Hunker, Quills, Flame Ward, enemy `OnDamaged`). Brittle is consumed after the hits. Dulled is recoil: after a card attack, the attacker loses HP equal to their Dulled, then one stack drops. Heft is not a rider on the card. At end of turn `PulseHeft` deals the stack through `DealDamage` (so block still applies) and drops one. Unsteady is `LoseHp` if you end the turn at 0 block.

Block usually dumps at the start of the next turn (`SettleBlock`). Holdfast and Hold Guard keep it for a turn. Poise keeps a cap. Kiln Rim keeps 40 percent.

Enemies are `EnemyDef` rows. Each has a `Choose` function that returns a `Move` (intent, damage, hits, block, heft, debuff). Combat stores that move as `CurrentMove` so the UI can draw intent before the enemy acts. After they act, they choose again. Encounters are string pools by map row: easy before row 4, mid until 9, hard after, elites and the boss are fixed ids.

### Climb

The map is 15 rows by 7 columns, generated at run start. Row 0 is three fights. Row 13 is rest. Row 14 is one boss. Other rooms roll elite, event, rest, shop, treasure, or fight. Edges prefer nearby columns, then a pass makes sure every node on the next row is reachable. You may only enter a neighbor of the current node.

Entering a node sets floor to `row + 1` and opens the matching screen. Combat copies HP back onto the run when the fight ends. Rest heals 30 percent of max HP or upgrades a card already in the deck. Shop stock is catalog rows with a price, plus a remove that gets more expensive each time. Events are a title, body, and a list of options whose apply functions mutate the run.

### Build

Unity 6.3 LTS (`6000.3.0f1`). Playable Windows zip is on the release. If you clone or unzip the GitHub source, open the folder that contains `Assets`, `Packages`, and `ProjectSettings` together. After a GitHub zip that is the inner `ash-tower-main` folder, not the wrapper around it. Open `Assets/Scenes/AshTower.unity` and press Play. The UI is built in code at runtime, so the scene looks sparse until then.
