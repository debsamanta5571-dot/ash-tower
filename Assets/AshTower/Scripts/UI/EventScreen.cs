using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AshTower
{
    public class EventScreen : MonoBehaviour
    {
        public Transform content;
        HashSet<CardRuntime> _cardsBefore;
        HashSet<RelicDef> _relicsBefore;
        HashSet<CardRuntime> _upgradedBefore;
        string _flavor;
        bool _outcome;

        public void Open()
        {
            gameObject.SetActive(true);
            UI.Ensure(transform, BuildChrome);
            var panel = transform.Find("Panel");
            if (content == null && panel != null) content = panel.Find("Content");
            if (!_outcome) Fill();
        }

        public void Close()
        {
            _outcome = false;
            gameObject.SetActive(false);
        }

        public void BuildChrome()
        {
            ScreenChrome.Background(transform);
            var panel = ScreenChrome.Panel(transform, "EVENT", 860, 660);
            content = ScreenChrome.Content(panel);
        }

        void Fill()
        {
            var app = AshTowerApp.I;
            var ev = app.CurrentEvent;
            if (ev == null) return;
            var run = app.Run;
            var panel = transform.Find("Panel");
            ScreenChrome.SetTitle(panel, ev.Title.ToUpper());
            if (content == null) content = ScreenChrome.Content(panel);
            else UI.Clear(content);

            var body = UI.Txt(content, "Body", ev.Body, 17, Theme.Cream, TextAnchor.UpperCenter, FontStyle.Normal, true);
            UI.Place(body.transform, 740, 150, new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(0.5f, 1));
            float y = -10;
            int i = 0;
            foreach (var opt in ev.Options)
            {
                var captured = opt;
                UI.Btn(content, "Option" + i, captured.Label, 640, 48, new Vector2(0.5f, 0.5f), new Vector2(0, y), () => Choose(captured), 16);
                y -= 58;
                i++;
            }
        }

        void Choose(EventOption opt)
        {
            var app = AshTowerApp.I;
            var run = app.Run;
            Snapshot(run);
            _flavor = opt.Result;
            opt.Apply(run, app);
            if (app.Screen != ScreenId.Event) return;
            if (opt.Leaves) Reveal();
        }

        public void AfterPicker()
        {
            gameObject.SetActive(true);
            Reveal();
        }

        void Snapshot(RunState run)
        {
            _cardsBefore = new HashSet<CardRuntime>(run.Deck);
            _relicsBefore = new HashSet<RelicDef>(run.Relics);
            _upgradedBefore = new HashSet<CardRuntime>(run.Deck.Where(c => c.Upgraded));
        }

        void Reveal()
        {
            var run = AshTowerApp.I.Run;
            if (run == null) return;
            _outcome = true;
            AshTowerApp.I.hud?.Bind(run);

            var gainedCards = _cardsBefore == null
                ? new List<CardRuntime>()
                : run.Deck.Where(c => !_cardsBefore.Contains(c)).ToList();
            var gainedRelics = _relicsBefore == null
                ? new List<RelicDef>()
                : run.Relics.Where(r => !_relicsBefore.Contains(r)).ToList();
            var upgraded = _upgradedBefore == null
                ? new List<CardRuntime>()
                : run.Deck.Where(c => c.Upgraded && !_upgradedBefore.Contains(c)).ToList();
            var removed = _cardsBefore == null
                ? new List<CardRuntime>()
                : _cardsBefore.Where(c => !run.Deck.Contains(c)).ToList();

            FillOutcome(_flavor, gainedCards, gainedRelics, upgraded, removed);
        }

        void FillOutcome(string flavor, List<CardRuntime> cards, List<RelicDef> relics, List<CardRuntime> upgraded, List<CardRuntime> removed)
        {
            var panel = transform.Find("Panel");
            if (content == null && panel != null) content = ScreenChrome.Content(panel);
            else UI.Clear(content);

            var body = UI.Txt(content, "Body", string.IsNullOrEmpty(flavor) ? "That's done." : flavor, 17, Theme.Cream, TextAnchor.UpperCenter, FontStyle.Normal, true);
            UI.Place(body.transform, 740, 80, new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(0.5f, 1));

            var items = new List<RewardItem>();
            foreach (var c in cards) items.Add(new RewardItem { Card = c, Kind = "NEW CARD" });
            foreach (var c in upgraded) items.Add(new RewardItem { Card = c, Kind = "UPGRADED" });
            foreach (var r in relics) items.Add(new RewardItem { Relic = r, Kind = "RELIC" });
            foreach (var c in removed) items.Add(new RewardItem { Removed = c, Kind = "REMOVED" });

            if (items.Count == 0)
            {
                var none = UI.Txt(content, "None", "Nothing else changed.", 15, Theme.CreamDim, TextAnchor.MiddleCenter, FontStyle.Italic, false);
                UI.Place(none.transform, 500, 24, new Vector2(0.5f, 0.5f), new Vector2(0, 20));
            }
            else
            {
                float slot = 250f;
                float start = items.Count == 1 ? 0f : -(items.Count - 1) * 0.5f * slot;
                for (int i = 0; i < items.Count; i++)
                    DrawReward(items[i], new Vector2(start + i * slot, 10));
            }

            UI.Btn(content, "Continue", "Continue", 200, 48, new Vector2(0.5f, 0), new Vector2(0, 40), () =>
            {
                _outcome = false;
                AshTowerApp.I.BackToMap();
            });
        }

        void DrawReward(RewardItem item, Vector2 pos)
        {
            var tag = UI.Txt(content, "Kind", item.Kind, 13, Theme.Gold, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UI.Place(tag.transform, 200, 20, new Vector2(0.5f, 0.5f), pos + new Vector2(0, 170));

            if (item.Card != null)
            {
                var v = CardView.Create(content, item.Card, 176, 248);
                v.LiftOnHover = true;
                v.Interactable = true;
                var rt = v.transform as RectTransform;
                UI.Place(rt, 176, 248, new Vector2(0.5f, 0.5f), pos + new Vector2(0, 20), new Vector2(0.5f, 0.5f));
                v.Home = rt.anchoredPosition;
                return;
            }

            if (item.Relic != null)
            {
                DrawTile(pos, item.Relic.Name, item.Relic.Desc);
                return;
            }

            if (item.Removed != null)
                DrawTile(pos, item.Removed.Def.DisplayName(item.Removed.Upgraded), "Taken out of your deck.");
        }

        void DrawTile(Vector2 pos, string name, string desc)
        {
            var tile = UI.Go("Tile", content);
            UI.Place(tile.transform, 230, 200, new Vector2(0.5f, 0.5f), pos + new Vector2(0, 10));
            var bg = tile.AddComponent<Image>();
            bg.sprite = Theme.White;
            bg.color = new Color(0.13f, 0.12f, 0.10f, 1f);
            UI.Border(tile.transform, Theme.Gold, 1.5f);
            var n = UI.Txt(tile.transform, "Name", name, 18, Theme.Gold, TextAnchor.UpperCenter, FontStyle.Bold, true);
            UI.Place(n.transform, 200, 48, new Vector2(0.5f, 1), new Vector2(0, -16), new Vector2(0.5f, 1));
            var d = UI.Txt(tile.transform, "Desc", desc, 15, Theme.Cream, TextAnchor.UpperCenter, FontStyle.Normal, true);
            UI.Place(d.transform, 200, 110, new Vector2(0.5f, 1), new Vector2(0, -68), new Vector2(0.5f, 1));
        }

        struct RewardItem
        {
            public string Kind;
            public CardRuntime Card;
            public RelicDef Relic;
            public CardRuntime Removed;
        }
    }
}
