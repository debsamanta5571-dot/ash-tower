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

There are two blobs of state and the screens sit on top of them. `RunState` is the climb: HP, gold, deck, relics, potions, map. `CombatState` is one fight: energy, piles, enemies, turns. `AshTowerApp` opens a screen and asks those objects to do work. The shop does not store your gold. The combat screen does not store your HP.

Cards, enemies, relics, potions, and events are C# lists filled once at boot. No JSON pack. No ScriptableObject per card. After an action, combat pushes a small floater list (`CombatFx`) and the UI redraws from that.

### Deckbuilding

`CardDef` is the printed card. Cost, type, targeting, the usual numbers, plus an extra effect if the card needs one. `CardRuntime` is the copy in your hand, which might be upgraded or free this turn.

You start with five Ember Cut, four Ash Guard, Slag Bash, and Kiln Spark. Anything that adds a card (rest, shop, rewards, events) goes through `RunState.AddCard`. Combat does not touch those objects. At fight start it clones each run card (def and upgrade only), shuffles, and deals.

`CanPlay` is the boring gate: in hand, playable, enough energy, has a target. `Play` spends, peels the card off, applies damage / block / draw from the def, then runs the extra effect if there is one. Attack, skill, and power are a field on the def. Powers stay in play. Exhaust leaves the fight. Everything else goes to discard. Hand tops out at 10. Empty draw pile shuffles discard back in, and shuffle relics fire there.

### Energy

You only have energy in a fight. Max is 3 unless the run bumped it. Turn start fills the pool and draws five.

Every cost goes through `GetCost`. Unplayable and free-this-turn come back 0. X-cost is whatever you have left (Ashcyclone stores that as `XValue` and hits once per). First Cut makes the first card of a fight free. Otherwise it is printed cost plus modifiers. `Play` subtracts the number it was given. Whatever you did not spend is still sitting on the combat object at turn end, which is how leftover-energy relics work.

### Relics

Each relic is a catalog row that can hang a function off a moment: fight start, turn start, turn end, you played a card, you lost HP, you shuffled, you picked the relic up, fight over. Combat just calls those moments. Pickup refuses a second copy of the same id.

Most relics live there. A couple still poke into damage or exhaust by name (Forge Wedge's extra damage, Splinter Bough when something exhausts). The turn order itself does not switch on relic ids.

### Combat

Hits become HP in one place, `DealDamage`. Relic riders first. Then block, unless Brittle is ignoring it. If block actually chipped, Nails hits the attacker (block still counted). Seal can eat the HP loss. Then HP. Then the on-hit stuff: Hunker, Quills, Flame Ward, the enemy's `OnDamaged`. Brittle falls off after the hits.

Dulled is recoil. After you swing with a card, you take your Dulled as HP and lose one stack. Heft is not baked into the card. At end of turn `PulseHeft` runs the stack through `DealDamage` (so block still matters) and drops one. Unsteady is a straight HP loss if you end the turn at 0 block.

Block usually dies at the start of the next turn. Holdfast and Hold Guard keep it. Poise keeps a cap. Kiln Rim keeps 40%.

Enemies pick a `Move` (intent, damage, hits, block, heft, debuff) and combat holds that as `CurrentMove` so the UI can show intent before they act. Then they pick again. Early floors roll easy encounter strings, mid floors mix, late floors get the ugly pairs. Elites and the boss are fixed ids.

### Climb

Map is 15 rows by 7 columns, rolled when the run starts. First row is three fights. Second to last is rest. Last row is one boss. Everything in between is a weighted roll: elite, event, rest, shop, treasure, or fight. Links prefer nearby columns, then a second pass attaches any room that nobody pointed at. You can only walk to a neighbor of where you are.

Entering a node sets the floor and opens that screen. After a fight, HP copies back onto the run. Rest is 30% of max HP or an upgrade on a card you already have. Shop is catalog rows with prices, and removing a card gets more expensive each time. Events are a prompt and a list of options that mutate the run.

### Build

Unity 6.3 LTS (`6000.3.0f1`). The Windows zip is on the release. For the source, open the folder that has `Assets`, `Packages`, and `ProjectSettings` in it. GitHub zips nest that inside `ash-tower-main`. Open `Assets/Scenes/AshTower.unity` and press Play. The UI is built in code, so the scene looks empty until then.
