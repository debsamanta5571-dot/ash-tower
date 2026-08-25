using System;
using System.Collections.Generic;
using UnityEngine;

namespace AshTower
{
    public enum CardType { Attack, Skill, Power, Status, Curse }
    public enum CardRarity { Basic, Common, Uncommon, Rare }
    public enum TargetMode { None, Enemy, AllEnemies, RandomEnemy }
    public enum RoomType { Monster, Elite, Rest, Shop, Treasure, Event, Boss }
    public enum IntentKind { Attack, AttackDebuff, Defend, DefendBuff, Buff, Debuff, Sleep, Stun, Unknown }
    public enum ScreenId { Title, Map, Combat, Rewards, Shop, Rest, Event, Treasure, GameOver, Victory, Picker }
    public enum StatusId
    {
        Brittle, Dulled, Unsteady, Cinderrot,
        Heft, Poise,
        Rite, Hunker, Kindled, Ironhide, InfernalForm, InnerPyre,
        NumbFlesh, DarkKindling, Savagery, CinderBreath, Holdfast,
        Nails, FadingHeft, EmptyHands, DoubleSwing, HexedHands,
        Seal, HoldGuard, PhaseShift, Dormant, Quills, WarEngine, FlameWard
    }

    public class CardDef
    {
        public string Id, Name, Text, UpText;
        public CardType Type = CardType.Attack;
        public CardRarity Rarity = CardRarity.Common;
        public int Cost = 1;
        public int UpCost = -1; // -1 = same as Cost
        public bool Exhaust, Ethereal, Innate, Unplayable, XCost;
        public bool ExhaustOnUpgrade;
        public TargetMode Target = TargetMode.None;
        public string Art = "card_attack";
        public int Dmg, UpDmg = -1, Hits = 1, Block, UpBlock = -1, Draw, UpDraw = -1;
        public Action<CombatState, CardRuntime, Combatant> Extra;

        public string DisplayName(bool up) => up ? Name + "+" : Name;
        public string DisplayText(bool up) => up && !string.IsNullOrEmpty(UpText) ? UpText : Text;
        public int Damage(bool up) => up && UpDmg >= 0 ? UpDmg : Dmg;
        public int BlockAmt(bool up) => up && UpBlock >= 0 ? UpBlock : Block;
        public int DrawAmt(bool up) => up && UpDraw >= 0 ? UpDraw : Draw;
        public int BaseCost(bool up) => up && UpCost >= 0 ? UpCost : Cost;
        public Color TypeColor => Type switch
        {
            CardType.Attack => new Color(0.72f, 0.22f, 0.16f),
            CardType.Skill => new Color(0.18f, 0.46f, 0.32f),
            CardType.Power => new Color(0.45f, 0.28f, 0.72f),
            CardType.Status => new Color(0.35f, 0.33f, 0.30f),
            _ => new Color(0.22f, 0.16f, 0.18f)
        };
    }

    public class CardRuntime
    {
        public CardDef Def;
        public bool Upgraded;
        public int CostMod;
        public bool FreeThisTurn;
        public bool RetainThisTurn;
        public string Uid = Guid.NewGuid().ToString("N");

        public int GetCost(CombatState c)
        {
            if (Unplayable) return 0;
            if (FreeThisTurn) return 0;
            if (Def.XCost) return Mathf.Max(0, c.Energy);
            if (c.LastPlayed == null && c.Run.HasRelic("first_cut")) return 0;
            int cost = Def.BaseCost(Upgraded) + CostMod;
            if (Def.Type == CardType.Skill && c.Player.Get(StatusId.HexedHands) > 0) return 0;
            return Mathf.Max(0, cost);
        }

        public bool Unplayable => Def.Unplayable;
        public bool Exhausts(CombatState c)
        {
            if (Def.Type == CardType.Skill && c.Player.Get(StatusId.HexedHands) > 0) return true;
            if (Upgraded && Def.Id == "limit_shatter") return false;
            return Def.Exhaust || (Upgraded && Def.ExhaustOnUpgrade);
        }
    }

    public class Combatant
    {
        public string Name;
        public bool IsPlayer;
        public int Hp, MaxHp, Block;
        public string ArtKey;
        public EnemyDef Def;
        public Move CurrentMove;
        public int PatternIndex;
        public bool GainedBlockThisTurn;
        public Dictionary<StatusId, int> St = new Dictionary<StatusId, int>();
        public string Uid = Guid.NewGuid().ToString("N");

        public bool Alive => Hp > 0;
        public int Get(StatusId s) => St.TryGetValue(s, out var v) ? v : 0;

        public void Add(StatusId s, int n)
        {
            if (n == 0) return;
            int v = Get(s) + n;
            if (v <= 0) St.Remove(s);
            else St[s] = v;
        }

        public void Set(StatusId s, int n)
        {
            if (n <= 0) St.Remove(s);
            else St[s] = n;
        }
    }

    public class Move
    {
        public string Name;
        public IntentKind Intent = IntentKind.Attack;
        public int Damage, Hits = 1, Block, Heft;
        public StatusId Debuff;
        public int DebuffAmt;
        public StatusId SelfBuff;
        public int SelfBuffAmt;
        public bool Sleep;
        public string Talk;
    }

    public class EnemyDef
    {
        public string Id, Name, Art;
        public int HpMin, HpMax;
        public Action<Combatant> OnSpawn;
        public Func<Combatant, CombatState, Move> Choose;
        public Action<CombatState, Combatant, CardRuntime> OnPlayerPlayed;
        public Action<CombatState, Combatant> OnDamaged;
    }

    public class RelicDef
    {
        public string Id, Name, Desc;
        public CardRarity Rarity = CardRarity.Common;
        public int Price = 160;
        public string Art = "relic";
        public Action<CombatState> CombatStart;
        public Action<CombatState> TurnStart;
        public Action<CombatState> TurnEnd;
        public Action<CombatState, CardRuntime> OnPlay;
        public Action<CombatState, int> OnHpLoss;
        public Action<CombatState> OnShuffle;
        public Action<RunState, bool> AfterCombat; // elite?
        public Action<RunState> OnPickup;
    }

    public class PotionDef
    {
        public string Id, Name, Desc;
        public Action<CombatState> Use;
    }

    public class Encounter
    {
        public string Id;
        public List<string> EnemyIds = new List<string>();
        public bool Elite, Boss;
    }

    public class MapNode
    {
        public int Id, Row, Col;
        public RoomType Type;
        public List<int> Next = new List<int>();
        public bool Seen;
    }

    public class GameEvent
    {
        public string Id, Title, Body;
        public List<EventOption> Options = new List<EventOption>();
    }

    public class EventOption
    {
        public string Label;
        public string Result;
        public Action<RunState, AshTowerApp> Apply;
        public bool Leaves;
    }

    public class ShopOffer
    {
        public CardRuntime Card;
        public RelicDef Relic;
        public PotionDef Potion;
        public int Price;
        public bool Sold;
    }

}
