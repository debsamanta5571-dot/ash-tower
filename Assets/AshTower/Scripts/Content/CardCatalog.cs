using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshTower
{
    public static class CardCatalog
    {
        public static void Register()
        {
            // ---- Basics ----
            Add(new CardDef { Id = "ember_cut", Name = "Ember Cut", Type = CardType.Attack, Rarity = CardRarity.Basic, Cost = 1, Target = TargetMode.Enemy, Dmg = 9, UpDmg = 13, Art = "card_attack",
                Text = "Deal 9 damage.", UpText = "Deal 13 damage." });
            Add(new CardDef { Id = "ash_guard", Name = "Ash Guard", Type = CardType.Skill, Rarity = CardRarity.Basic, Cost = 1, Block = 8, UpBlock = 12, Art = "card_skill",
                Text = "Gain 8 Block.", UpText = "Gain 12 Block." });
            Add(new CardDef { Id = "slag_bash", Name = "Slag Bash", Type = CardType.Attack, Rarity = CardRarity.Basic, Cost = 2, Target = TargetMode.Enemy, Dmg = 14, UpDmg = 18, Block = 6, UpBlock = 10, Art = "card_attack",
                Text = "Deal 14 damage.\nGain 6 Block.", UpText = "Deal 18 damage.\nGain 10 Block." });

            // ---- Commons ----
            Add(new CardDef { Id = "twin_cinders", Name = "Twin Cinders", Type = CardType.Attack, Rarity = CardRarity.Common, Cost = 1, Target = TargetMode.Enemy, Dmg = 7, UpDmg = 10, Hits = 2, Block = 4, UpBlock = 6, Art = "card_attack",
                Text = "Deal 7 damage twice.\nGain 4 Block.", UpText = "Deal 10 damage twice.\nGain 6 Block." });
            Add(new CardDef { Id = "cinder_arc", Name = "Cinder Arc", Type = CardType.Attack, Rarity = CardRarity.Common, Cost = 2, Target = TargetMode.AllEnemies, Dmg = 16, UpDmg = 22, Art = "card_attack",
                Text = "Deal 16 damage to ALL enemies.", UpText = "Deal 22 damage to ALL enemies." });
            Add(new CardDef { Id = "iron_surge", Name = "Iron Surge", Type = CardType.Attack, Rarity = CardRarity.Common, Cost = 1, Target = TargetMode.Enemy, Art = "card_attack",
                Text = "Deal 10 damage.\nIf you already have Block, gain 10 Block instead.", UpText = "Deal 14 damage.\nIf you already have Block, gain 14 Block instead.",
                Extra = (c, card, t) =>
                {
                    int n = card.Upgraded ? 14 : 10;
                    if (c.Player.Block > 0) c.GainBlock(c.Player, n);
                    else if (t != null) c.DealDamage(c.Player, t, n, 1, true);
                } });
            Add(new CardDef { Id = "pommel_flare", Name = "Pommel Flare", Type = CardType.Attack, Rarity = CardRarity.Common, Cost = 1, Target = TargetMode.Enemy, Dmg = 12, UpDmg = 16, Draw = 1, Art = "card_attack",
                Text = "Deal 12 damage.\nDraw 1 card.", UpText = "Deal 16 damage.\nDraw 1 card.\nGain 1 Energy if your HP is above half.",
                Extra = (c, card, t) => { if (card.Upgraded && c.Player.Hp * 2 > c.Player.MaxHp) c.Energy += 1; } });
            Add(new CardDef { Id = "dust_off", Name = "Dust Off", Type = CardType.Skill, Rarity = CardRarity.Common, Cost = 1, Block = 12, UpBlock = 16, Art = "card_skill",
                Text = "Gain 12 Block.\nDraw 1 card if you have no Attacks in hand.", UpText = "Gain 16 Block.\nDraw 1 card if you have no Attacks in hand.",
                Extra = (c, card, t) => { if (!c.Hand.Any(h => h.Def.Type == CardType.Attack)) c.DrawCards(1); } });
            Add(new CardDef { Id = "temper", Name = "Temper", Type = CardType.Attack, Rarity = CardRarity.Common, Cost = 1, Target = TargetMode.Enemy, Dmg = 10, UpDmg = 14, Art = "card_attack",
                Text = "Deal 10 damage.\nGain 4 Heft this turn.", UpText = "Deal 14 damage.\nGain 6 Heft this turn.",
                Extra = (c, card, t) => { int n = card.Upgraded ? 6 : 4; c.Player.Add(StatusId.Heft, n); c.Player.Add(StatusId.FadingHeft, n); } });
            Add(new CardDef { Id = "throat_hook", Name = "Throat Hook", Type = CardType.Attack, Rarity = CardRarity.Common, Cost = 2, Target = TargetMode.Enemy, Dmg = 18, UpDmg = 24, Art = "card_attack",
                Text = "Deal 18 damage.\nApply 4 Unsteady.", UpText = "Deal 24 damage.\nApply 6 Unsteady.",
                Extra = (c, card, t) => { if (t != null) { t.Add(StatusId.Unsteady, card.Upgraded ? 6 : 4); c.Floater(t, StatusUtil.Label(StatusId.Unsteady), StatusUtil.ColorOf(StatusId.Unsteady)); } } });
            Add(new CardDef { Id = "greatslag", Name = "Greatslag", Type = CardType.Attack, Rarity = CardRarity.Common, Cost = 2, Target = TargetMode.Enemy, Dmg = 22, UpDmg = 28, Art = "card_attack",
                Text = "Deal 22 damage.\nIf you have Heft, also gain 8 Block.", UpText = "Deal 28 damage.\nIf you have Heft, also gain 12 Block.",
                Extra = (c, card, t) => { if (c.Player.Get(StatusId.Heft) > 0) c.GainBlock(c.Player, card.Upgraded ? 12 : 8); } });
            Add(new CardDef { Id = "ember_return", Name = "Ember Return", Type = CardType.Attack, Rarity = CardRarity.Common, Cost = 1, Target = TargetMode.RandomEnemy, Dmg = 6, UpDmg = 8, Hits = 2, Art = "card_attack",
                Text = "Deal 6 damage to a random enemy twice.\nGain 4 Block.", UpText = "Deal 8 damage to a random enemy twice.\nGain 6 Block.",
                Extra = (c, card, t) => c.GainBlock(c.Player, card.Upgraded ? 6 : 4) });
            Add(new CardDef { Id = "emberclap", Name = "Emberclap", Type = CardType.Attack, Rarity = CardRarity.Common, Cost = 1, Target = TargetMode.AllEnemies, Dmg = 8, UpDmg = 12, Art = "card_attack",
                Text = "Deal 8 damage to ALL enemies.\nApply 2 Dulled to ALL enemies.", UpText = "Deal 12 damage to ALL enemies.\nApply 3 Dulled to ALL enemies.",
                Extra = (c, card, t) => { int n = card.Upgraded ? 3 : 2; foreach (var e in c.AliveEnemies.ToList()) { e.Add(StatusId.Dulled, n); c.Floater(e, StatusUtil.Label(StatusId.Dulled), StatusUtil.ColorOf(StatusId.Dulled)); } } });
            Add(new CardDef { Id = "heat_up", Name = "Heat Up", Type = CardType.Skill, Rarity = CardRarity.Common, Cost = 1, Block = 6, UpBlock = 6, Art = "card_skill",
                Text = "Gain 6 Block.\nGain 5 Heft this turn.", UpText = "Gain 6 Block.\nGain 8 Heft this turn.",
                Extra = (c, card, t) => { int n = card.Upgraded ? 8 : 5; c.Player.Add(StatusId.Heft, n); c.Player.Add(StatusId.FadingHeft, n); c.Floater(c.Player, $"+{n} Heft", StatusUtil.ColorOf(StatusId.Heft)); } });
            Add(new CardDef { Id = "temper_arms", Name = "Temper Arms", Type = CardType.Skill, Rarity = CardRarity.Common, Cost = 1, Block = 8, UpBlock = 8, Art = "card_skill",
                Text = "Gain 8 Block.\nUpgrade a random card in your hand for this combat.", UpText = "Gain 8 Block.\nUpgrade ALL cards in your hand for this combat.",
                Extra = (c, card, t) =>
                {
                    if (card.Upgraded) { foreach (var h in c.Hand) h.Upgraded = true; }
                    else
                    {
                        var pick = c.Hand.FirstOrDefault(h => !h.Upgraded);
                        if (pick != null) pick.Upgraded = true;
                    }
                } });
            Add(new CardDef { Id = "black_grit", Name = "Black Grit", Type = CardType.Skill, Rarity = CardRarity.Common, Cost = 1, Block = 11, UpBlock = 15, Art = "card_skill",
                Text = "Gain 11 Block.\nIf you have a Status in hand, Exhaust it.", UpText = "Gain 15 Block.\nIf you have a Status in hand, Exhaust it.",
                Extra = (c, card, t) =>
                {
                    var st = c.Hand.FirstOrDefault(h => h.Def.Type == CardType.Status || h.Def.Type == CardType.Curse);
                    if (st != null) c.ExhaustCard(st);
                } });
            Add(new CardDef { Id = "blood_tithe", Name = "Blood Tithe", Type = CardType.Skill, Rarity = CardRarity.Common, Cost = 0, Draw = 1, Art = "card_skill",
                Text = "Lose 5 HP.\nGain 2 Energy.\nDraw 1 card.", UpText = "Lose 5 HP.\nGain 3 Energy.\nDraw 1 card.",
                Extra = (c, card, t) => { c.LoseHp(c.Player, 5); c.Energy += card.Upgraded ? 3 : 2; } });
            Add(new CardDef { Id = "kiln_shout", Name = "Kiln Shout", Type = CardType.Skill, Rarity = CardRarity.Common, Cost = 1, Exhaust = true, Draw = 2, UpDraw = 3, Art = "card_skill",
                Text = "Draw 2 cards.\nGain 4 Block.\nExhaust.", UpText = "Draw 3 cards.\nGain 6 Block.\nExhaust.",
                Extra = (c, card, t) => c.GainBlock(c.Player, card.Upgraded ? 6 : 4) });
            Add(new CardDef { Id = "helmbutt", Name = "Helmbutt", Type = CardType.Attack, Rarity = CardRarity.Common, Cost = 1, Target = TargetMode.Enemy, Dmg = 14, UpDmg = 18, Draw = 1, Art = "card_attack",
                Text = "Deal 14 damage.\nDraw 1 card.", UpText = "Deal 18 damage.\nDraw 1 card.\nApply 1 Brittle.",
                Extra = (c, card, t) => { if (card.Upgraded && t != null) t.Add(StatusId.Brittle, 1); } });

            // ---- Uncommons ----
            Add(new CardDef { Id = "kindle", Name = "Kindle", Type = CardType.Power, Rarity = CardRarity.Uncommon, Cost = 1, Art = "card_power",
                Text = "Gain 5 Heft.", UpText = "Gain 8 Heft.",
                Extra = (c, card, t) => { int n = card.Upgraded ? 8 : 5; c.Player.Add(StatusId.Heft, n); c.Floater(c.Player, $"+{n} Heft", StatusUtil.ColorOf(StatusId.Heft)); } });
            Add(new CardDef { Id = "cinder_wall", Name = "Cinder Wall", Type = CardType.Skill, Rarity = CardRarity.Uncommon, Cost = 2, Block = 18, UpBlock = 24, Art = "card_skill",
                Text = "Gain 18 Block.\nDeal 8 damage back when attacked this turn.", UpText = "Gain 24 Block.\nDeal 12 damage back when attacked this turn.",
                Extra = (c, card, t) => c.Player.Add(StatusId.FlameWard, card.Upgraded ? 12 : 8) });
            Add(new CardDef { Id = "battle_fever", Name = "Battle Fever", Type = CardType.Skill, Rarity = CardRarity.Uncommon, Cost = 0, Draw = 2, UpDraw = 3, Art = "card_skill",
                Text = "Draw 2 cards.\nThe next card you play this turn costs 0.", UpText = "Draw 3 cards.\nThe next card you play this turn costs 0.",
                Extra = (c, card, t) =>
                {
                    var n = c.Hand.FirstOrDefault();
                    if (n != null) n.FreeThisTurn = true;
                } });
            Add(new CardDef { Id = "pyre_gift", Name = "Pyre Gift", Type = CardType.Skill, Rarity = CardRarity.Uncommon, Cost = 0, Exhaust = true, Draw = 2, UpDraw = 3, Art = "card_skill",
                Text = "Lose 8 HP.\nGain 3 Energy.\nDraw 2 cards.\nExhaust.", UpText = "Lose 8 HP.\nGain 3 Energy.\nDraw 3 cards.\nExhaust.",
                Extra = (c, card, t) => { c.LoseHp(c.Player, 8); c.Energy += 3; } });
            Add(new CardDef { Id = "rising_cinder", Name = "Rising Cinder", Type = CardType.Attack, Rarity = CardRarity.Uncommon, Cost = 2, Target = TargetMode.Enemy, Dmg = 18, UpDmg = 24, Block = 8, UpBlock = 12, Art = "card_attack",
                Text = "Deal 18 damage.\nGain 8 Block.", UpText = "Deal 24 damage.\nGain 12 Block." });
            Add(new CardDef { Id = "slaughter", Name = "Slaughter", Type = CardType.Attack, Rarity = CardRarity.Uncommon, Cost = 2, Target = TargetMode.Enemy, Dmg = 32, UpDmg = 42, Art = "card_attack",
                Text = "Deal 32 damage.\nLose 4 HP.", UpText = "Deal 42 damage.\nLose 4 HP.",
                Extra = (c, card, t) => c.LoseHp(c.Player, 4) });
            Add(new CardDef { Id = "beatdown", Name = "Beatdown", Type = CardType.Attack, Rarity = CardRarity.Uncommon, Cost = 1, Target = TargetMode.Enemy, Dmg = 5, Hits = 3, Exhaust = true, Art = "card_attack",
                Text = "Deal 5 damage 3 times.\nExhaust.", UpText = "Deal 5 damage 4 times.\nExhaust.",
                Extra = (c, card, t) => { if (card.Upgraded && t != null && t.Alive) c.DealDamage(c.Player, t, 5, 1, true); } });
            Add(new CardDef { Id = "drop_heel", Name = "Drop Heel", Type = CardType.Attack, Rarity = CardRarity.Uncommon, Cost = 1, Target = TargetMode.Enemy, Dmg = 11, UpDmg = 15, Art = "card_attack",
                Text = "Deal 11 damage.\nIf the enemy intends to Attack, gain 1 Energy.", UpText = "Deal 15 damage.\nIf the enemy intends to Attack, gain 1 Energy and draw 1 card.",
                Extra = (c, card, t) =>
                {
                    if (t?.CurrentMove == null) return;
                    if (t.CurrentMove.Intent == IntentKind.Attack || t.CurrentMove.Intent == IntentKind.AttackDebuff)
                    {
                        c.Energy += 1;
                        if (card.Upgraded) c.DrawCards(1);
                    }
                } });
            Add(new CardDef { Id = "body_crash", Name = "Body Crash", Type = CardType.Attack, Rarity = CardRarity.Uncommon, Cost = 1, Target = TargetMode.Enemy, Art = "card_attack",
                Text = "Deal damage equal to your Block + 8.", UpText = "Deal damage equal to your Block + 12.",
                Extra = (c, card, t) => { if (t != null) c.DealDamage(c.Player, t, (card.Upgraded ? 12 : 8), 1, true); } });
            Add(new CardDef { Id = "blood_heat", Name = "Blood Heat", Type = CardType.Skill, Rarity = CardRarity.Uncommon, Cost = 0, Exhaust = true, Art = "card_skill",
                Text = "Gain 2 Energy.\nTake 3 damage.\nExhaust.", UpText = "Gain 3 Energy.\nTake 3 damage.\nExhaust.",
                Extra = (c, card, t) => { c.Energy += card.Upgraded ? 3 : 2; c.LoseHp(c.Player, 3); } });
            Add(new CardDef { Id = "strip_arms", Name = "Strip Arms", Type = CardType.Skill, Rarity = CardRarity.Uncommon, Cost = 1, Exhaust = true, Target = TargetMode.Enemy, Art = "card_skill",
                Text = "Deal 8 damage.\nEnemy loses 3 Heft.\nExhaust.", UpText = "Deal 12 damage.\nEnemy loses 4 Heft.\nExhaust.",
                Extra = (c, card, t) =>
                {
                    if (t == null) return;
                    c.DealDamage(c.Player, t, card.Upgraded ? 12 : 8, 1, true);
                    t.Add(StatusId.Heft, card.Upgraded ? -4 : -3);
                } });
            Add(new CardDef { Id = "dig_in", Name = "Dig In", Type = CardType.Skill, Rarity = CardRarity.Uncommon, Cost = 1, Art = "card_skill",
                Text = "Gain 4 Block for each card in your hand.", UpText = "Gain 6 Block for each card in your hand.",
                Extra = (c, card, t) => c.GainBlock(c.Player, c.Hand.Count * (card.Upgraded ? 6 : 4)) });
            Add(new CardDef { Id = "second_breath", Name = "Second Breath", Type = CardType.Skill, Rarity = CardRarity.Uncommon, Cost = 2, Block = 20, UpBlock = 28, Draw = 2, Art = "card_skill",
                Text = "Gain 20 Block.\nDraw 2 cards.", UpText = "Gain 28 Block.\nDraw 2 cards." });
            Add(new CardDef { Id = "wraith_plate", Name = "Wraith Plate", Type = CardType.Skill, Rarity = CardRarity.Uncommon, Cost = 0, Block = 14, UpBlock = 20, Ethereal = true, Art = "card_skill",
                Text = "Ethereal.\nGain 14 Block.", UpText = "Ethereal.\nGain 20 Block." });
            Add(new CardDef { Id = "find_the_crack", Name = "Find the Crack", Type = CardType.Skill, Rarity = CardRarity.Uncommon, Cost = 1, Target = TargetMode.Enemy, Art = "card_skill",
                Text = "If the enemy intends to Attack, gain 6 Heft. Otherwise deal 10 damage.", UpText = "If the enemy intends to Attack, gain 9 Heft. Otherwise deal 14 damage.",
                Extra = (c, card, t) =>
                {
                    bool atk = t?.CurrentMove != null && (t.CurrentMove.Intent == IntentKind.Attack || t.CurrentMove.Intent == IntentKind.AttackDebuff);
                    if (atk)
                    {
                        int n = card.Upgraded ? 9 : 6;
                        c.Player.Add(StatusId.Heft, n);
                        c.Floater(c.Player, $"+{n} Heft", StatusUtil.ColorOf(StatusId.Heft));
                    }
                    else if (t != null) c.DealDamage(c.Player, t, card.Upgraded ? 14 : 10, 1, true);
                } });
            Add(new CardDef { Id = "ashcyclone", Name = "Ashcyclone", Type = CardType.Attack, Rarity = CardRarity.Uncommon, Cost = 0, XCost = true, Target = TargetMode.AllEnemies, Dmg = 8, UpDmg = 12, Art = "card_attack",
                Text = "Deal 8 damage to ALL enemies X times.", UpText = "Deal 12 damage to ALL enemies X times." });
            Add(new CardDef { Id = "ironhide", Name = "Ironhide", Type = CardType.Power, Rarity = CardRarity.Uncommon, Cost = 1, Art = "card_power",
                Text = "If you have no Block at the end of your turn, gain 10 Block.", UpText = "If you have no Block at the end of your turn, gain 14 Block.",
                Extra = (c, card, t) => c.Player.Add(StatusId.Ironhide, card.Upgraded ? 14 : 10) });
            Add(new CardDef { Id = "grind_through", Name = "Grind Through", Type = CardType.Skill, Rarity = CardRarity.Uncommon, Cost = 1, Block = 22, UpBlock = 30, Art = "card_skill",
                Text = "Gain 22 Block.\nTake 4 damage.", UpText = "Gain 30 Block.\nTake 4 damage.",
                Extra = (c, card, t) => c.LoseHp(c.Player, 4) });
            Add(new CardDef { Id = "reckless_rush", Name = "Reckless Rush", Type = CardType.Attack, Rarity = CardRarity.Uncommon, Cost = 0, Target = TargetMode.Enemy, Dmg = 16, UpDmg = 22, Art = "card_attack",
                Text = "Deal 16 damage.\nApply 1 Brittle to yourself.", UpText = "Deal 22 damage.\nApply 1 Brittle to yourself.",
                Extra = (c, card, t) => c.Player.Add(StatusId.Brittle, 1) });
            Add(new CardDef { Id = "back_off", Name = "Back Off", Type = CardType.Skill, Rarity = CardRarity.Uncommon, Cost = 1, Block = 6, UpBlock = 10, Art = "card_skill",
                Text = "Gain 6 Block.\nApply 2 Dulled to ALL enemies.", UpText = "Gain 10 Block.\nApply 3 Dulled to ALL enemies.",
                Extra = (c, card, t) => { foreach (var e in c.AliveEnemies.ToList()) e.Add(StatusId.Dulled, card.Upgraded ? 3 : 2); } });
            Add(new CardDef { Id = "branding_blow", Name = "Branding Blow", Type = CardType.Attack, Rarity = CardRarity.Uncommon, Cost = 2, Target = TargetMode.Enemy, Dmg = 20, UpDmg = 26, Art = "card_attack",
                Text = "Deal 20 damage.\nIf this kills, draw 2 cards.", UpText = "Deal 26 damage.\nIf this kills, draw 2 cards.",
                Extra = (c, card, t) => { if (t != null && !t.Alive) c.DrawCards(2); } });
            Add(new CardDef { Id = "cinder_pact", Name = "Cinder Pact", Type = CardType.Skill, Rarity = CardRarity.Uncommon, Cost = 1, Draw = 3, UpDraw = 4, Art = "card_skill",
                Text = "Exhaust 1 card.\nDraw 3 cards.", UpText = "Exhaust 1 card.\nDraw 4 cards.",
                Extra = (c, card, t) => { if (c.Hand.Count > 0) c.ExhaustCard(c.Hand[0]); } });

            // ---- Rares ----
            Add(new CardDef { Id = "skullcrush", Name = "Skullcrush", Type = CardType.Attack, Rarity = CardRarity.Rare, Cost = 3, Target = TargetMode.Enemy, Dmg = 48, UpDmg = 62, Art = "card_attack",
                Text = "Deal 48 damage.", UpText = "Deal 62 damage." });
            Add(new CardDef { Id = "infernal_form", Name = "Infernal Form", Type = CardType.Power, Rarity = CardRarity.Rare, Cost = 3, Art = "card_power",
                Text = "At the start of your turn, gain 5 Heft.", UpText = "At the start of your turn, gain 7 Heft.",
                Extra = (c, card, t) => c.Player.Add(StatusId.InfernalForm, card.Upgraded ? 7 : 5) });
            Add(new CardDef { Id = "holdfast", Name = "Holdfast", Type = CardType.Power, Rarity = CardRarity.Rare, Cost = 2, Art = "card_power",
                Text = "The next time Block would empty at turn start, keep it.\nIf you have no Block at the end of your turn, gain 8 Block.", UpText = "The next time Block would empty at turn start, keep it.\nIf you have no Block at the end of your turn, gain 12 Block.",
                Extra = (c, card, t) => { c.Player.Set(StatusId.Holdfast, 1); c.Player.Add(StatusId.Ironhide, card.Upgraded ? 12 : 8); } });
            Add(new CardDef { Id = "impenetrable", Name = "Impenetrable", Type = CardType.Skill, Rarity = CardRarity.Rare, Cost = 2, Block = 45, UpBlock = 60, Exhaust = true, Art = "card_skill",
                Text = "Gain 45 Block.\nExhaust.", UpText = "Gain 60 Block.\nExhaust." });
            Add(new CardDef { Id = "limit_shatter", Name = "Limit Shatter", Type = CardType.Skill, Rarity = CardRarity.Rare, Cost = 1, Exhaust = true, Art = "card_skill",
                Text = "Gain 8 Heft.\nExhaust.", UpText = "Gain 12 Heft.",
                Extra = (c, card, t) => c.Player.Add(StatusId.Heft, card.Upgraded ? 12 : 8) });
            Add(new CardDef { Id = "fiend_pyre", Name = "Fiend Pyre", Type = CardType.Attack, Rarity = CardRarity.Rare, Cost = 2, Target = TargetMode.Enemy, Exhaust = true, Art = "card_attack",
                Text = "Exhaust your hand.\nDeal 12 damage for each Exhausted card.\nExhaust.", UpText = "Exhaust your hand.\nDeal 16 damage for each Exhausted card.\nExhaust.",
                Extra = (c, card, t) =>
                {
                    int n = c.Hand.Count;
                    var list = c.Hand.ToList();
                    foreach (var h in list) c.ExhaustCard(h);
                    if (t != null && n > 0) c.DealDamage(c.Player, t, card.Upgraded ? 16 : 12, n, true);
                } });
            Add(new CardDef { Id = "cremate", Name = "Cremate", Type = CardType.Attack, Rarity = CardRarity.Rare, Cost = 2, Target = TargetMode.AllEnemies, Dmg = 32, UpDmg = 42, Art = "card_attack",
                Text = "Deal 32 damage to ALL enemies.", UpText = "Deal 42 damage to ALL enemies." });
            Add(new CardDef { Id = "devour", Name = "Devour", Type = CardType.Attack, Rarity = CardRarity.Rare, Cost = 2, Target = TargetMode.Enemy, Dmg = 16, UpDmg = 22, Exhaust = true, Art = "card_attack",
                Text = "Deal 16 damage.\nIf this kills, raise your Max HP by 6. Exhaust.", UpText = "Deal 22 damage.\nIf this kills, raise your Max HP by 8. Exhaust.",
                Extra = (c, card, t) =>
                {
                    if (t != null && !t.Alive)
                    {
                        int n = card.Upgraded ? 8 : 6;
                        c.Player.MaxHp += n; c.Run.MaxHp += n; c.Player.Hp += n; c.Run.Hp += n;
                        c.Floater(c.Player, $"+{n} Max HP", Theme.Gold);
                    }
                } });
            Add(new CardDef { Id = "harvest", Name = "Harvest", Type = CardType.Attack, Rarity = CardRarity.Rare, Cost = 2, Target = TargetMode.AllEnemies, Dmg = 8, UpDmg = 12, Exhaust = true, Art = "card_attack",
                Text = "Deal 8 damage to ALL enemies.\nHeal 10 HP. Exhaust.", UpText = "Deal 12 damage to ALL enemies.\nHeal 14 HP. Exhaust.",
                Extra = (c, card, t) => c.Heal(c.Player, card.Upgraded ? 14 : 10) });
            // ---- Status ----
            Add(new CardDef { Id = "gash", Name = "Gash", Type = CardType.Status, Rarity = CardRarity.Common, Cost = 0, Unplayable = true, Art = "card_status",
                Text = "Unplayable." });
            Add(new CardDef { Id = "addled", Name = "Addled", Type = CardType.Status, Rarity = CardRarity.Common, Cost = 0, Unplayable = true, Ethereal = true, Art = "card_status",
                Text = "Unplayable.\nEthereal." });
            Add(new CardDef { Id = "scorch", Name = "Scorch", Type = CardType.Status, Rarity = CardRarity.Common, Cost = 0, Unplayable = true, Ethereal = true, Art = "card_status",
                Text = "Unplayable.\nEthereal.\nAt the end of your turn, take 2 damage.", UpText = "Unplayable.\nEthereal.\nAt the end of your turn, take 4 damage." });
            Add(new CardDef { Id = "dishonor", Name = "Dishonor", Type = CardType.Curse, Rarity = CardRarity.Common, Cost = 0, Unplayable = true, Art = "card_status",
                Text = "Unplayable.\nAt the end of your turn, gain 1 Unsteady.",
                Extra = (c, card, t) => { } });
        }

        static void Add(CardDef c) { Catalog.AllCards.Add(c); }
    }
}
