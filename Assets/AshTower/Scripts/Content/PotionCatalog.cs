using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshTower
{
    public static class PotionCatalog
    {
        public static void Register()
        {
            Catalog.AllPotions.Add(new PotionDef { Id = "slag_vial", Name = "Slag Vial", Desc = "Deal 8 damage to every enemy and gain 6 Block.",
                Use = c => { foreach (var e in c.AliveEnemies.ToList()) c.DealDamage(c.Player, e, 8, 1, false); c.GainBlock(c.Player, 6); } });
            Catalog.AllPotions.Add(new PotionDef { Id = "kiln_salve", Name = "Kiln Salve", Desc = "Gain 8 Block and draw 2 cards.",
                Use = c => { c.GainBlock(c.Player, 8); c.DrawCards(2, forced: true); } });
            Catalog.AllPotions.Add(new PotionDef { Id = "soot_draft", Name = "Soot Draft", Desc = "Draw 2 cards. They cost 0 this turn.",
                Use = c =>
                {
                    int n = c.Hand.Count;
                    c.DrawCards(2, forced: true);
                    for (int i = n; i < c.Hand.Count; i++) c.Hand[i].FreeThisTurn = true;
                } });
            Catalog.AllPotions.Add(new PotionDef { Id = "temper_oil", Name = "Temper Oil", Desc = "Gain 6 Heft this turn.",
                Use = c => { c.Player.Add(StatusId.Heft, 6); c.Player.Add(StatusId.FadingHeft, 6); } });
            Catalog.AllPotions.Add(new PotionDef { Id = "nail_tincture", Name = "Nail Tincture", Desc = "Apply 3 Brittle and 3 Dulled to every enemy.",
                Use = c => { foreach (var e in c.AliveEnemies.ToList()) { e.Add(StatusId.Brittle, 3); e.Add(StatusId.Dulled, 3); } } });
            Catalog.AllPotions.Add(new PotionDef { Id = "cinder_sip", Name = "Cinder Sip", Desc = "Deal 14 damage to the enemy with the most HP and heal 4.",
                Use = c =>
                {
                    var t = c.AliveEnemies.OrderByDescending(e => e.Hp).FirstOrDefault();
                    if (t != null) c.DealDamage(c.Player, t, 14, 1, false);
                    c.Heal(c.Player, 4);
                } });
            Catalog.AllPotions.Add(new PotionDef { Id = "bone_broth", Name = "Bone Broth", Desc = "Gain 8 Max HP. Lose 3 HP.",
                Use = c => { c.Player.MaxHp += 8; c.Run.MaxHp += 8; c.Player.Hp = Mathf.Max(1, c.Player.Hp - 3); c.Run.Hp = c.Player.Hp; } });
            Catalog.AllPotions.Add(new PotionDef { Id = "spark_phial", Name = "Spark Phial", Desc = "Gain 1 Energy, draw a card, and gain 4 Block.",
                Use = c => { c.Energy += 1; c.DrawCards(1, forced: true); c.GainBlock(c.Player, 4); } });
            Catalog.AllPotions.Add(new PotionDef { Id = "ward_pitch", Name = "Ward Pitch", Desc = "Gain 1 Seal and 8 Block. Seal stops the next HP loss.",
                Use = c => { c.Player.Add(StatusId.Seal, 1); c.GainBlock(c.Player, 8); } });
            Catalog.AllPotions.Add(new PotionDef { Id = "quench_flask", Name = "Quench Flask", Desc = "Exhaust a random card in your hand. Gain 2 Energy.",
                Use = c =>
                {
                    if (c.Hand.Count > 0) c.ExhaustCard(c.Hand[c.Rng.Next(c.Hand.Count)]);
                    c.Energy += 2;
                } });
        }
    }
}
