using System.Collections.Generic;
using UnityEngine;

namespace AshTower
{
    public static class Art
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        static readonly Dictionary<string, Texture2D> Tex = new Dictionary<string, Texture2D>();

        public static Sprite Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return Theme.White;
            if (Cache.TryGetValue(key, out var s) && s != null) return s;
            var tex = Tex2D(key);
            if (tex == null)
            {
                Cache[key] = Theme.White;
                return Theme.White;
            }
            var sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            Cache[key] = sp;
            return sp;
        }

        public static Texture2D Tex2D(string key)
        {
            if (Tex.TryGetValue(key, out var t) && t != null) return t;
            t = Resources.Load<Texture2D>("Art/" + key);
            if (t != null)
            {
                t.filterMode = FilterMode.Bilinear;
                t.anisoLevel = 0;
                t.wrapMode = TextureWrapMode.Clamp;
                t.mipMapBias = -4f;
            }
            Tex[key] = t;
            return t;
        }

        public static Sprite CardFrame(CardType t) => t switch
        {
            CardType.Skill => Get("card_frame_skill") == Theme.White ? Get("card_frame") : Get("card_frame_skill"),
            CardType.Power => Get("card_frame_power") == Theme.White ? Get("card_frame") : Get("card_frame_power"),
            _ => Get("card_frame")
        };

        public static Sprite CardArt(CardDef def)
        {
            if (def == null) return Theme.White;
            var named = Get(def.Art);
            if (named != Theme.White) return named;
            return def.Type switch
            {
                CardType.Skill => Get("intent_defend"),
                CardType.Power => Get("energy"),
                CardType.Status => Get("intent_buff"),
                CardType.Curse => Get("intent_buff"),
                _ => Get("card_attack")
            };
        }

        public static Sprite Enemy(string key)
        {
            var s = Get(key);
            return s == Theme.White ? Get("cinder_choir") : s;
        }

        public static Sprite Intent(IntentKind k) => k switch
        {
            IntentKind.Defend or IntentKind.DefendBuff => Get("intent_defend"),
            IntentKind.Buff or IntentKind.Sleep => Get("intent_buff"),
            IntentKind.Debuff => Get("intent_buff"),
            _ => Get("intent_attack")
        };

        public static Sprite Button(bool hover) => hover ? Get("button_hover") : Get("button");
    }
}
