using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshTower
{
    public static class EventCatalog
    {
        public static void Register()
        {
            Catalog.AllEvents.Add(new GameEvent
            {
                Id = "shrine",
                Title = "Cracked Idol",
                Body = "Someone jammed an idol into a niche in the wall. It's split down the middle, and gold is leaking out of the crack. You can feel the heat from a few feet away.",
                Options =
                {
                    new EventOption { Label = "Cut your hand on it  (+7 Max HP, lose 10 HP)", Result = "The crack drinks. You feel sick for a minute, then heavier than before.", Apply = (r, a) => { r.MaxHp += 7; r.Hp = Mathf.Max(1, r.Hp - 10); }, Leaves = true },
                    new EventOption { Label = "Work a shard loose  (relic, gain Dishonor)", Result = "You get it free, but the metal sticks to your glove and won't come off.", Apply = (r, a) => { r.AddRelic(r.RandomRelic()); r.AddCard(Catalog.Card("dishonor")); }, Leaves = true },
                    new EventOption { Label = "Leave it", Result = "You keep walking. The drip follows you for a few steps, then stops.", Apply = (r, a) => { }, Leaves = true }
                }
            });
            Catalog.AllEvents.Add(new GameEvent
            {
                Id = "beggar",
                Title = "Pilgrim",
                Body = "A man in a burned mask is sitting on the landing with a cup of teeth in his lap. He doesn't look up. \"Gold,\" he says. \"I've got something.\"",
                Options =
                {
                    new EventOption { Label = "Give him 50 gold", Result = "He puts a relic in your palm. It's still warm.", Apply = (r, a) => { if (r.Gold >= 50) { r.Gold -= 50; r.AddRelic(r.RandomRelic()); } }, Leaves = true },
                    new EventOption { Label = "Rob him  (30 gold, maybe a fight)", Result = "The cup goes over and the teeth scatter down the stairs.", Apply = (r, a) => { r.Gold += 30; if (r.Rng.Next(100) < 30) a.StartCombat(new Encounter { Id = "ambush", EnemyIds = { "ashpicker", "ashpicker" } }); }, Leaves = true },
                    new EventOption { Label = "Keep walking", Result = "You hear the cup again a couple of floors later.", Apply = (r, a) => { }, Leaves = true }
                }
            });
            Catalog.AllEvents.Add(new GameEvent
            {
                Id = "book",
                Title = "Chained Book",
                Body = "A book is chained to a lectern on the landing. The pages keep turning, even though nobody is touching them.",
                Options =
                {
                    new EventOption { Label = "Read it  (rare card, take 10 damage)", Result = "You get the card. Your eyes ache for a while after.", Apply = (r, a) => { r.Damage(10); r.AddCard(r.WeightedCard(CardRarity.Rare), false); }, Leaves = true },
                    new EventOption { Label = "Tear out a page  (upgrade a random card)", Result = "Ash from the page sticks to one of the cards in your deck.", Apply = (r, a) => { var c = r.Deck.FirstOrDefault(x => !x.Upgraded); if (c != null) c.Upgraded = true; }, Leaves = true },
                    new EventOption { Label = "Leave it", Result = "It slams shut after you pass.", Apply = (r, a) => { }, Leaves = true }
                }
            });
            Catalog.AllEvents.Add(new GameEvent
            {
                Id = "statue",
                Title = "Stone Angel",
                Body = "There's a statue with no face at the turn of the stair. It holds a hammer in one hand and a coin in the other.",
                Options =
                {
                    new EventOption { Label = "Pray  (upgrade a card)", Result = "The hammer taps once, then nothing.", Apply = (r, a) => a.OpenUpgradePicker(() => a.eventScreen.AfterPicker()), Leaves = false },
                    new EventOption { Label = "Smash it  (take 7 damage, gain 75 gold)", Result = "Coins spill out of the crack you made.", Apply = (r, a) => { r.Damage(7); r.Gold += 75; }, Leaves = true },
                    new EventOption { Label = "Walk on", Result = "It stays where it is.", Apply = (r, a) => { }, Leaves = true }
                }
            });
            Catalog.AllEvents.Add(new GameEvent
            {
                Id = "dead_adv",
                Title = "Body in Armor",
                Body = "Someone in armor is slumped against the wall. The metal is still warm, and you can't see what's sitting behind them.",
                Options =
                {
                    new EventOption { Label = "Search the body  (relic, or a hard fight)", Result = "You go through the pockets.", Apply = (r, a) =>
                    {
                        if (r.Rng.Next(100) < 50) r.AddRelic(r.RandomRelic());
                        else a.StartCombat(new Encounter { Id = "ambush_elite", Elite = true, EnemyIds = { "forge_brute" } });
                    }, Leaves = true },
                    new EventOption { Label = "Take the purse  (35 gold)", Result = "Just coins. Nothing else.", Apply = (r, a) => r.Gold += 35, Leaves = true },
                    new EventOption { Label = "Leave them", Result = "You step around the legs and keep going.", Apply = (r, a) => { }, Leaves = true }
                }
            });
            Catalog.AllEvents.Add(new GameEvent
            {
                Id = "cleric",
                Title = "Old Man with a Lantern",
                Body = "An old man is sitting on a crate with a dead lantern beside him. He looks at your wounds like he's counting them.",
                Options =
                {
                    new EventOption { Label = "Pay 30 gold, heal 35", Result = "It's not much, but it holds.", Apply = (r, a) => { if (r.Gold >= 30) { r.Gold -= 30; r.Heal(35); } }, Leaves = true },
                    new EventOption { Label = "Pay 50 gold, remove a card", Result = "He takes a card from you and feeds it to the lantern.", Apply = (r, a) => { if (r.Gold >= 50) { r.Gold -= 50; a.OpenRemovePicker(() => a.eventScreen.AfterPicker()); } else a.eventScreen.AfterPicker(); }, Leaves = false },
                    new EventOption { Label = "Not today", Result = "He shrugs. He doesn't argue.", Apply = (r, a) => { }, Leaves = true }
                }
            });
        }
    }
}
