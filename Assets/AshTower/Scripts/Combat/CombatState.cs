using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshTower
{
    public class CombatFx
    {
        public enum Kind { Damage, Block, Heal, Status, Play, Death, Talk, Shuffle, Energy, Draw, Exhaust }
        public Kind Type;
        public Combatant Who, Src;
        public int N;
        public string Msg;
        public Color Color = Color.white;
    }

    public class CombatState
    {
        public RunState Run;
        public Combatant Player;
        public List<Combatant> Enemies = new List<Combatant>();
        public List<CardRuntime> DrawPile = new List<CardRuntime>();
        public List<CardRuntime> Hand = new List<CardRuntime>();
        public List<CardRuntime> Discard = new List<CardRuntime>();
        public List<CardRuntime> ExhaustPile = new List<CardRuntime>();
        public List<CardRuntime> Powers = new List<CardRuntime>();
        public int Energy, EnergyMax = 3;
        public int Turn;
        public bool PlayerTurn = true;
        public bool Over;
        public bool Won;
        public bool Elite, Boss;
        public int CardsPlayedThisTurn;
        public int AttacksPlayedThisTurn;
        public int SkillsPlayedThisTurn;
        public int DamageThisTurn;
        public int Shuffles;
        public int HpLostThisCombat;
        public int AttacksPlayedCombat;
        public int CardsPlayedCombat;
        public CardRuntime LastPlayed;
        public int XValue;
        public bool CannotDrawExtra;
        public List<CombatFx> Fx = new List<CombatFx>();
        public System.Random Rng;

        public IEnumerable<Combatant> AliveEnemies => Enemies.Where(e => e.Alive);
        public int AliveCount => Enemies.Count(e => e.Alive);

        public void Begin(RunState run, Encounter enc)
        {
            Run = run;
            Rng = run.Rng;
            Elite = enc.Elite;
            Boss = enc.Boss;
            Over = Won = false;
            Turn = 0;
            EnergyMax = 3 + run.EnergyBonus;
            CardsPlayedThisTurn = AttacksPlayedThisTurn = SkillsPlayedThisTurn = 0;
            AttacksPlayedCombat = 0;
            HpLostThisCombat = 0;
            Shuffles = 0;
            CannotDrawExtra = false;
            Fx.Clear();
            DrawPile.Clear(); Hand.Clear(); Discard.Clear(); ExhaustPile.Clear(); Powers.Clear();

            Player = new Combatant
            {
                Name = "Cinder Knight",
                IsPlayer = true,
                Hp = run.Hp,
                MaxHp = run.MaxHp,
                ArtKey = "knight"
            };

            Enemies.Clear();
            foreach (var id in enc.EnemyIds)
            {
                var def = Catalog.Enemy(id);
                var e = new Combatant
                {
                    Name = def.Name,
                    IsPlayer = false,
                    Def = def,
                    ArtKey = def.Art,
                    MaxHp = Rng.Next(def.HpMin, def.HpMax + 1),
                    PatternIndex = 0
                };
                e.Hp = e.MaxHp;
                def.OnSpawn?.Invoke(e);
                e.CurrentMove = def.Choose(e, this);
                Enemies.Add(e);
            }

            foreach (var c in run.Deck)
                DrawPile.Add(new CardRuntime { Def = c.Def, Upgraded = c.Upgraded });
            Shuffle(DrawPile);

            foreach (var r in run.Relics)
                r.CombatStart?.Invoke(this);

            // Draw Innate cards first.
            var innate = DrawPile.Where(c => c.Def.Innate).ToList();
            foreach (var c in innate)
            {
                DrawPile.Remove(c);
                DrawPile.Insert(0, c);
            }

            StartPlayerTurn(true);
        }

        public void StartPlayerTurn(bool first)
        {
            if (Over) return;
            PlayerTurn = true;
            Turn++;
            CardsPlayedThisTurn = AttacksPlayedThisTurn = SkillsPlayedThisTurn = DamageThisTurn = 0;
            XValue = 0;
            CannotDrawExtra = false;
            Player.GainedBlockThisTurn = false;
            Player.Set(StatusId.EmptyHands, 0);
            SettleBlock(Player);

            TickCinderrot(Player);

            int demon = Player.Get(StatusId.InfernalForm);
            if (demon > 0) { Player.Add(StatusId.Heft, demon); Floater(Player, $"+{demon} Heft", StatusUtil.ColorOf(StatusId.Heft)); }
            int comb = Player.Get(StatusId.InnerPyre);
            if (comb > 0)
            {
                LoseHp(Player, 2);
                foreach (var e in AliveEnemies.ToList()) DealDamage(Player, e, comb, 1, false);
            }
            int brut = Player.Get(StatusId.Savagery);
            if (brut > 0) { LoseHp(Player, brut); DrawCards(brut, forced: true); }

            Energy = EnergyMax;

            foreach (var r in Run.Relics) r.TurnStart?.Invoke(this);

            DrawCards(5, forced: true);

            foreach (var c in Hand) c.FreeThisTurn = false;
        }

        public bool CanPlay(CardRuntime card)
        {
            if (Over || !PlayerTurn || card == null) return false;
            if (!Hand.Contains(card)) return false;
            if (card.Unplayable) return false;
            if (Hand.Count >= 0 && card.GetCost(this) > Energy && !card.Def.XCost) return false;
            if (card.Def.XCost && Energy < 0) return false;
            if (card.Def.Target == TargetMode.Enemy && AliveCount == 0) return false;
            return true;
        }

        public bool NeedsTarget(CardRuntime card) =>
            card != null && card.Def.Target == TargetMode.Enemy && AliveCount > 1;

        public void Play(CardRuntime card, Combatant target)
        {
            if (!CanPlay(card)) return;
            if (card.Def.Target == TargetMode.Enemy)
            {
                if (target == null || !target.Alive)
                    target = AliveEnemies.FirstOrDefault();
                if (target == null) return;
            }

            int cost = card.GetCost(this);
            if (card.Def.XCost) { XValue = Energy; cost = Energy; }
            Energy -= cost;
            Hand.Remove(card);
            LastPlayed = card;
            CardsPlayedThisTurn++;
            CardsPlayedCombat++;
            Push(CombatFx.Kind.Play, Player, 0, card.Def.DisplayName(card.Upgraded));

            if (card.Def.Type == CardType.Attack) { AttacksPlayedThisTurn++; AttacksPlayedCombat++; }
            if (card.Def.Type == CardType.Skill) SkillsPlayedThisTurn++;

            ResolveCard(card, target);

            if (Player.Get(StatusId.DoubleSwing) > 0 && card.Def.Type == CardType.Attack)
            {
                Player.Add(StatusId.DoubleSwing, -1);
                ResolveCard(card, target);
            }

            foreach (var r in Run.Relics) r.OnPlay?.Invoke(this, card);
            foreach (var e in AliveEnemies.ToList())
                e.Def.OnPlayerPlayed?.Invoke(this, e, card);

            if (card.Def.Type == CardType.Power)
            {
                Powers.Add(card);
            }
            else if (card.Exhausts(this))
            {
                ExhaustCard(card);
            }
            else
            {
                Discard.Add(card);
            }

            CheckEnd();
        }

        void ResolveCard(CardRuntime card, Combatant target)
        {
            bool up = card.Upgraded;
            int dmg = card.Def.Damage(up);
            int hits = card.Def.Hits;
            int blk = card.Def.BlockAmt(up);
            int drw = card.Def.DrawAmt(up);

            if (card.Def.Id == "body_crash")
            {
                if (target != null) DealDamage(Player, target, Player.Block, 1, true);
            }
            else if (card.Def.Id == "ashcyclone")
            {
                int x = Mathf.Max(0, XValue);
                for (int i = 0; i < x; i++)
                    foreach (var e in AliveEnemies.ToList())
                        DealDamage(Player, e, dmg, 1, true);
            }
            else if (card.Def.Target == TargetMode.AllEnemies && dmg > 0)
            {
                foreach (var e in AliveEnemies.ToList())
                    DealDamage(Player, e, dmg, hits, true);
            }
            else if (card.Def.Target == TargetMode.RandomEnemy && dmg > 0)
            {
                for (int i = 0; i < hits; i++)
                {
                    var e = RandomAlive();
                    if (e != null) DealDamage(Player, e, dmg, 1, true);
                }
            }
            else if (dmg > 0 && target != null)
            {
                DealDamage(Player, target, dmg, hits, true);
            }

            if (blk > 0) GainBlock(Player, blk);
            if (drw > 0) DrawCards(drw);

            card.Def.Extra?.Invoke(this, card, target);
        }

        public void EndPlayerTurn()
        {
            if (Over || !PlayerTurn) return;
            PlayerTurn = false;

            int met = Player.Get(StatusId.Ironhide);
            if (met > 0) GainBlock(Player, met);
            if (Hand.Any(c => c.Def.Id == "dishonor"))
                Player.Add(StatusId.Unsteady, 2);

            foreach (var r in Run.Relics) r.TurnEnd?.Invoke(this);

            foreach (var c in Hand.ToList())
            {
                if (c.Def.Id == "scorch") LoseHp(Player, c.Upgraded ? 4 : 2);
            }

            foreach (var c in Hand.ToList())
            {
                Hand.Remove(c);
                if (c.Def.Ethereal) ExhaustCard(c);
                else Discard.Add(c);
            }

            PulseHeft(Player);
            PunishOpen(Player);
            if (Player.Block == 0 && Player.Get(StatusId.Ironhide) > 0)
                GainBlock(Player, Player.Get(StatusId.Ironhide));

            if (Player.Get(StatusId.FadingHeft) > 0)
            {
                Player.Add(StatusId.Heft, -Player.Get(StatusId.FadingHeft));
                Player.Set(StatusId.FadingHeft, 0);
            }

            Decay(Player);
            CheckEnd();
        }

        public void ExecuteEnemy(Combatant e)
        {
            if (Over || !e.Alive) return;
            e.GainedBlockThisTurn = false;
            SettleBlock(e);
            TickCinderrot(e);

            int ritual = e.Get(StatusId.Rite);
            if (ritual > 0) { e.Add(StatusId.Heft, ritual); Floater(e, $"+{ritual} Heft", StatusUtil.ColorOf(StatusId.Heft)); }

            var m = e.CurrentMove;
            if (m != null && e.Get(StatusId.Dormant) == 0)
            {
                if (!string.IsNullOrEmpty(m.Talk)) Push(CombatFx.Kind.Talk, e, 0, m.Talk);
                if (m.Damage > 0)
                    DealDamage(e, Player, m.Damage, Mathf.Max(1, m.Hits), true);
                if (m.Block > 0) GainBlock(e, m.Block);
                if (m.Heft > 0) { e.Add(StatusId.Heft, m.Heft); Floater(e, $"+{m.Heft} Heft", StatusUtil.ColorOf(StatusId.Heft)); }
                if (m.DebuffAmt > 0) { Player.Add(m.Debuff, m.DebuffAmt); Floater(Player, StatusUtil.Label(m.Debuff), StatusUtil.ColorOf(m.Debuff)); }
                if (m.SelfBuffAmt > 0) e.Add(m.SelfBuff, m.SelfBuffAmt);
                if (m.Sleep) e.Add(StatusId.Dormant, 1);
                if (m.Talk == "GASH") AddToDiscard("gash");
                if (m.Talk == "STEAL" && Hand.Count > 0)
                {
                    var stolen = Hand[Rng.Next(Hand.Count)];
                    ExhaustCard(stolen);
                    Floater(Player, "Stolen!", Theme.Ember);
                }
            }

            if (e.Get(StatusId.Dormant) > 0)
            {
                e.Add(StatusId.Dormant, -1);
                if (e.Get(StatusId.Dormant) <= 0) Floater(e, "Awakens", Theme.Ember);
            }

            PulseHeft(e);
            PunishOpen(e);
            if (e.Alive && e.Block == 0 && e.Get(StatusId.Ironhide) > 0)
                GainBlock(e, e.Get(StatusId.Ironhide));

            Decay(e);
            if (e.Alive) e.CurrentMove = e.Def.Choose(e, this);
            CheckEnd();
        }

        public void DrawCards(int n, bool forced = false)
        {
            if (n <= 0) return;
            if (!forced && (CannotDrawExtra || Player.Get(StatusId.EmptyHands) > 0)) return;
            for (int i = 0; i < n; i++)
            {
                if (Hand.Count >= 10) break;
                if (DrawPile.Count == 0) Recycle();
                if (DrawPile.Count == 0) break;
                var c = DrawPile[0];
                DrawPile.RemoveAt(0);
                Hand.Add(c);
                Push(CombatFx.Kind.Draw, Player, 1, c.Def.Name);
                if (Player.Get(StatusId.CinderBreath) > 0 && (c.Def.Type == CardType.Status || c.Def.Type == CardType.Curse))
                    foreach (var e in AliveEnemies.ToList())
                        DealDamage(Player, e, Player.Get(StatusId.CinderBreath), 1, false);
            }
        }

        public void Recycle()
        {
            if (Discard.Count == 0) return;
            DrawPile.AddRange(Discard);
            Discard.Clear();
            Shuffle(DrawPile);
            Shuffles++;
            Push(CombatFx.Kind.Shuffle, Player, 0, "Shuffle");
            foreach (var r in Run.Relics) r.OnShuffle?.Invoke(this);
        }

        public void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public void DealDamage(Combatant src, Combatant dst, int amount, int hits, bool fromCard)
        {
            if (dst == null || !dst.Alive || amount < 0) return;
            bool pierce = fromCard && dst.Get(StatusId.Brittle) > 0;
            for (int h = 0; h < hits; h++)
            {
                if (!dst.Alive) break;
                int dmg = amount;
                if (fromCard && src != null && src.IsPlayer)
                {
                    if (Run.HasRelic("forge_wedge")) dmg += 1;
                    if (h == 0 && Run.HasRelic("soot_stamp") && AttacksPlayedCombat == 4) dmg += 12;
                }
                dmg = Mathf.Max(0, dmg);

                int toBlock = 0;
                int hp = dmg;
                if (!pierce)
                {
                    toBlock = Mathf.Min(dst.Block, dmg);
                    dst.Block -= toBlock;
                    hp = dmg - toBlock;
                }

                if (toBlock > 0 && src != null && src.Alive && dst.Get(StatusId.Nails) > 0)
                    HitThroughBlock(dst, src, dst.Get(StatusId.Nails));

                if (hp > 0)
                {
                    if (AbsorbHit(dst))
                    {
                        Push(CombatFx.Kind.Block, dst, 0, "Seal", src);
                    }
                    else
                    {
                        dst.Hp = Mathf.Max(0, dst.Hp - hp);
                        if (dst.IsPlayer)
                        {
                            HpLostThisCombat += hp;
                            foreach (var r in Run.Relics) r.OnHpLoss?.Invoke(this, hp);
                        }
                        Push(CombatFx.Kind.Damage, dst, hp, $"-{hp}", src);
                        if (dst.Get(StatusId.Hunker) > 0)
                        {
                            int b = dst.Get(StatusId.Hunker);
                            dst.Set(StatusId.Hunker, 0);
                            GainBlock(dst, b);
                        }
                        dst.Def?.OnDamaged?.Invoke(this, dst);
                        int spikes = dst.Get(StatusId.Quills) + dst.Get(StatusId.FlameWard);
                        if (spikes > 0 && src != null && src.Alive)
                            LoseHp(src, spikes);
                    }
                }
                else
                {
                    Push(CombatFx.Kind.Block, dst, toBlock, "Blocked", src);
                }
                if (src != null && src.IsPlayer) DamageThisTurn += dmg;
                if (!dst.Alive)
                {
                    dst.Hp = 0;
                    Push(CombatFx.Kind.Death, dst, 0, "Death");
                }
            }
            if (pierce) dst.Add(StatusId.Brittle, -1);
            if (fromCard && src != null && src.Alive && src.Get(StatusId.Dulled) > 0)
            {
                int recoil = src.Get(StatusId.Dulled);
                LoseHp(src, recoil);
                src.Add(StatusId.Dulled, -1);
                Floater(src, "Recoil " + recoil, StatusUtil.ColorOf(StatusId.Dulled));
            }
        }

        public void GainBlock(Combatant c, int amount)
        {
            if (c == null || !c.Alive || amount <= 0) return;
            if (c.Get(StatusId.Cinderrot) > 0)
            {
                Floater(c, "Cinderrot", StatusUtil.ColorOf(StatusId.Cinderrot));
                return;
            }
            c.GainedBlockThisTurn = true;
            c.Block += amount;
            Push(CombatFx.Kind.Block, c, amount, $"+{amount} Block");
            int jug = c.Get(StatusId.WarEngine);
            if (c.IsPlayer && jug > 0)
            {
                var e = RandomAlive();
                if (e != null) DealDamage(c, e, jug, 1, false);
            }
        }

        void HitThroughBlock(Combatant src, Combatant dst, int amount)
        {
            if (dst == null || !dst.Alive || amount <= 0) return;
            int toBlock = Mathf.Min(dst.Block, amount);
            dst.Block -= toBlock;
            int hp = amount - toBlock;
            if (hp > 0) LoseHp(dst, hp);
            else Push(CombatFx.Kind.Block, dst, toBlock, "Blocked", src);
        }

        public void LoseHp(Combatant c, int n)
        {
            if (c == null || !c.Alive || n <= 0) return;
            if (AbsorbHit(c))
            {
                Floater(c, "Seal", StatusUtil.ColorOf(StatusId.Seal));
                return;
            }
            c.Hp = Mathf.Max(0, c.Hp - n);
            if (c.IsPlayer)
            {
                HpLostThisCombat += n;
                foreach (var r in Run.Relics) r.OnHpLoss?.Invoke(this, n);
            }
            Push(CombatFx.Kind.Damage, c, n, $"-{n}");
            if (!c.Alive) Push(CombatFx.Kind.Death, c, 0, "Death");
        }

        public void Heal(Combatant c, int n)
        {
            if (c == null || n <= 0) return;
            int before = c.Hp;
            c.Hp = Mathf.Min(c.MaxHp, c.Hp + n);
            int g = c.Hp - before;
            if (g > 0) Push(CombatFx.Kind.Heal, c, g, $"+{g}");
        }

        public void ExhaustCard(CardRuntime card)
        {
            Hand.Remove(card);
            Discard.Remove(card);
            DrawPile.Remove(card);
            if (!ExhaustPile.Contains(card)) ExhaustPile.Add(card);
            Push(CombatFx.Kind.Exhaust, Player, 0, card.Def.Name);
            int fnp = Player.Get(StatusId.NumbFlesh);
            if (fnp > 0) GainBlock(Player, fnp);
            int de = Player.Get(StatusId.DarkKindling);
            if (de > 0) DrawCards(de);
            if (Run.HasRelic("cinder_wake")) Energy += 1;
            if (Run.HasRelic("splinter_bough"))
            {
                var e = RandomAlive();
                if (e != null) DealDamage(Player, e, 6, 1, false);
                GainBlock(Player, 3);
            }
        }

        public void AddToDiscard(string id, bool upgraded = false)
        {
            var def = Catalog.Card(id);
            if (def == null) return;
            Discard.Add(new CardRuntime { Def = def, Upgraded = upgraded });
        }

        public void AddToHand(string id, bool upgraded = false, bool free = false)
        {
            var def = Catalog.Card(id);
            if (def == null) return;
            if (Hand.Count >= 10) { Discard.Add(new CardRuntime { Def = def, Upgraded = upgraded }); return; }
            Hand.Add(new CardRuntime { Def = def, Upgraded = upgraded, FreeThisTurn = free });
        }

        public void AddToDraw(string id, bool upgraded = false)
        {
            var def = Catalog.Card(id);
            if (def == null) return;
            DrawPile.Insert(0, new CardRuntime { Def = def, Upgraded = upgraded });
        }

        public Combatant RandomAlive()
        {
            var a = AliveEnemies.ToList();
            if (a.Count == 0) return null;
            return a[Rng.Next(a.Count)];
        }

        public List<CardRuntime> RandomCards(CardType? type, CardRarity? rarity, int n)
        {
            var pool = Catalog.AllCards.Where(c =>
                c.Rarity != CardRarity.Basic &&
                c.Type != CardType.Status && c.Type != CardType.Curse &&
                (type == null || c.Type == type.Value) &&
                (rarity == null || c.Rarity == rarity.Value)).ToList();
            Shuffle(pool);
            return pool.Take(n).Select(d => new CardRuntime { Def = d }).ToList();
        }

        void TickCinderrot(Combatant c)
        {
            int p = c.Get(StatusId.Cinderrot);
            if (p <= 0) return;
            LoseHp(c, p);
            if (!c.IsPlayer)
            {
                var others = AliveEnemies.Where(e => e != c).ToList();
                if (others.Count > 0)
                    others[Rng.Next(others.Count)].Add(StatusId.Cinderrot, 1);
            }
            c.Add(StatusId.Cinderrot, -1);
        }

        void PulseHeft(Combatant c)
        {
            int h = c.Get(StatusId.Heft);
            if (h <= 0 || !c.Alive) return;
            if (c.IsPlayer)
            {
                foreach (var e in AliveEnemies.ToList())
                    DealDamage(c, e, h, 1, false);
            }
            else
                DealDamage(c, Player, h, 1, false);
            c.Add(StatusId.Heft, -1);
            Floater(c, "Heft " + h, StatusUtil.ColorOf(StatusId.Heft));
        }

        void PunishOpen(Combatant c)
        {
            int u = c.Get(StatusId.Unsteady);
            if (u <= 0 || !c.Alive || c.Block > 0) return;
            LoseHp(c, u);
            Floater(c, "Unsteady", StatusUtil.ColorOf(StatusId.Unsteady));
        }

        void SettleBlock(Combatant c)
        {
            int had = c.Block;
            int keep;
            if (c.Get(StatusId.Holdfast) > 0 || c.Get(StatusId.HoldGuard) > 0)
            {
                keep = had;
                if (c.Get(StatusId.Holdfast) > 0 && had > 0)
                    c.Add(StatusId.Holdfast, -1);
            }
            else
                keep = Mathf.Min(had, c.Get(StatusId.Poise));
            if (c.IsPlayer && Run.HasRelic("kiln_rim") && had > 0)
                keep = Mathf.Max(keep, Mathf.FloorToInt(had * 0.4f));
            c.Block = keep;
        }

        bool AbsorbHit(Combatant c)
        {
            if (c.Get(StatusId.Seal) <= 0) return false;
            c.Add(StatusId.Seal, -1);
            return true;
        }

        void Decay(Combatant c)
        {
            var keys = c.St.Keys.ToList();
            foreach (var k in keys)
                if (StatusUtil.DecaysAtTurnEnd(k) && c.Get(k) > 0)
                    c.Add(k, -1);
        }

        public void Floater(Combatant who, string msg, Color col)
        {
            Fx.Add(new CombatFx { Type = CombatFx.Kind.Status, Who = who, Msg = msg, Color = col });
        }

        void Push(CombatFx.Kind k, Combatant who, int n, string msg, Combatant src = null)
        {
            Fx.Add(new CombatFx { Type = k, Who = who, Src = src, N = n, Msg = msg });
        }

        public void CheckEnd()
        {
            if (Over) return;
            if (!Player.Alive)
            {
                Over = true; Won = false;
                Run.Hp = 0;
                return;
            }
            if (AliveCount == 0)
            {
                Over = true; Won = true;
                Run.Hp = Player.Hp;
            }
        }

        public int IntentDamage(Combatant e)
        {
            var m = e.CurrentMove;
            if (m == null || m.Damage <= 0) return 0;
            return m.Damage;
        }
    }
}
