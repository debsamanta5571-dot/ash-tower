# Ash Tower

A totally original deckbuilding roguelike.

![Ash Tower](Docs/preview.gif)

Someone once told me you aren't a real game developer until you've made a roguelike deckbuilder. Tell me if you can guess the inspiration.

You play as the Cinder Knight. Build a deck as you climb and fight to make it to the top. Can you survive the ash tower?

## Play

[Download for Windows](https://github.com/debsamanta5571-dot/ash-tower/releases/download/v0.1/AshTower-Windows.zip)

Unzip and run `AshTower.exe`.

## Features

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

The climb and the fight are separate. `RunState` keeps whatever should survive a room: HP, gold, deck, relics, potions, and the map. `CombatState` exists for one encounter: energy, draw, hand, discard, exhaust, enemies, and the current turn. `AshTowerApp` switches screens and calls into those two objects. Shop, rest, and combat UI do not keep their own copy of gold or HP. They read the run or the fight and draw what is there.

Content is registered once at startup in C# catalogs (`CardCatalog`, `EnemyCatalog`, `RelicCatalog`, `PotionCatalog`, `EventCatalog`). After an action, combat appends a short list of floaters (`CombatFx`) and the screen rebuilds from current state.

### Deckbuilding

`CardDef` is the definition: cost, type, targeting, damage, block, draw, exhaust, ethereal, and an optional extra effect. `CardRuntime` is an instance in the deck or hand. It points at a definition and carries the upgrade flag and a few temporary flags (free this turn, cost modifiers).

A new run starts with five Ember Cut, four Ash Guard, one Slag Bash, and the Kiln Spark relic. Rest, shop, rewards, and events add cards only through `RunState.AddCard`. When a fight begins, combat clones each run card (definition and upgrade, not the same object), shuffles the draw pile, and starts the first turn.

`CanPlay` checks that the card is in hand, playable, affordable, and has a legal target. `Play` spends energy, removes it from hand, applies the printed damage, block, and draw, then runs the extra effect if the card has one. Attack, skill, and power are a type field on the definition, not subclasses. Powers remain in play. Exhausted cards leave the fight. The rest go to discard. Hand size is capped at 10. If the draw pile is empty, discard is shuffled back in and shuffle relics run.

### Energy

Energy exists only during combat. The cap is 3 plus any bonus on the run. At the start of your turn the pool fills and you draw five cards.

All costs go through `GetCost`. Unplayable cards and free-this-turn cards cost 0. X-cost cards cost whatever energy you have left; that amount is stored as `XValue` so Ashcyclone can hit once per energy spent. First Cut makes the first card of a fight cost 0. Otherwise the cost is the printed value plus modifiers. `Play` subtracts that result. Unspent energy stays on the combat object until the turn ends, so relics that convert leftovers into block can read it directly.

### Relics

Each relic is a catalog entry that can attach a function to a moment in the fight: combat start, turn start, turn end, a card being played, the player losing HP, a shuffle, pickup, or the fight ending. `CombatState` invokes those hooks at the matching points. `AddRelic` will not add a second copy of the same id.

Most relics are only those hooks. A few still branch by name inside damage or exhaust, such as Forge Wedge adding damage and Splinter Bough triggering on exhaust. The turn loop itself does not switch on relic ids.

### Combat

Incoming hits go through `DealDamage`. Relic damage bonuses apply first. Damage then hits block unless Brittle ignores it. If block was reduced, Nails deals that stack back at the attacker; the original block still absorbed its share. Seal can cancel the HP loss. Remaining damage comes off HP. Then on-hit effects run: Hunker, Quills, Flame Ward, and the enemy's `OnDamaged`. Brittle is removed after the hits resolve.

Dulled is recoil. After a card attack, the attacker loses HP equal to their Dulled and one stack falls off. Heft is applied at end of turn by `PulseHeft`, which calls `DealDamage` so block still applies, then drops one stack. Unsteady deals its stack as HP loss if the turn ends at 0 block.

Block is cleared at the start of the next turn unless something keeps it. Holdfast and Hold Guard keep the current block for a turn. Poise keeps up to its stack. Kiln Rim keeps 40 percent.

Each enemy has a `Choose` function that returns a `Move` (intent, damage, hits, block, heft, debuff). Combat stores that as `CurrentMove` so the UI can show intent before the enemy acts. After they act, they choose again. Encounters are drawn from string pools by map row: easier mixes early, harder pairs later. Elites and the boss use fixed ids.

### Climb

The map is generated at run start on a 15 by 7 grid. The first row is three fights. The second-to-last row is rest. The last row is a single boss. Other rooms are a weighted roll among elite, event, rest, shop, treasure, and fight. Edges prefer nearby columns, then a second pass connects any room that would have been unreachable. You can only enter a neighbor of the current node.

Entering a node sets the floor and opens the matching screen. When a fight ends, HP is written back to the run. Rest heals 30 percent of max HP or upgrades a card already in the deck. The shop sells catalog rows; removing a card from the deck costs more each time. Events are a prompt and a list of options that change the run.

### Build

Made with Unity 6.3 LTS (`6000.3.0f1`). The playable Windows build is on the release page. To open the source, add the folder that contains `Assets`, `Packages`, and `ProjectSettings`. A GitHub zip puts that folder one level down, inside `ash-tower-main`. Open `Assets/Scenes/AshTower.unity` and press Play. The UI is constructed in code, so the scene looks sparse until Play.
