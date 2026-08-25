using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AshTower
{
    public static class UI
    {
        public static Canvas Canvas;
        public static RectTransform Root;

        public static GameObject Go(string name, Transform parent)
        {
            var g = new GameObject(name, typeof(RectTransform));
            g.transform.SetParent(parent, false);
            return g;
        }

        public static RectTransform RT(Component c) => c.transform as RectTransform;
        public static RectTransform RT(GameObject g) => g.GetComponent<RectTransform>();

        public static RectTransform Stretch(Transform t, float pad = 0)
        {
            var r = t as RectTransform;
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(pad, pad);
            r.offsetMax = new Vector2(-pad, -pad);
            return r;
        }

        public static RectTransform Place(Transform t, float w, float h, Vector2 anchor, Vector2 pos, Vector2? pivot = null)
        {
            var r = t as RectTransform;
            r.anchorMin = r.anchorMax = anchor;
            r.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(w, h);
            r.anchoredPosition = pos;
            return r;
        }

        public static Image Img(Transform p, string n, Color col, Sprite sp = null, bool raycast = false)
        {
            if (Theme.White == null || Theme.Circle == null) Theme.Init();
            var g = Go(n, p);
            var i = g.AddComponent<Image>();
            i.sprite = sp != null ? sp : Theme.White;
            i.color = col;
            i.raycastTarget = raycast;
            i.preserveAspect = false;
            return i;
        }

        public static RawImage Raw(Transform p, string n, Texture tex, bool raycast = false)
        {
            var g = Go(n, p);
            var i = g.AddComponent<RawImage>();
            i.texture = tex;
            i.color = Color.white;
            i.raycastTarget = raycast;
            return i;
        }

        public static Text Txt(Transform p, string n, string text, int size, Color col, TextAnchor align, FontStyle style = FontStyle.Normal, bool wrap = true)
        {
            if (Theme.Font == null) Theme.Init();
            var g = Go(n, p);
            var t = g.AddComponent<Text>();
            t.font = size >= 40 ? Theme.FontTitle : Theme.Font;
            t.fontSize = size;
            t.color = col;
            t.alignment = align;
            t.fontStyle = style;
            t.text = text;
            t.raycastTarget = false;
            t.horizontalOverflow = wrap ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
            t.verticalOverflow = wrap ? VerticalWrapMode.Truncate : VerticalWrapMode.Truncate;
            t.lineSpacing = 1f;
            t.alignByGeometry = true;
            t.supportRichText = true;
            return t;
        }

        public static void Outline(Text t, Color c, float d = 1.2f)
        {
            var o = t.gameObject.AddComponent<Outline>();
            o.effectColor = c;
            o.effectDistance = new Vector2(d, -d);
        }

        public static void Shadow(Text t)
        {
            var s = t.gameObject.AddComponent<Shadow>();
            s.effectColor = Theme.Shadow;
            s.effectDistance = new Vector2(2, -2);
        }

        public static RectTransform Panel(Transform p, string n, float w, float h, Vector2 anchor, Vector2 pos, float alpha = 0.92f, bool raycast = true)
        {
            if (Theme.White == null) Theme.Init();
            var g = Go(n, p);
            Place(g.transform, w, h, anchor, pos);
            var bg = g.AddComponent<Image>();
            bg.sprite = Theme.White;
            bg.color = new Color(Theme.PanelSolid.r, Theme.PanelSolid.g, Theme.PanelSolid.b, alpha);
            bg.raycastTarget = raycast;
            Border(g.transform, Theme.Gold, 2f);
            return RT(g);
        }

        public static void Border(Transform p, Color c, float th)
        {
            Edge("Top", p, c, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -th), Vector2.zero);
            Edge("Bottom", p, c, new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, th));
            Edge("Left", p, c, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(th, 0));
            Edge("Right", p, c, new Vector2(1, 0), new Vector2(1, 1), new Vector2(-th, 0), Vector2.zero);
        }

        static void Edge(string n, Transform p, Color c, Vector2 amin, Vector2 amax, Vector2 omin, Vector2 omax)
        {
            var i = Img(p, n, c);
            var r = i.rectTransform;
            r.anchorMin = amin; r.anchorMax = amax;
            r.offsetMin = omin; r.offsetMax = omax;
        }

        public static Button Btn(Transform p, string n, string label, float w, float h, Vector2 anchor, Vector2 pos, Action click, int font = 22)
        {
            if (Theme.White == null) Theme.Init();
            var g = Go(n, p);
            Place(g.transform, w, h, anchor, pos);
            var img = g.AddComponent<Image>();
            img.sprite = Theme.White;
            img.color = Theme.PanelHi;
            img.raycastTarget = true;
            var b = g.AddComponent<Button>();
            var cols = b.colors;
            cols.normalColor = Theme.PanelHi;
            cols.highlightedColor = new Color(0.32f, 0.28f, 0.22f, 1f);
            cols.pressedColor = new Color(0.12f, 0.10f, 0.08f, 1f);
            cols.fadeDuration = 0.08f;
            b.colors = cols;
            b.onClick.AddListener(() =>
            {
                Sfx.Ui();
                if (UnityEngine.EventSystems.EventSystem.current != null)
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                click?.Invoke();
            });
            var tx = Txt(g.transform, "Label", label, font, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            Stretch(tx.transform);
            Border(g.transform, new Color(1f, 1f, 1f, 0.25f), 1f);
            return b;
        }

        public static Image Bar(Transform p, string n, Color fill, float w, float h, Vector2 anchor, Vector2 pos)
        {
            if (Theme.White == null) Theme.Init();
            var g = Go(n, p);
            Place(g.transform, w, h, anchor, pos);
            var bg = g.AddComponent<Image>();
            bg.sprite = Theme.White;
            bg.color = new Color(0, 0, 0, 0.7f);
            Border(g.transform, Theme.GoldDim, 1f);
            var f = Img(g.transform, "Fill", fill);
            var fr = f.rectTransform;
            fr.anchorMin = Vector2.zero;
            fr.anchorMax = new Vector2(1, 1);
            fr.offsetMin = new Vector2(2, 2);
            fr.offsetMax = new Vector2(-2, -2);
            f.type = Image.Type.Filled;
            f.fillMethod = Image.FillMethod.Horizontal;
            f.fillOrigin = 0;
            f.fillAmount = 1;
            return f;
        }

        public static void Line(Transform p, Vector2 a, Vector2 b, float w, Color c)
        {
            var i = Img(p, "Line", c);
            var r = i.rectTransform;
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0);
            r.pivot = new Vector2(0.5f, 0.5f);
            var d = b - a;
            r.sizeDelta = new Vector2(d.magnitude, w);
            r.anchoredPosition = (a + b) * 0.5f;
            r.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
        }

        public static ScrollRect Scroll(Transform p, string n, float w, float h, Vector2 anchor, Vector2 pos, out RectTransform content)
        {
            var g = Go(n, p);
            Place(g.transform, w, h, anchor, pos);
            var img = g.AddComponent<Image>();
            img.sprite = Theme.White;
            img.color = new Color(0, 0, 0, 0.25f);
            img.raycastTarget = true;
            var mask = g.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            var sr = g.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 40;
            var cgo = Go("Content", g.transform);
            content = RT(cgo);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0, h);
            sr.content = content;
            sr.viewport = RT(g);
            return sr;
        }

        public static void Clear(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
        }

        public static void FixFonts(Transform root) => Restore(root);

        public static bool Broken(Transform root)
        {
            if (root == null || root.childCount == 0) return true;
            if (Theme.White == null || Theme.Circle == null || Theme.Font == null) Theme.Init();
            var imgs = root.GetComponentsInChildren<Image>(true);
            if (imgs.Length == 0) return true;
            for (int i = 0; i < imgs.Length; i++)
            {
                if (imgs[i] == null) continue;
                if (imgs[i].sprite == null || imgs[i].sprite.texture == null) return true;
            }
            var texts = root.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
                if (texts[i] != null && texts[i].font == null) return true;
            return false;
        }

        public static void Wipe(Transform t)
        {
            if (t == null) return;
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                var g = t.GetChild(i).gameObject;
                g.transform.SetParent(null, false);
                UnityEngine.Object.Destroy(g);
            }
        }

        public static void Ensure(Transform root, Action build)
        {
            if (root == null) return;
            bool ready = root.GetComponent<RuntimeChrome>() != null;
            if (!ready || Broken(root))
            {
                Wipe(root);
                build?.Invoke();
                if (root.GetComponent<RuntimeChrome>() == null)
                    root.gameObject.AddComponent<RuntimeChrome>();
            }
            Restore(root);
        }

        public static void Restore(Transform root)
        {
            if (root == null) return;
            if (Theme.White == null || Theme.Circle == null || Theme.Font == null) Theme.Init();
            var imgs = root.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < imgs.Length; i++)
            {
                var img = imgs[i];
                if (img == null) continue;
                bool circle = WantsCircle(img);
                if (img.sprite == null || img.sprite.texture == null)
                    img.sprite = circle ? Theme.Circle : Theme.White;
                else if (circle && img.sprite != Theme.Circle && !img.preserveAspect)
                    img.sprite = Theme.Circle;
            }
            var texts = root.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                var t = texts[i];
                if (t == null || t.font != null) continue;
                t.font = t.fontSize >= 40 ? Theme.FontTitle : Theme.Font;
            }
        }

        static bool WantsCircle(Image img)
        {
            if (img.preserveAspect) return false;
            switch (img.gameObject.name)
            {
                case "Glow":
                case "Gem":
                case "Art":
                case "Rare":
                case "EnergyOrb":
                case "PlayerGlow":
                case "Disc":
                case "BlockIcon":
                case "TargetRing":
                    return true;
            }
            if (img.gameObject.name.StartsWith("Slot")) return true;
            return img.GetComponent<MapNodeView>() != null;
        }

        public static void HpBar(Transform p, Combatant c, float w, float h, Vector2 anchor, Vector2 pos)
        {
            var fill = Bar(p, "Bar", Theme.Hp, w, h, anchor, pos);
            fill.fillAmount = c.MaxHp <= 0 ? 0 : (float)c.Hp / c.MaxHp;
            var label = Txt(p, "Label", c.Block > 0 ? $"{c.Hp}/{c.MaxHp}  +{c.Block}" : $"{c.Hp}/{c.MaxHp}", 16, Theme.Cream, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            Place(label.transform, w, h, anchor, pos);
            Outline(label, Theme.Ink);
            if (c.Block > 0)
            {
                var sh = Img(p, "BlockIcon", new Color(Theme.Block.r, Theme.Block.g, Theme.Block.b, 0.85f), Theme.Circle, false);
                Place(sh.transform, 28, 28, anchor, pos + new Vector2(-w * 0.5f - 6, 0));
                var bt = Txt(p, "BlockLabel", c.Block.ToString(), 13, Theme.Ink, TextAnchor.MiddleCenter, FontStyle.Bold, false);
                Place(bt.transform, 28, 28, anchor, pos + new Vector2(-w * 0.5f - 6, 0));
            }
        }
    }

    // Marks a screen whose layout was built at runtime.
    public class RuntimeChrome : MonoBehaviour { }
}
