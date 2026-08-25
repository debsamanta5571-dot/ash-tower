using UnityEngine;
using UnityEngine.UI;

namespace AshTower
{
    public static class ScreenChrome
    {
        public static Image Background(Transform root)
        {
            var existing = root.Find("Background");
            if (existing != null)
            {
                var found = existing.GetComponent<Image>();
                if (found != null && found.sprite == null) found.sprite = Theme.White;
                return found;
            }
            var img = UI.Img(root, "Background", new Color(0.08f, 0.08f, 0.09f, 1f));
            UI.Stretch(img.transform);
            img.transform.SetAsFirstSibling();
            return img;
        }

        public static RectTransform Panel(Transform root, string title, float w, float h, Vector2? pos = null)
        {
            var existing = root.Find("Panel") as RectTransform;
            if (existing != null)
            {
                var bg = existing.GetComponent<Image>();
                if (bg != null && bg.sprite == null) bg.sprite = Theme.White;
                SetTitle(existing, title);
                return existing;
            }

            var panel = UI.Panel(root, "Panel", w, h, new Vector2(0.5f, 0.5f), pos ?? Vector2.zero);
            var header = UI.Txt(panel, "Title", title, 26, Theme.Gold, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UI.Place(header.transform, 760, 34, new Vector2(0.5f, 1), new Vector2(0, -14), new Vector2(0.5f, 1));
            UI.Outline(header, Theme.Ink);
            return panel;
        }

        public static void SetTitle(Transform panel, string title)
        {
            var header = panel.Find("Title")?.GetComponent<Text>();
            if (header != null) header.text = title;
        }

        public static Transform Content(Transform panel)
        {
            var c = panel.Find("Content");
            if (c == null)
            {
                var g = UI.Go("Content", panel);
                UI.Stretch(g.transform, 8);
                c = g.transform;
            }
            UI.Clear(c);
            return c;
        }

        public static Transform Find(Transform root, params string[] names)
        {
            foreach (var name in names)
            {
                var t = root.Find(name);
                if (t != null) return t;
            }
            return null;
        }
    }
}
