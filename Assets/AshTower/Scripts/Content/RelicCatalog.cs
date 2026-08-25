using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshTower
{
    public static class RelicCatalog
    {
        public static void Register()
        {
            Catalog.AllRelics.Add(new RelicDef { Id = "kiln_spark", Name = "Kiln Spark", Desc = "At the start of each fight, gain 3 Heft and 6 Block. Heft burns all enemies at the end of your turn.", Rarity = CardRarity.Basic, Art = "relic_heart",
                CombatStart = c => { c.Player.Add(StatusId.Heft, 3); c.GainBlock(c.Player, 6); } });

            Catalog.AllRelics.Add(new RelicDef { Id = "coal_rusk", Name = "Coal Rusk", Desc = "At the start of each fight, heal 4.", Rarity = CardRarity.Common, Price = 145,
                CombatStart = c => c.Heal(c.Player, 4) });
            Catalog.AllRelics.Add(new RelicDef { Id = "bellows_pouch", Name = "Bellows Pouch", Desc = "On the first turn of a fight, gain 1 Energy and draw an extra card.", Rarity = CardRarity.Common, Price = 160,
                TurnStart = c => { if (c.Turn == 1) { c.Energy += 1; c.DrawCards(1, forced: true); } } });
            Catalog.AllRelics.Add(new RelicDef { Id = "slag_doorstop", Name = "Slag Doorstop", Desc = "Start each fight with 8 Block. The first Skill you play each turn also gives 4 Block.", Rarity = CardRarity.Common, Price = 155,
                CombatStart = c => c.GainBlock(c.Player, 8),
                OnPlay = (c, card) => { if (card.Def.Type == CardType.Skill && c.SkillsPlayedThisTurn == 1) c.GainBlock(c.Player, 4); } });
            Catalog.AllRelics.Add(new RelicDef { Id = "nail_collar", Name = "Nail Collar", Desc = "When you lose HP, deal 2 damage to every enemy.", Rarity = CardRarity.Common, Price = 155,
                OnHpLoss = (c, n) => { foreach (var e in c.AliveEnemies.ToList()) c.DealDamage(c.Player, e, 2, 1, false); } });
            Catalog.AllRelics.Add(new RelicDef { Id = "forge_wedge", Name = "Forge Wedge", Desc = "Your attacks deal 1 extra damage.", Rarity = CardRarity.Common, Price = 150 });
            Catalog.AllRelics.Add(new RelicDef { Id = "wick_spool", Name = "Wick Spool", Desc = "Every fourth card you play, gain 1 Energy.", Rarity = CardRarity.Common, Price = 165,
                OnPlay = (c, card) => { if (c.CardsPlayedCombat % 4 == 0) { c.Energy += 1; c.Floater(c.Player, "+1 Energy", Theme.Gold); } } });
            Catalog.AllRelics.Add(new RelicDef { Id = "cinder_plug", Name = "Cinder Plug", Desc = "If you end your turn with Energy left, gain Block equal to three times that Energy.", Rarity = CardRarity.Common, Price = 155,
                TurnEnd = c => { if (c.Energy > 0) c.GainBlock(c.Player, c.Energy * 3); } });
            Catalog.AllRelics.Add(new RelicDef { Id = "soot_stamp", Name = "Soot Stamp", Desc = "Your fourth Attack each fight deals 12 extra damage.", Rarity = CardRarity.Common, Price = 165 });
            Catalog.AllRelics.Add(new RelicDef { Id = "marrow_tin", Name = "Marrow Tin", Desc = "After each fight, heal 6.", Rarity = CardRarity.Common, Price = 150,
                AfterCombat = (run, elite) => run.Heal(6) });
            Catalog.AllRelics.Add(new RelicDef { Id = "cracked_mask", Name = "Cracked Mask", Desc = "If you start a fight below 60% HP, keep up to 6 Block between turns.", Rarity = CardRarity.Common, Price = 150,
                CombatStart = c => { if (c.Player.Hp * 10 <= c.Player.MaxHp * 6) c.Player.Add(StatusId.Poise, 6); } });

            Catalog.AllRelics.Add(new RelicDef { Id = "kiln_glaze", Name = "Kiln Glaze", Desc = "When you lose HP, gain 4 Block and draw a card.", Rarity = CardRarity.Uncommon, Price = 190,
                OnHpLoss = (c, n) => { c.GainBlock(c.Player, 4); c.DrawCards(1); } });
            Catalog.AllRelics.Add(new RelicDef { Id = "temper_mark", Name = "Temper Mark", Desc = "Cards you add to your deck cost 1 less.", Rarity = CardRarity.Uncommon, Price = 185 });
            Catalog.AllRelics.Add(new RelicDef { Id = "flue_key", Name = "Flue Key", Desc = "When you shuffle your draw pile, gain 1 Energy.", Rarity = CardRarity.Uncommon, Price = 180,
                OnShuffle = c => c.Energy += 1 });
            Catalog.AllRelics.Add(new RelicDef { Id = "rest_ember", Name = "Rest Ember", Desc = "When you enter a Rest, heal 8.", Rarity = CardRarity.Uncommon, Price = 170 });
            Catalog.AllRelics.Add(new RelicDef { Id = "first_cut", Name = "First Cut", Desc = "The first card you play each fight costs 0.", Rarity = CardRarity.Uncommon, Price = 185 });
            Catalog.AllRelics.Add(new RelicDef { Id = "vent_needle", Name = "Vent Needle", Desc = "At the start of your turn, deal 4 damage to the enemy with the most HP.", Rarity = CardRarity.Uncommon, Price = 175,
                TurnStart = c =>
                {
                    var t = c.AliveEnemies.OrderByDescending(e => e.Hp).FirstOrDefault();
                    if (t != null) c.DealDamage(c.Player, t, 4, 1, false);
                } });
            Catalog.AllRelics.Add(new RelicDef { Id = "three_tongs", Name = "Three Tongs", Desc = "Every third Skill you play in a turn, gain 6 Block.", Rarity = CardRarity.Uncommon, Price = 175,
                OnPlay = (c, card) => { if (card.Def.Type == CardType.Skill && c.SkillsPlayedThisTurn % 3 == 0) c.GainBlock(c.Player, 6); } });
            Catalog.AllRelics.Add(new RelicDef { Id = "hammer_loop", Name = "Hammer Loop", Desc = "Every second Attack you play in a turn, gain 3 Heft until the turn ends.", Rarity = CardRarity.Uncommon, Price = 180,
                OnPlay = (c, card) =>
                {
                    if (card.Def.Type == CardType.Attack && c.AttacksPlayedThisTurn % 2 == 0)
                    {
                        c.Player.Add(StatusId.Heft, 3);
                        c.Player.Add(StatusId.FadingHeft, 3);
                    }
                } });
            Catalog.AllRelics.Add(new RelicDef { Id = "cinder_fan", Name = "Cinder Fan", Desc = "When you play a Power, gain 8 Block.", Rarity = CardRarity.Uncommon, Price = 175,
                OnPlay = (c, card) => { if (card.Def.Type == CardType.Power) c.GainBlock(c.Player, 8); } });

            Catalog.AllRelics.Add(new RelicDef { Id = "coal_ledger", Name = "Coal Ledger", Desc = "Shop prices are 40 gold cheaper.", Rarity = CardRarity.Rare, Price = 290 });
            Catalog.AllRelics.Add(new RelicDef { Id = "wide_draw", Name = "Wide Draw", Desc = "When you empty your hand, draw 2 cards.", Rarity = CardRarity.Rare, Price = 280,
                OnPlay = (c, card) => { if (c.Hand.Count == 0) c.DrawCards(2); } });
            Catalog.AllRelics.Add(new RelicDef { Id = "hot_stone", Name = "Hot Stone", Desc = "Gain 1 Energy each turn. At the start of each fight, lose 8 HP.", Rarity = CardRarity.Rare, Price = 250,
                CombatStart = c => c.Player.Hp = Mathf.Max(1, c.Player.Hp - 8),
                TurnStart = c => c.Energy += 1 });
            Catalog.AllRelics.Add(new RelicDef { Id = "anvil_rite", Name = "Anvil Rite", Desc = "At Rest, you can Stoke twice: gain 1 Heft and lose 5 Max HP.", Rarity = CardRarity.Rare, Price = 250 });
            Catalog.AllRelics.Add(new RelicDef { Id = "trophy_hook", Name = "Trophy Hook", Desc = "Elites drop a potion and 40 extra gold.", Rarity = CardRarity.Rare, Price = 300,
                AfterCombat = (run, elite) => { if (elite) run.Gold += 40; } });
            Catalog.AllRelics.Add(new RelicDef { Id = "warden_seal", Name = "Warden Seal", Desc = "Enemies start each fight with 3 Brittle. The next Attacks against them ignore Block.", Rarity = CardRarity.Rare, Price = 280,
                CombatStart = c => { foreach (var e in c.Enemies) e.Add(StatusId.Brittle, 3); } });
            Catalog.AllRelics.Add(new RelicDef { Id = "kiln_rim", Name = "Kiln Rim", Desc = "You keep 40% of your Block between turns.", Rarity = CardRarity.Rare, Price = 280 });
            Catalog.AllRelics.Add(new RelicDef { Id = "cinder_wake", Name = "Cinder Wake", Desc = "When a card is Exhausted, gain 1 Energy.", Rarity = CardRarity.Rare, Price = 260 });
            Catalog.AllRelics.Add(new RelicDef { Id = "splinter_bough", Name = "Splinter Bough", Desc = "When a card is Exhausted, deal 6 damage to a random enemy and gain 3 Block.", Rarity = CardRarity.Rare, Price = 270 });
        }
    }
}
