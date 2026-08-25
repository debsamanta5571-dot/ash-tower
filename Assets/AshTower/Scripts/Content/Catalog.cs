using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshTower
{
    public static class Catalog
    {
        public static readonly List<CardDef> AllCards = new List<CardDef>();
        public static readonly List<EnemyDef> AllEnemies = new List<EnemyDef>();
        public static readonly List<RelicDef> AllRelics = new List<RelicDef>();
        public static readonly List<PotionDef> AllPotions = new List<PotionDef>();
        public static readonly List<GameEvent> AllEvents = new List<GameEvent>();
        static bool _built;

        public static CardDef Card(string id) => AllCards.FirstOrDefault(c => c.Id == id);
        public static EnemyDef Enemy(string id) => AllEnemies.FirstOrDefault(e => e.Id == id);
        public static RelicDef Relic(string id) => AllRelics.FirstOrDefault(r => r.Id == id);
        public static PotionDef Potion(string id) => AllPotions.FirstOrDefault(p => p.Id == id);

        public static void Build()
        {
            if (_built) return;
            _built = true;
            CardCatalog.Register();
            EnemyCatalog.Register();
            RelicCatalog.Register();
            PotionCatalog.Register();
            EventCatalog.Register();
        }

        public static Encounter EncounterFor(RoomType type, int row, System.Random rng)
        {
            if (type == RoomType.Boss) return new Encounter { Id = "boss", Boss = true, EnemyIds = { "ash_warden" } };
            if (type == RoomType.Elite)
            {
                string[] elites = { "forge_brute", "cinder_cocoon", "ash_mite|ash_mite|ash_mite" };
                var pick = elites[rng.Next(elites.Length)];
                var enc = new Encounter { Id = "elite", Elite = true };
                foreach (var p in pick.Split('|')) enc.EnemyIds.Add(p);
                return enc;
            }
            string[] easy = { "cinder_choir", "ash_mite|ash_mite", "slag_glob", "kiln_maw" };
            string[] mid = { "cinder_choir|ash_mite", "kiln_maw", "slag_glob|ash_mite", "nailback", "ashpicker|ashpicker" };
            string[] hard = { "cinder_choir|slag_glob", "kiln_maw|ash_mite", "nailback|slag_glob", "ashpicker|ashpicker|ashpicker", "nailback|cinder_choir" };
            var pool = row < 4 ? easy : row < 9 ? mid : hard;
            var s = pool[rng.Next(pool.Length)];
            var e = new Encounter { Id = "mon" };
            foreach (var p in s.Split('|')) e.EnemyIds.Add(p);
            return e;
        }

        public static CardDef WeightedCard(System.Random rng, CardRarity? force = null, bool rareBias = false)
        {
            var rarity = force ?? RollRarity(rng, rareBias);
            var pool = AllCards.Where(c => c.Rarity == rarity && c.Type != CardType.Status && c.Type != CardType.Curse).ToList();
            if (pool.Count == 0) pool = AllCards.Where(c => c.Rarity == CardRarity.Common && c.Type != CardType.Status).ToList();
            return pool[rng.Next(pool.Count)];
        }

        public static CardRarity RollRarity(System.Random rng, bool rareBias)
        {
            int n = rng.Next(100);
            if (rareBias) return n < 20 ? CardRarity.Rare : n < 70 ? CardRarity.Uncommon : CardRarity.Common;
            if (n < 3) return CardRarity.Rare;
            if (n < 37) return CardRarity.Uncommon;
            return CardRarity.Common;
        }
    }
}
