using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AshTower
{
    public class MapScreen : MonoBehaviour
    {
        public Transform mapContent;

        public void Open()
        {
            gameObject.SetActive(true);
            UI.Ensure(transform, BuildChrome);
            RebuildGraph();
        }

        public void Close() => gameObject.SetActive(false);

        public void BuildChrome()
        {
            var bg = UI.Img(transform, "Background", new Color(0.08f, 0.08f, 0.09f, 1f));
            UI.Stretch(bg.transform);

            var title = UI.Txt(transform, "Title", "ASH TOWER", 28, Theme.Gold, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UI.Place(title.transform, 400, 32, new Vector2(0.5f, 1), new Vector2(0, -86), new Vector2(0.5f, 1));
            UI.Outline(title, Theme.Ink);

            var mapHost = UI.Go("MapHost", transform);
            UI.Place(mapHost.transform, 1100, 780, new Vector2(0.5f, 0.5f), new Vector2(0, -40));
            var maskImg = mapHost.AddComponent<Image>();
            maskImg.sprite = Theme.White;
            maskImg.color = new Color(0, 0, 0, 0.2f);
            maskImg.raycastTarget = true;
            mapHost.AddComponent<Mask>().showMaskGraphic = false;
            var sr = mapHost.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 55;

            var content = UI.Go("Content", mapHost.transform);
            mapContent = content.transform;
            var crt = UI.RT(content);
            float rowH = 108, pad = 80;
            float height = pad * 2 + RunState.Rows * rowH;
            crt.anchorMin = new Vector2(0, 0);
            crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 0);
            crt.sizeDelta = new Vector2(0, height - 860);
            crt.anchoredPosition = Vector2.zero;
            sr.content = crt;
            sr.viewport = UI.RT(mapHost);

            DrawLegend(transform);
        }

        void RebuildGraph()
        {
            var run = AshTowerApp.I.Run;
            if (mapContent == null)
            {
                var c = transform.Find("MapHost/Content");
                mapContent = c != null ? c : transform;
            }
            UI.Clear(mapContent);
            var crt = UI.RT(mapContent.gameObject);
            const float rowH = 108, colW = 130, pad = 80;

            Vector2 Pos(MapNode n)
            {
                float x = (n.Col - 3) * colW;
                float y = pad + n.Row * rowH + 40;
                return new Vector2(x, y);
            }

            foreach (var n in run.Nodes)
                foreach (var nid in n.Next)
                    UI.Line(crt, Pos(n), Pos(run.Nodes[nid]), n.Seen ? 5f : 3f, n.Seen ? Theme.Gold : Theme.MapPath);

            var available = new HashSet<int>(run.Available().Select(a => a.Id));
            foreach (var n in run.Nodes)
            {
                bool av = available.Contains(n.Id);
                bool cur = run.CurrentNode == n.Id;
                float sz = n.Type == RoomType.Boss ? 86 : 64;
                MapNodeView view;
                if (UiLibrary.HasMapNode)
                {
                    view = Instantiate(UiLibrary.I.mapNode, crt);
                    UI.Restore(view.transform);
                }
                else
                {
                    var node = UI.Go("Node" + n.Id, crt);
                    view = node.AddComponent<MapNodeView>();
                    var img = node.AddComponent<Image>();
                    img.sprite = Theme.Circle;
                    view.disc = img;
                    view.button = node.AddComponent<Button>();
                    var ico = UI.Img(node.transform, "Icon", Color.white);
                    ico.preserveAspect = true;
                    UI.Stretch(ico.transform, 10);
                    view.icon = ico;
                    var lb = UI.Txt(node.transform, "BossLabel", "BOSS", 12, Theme.Cream, TextAnchor.LowerCenter, FontStyle.Bold, false);
                    UI.Place(lb.transform, 80, 18, new Vector2(0.5f, 0), new Vector2(0, -16));
                    view.bossLabel = lb;
                }
                UI.Place(view.transform, sz, sz, new Vector2(0.5f, 0), Pos(n));
                view.Bind(n, av, cur, n.Seen);
            }
        }

        static void DrawLegend(Transform root)
        {
            var row = UI.Go("Legend", root);
            UI.Place(row.transform, 1120, 36, new Vector2(0.5f, 0), new Vector2(0, 26));
            var items = new (RoomType type, string label)[]
            {
                (RoomType.Monster, "Fight"),
                (RoomType.Elite, "Elite"),
                (RoomType.Rest, "Rest"),
                (RoomType.Shop, "Shop"),
                (RoomType.Event, "Event"),
                (RoomType.Treasure, "Chest"),
                (RoomType.Boss, "Boss"),
            };
            const float slot = 156f;
            float start = -(items.Length - 1) * slot * 0.5f;
            for (int i = 0; i < items.Length; i++)
            {
                var item = UI.Go(items[i].label, row.transform);
                UI.Place(item.transform, slot, 36, new Vector2(0.5f, 0.5f), new Vector2(start + i * slot, 0));
                var disc = UI.Img(item.transform, "Disc", NodeColor(items[i].type, true, false, false), Theme.Circle);
                UI.Place(disc.transform, 22, 22, new Vector2(0, 0.5f), new Vector2(18, 0));
                var ico = UI.Img(item.transform, "Icon", Color.white, IconFor(items[i].type));
                ico.preserveAspect = true;
                UI.Place(ico.transform, 14, 14, new Vector2(0, 0.5f), new Vector2(18, 0));
                var tx = UI.Txt(item.transform, "Label", items[i].label, 14, Theme.CreamDim, TextAnchor.MiddleLeft, FontStyle.Normal, false);
                UI.Place(tx.transform, 110, 22, new Vector2(0, 0.5f), new Vector2(34, 0), new Vector2(0, 0.5f));
            }
        }

        public static Color NodeColor(RoomType t, bool av, bool cur, bool seen)
        {
            Color c = t switch
            {
                RoomType.Elite => Theme.Ember,
                RoomType.Rest => Theme.Gold,
                RoomType.Shop => Theme.Uncommon,
                RoomType.Treasure => Theme.Rare,
                RoomType.Event => Theme.Power,
                RoomType.Boss => Theme.Blood,
                _ => Theme.Attack
            };
            if (cur) return Color.Lerp(c, Color.white, 0.35f);
            if (av) return c;
            if (seen) return Color.Lerp(c, Theme.Ink, 0.45f);
            return Color.Lerp(c, Theme.Ink, 0.65f);
        }

        public static Sprite IconFor(RoomType t) => t switch
        {
            RoomType.Rest => Art.Get("energy"),
            RoomType.Shop => Art.Get("relic_heart"),
            RoomType.Treasure => Art.Get("relic_heart"),
            RoomType.Elite => Art.Get("intent_attack"),
            RoomType.Boss => Art.Get("ash_warden"),
            RoomType.Event => Art.Get("intent_buff"),
            _ => Art.Get("intent_defend")
        };
    }
}
