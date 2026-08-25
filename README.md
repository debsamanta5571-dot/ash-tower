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

The deck is the run. You start with a small kit and grow it through rewards, shops, and events. Cards cycle draw, hand, discard. Exhaust takes a card out of the fight, which is both a cost and a way to thin. Upgrades happen at rest, not automatically, so you choose what to invest in. Events can add cards you do not want. The point is a deck that drifts toward a plan, then has to live with whatever you picked two floors ago.

**Energy**

Energy is the turn budget. You get a fresh pool each turn and spend it on cards. Some cards dump the rest of the pool in one play. Relics and statuses can discount, refund, or pay you for shuffling. Leaving energy unspent is a choice, not a waste: some relics convert leftovers into block. Sequencing matters more than raw card quality because three energy only buys so much.

**Relics**

Relics are passive rules you collect, not extra buttons. They fire at the start of a fight, on play, on hit, on shuffle, or after combat. The design is that they stack with cards instead of replacing them. A relic that refunds energy changes which expensive cards are even legal. A relic that triggers on HP loss turns taking a hit into part of the turn. You find them in shops, events, and elites, and they quietly rewrite the same combat loop.

**Combat**

Enemies telegraph intent, so you play around the next hit instead of guessing. Block is a layer in front of HP and usually dumps at the start of the next turn. Statuses are built to force a decision: Heft burns everyone at end of turn, Brittle punches through block, Dulled cuts outgoing damage, Unsteady punishes ending a turn at 0 block, Nails bite back when block gets chipped. Damage, block, and relics all go through that same pipeline so a relic never skips the rules a card has to follow.

## Play

[Download for Windows](https://github.com/debsamanta5571-dot/ash-tower/releases/download/v0.1/AshTower-Windows.zip)

Unzip and run `AshTower.exe`.

## Features

- Card combat with intent, block, status, and relic hooks
- Procedural climb with a map
- Cards, relics, potions, and events
- Shop, rest, and reward screens

Made with Unity 6.3 LTS (`6000.3.0f1`).
