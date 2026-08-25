using UnityEngine;
using UnityEngine.UI;

namespace AshTower
{
    public class EnemyView : MonoBehaviour
    {
        public Text nameLabel;
        public Image art;
        public Image intentIcon;
        public Text intentLabel;
        public GameObject intentRow;
        public Transform hpRoot;
        public Transform statusRoot;
        public Text deadLabel;
        public Image targetRing;
        public Button artButton;

        Combatant _enemy;

        public Combatant Enemy => _enemy;

        public void Bind(Combatant e, CombatState combat, System.Action<Combatant> clicked, bool showTarget)
        {
            _enemy = e;
            if (nameLabel != null) nameLabel.text = e.Name;

            if (art != null)
            {
                art.sprite = Art.Enemy(e.ArtKey);
                art.color = e.Alive ? Color.white : new Color(0.3f, 0.3f, 0.3f, 0.5f);
                art.preserveAspect = true;
                art.raycastTarget = e.Alive;
            }

            if (targetRing != null)
            {
                targetRing.sprite = Theme.Circle;
                targetRing.color = new Color(Theme.Gold.r, Theme.Gold.g, Theme.Gold.b, 0.22f);
            }

            if (intentRow != null)
                intentRow.SetActive(e.Alive && e.CurrentMove != null);

            if (e.Alive && e.CurrentMove != null)
            {
                if (intentIcon != null)
                {
                    intentIcon.sprite = Art.Intent(e.CurrentMove.Intent);
                    intentIcon.preserveAspect = true;
                }
                if (intentLabel != null)
                    intentLabel.text = IntentText(e, combat);
                var tip = intentRow != null ? intentRow.GetComponent<TooltipHover>() : null;
                if (tip == null && intentRow != null) tip = intentRow.AddComponent<TooltipHover>();
                if (tip != null)
                {
                    tip.Title = e.CurrentMove.Name;
                    tip.Body = IntentBody(e, combat);
                }
            }

            if (hpRoot != null)
            {
                UI.Clear(hpRoot);
                UI.HpBar(hpRoot, e, 190, 16, new Vector2(0.5f, 0.5f), Vector2.zero);
            }

            if (statusRoot != null)
            {
                UI.Clear(statusRoot);
                StatusChip.LayoutRow(statusRoot, e, Vector2.zero, 220f);
            }

            if (deadLabel != null)
                deadLabel.gameObject.SetActive(!e.Alive);

            if (targetRing != null)
                targetRing.gameObject.SetActive(showTarget && e.Alive);

            if (artButton != null)
            {
                artButton.enabled = e.Alive;
                artButton.onClick.RemoveAllListeners();
                if (e.Alive && clicked != null)
                {
                    var captured = e;
                    artButton.onClick.AddListener(() => clicked(captured));
                }
            }
        }

        static string IntentText(Combatant e, CombatState c)
        {
            return StatusUtil.IntentShort(e.CurrentMove, c.IntentDamage(e));
        }

        static string IntentBody(Combatant e, CombatState c)
        {
            var tip = StatusUtil.IntentTip(e.CurrentMove, c.IntentDamage(e));
            var m = e.CurrentMove;
            if (m != null && (m.Intent == IntentKind.Attack || m.Intent == IntentKind.AttackDebuff))
            {
                if (c.Player.Get(StatusId.Brittle) > 0) tip += " Hits through Block.";
                if (e.Get(StatusId.Dulled) > 0) tip += " Recoil " + e.Get(StatusId.Dulled) + ".";
            }
            if (e.Get(StatusId.Heft) > 0) tip += " Then Heft burns you for " + e.Get(StatusId.Heft) + ".";
            return tip;
        }
    }
}
