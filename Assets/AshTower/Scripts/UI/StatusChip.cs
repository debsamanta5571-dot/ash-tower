using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AshTower
{
    public class StatusChip : MonoBehaviour
    {
        public Image background;
        public Text label;
        TooltipHover _tip;

        public void Bind(StatusId id, int amount)
        {
            if (background == null) background = GetComponent<Image>();
            if (label == null) label = GetComponentInChildren<Text>(true);
            var col = StatusUtil.ColorOf(id);
            if (background != null)
            {
                if (background.sprite == null) background.sprite = Theme.White;
                background.color = new Color(col.r, col.g, col.b, 0.28f);
                background.raycastTarget = true;
            }
            string name = StatusUtil.Label(id);
            if (label != null)
            {
                if (label.font == null && Theme.Font != null) label.font = Theme.Font;
                label.text = name + " " + amount;
                label.color = col;
                label.raycastTarget = false;
                label.horizontalOverflow = HorizontalWrapMode.Overflow;
            }
            _tip = GetComponent<TooltipHover>();
            if (_tip == null) _tip = gameObject.AddComponent<TooltipHover>();
            _tip.Title = name;
            _tip.Body = StatusUtil.Describe(id);
        }

        public float FitWidth()
        {
            float w = 96f;
            if (label != null)
                w = Mathf.Clamp(label.preferredWidth + 20f, 78f, 170f);
            var rt = transform as RectTransform;
            rt.sizeDelta = new Vector2(w, 22f);
            if (label != null) UI.Stretch(label.transform, 2);
            return w;
        }

        public static StatusChip Create(Transform parent, StatusId id, int amount, Vector2 pos)
        {
            var chip = Spawn(parent, id, amount);
            float w = chip.FitWidth();
            UI.Place(chip.transform, w, 22, new Vector2(0.5f, 0), pos);
            return chip;
        }

        public static void LayoutRow(Transform parent, Combatant c, Vector2 origin, float maxWidth)
        {
            if (parent == null || c == null) return;
            var chips = new List<StatusChip>();
            foreach (var kv in c.St)
            {
                if (kv.Value == 0) continue;
                chips.Add(Spawn(parent, kv.Key, kv.Value));
                if (chips.Count >= 8) break;
            }
            if (chips.Count == 0) return;

            const float gap = 8f;
            const float rowH = 26f;
            var rows = new List<List<StatusChip>>();
            var row = new List<StatusChip>();
            float used = 0f;
            for (int i = 0; i < chips.Count; i++)
            {
                float w = chips[i].FitWidth();
                if (row.Count > 0 && used + gap + w > maxWidth)
                {
                    rows.Add(row);
                    row = new List<StatusChip>();
                    used = 0f;
                }
                if (row.Count > 0) used += gap;
                used += w;
                row.Add(chips[i]);
            }
            if (row.Count > 0) rows.Add(row);

            float y = origin.y;
            for (int r = 0; r < rows.Count; r++)
            {
                float total = 0f;
                for (int i = 0; i < rows[r].Count; i++)
                {
                    if (i > 0) total += gap;
                    total += (rows[r][i].transform as RectTransform).sizeDelta.x;
                }
                float x = origin.x - total * 0.5f;
                for (int i = 0; i < rows[r].Count; i++)
                {
                    var rt = rows[r][i].transform as RectTransform;
                    float w = rt.sizeDelta.x;
                    UI.Place(rt, w, 22, new Vector2(0.5f, 0), new Vector2(x + w * 0.5f, y));
                    x += w + gap;
                }
                y -= rowH;
            }
        }

        static StatusChip Spawn(Transform parent, StatusId id, int amount)
        {
            StatusChip chip;
            if (UiLibrary.HasChip)
            {
                chip = Instantiate(UiLibrary.I.statusChip, parent);
                UI.Restore(chip.transform);
            }
            else
            {
                var g = UI.Go("chip", parent);
                chip = g.AddComponent<StatusChip>();
                var bg = g.AddComponent<Image>();
                bg.sprite = Theme.White;
                bg.raycastTarget = true;
                chip.background = bg;
                var t = UI.Txt(g.transform, "Label", "", 12, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold, false);
                UI.Stretch(t.transform);
                t.raycastTarget = false;
                chip.label = t;
            }
            chip.Bind(id, amount);
            return chip;
        }
    }
}
