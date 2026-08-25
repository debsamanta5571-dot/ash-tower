using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshTower
{
    public class RunState
    {
        public System.Random Rng;
        public int Seed;
        public int Hp = 110, MaxHp = 110, Gold = 99;
        public int Floor, EnergyBonus, Girya, RemoveCost = 75;
        public List<CardRuntime> Deck = new List<CardRuntime>();
        public List<RelicDef> Relics = new List<RelicDef>();
        public List<PotionDef> Potions = new List<PotionDef>();
        public List<MapNode> Nodes = new List<MapNode>();
        public int CurrentNode = -1;
        public bool BossDefeated;
        public int MonstersKilled, ElitesKilled, CardsAdded;
        public const int PotionSlots = 3;
        public const int Rows = 15;
        public const int Cols = 7;

        public MapNode Current => CurrentNode >= 0 ? Nodes[CurrentNode] : null;
        public bool HasRelic(string id) => Relics.Any(r => r.Id == id);

        public void NewRun(int seed)
        {
            Seed = seed;
            Rng = new System.Random(seed);
            Hp = MaxHp = 110;
            Gold = 99;
            Floor = 0;
            EnergyBonus = 0;
            Girya = 0;
            RemoveCost = 75;
            BossDefeated = false;
            MonstersKilled = ElitesKilled = CardsAdded = 0;
            Deck.Clear(); Relics.Clear(); Potions.Clear();
            for (int i = 0; i < 5; i++) Deck.Add(Make("ember_cut"));
            for (int i = 0; i < 4; i++) Deck.Add(Make("ash_guard"));
            Deck.Add(Make("slag_bash"));
            AddRelic(Catalog.Relic("kiln_spark"));
            GenerateMap();
            CurrentNode = -1;
        }

        public CardRuntime Make(string id, bool up = false) => new CardRuntime { Def = Catalog.Card(id), Upgraded = up };

        public void AddCard(CardDef def, bool upgraded = false)
        {
            if (def == null) return;
            bool up = upgraded;
            var card = new CardRuntime { Def = def, Upgraded = up };
            if (HasRelic("temper_mark") && def.Type != CardType.Status && def.Type != CardType.Curse)
                card.CostMod -= 1;
            Deck.Add(card);
            CardsAdded++;
        }

        public void AddCard(CardRuntime c)
        {
            if (c?.Def == null) return;
            AddCard(c.Def, c.Upgraded);
        }

        public void RemoveCard(CardRuntime c)
        {
            Deck.Remove(c);
            RemoveCost += 25;
        }

        public void AddRelic(RelicDef r)
        {
            if (r == null || HasRelic(r.Id)) return;
            Relics.Add(r);
            r.OnPickup?.Invoke(this);
        }

        public RelicDef RandomRelic(CardRarity? min = null)
        {
            var owned = new HashSet<string>(Relics.Select(x => x.Id));
            var pool = Catalog.AllRelics.Where(r => r.Rarity != CardRarity.Basic && !owned.Contains(r.Id)).ToList();
            if (min == CardRarity.Uncommon) pool = pool.Where(r => r.Rarity != CardRarity.Common).ToList();
            if (min == CardRarity.Rare) pool = pool.Where(r => r.Rarity == CardRarity.Rare).ToList();
            if (pool.Count == 0) pool = Catalog.AllRelics.Where(r => r.Rarity != CardRarity.Basic && !owned.Contains(r.Id)).ToList();
            if (pool.Count == 0) return null;
            return pool[Rng.Next(pool.Count)];
        }

        public CardDef WeightedCard(CardRarity? force = null, bool rareBias = false) => Catalog.WeightedCard(Rng, force, rareBias);

        public PotionDef RandomPotion() => Catalog.AllPotions[Rng.Next(Catalog.AllPotions.Count)];

        public bool AddPotion(PotionDef p)
        {
            if (p == null || Potions.Count >= PotionSlots) return false;
            Potions.Add(p);
            return true;
        }

        public void Heal(int n) => Hp = Mathf.Min(MaxHp, Hp + n);
        public void Damage(int n) => Hp = Mathf.Max(0, Hp - n);

        public void GenerateMap()
        {
            Nodes.Clear();
            int id = 0;
            var grid = new MapNode[Rows, Cols];
            for (int r = 0; r < Rows; r++)
            {
                int count = r == Rows - 1 ? 1 : r == 0 ? 3 : Rng.Next(3, 6);
                var cols = Enumerable.Range(0, Cols).OrderBy(_ => Rng.Next()).Take(r == Rows - 1 ? 1 : count).OrderBy(x => x).ToList();
                if (r == Rows - 1) cols = new List<int> { 3 };
                foreach (var col in cols)
                {
                    var n = new MapNode { Id = id++, Row = r, Col = col, Type = PickType(r, col) };
                    grid[r, col] = n;
                    Nodes.Add(n);
                }
            }
            for (int r = 0; r < Rows - 1; r++)
            {
                var here = Nodes.Where(n => n.Row == r).ToList();
                var next = Nodes.Where(n => n.Row == r + 1).ToList();
                foreach (var a in here)
                {
                    var targets = next.OrderBy(b => Mathf.Abs(b.Col - a.Col)).ThenBy(_ => Rng.Next()).ToList();
                    int links = r == Rows - 2 ? 1 : Rng.Next(1, 3);
                    foreach (var b in targets.Take(links))
                        if (!a.Next.Contains(b.Id)) a.Next.Add(b.Id);
                }
                // Ensure every next node is reachable
                foreach (var b in next)
                {
                    if (Nodes.Any(n => n.Row == r && n.Next.Contains(b.Id))) continue;
                    var a = here.OrderBy(x => Mathf.Abs(x.Col - b.Col)).First();
                    if (!a.Next.Contains(b.Id)) a.Next.Add(b.Id);
                }
            }
        }

        RoomType PickType(int r, int col)
        {
            if (r == Rows - 1) return RoomType.Boss;
            if (r == 0) return RoomType.Monster;
            if (r == Rows - 2) return RoomType.Rest;
            if (r == 8) return RoomType.Treasure;
            int n = Rng.Next(100);
            if (r >= 5 && n < 16) return RoomType.Elite;
            if (n < 28) return RoomType.Event;
            if (n < 38) return RoomType.Rest;
            if (n < 48) return RoomType.Shop;
            if (n < 54 && r > 3) return RoomType.Treasure;
            return RoomType.Monster;
        }

        public IEnumerable<MapNode> Available()
        {
            if (CurrentNode < 0) return Nodes.Where(n => n.Row == 0);
            return Current.Next.Select(i => Nodes[i]);
        }

        public bool CanEnter(MapNode n) => Available().Any(x => x.Id == n.Id);

        public void Enter(MapNode n)
        {
            CurrentNode = n.Id;
            n.Seen = true;
            Floor = n.Row + 1;
        }

        public int ShopPrice(int basePrice)
        {
            int p = basePrice;
            if (HasRelic("coal_ledger")) p -= 40;
            return Mathf.Max(1, p);
        }
    }
}
