using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AshTower
{
    public class RewardsScreen : MonoBehaviour
    {
        public Transform content;
        List<CardRuntime> _cards;
        RelicDef _relic;
        PotionDef _potion;

        public void Open()
        {
            gameObject.SetActive(true);
            UI.Ensure(transform, BuildChrome);
            var panel = transform.Find("Panel");
            if (content == null && panel != null) content = panel.Find("Content");
            Fill();
        }

        public void Close() => gameObject.SetActive(false);

        public void BuildChrome()
        {
            ScreenChrome.Background(transform);
            var panel = ScreenChrome.Panel(transform, "LOOT", 1400, 820, new Vector2(0, -20));
            content = ScreenChrome.Content(panel);
        }

        void Fill()
        {
            var run = AshTowerApp.I.Run;
            var cbt = AshTowerApp.I.Combat;
            var panel = transform.Find("Panel");
            if (content == null) content = ScreenChrome.Content(panel);
            else UI.Clear(content);

            _cards = new List<CardRuntime>();
            bool elite = cbt != null && cbt.Elite;
            for (int i = 0; i < 3; i++)
                _cards.Add(new CardRuntime { Def = run.WeightedCard(null, elite) });
            _relic = elite ? run.RandomRelic(CardRarity.Uncommon) : null;
            _potion = run.Rng.Next(100) < 28 ? run.RandomPotion() : null;
            if (elite && run.HasRelic("trophy_hook"))
                _potion = run.RandomPotion();

            var g = UI.Txt(content, "Gold", $"Gold  {run.Gold}", 20, Theme.Gold, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UI.Place(g.transform, 500, 28, new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(0.5f, 1));

            var skip = UI.Txt(content, "Hint", "You can take one card.", 16, Theme.CreamDim, TextAnchor.MiddleCenter, FontStyle.Normal, false);
            UI.Place(skip.transform, 500, 22, new Vector2(0.5f, 1), new Vector2(0, -102), new Vector2(0.5f, 1));

            for (int i = 0; i < _cards.Count; i++)
            {
                var card = _cards[i];
                var v = CardView.Create(content, card, 210, 318);
                v.LiftOnHover = true;
                v.Interactable = true;
                var rt = v.transform as RectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2((i - 1) * 240, 40);
                v.Clicked = _ =>
                {
                    run.AddCard(card);
                    Sfx.Ui();
                    AfterCard();
                };
            }

            if (_relic != null)
            {
                var rr = UI.Txt(content, "Relic", "They dropped this too: " + _relic.Name + ". " + _relic.Desc, 15, Theme.EmberHi, TextAnchor.MiddleCenter);
                UI.Place(rr.transform, 900, 40, new Vector2(0.5f, 0), new Vector2(0, 100));
            }
            UI.Btn(content, "Skip", "Skip", 180, 44, new Vector2(0.5f, 0), new Vector2(0, 40), AfterCard);
        }

        void AfterCard()
        {
            var run = AshTowerApp.I.Run;
            if (_relic != null)
            {
                run.AddRelic(_relic);
                _relic = null;
            }
            if (_potion != null)
            {
                run.AddPotion(_potion);
                _potion = null;
            }
            AshTowerApp.I.BackToMap();
        }
    }
}
