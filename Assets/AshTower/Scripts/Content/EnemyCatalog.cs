using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AshTower
{
    public static class EnemyCatalog
    {
        public static void Register()
        {
            Catalog.AllEnemies.Add(new EnemyDef
            {
                Id = "cinder_choir", Name = "Cinder Choir", Art = "cinder_choir", HpMin = 76, HpMax = 88,
                Choose = (e, c) =>
                {
                    int step = e.PatternIndex++ % 3;
                    if (step == 0) return new Move { Name = "Hymn", Intent = IntentKind.Debuff, Debuff = StatusId.Dulled, DebuffAmt = 1, Block = 6 };
                    if (step == 1) return Atk("Verse", 12);
                    return new Move { Name = "Chorus", Intent = IntentKind.AttackDebuff, Damage = 10, Hits = 1, Debuff = StatusId.Unsteady, DebuffAmt = 1 };
                }
            });
            Catalog.AllEnemies.Add(new EnemyDef
            {
                Id = "kiln_maw", Name = "Kiln Maw", Art = "kiln_maw", HpMin = 68, HpMax = 80,
                Choose = (e, c) =>
                {
                    int step = e.PatternIndex++ % 3;
                    if (step == 0) return new Move { Name = "Stoke", Intent = IntentKind.DefendBuff, Block = 12, Heft = 2 };
                    if (step == 1) return Atk("Erupt", 20);
                    return new Move { Name = "Vent", Intent = IntentKind.AttackDebuff, Damage = 8, Debuff = StatusId.Unsteady, DebuffAmt = 2 };
                }
            });
            Catalog.AllEnemies.Add(new EnemyDef
            {
                Id = "ash_mite", Name = "Ash Mite", Art = "ash_mite", HpMin = 26, HpMax = 34,
                OnDamaged = (c, e) =>
                {
                    if (e.Get(StatusId.PhaseShift) == 0)
                    {
                        e.Set(StatusId.PhaseShift, 1);
                        c.AddToDiscard("gash");
                        c.Floater(c.Player, "Infested", Theme.Ember);
                    }
                },
                OnPlayerPlayed = (c, e, card) =>
                {
                    if (card.Def.Type == CardType.Skill && e.Alive)
                    {
                        e.Hp = Mathf.Min(e.MaxHp, e.Hp + 6);
                        c.Floater(e, "+6", Theme.Hp);
                    }
                },
                Choose = (e, c) => c.Rng.Next(100) < 55 ? Atk("Nibble", 9) : new Move { Name = "Infest", Intent = IntentKind.Debuff, Talk = "GASH" }
            });
            Catalog.AllEnemies.Add(new EnemyDef
            {
                Id = "slag_glob", Name = "Slag Glob", Art = "slag_glob", HpMin = 48, HpMax = 58,
                OnDamaged = (c, e) =>
                {
                    if (e.Get(StatusId.PhaseShift) == 0 && e.Hp * 2 <= e.MaxHp)
                    {
                        e.Set(StatusId.PhaseShift, 1);
                        c.Heal(e, 16);
                        c.GainBlock(e, 10);
                        c.Floater(e, "Reforms", Theme.Block);
                    }
                },
                Choose = (e, c) =>
                {
                    if (c.Player.Block > 0)
                        return new Move { Name = "Smother", Intent = IntentKind.AttackDebuff, Damage = 14, Debuff = StatusId.Unsteady, DebuffAmt = 3 };
                    return Atk("Splash", 16);
                }
            });
            Catalog.AllEnemies.Add(new EnemyDef
            {
                Id = "nailback", Name = "Nailback", Art = "nailback", HpMin = 58, HpMax = 70,
                OnSpawn = e => e.Set(StatusId.Nails, 8),
                Choose = (e, c) =>
                {
                    int step = e.PatternIndex++ % 2;
                    return step == 0
                        ? Def("Brace", 16)
                        : Atk("Pin", 17);
                }
            });
            Catalog.AllEnemies.Add(new EnemyDef
            {
                Id = "forge_brute", Name = "Forge Brute", Art = "forge_brute", HpMin = 130, HpMax = 142,
                OnPlayerPlayed = (c, e, card) =>
                {
                    if (card.Def.Type == CardType.Attack && e.Alive)
                    {
                        c.GainBlock(e, 6);
                        c.Floater(e, "Quenched", Theme.Block);
                    }
                },
                Choose = (e, c) =>
                {
                    int step = e.PatternIndex++ % 3;
                    if (step == 0) return new Move { Name = "Temper", Intent = IntentKind.DefendBuff, Block = 12, Heft = 4 };
                    if (step == 1) return new Move { Name = "Quench", Intent = IntentKind.AttackDebuff, Damage = 12, Debuff = StatusId.Brittle, DebuffAmt = 2 };
                    return Atk("Hammer", 22);
                }
            });
            Catalog.AllEnemies.Add(new EnemyDef
            {
                Id = "cinder_cocoon", Name = "Cinder Cocoon", Art = "cinder_cocoon", HpMin = 170, HpMax = 182,
                OnSpawn = e => { e.Set(StatusId.Ironhide, 12); e.Set(StatusId.PhaseShift, 0); },
                OnDamaged = (c, e) =>
                {
                    if (e.Get(StatusId.PhaseShift) == 0 && e.Hp * 2 <= e.MaxHp)
                    {
                        e.Set(StatusId.PhaseShift, 1);
                        e.Set(StatusId.Ironhide, 0);
                        e.Add(StatusId.Heft, 8);
                        c.Floater(e, "Hatches", Theme.Ember);
                    }
                },
                Choose = (e, c) =>
                {
                    if (e.Get(StatusId.PhaseShift) == 0)
                        return e.PatternIndex++ % 2 == 0 ? Def("Weave", 14) : Atk("Sting", 11);
                    return e.PatternIndex++ % 2 == 0 ? Atk("Rend", 26) : Atk("Frenzy", 13, 2);
                }
            });
            Catalog.AllEnemies.Add(new EnemyDef
            {
                Id = "ashpicker", Name = "Ashpicker", Art = "ashpicker", HpMin = 34, HpMax = 44,
                OnPlayerPlayed = (c, e, card) =>
                {
                    e.PatternIndex++;
                    if (e.PatternIndex % 4 == 0 && c.Hand.Count > 0 && e.Alive)
                    {
                        var stolen = c.Hand[c.Rng.Next(c.Hand.Count)];
                        c.ExhaustCard(stolen);
                        c.Floater(c.Player, "Pickpocket", Theme.Ember);
                    }
                },
                Choose = (e, c) => c.Rng.Next(100) < 40
                    ? new Move { Name = "Filch", Intent = IntentKind.Debuff, Talk = "STEAL", Block = 6 }
                    : Atk("Swipe", 12)
            });
            Catalog.AllEnemies.Add(new EnemyDef
            {
                Id = "ash_warden", Name = "Ash Warden", Art = "ash_warden", HpMin = 380, HpMax = 380,
                Choose = (e, c) =>
                {
                    int cycle = e.PatternIndex++ % 5;
                    return cycle switch
                    {
                        0 => new Move { Name = "Toll", Intent = IntentKind.Debuff, Debuff = StatusId.Dulled, DebuffAmt = 3, Block = 14 },
                        1 => Atk("Ember Volley", 8, 3),
                        2 => new Move { Name = "Warding Bell", Intent = IntentKind.DefendBuff, Block = 22, SelfBuff = StatusId.Nails, SelfBuffAmt = 6 },
                        3 => Atk("Collapse", 20),
                        _ => new Move { Name = "Judgment", Intent = IntentKind.Attack, Damage = 12, Hits = 2, Heft = 3 }
                    };
                }
            });
        }

        static Move Atk(string n, int d, int hits = 1, StatusId deb = default, int da = 0) => new Move
        {
            Name = n, Intent = da > 0 ? IntentKind.AttackDebuff : IntentKind.Attack, Damage = d, Hits = hits, Debuff = deb, DebuffAmt = da
        };
        static Move Def(string n, int b, int str = 0) => new Move
        {
            Name = n, Intent = str > 0 ? IntentKind.DefendBuff : IntentKind.Defend, Block = b, Heft = str
        };
        static Move Buff(string n, int str = 0, StatusId self = default, int sa = 0) => new Move
        {
            Name = n, Intent = IntentKind.Buff, Heft = str, SelfBuff = self, SelfBuffAmt = sa
        };
    }
}
