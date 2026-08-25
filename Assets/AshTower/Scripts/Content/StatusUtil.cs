using UnityEngine;

namespace AshTower
{
    public static class StatusUtil
    {
        public static bool DecaysAtTurnEnd(StatusId s) =>
            s == StatusId.Brittle || s == StatusId.Dulled || s == StatusId.Unsteady ||
            s == StatusId.EmptyHands || s == StatusId.DoubleSwing || s == StatusId.HoldGuard ||
            s == StatusId.FadingHeft || s == StatusId.FlameWard;

        public static string Label(StatusId s) => s switch
        {
            StatusId.Brittle => "Brittle",
            StatusId.Dulled => "Dulled",
            StatusId.Unsteady => "Unsteady",
            StatusId.Cinderrot => "Cinderrot",
            StatusId.Heft => "Heft",
            StatusId.Poise => "Poise",
            StatusId.Rite => "Rite",
            StatusId.Hunker => "Hunker",
            StatusId.Kindled => "Kindled",
            StatusId.Ironhide => "Ironhide",
            StatusId.InfernalForm => "Infernal Form",
            StatusId.InnerPyre => "Inner Pyre",
            StatusId.NumbFlesh => "Numb Flesh",
            StatusId.DarkKindling => "Dark Kindling",
            StatusId.Savagery => "Savagery",
            StatusId.CinderBreath => "Cinder Breath",
            StatusId.Holdfast => "Holdfast",
            StatusId.Nails => "Nails",
            StatusId.Seal => "Seal",
            StatusId.Dormant => "Dormant",
            StatusId.FlameWard => "Flame Ward",
            StatusId.WarEngine => "War Engine",
            StatusId.DoubleSwing => "Double Swing",
            StatusId.HexedHands => "Hexed Hands",
            StatusId.EmptyHands => "Empty Hands",
            StatusId.FadingHeft => "Fading Heft",
            StatusId.Quills => "Quills",
            StatusId.HoldGuard => "Hold Guard",
            StatusId.PhaseShift => "Phase Shift",
            _ => Spaced(s.ToString())
        };

        static string Spaced(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return raw;
            var chars = new System.Text.StringBuilder();
            for (int i = 0; i < raw.Length; i++)
            {
                if (i > 0 && char.IsUpper(raw[i]) && !char.IsUpper(raw[i - 1]))
                    chars.Append(' ');
                chars.Append(raw[i]);
            }
            return chars.ToString();
        }

        public static string Describe(StatusId s) => s switch
        {
            StatusId.Brittle => "The next Attack against them ignores Block, then Brittle drops by 1.",
            StatusId.Dulled => "When they Attack, they take this much damage, then Dulled drops by 1.",
            StatusId.Unsteady => "If they end their turn with no Block, they lose this much HP.",
            StatusId.Cinderrot => "At turn start they lose this much HP and cannot gain Block. Then 1 stack jumps to another enemy if there is one, and this drops by 1.",
            StatusId.Heft => "At the end of their turn, they deal this much to everyone they are fighting, then Heft drops by 1.",
            StatusId.Poise => "They keep up to this much Block between turns.",
            StatusId.Rite => "At the start of their turn, they gain this much Heft.",
            StatusId.Hunker => "The first time they lose HP, they gain this much Block.",
            StatusId.Kindled => "When you play a Skill, they gain Heft.",
            StatusId.Ironhide => "If they have no Block at the end of their turn, they gain this much Block.",
            StatusId.InfernalForm => "At the start of your turn, gain this much Heft.",
            StatusId.InnerPyre => "At the start of your turn, lose 2 HP and deal this much to all enemies.",
            StatusId.NumbFlesh => "When a card is Exhausted, gain this much Block.",
            StatusId.DarkKindling => "When a card is Exhausted, draw this many cards.",
            StatusId.Savagery => "At the start of your turn, lose this much HP and draw that many cards.",
            StatusId.CinderBreath => "When you draw a Status or Curse, deal this much to all enemies.",
            StatusId.Holdfast => "The next time Block would empty at turn start, keep it, then Holdfast drops by 1.",
            StatusId.Nails => "When an Attack chips their Block, the attacker takes this much damage. Their Block still applies.",
            StatusId.FlameWard => "This turn, anyone who hits them for HP loses this much HP.",
            StatusId.Seal => "The next time they would lose HP, they don't, then Seal drops by 1.",
            StatusId.Dormant => "They skip their action until this runs out.",
            StatusId.WarEngine => "When you gain Block, deal this much to a random enemy.",
            StatusId.DoubleSwing => "Your next Attack this turn plays twice.",
            StatusId.HexedHands => "Skills cost 0 and Exhaust when played.",
            StatusId.EmptyHands => "You can't draw more cards this turn.",
            StatusId.FadingHeft => "At the end of the turn, lose this much Heft (after Heft burns).",
            StatusId.Quills => "Anyone who hits them for HP loses this much HP.",
            StatusId.HoldGuard => "They keep their Block through the next turn start.",
            StatusId.PhaseShift => "This enemy is about to change how it fights.",
            _ => "A combat effect."
        };

        public static string IntentShort(Move m, int dmg)
        {
            if (m == null) return "";
            return m.Intent switch
            {
                IntentKind.Attack => m.Hits > 1 ? $"{dmg} x {m.Hits}" : dmg.ToString(),
                IntentKind.AttackDebuff => AttackDebuffShort(m, dmg),
                IntentKind.Defend or IntentKind.DefendBuff => "Block " + m.Block,
                IntentKind.Sleep => "zzz",
                IntentKind.Debuff => m.DebuffAmt > 0 ? Label(m.Debuff) : m.Name,
                _ => m.Name
            };
        }

        public static string IntentTip(Move m, int dmg)
        {
            if (m == null) return "";
            return m.Intent switch
            {
                IntentKind.Attack => m.Hits > 1 ? $"This enemy will hit {m.Hits} times for {dmg} each." : $"This enemy will hit you for {dmg}.",
                IntentKind.AttackDebuff => m.DebuffAmt > 0
                    ? $"This enemy will hit you for {dmg} and apply {Label(m.Debuff)}."
                    : $"This enemy will hit you for {dmg} and apply a debuff.",
                IntentKind.Defend => $"This enemy is going to gain {m.Block} Block.",
                IntentKind.DefendBuff => "This enemy is going to block and buff itself.",
                IntentKind.Buff => "This enemy is buffing itself.",
                IntentKind.Debuff => m.DebuffAmt > 0 ? $"This enemy is going to apply {Label(m.Debuff)}." : "This enemy is going to apply a debuff.",
                IntentKind.Sleep => "This enemy is sitting this turn out.",
                _ => m.Name
            };
        }

        static string AttackDebuffShort(Move m, int dmg)
        {
            string hit = m.Hits > 1 ? $"{dmg} x {m.Hits}" : dmg.ToString();
            if (m.DebuffAmt > 0) return hit + "  " + Label(m.Debuff);
            return hit;
        }

        public static Color ColorOf(StatusId s) => s switch
        {
            StatusId.Brittle => new Color(0.85f, 0.55f, 0.15f),
            StatusId.Dulled => new Color(0.55f, 0.72f, 0.35f),
            StatusId.Unsteady => new Color(0.55f, 0.45f, 0.75f),
            StatusId.Cinderrot => new Color(0.35f, 0.75f, 0.35f),
            StatusId.Heft => new Color(0.85f, 0.25f, 0.2f),
            StatusId.Poise => new Color(0.35f, 0.55f, 0.9f),
            _ => new Color(0.8f, 0.72f, 0.5f)
        };
    }
}
