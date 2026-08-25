using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AshTower
{
    public class TooltipHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string Title, Body;
        GameObject _tip;

        public void OnPointerEnter(PointerEventData e)
        {
            Show(e.position);
        }

        public void OnPointerExit(PointerEventData e) => DestroyTip();
        void OnDisable() => DestroyTip();

        void Show(Vector2 screen)
        {
            DestroyTip();
            if (string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Body)) return;
            var parent = AshTowerApp.I != null && AshTowerApp.I.Overlay != null
                ? AshTowerApp.I.Overlay
                : UI.Root;
            if (parent == null) return;
            parent.SetAsLastSibling();

            int lines = 1;
            if (!string.IsNullOrEmpty(Body))
                lines = Mathf.Max(1, (Body.Length + 36) / 37);
            float bodyH = Mathf.Clamp(lines * 18f + 8f, 40f, 140f);
            float h = 40f + bodyH + 12f;

            var panel = UI.Panel(parent, "Tooltip", 300, h, new Vector2(0.5f, 0.5f), Vector2.zero, 0.96f, false);
            _tip = panel.gameObject;
            var t = UI.Txt(_tip.transform, "Title", Title ?? "", 16, Theme.Gold, TextAnchor.UpperLeft, FontStyle.Bold, false);
            UI.Place(t.transform, 270, 24, new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(0.5f, 1));
            var b = UI.Txt(_tip.transform, "Body", Body ?? "", 14, Theme.Cream, TextAnchor.UpperLeft, FontStyle.Normal, true);
            UI.Place(b.transform, 270, bodyH, new Vector2(0.5f, 1), new Vector2(0, -36), new Vector2(0.5f, 1));

            var parentRt = parent as RectTransform ?? UI.Root;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, screen, null, out var lp);
            var pos = lp + new Vector2(20, -90);
            float hw = 150f, hh = h * 0.5f;
            var half = parentRt.rect.size * 0.5f;
            pos.x = Mathf.Clamp(pos.x, -half.x + hw + 8, half.x - hw - 8);
            pos.y = Mathf.Clamp(pos.y, -half.y + hh + 8, half.y - hh - 8);
            panel.anchoredPosition = pos;
        }

        void DestroyTip()
        {
            if (_tip == null) return;
            Destroy(_tip);
            _tip = null;
        }
    }
}
