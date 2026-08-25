using UnityEngine;

namespace AshTower
{
    public static class Theme
    {
        public static readonly Color Ink = Hex("0d0a08");
        public static readonly Color Panel = Hex("1a1410e8");
        public static readonly Color PanelSolid = Hex("1c1612");
        public static readonly Color PanelHi = Hex("2a221c");
        public static readonly Color Gold = Hex("d4b45a");
        public static readonly Color GoldDim = Hex("8a7030");
        public static readonly Color Ember = Hex("e85d04");
        public static readonly Color EmberHi = Hex("ff9f1c");
        public static readonly Color Cream = Hex("f4e8d0");
        public static readonly Color CreamDim = Hex("cbbfa6");
        public static readonly Color Blood = Hex("8b1e1e");
        public static readonly Color Hp = Hex("c0392b");
        public static readonly Color Block = Hex("5dade2");
        public static readonly Color Energy = Hex("f4d03f");
        public static readonly Color Skill = Hex("2e8b57");
        public static readonly Color Power = Hex("7d3c98");
        public static readonly Color Attack = Hex("a93226");
        public static readonly Color Rare = Hex("f1c40f");
        public static readonly Color Uncommon = Hex("3498db");
        public static readonly Color Common = Hex("bdc3c7");
        public static readonly Color Overlay = Hex("000000cc");
        public static readonly Color MapPath = Hex("6b5428");
        public static readonly Color Shadow = Hex("00000099");

        public static Font Font;
        public static Font FontTitle;
        public static Sprite White;
        public static Sprite Circle;

        public static void Init()
        {
            if (White != null && Circle != null && Font != null) return;
            Font = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial", "Tahoma" }, 96);
            FontTitle = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial", "Tahoma" }, 128);
            if (Font == null) Font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (FontTitle == null) FontTitle = Font;
            SharpenFont(Font);
            SharpenFont(FontTitle);

            var t = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var c = new Color[16];
            for (int i = 0; i < 16; i++) c[i] = Color.white;
            t.SetPixels(c); t.Apply(); t.wrapMode = TextureWrapMode.Clamp; t.filterMode = FilterMode.Point;
            White = Sprite.Create(t, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f, 0, SpriteMeshType.FullRect);

            int s = 128;
            var ct = new Texture2D(s, s, TextureFormat.RGBA32, false);
            var px = new Color[s * s];
            float mid = (s - 1) * 0.5f;
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(mid, mid)) / (mid);
                    px[y * s + x] = d <= 1f ? new Color(1, 1, 1, Mathf.Clamp01(1.15f - d)) : Color.clear;
                }
            ct.SetPixels(px); ct.Apply(); ct.filterMode = FilterMode.Bilinear;
            Circle = Sprite.Create(ct, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 128f);
        }

        static void SharpenFont(Font font)
        {
            if (font == null || font.material == null) return;
            var tex = font.material.mainTexture;
            if (tex == null) return;
            tex.filterMode = FilterMode.Bilinear;
            tex.anisoLevel = 0;
            tex.mipMapBias = -2f;
        }

        public static Color Hex(string h)
        {
            if (h.Length == 6) h += "ff";
            byte r = byte.Parse(h.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(h.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(h.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte a = byte.Parse(h.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color32(r, g, b, a);
        }

        public static Color RarityColor(CardRarity r) => r switch
        {
            CardRarity.Rare => Rare,
            CardRarity.Uncommon => Uncommon,
            CardRarity.Basic => Gold,
            _ => Common
        };
    }
}
