# Ash Tower

A totally original deckbuilding roguelike.

![Ash Tower](Docs/preview.gif)

Someone once told me you aren't a real game developer until you've made a roguelike deckbuilder. Tell me if you can guess the inspiration.

You play as the Cinder Knight. Build a deck as you climb and fight to make it to the top. Can you survive the ash tower?

## Play

[Download for Windows](https://github.com/debsamanta5571-dot/ash-tower/releases/download/v0.1/AshTower-Windows.zip)

Unzip and run `AshTower.exe`.

## Game features

- 54 forged cards
- 9 scary enemies
- 29 relics
- 10 tasty potions
- 6 interesting events
- Card combat with intent, block, status, and relic hooks
- Procedural climb with a map
- Shop, rest, and reward screens

## Technical Mechanics

### Architecture

`RunState` holds the data that lasts for the whole climb: HP, gold, deck, relics, potions, and the map. `CombatState` is created for a single encounter and holds energy, the card piles, the enemies, and the turn. `AshTowerApp` is the Unity host. It opens a screen and calls into those two objects rather than storing gold on the shop or HP on the combat UI.

Cards, enemies, relics, potions, and events are registered in C# catalogs at startup (`CardCatalog`, `EnemyCatalog`, `RelicCatalog`, `PotionCatalog`, `EventCatalog`). After an action, combat records a few floaters in `CombatFx` and the visible screen redraws from the current run or fight.

### Deckbuilding

A `CardDef` is the printed card: cost, type, targeting, damage, block, draw, exhaust, ethereal, and an extra effect when the card needs one. A `CardRuntime` is the copy in your deck or hand. It points at that definition and stores whether the copy is upgraded, along with short-lived flags such as costing nothing this turn.

A run begins with five Ember Cut, four Ash Guard, one Slag Bash, and Kiln Spark. Rest, shop, rewards, and events add cards through `RunState.AddCard`. Combat does not edit those run objects. At the start of a fight it copies each card's definition and upgrade into a new runtime, shuffles the draw pile, and deals.

`CanPlay` asks whether the card is in hand, playable, affordable, and aimed at a legal target. `Play` spends the energy, takes the card out of the hand, applies the printed damage, block, and draw, and then runs the extra effect if there is one. Attack, skill, and power are a type on the definition. Powers stay in play for the rest of the fight, exhausted cards leave, and everything else goes to discard. The hand cannot exceed ten cards. When the draw pile is empty, discard is shuffled back in, which is also when shuffle relics run.

### Energy

Energy is only used in combat. The cap is 3 unless the run has added a bonus. Your turn starts by refilling the pool and drawing five cards.

`GetCost` is the single place a card's cost is decided. Unplayable cards and free-this-turn cards cost 0. X-cost cards cost whatever energy you still have; that amount is stored as `XValue` so Ashcyclone can hit once per energy spent. First Cut makes the first card of a fight free. Otherwise the cost is the printed value plus modifiers, and `Play` subtracts that number. Energy you do not spend remains on the combat object until the turn ends, which is what leftover-energy relics read.

### Relics

A relic is a catalog entry that can run a function at a point in the fight: combat start, turn start, turn end, a card being played, the player losing HP, a shuffle, pickup, or the fight ending. `CombatState` calls those functions at the matching points. `AddRelic` skips a relic you already have.

Most relics are only those hooks. A few still check an id inside damage or exhaust, including Forge Wedge (extra damage) and Splinter Bough (on exhaust). The turn sequence itself is not a switch over relic names.

### Combat

`DealDamage` is where a hit becomes HP loss. Relic damage bonuses apply first. The remainder hits block unless Brittle ignores block for that attack. If block was actually reduced, Nails deals its stack back at the attacker while the original block still counts. Seal can cancel the HP loss. What is left comes off HP, then on-hit effects run (Hunker, Quills, Flame Ward, and the enemy's `OnDamaged`). Brittle is cleared after the hits finish.

If the attacker has Dulled, they take that much HP after a card attack and lose one stack. Heft is not part of the card. At the end of the turn, `PulseHeft` sends the stack through `DealDamage` so block still applies, then drops one. Unsteady deals its stack as HP if the turn ends at 0 block.

Block is removed at the start of the next turn unless something keeps it. Holdfast and Hold Guard keep the current amount for a turn, Poise keeps up to its stack, and Kiln Rim keeps 40 percent.

Each enemy has a `Choose` function that returns a `Move` with intent, damage, hits, block, heft, and any debuff. Combat stores that as `CurrentMove` so the UI can show intent before the enemy acts, then the enemy chooses again afterward. Early floors roll easier encounter strings, later floors roll harder pairs, and elites and the boss use fixed ids.

### The Climb

The map is generated when the run starts, on a 15 by 7 grid. The first row is three fights, the second-to-last row is rest, and the last row is one boss. Other rooms are a weighted roll among elite, event, rest, shop, treasure, and fight. Paths prefer nearby columns, then a second pass connects any room that would have been unreachable. You can only enter a neighbor of the node you are on.

Entering a node sets the floor and opens that screen. When a fight ends, HP is written back to the run. Rest heals 30 percent of max HP or upgrades a card already in the deck. The shop sells catalog rows, and removing a card from the deck costs more each time. Events are a prompt plus a list of options that change the run.

### Build

Made with Unity 6.3 LTS (`6000.3.0f1`).
