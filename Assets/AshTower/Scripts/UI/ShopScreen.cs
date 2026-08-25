using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AshTower
{
    public class ShopScreen : MonoBehaviour
    {
        public Text goldLabel;
        public Button removeButton;
        public Button leaveButton;
        public Transform content;
        List<ShopOffer> _stock;

        public void Open()
        {
            gameObject.SetActive(true);
            UI.Ensure(transform, BuildChrome);
            Cache();
            Fill();
        }

        public void Close() => gameObject.SetActive(false);

        public void ForgetStock() => _stock = null;

        public void BuildChrome()
        {
            ScreenChrome.Background(transform);
            var panel = ScreenChrome.Panel(transform, "SHOP", 1480, 860, new Vector2(0, -18));
            var flavor = UI.Txt(panel, "Flavor", "He doesn't talk much. The prices are on the goods.", 14, Theme.CreamDim, TextAnchor.MiddleCenter, FontStyle.Normal, false);
            UI.Place(flavor.transform, 640, 22, new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(0.5f, 1));

            goldLabel = UI.Txt(panel, "Gold", "Gold  0", 22, Theme.Gold, TextAnchor.MiddleRight, FontStyle.Bold, false);
            UI.Place(goldLabel.transform, 220, 28, new Vector2(1, 1), new Vector2(-36, -18), new Vector2(1, 1));

            var cardsHeader = UI.Txt(panel, "CardsHeader", "CARDS", 14, Theme.GoldDim, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UI.Place(cardsHeader.transform, 200, 20, new Vector2(0.5f, 1), new Vector2(0, -88), new Vector2(0.5f, 1));

            var relicsHeader = UI.Txt(panel, "RelicsHeader", "RELICS & POTIONS", 14, Theme.GoldDim, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UI.Place(relicsHeader.transform, 280, 20, new Vector2(0.5f, 0.5f), new Vector2(0, -42));

            removeButton = UI.Btn(panel, "Remove", "Remove a card", 280, 44, new Vector2(0.5f, 0), new Vector2(-160, 40), OnRemove, 16);
            leaveButton = UI.Btn(panel, "Leave", "Leave", 160, 44, new Vector2(0.5f, 0), new Vector2(140, 40), OnLeave);
            content = ScreenChrome.Content(panel);
        }

        void Cache()
        {
            var panel = transform.Find("Panel");
            if (goldLabel == null && panel != null) goldLabel = panel.Find("Gold")?.GetComponent<Text>();
            if (removeButton == null && panel != null) removeButton = panel.Find("Remove")?.GetComponent<Button>();
            if (leaveButton == null && panel != null) leaveButton = panel.Find("Leave")?.GetComponent<Button>();
            if (content == null && panel != null) content = panel.Find("Content");
            if (removeButton != null)
            {
                removeButton.onClick.RemoveAllListeners();
                removeButton.onClick.AddListener(OnRemove);
            }
            if (leaveButton != null)
            {
                leaveButton.onClick.RemoveAllListeners();
                leaveButton.onClick.AddListener(OnLeave);
            }
        }

        void Fill()
        {
            var run = AshTowerApp.I.Run;
            EnsureStock(run);
            if (goldLabel != null) goldLabel.text = $"Gold  {run.Gold}";
            int removeCost = run.ShopPrice(run.RemoveCost);
            if (removeButton != null)
            {
                var label = removeButton.GetComponentInChildren<Text>();
                if (label != null) label.text = $"Remove a card  ({removeCost}g)";
            }

            if (content == null) content = ScreenChrome.Content(transform.Find("Panel"));
            else UI.Clear(content);

            var cards = _stock.Where(o => o.Card != null).ToList();
            var extras = _stock.Where(o => o.Relic != null || o.Potion != null).ToList();

            const float cardW = 176f, cardH = 248f, cardGap = 20f;
            float cardSlot = cardW + cardGap;
            float cardStart = cards.Count <= 1 ? 0f : -(cards.Count - 1) * 0.5f * cardSlot;
            const float cardY = 168f;
            for (int i = 0; i < cards.Count; i++)
            {
                var offer = cards[i];
                float x = cardStart + i * cardSlot;
                var v = CardView.Create(content, offer.Card, cardW, cardH);
                v.LiftOnHover = !offer.Sold;
                v.Interactable = !offer.Sold;
                var rt = v.transform as RectTransform;
                UI.Place(rt, cardW, cardH, new Vector2(0.5f, 0.5f), new Vector2(x, cardY), new Vector2(0.5f, 0.5f));
                v.Home = rt.anchoredPosition;
                v.HomeSibling = rt.GetSiblingIndex();
                if (!offer.Sold) v.Clicked = _ => Buy(offer);
                else StampSold(rt);

                var price = UI.Txt(content, "Price" + i, offer.Sold ? "SOLD" : offer.Price + "g", 16,
                    offer.Sold ? Theme.CreamDim : Theme.Gold, TextAnchor.MiddleCenter, FontStyle.Bold, false);
                UI.Place(price.transform, 90, 22, new Vector2(0.5f, 0.5f),
                    new Vector2(x, cardY - cardH * 0.54f - 16f));
            }

            const float tw = 250f, th = 186f, tGap = 18f;
            float tSlot = tw + tGap;
            float tStart = extras.Count <= 1 ? 0f : -(extras.Count - 1) * 0.5f * tSlot;
            const float tY = -168f;
            for (int i = 0; i < extras.Count; i++)
                DrawMerchantTile(content, extras[i], new Vector2(tStart + i * tSlot, tY), tw, th);
        }

        void EnsureStock(RunState run)
        {
            if (_stock != null) return;
            _stock = new List<ShopOffer>();
            for (int i = 0; i < 5; i++)
            {
                var d = run.WeightedCard();
                int price = d.Rarity == CardRarity.Rare ? 150 : d.Rarity == CardRarity.Uncommon ? 75 : 50;
                _stock.Add(new ShopOffer { Card = new CardRuntime { Def = d }, Price = run.ShopPrice(price) });
            }
            for (int i = 0; i < 3; i++)
            {
                var r = run.RandomRelic();
                if (r != null) _stock.Add(new ShopOffer { Relic = r, Price = run.ShopPrice(r.Price) });
            }
            _stock.Add(new ShopOffer { Potion = run.RandomPotion(), Price = run.ShopPrice(50) });
        }

        void DrawMerchantTile(Transform parent, ShopOffer offer, Vector2 pos, float w, float h)
        {
            var tile = UI.Go("Tile", parent);
            UI.Place(tile.transform, w, h, new Vector2(0.5f, 0.5f), pos);
            var bg = tile.AddComponent<Image>();
            bg.sprite = Theme.White;
            bg.color = offer.Sold ? new Color(0.08f, 0.08f, 0.08f, 1f) : new Color(0.13f, 0.12f, 0.10f, 1f);
            bg.raycastTarget = true;
            UI.Border(tile.transform, offer.Sold ? Theme.GoldDim : Theme.Gold, 1.5f);

            bool relic = offer.Relic != null;
            string name = relic ? offer.Relic.Name : offer.Potion.Name;
            string desc = relic ? offer.Relic.Desc : offer.Potion.Desc;

            var kind = UI.Txt(tile.transform, "Kind", relic ? "RELIC" : "POTION", 12, Theme.GoldDim, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UI.Place(kind.transform, w - 16, 18, new Vector2(0.5f, 1), new Vector2(0, -8), new Vector2(0.5f, 1));

            var n = UI.Txt(tile.transform, "Name", name, 17, Theme.Gold, TextAnchor.UpperCenter, FontStyle.Bold, true);
            UI.Place(n.transform, w - 24, 40, new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(0.5f, 1));

            var d = UI.Txt(tile.transform, "Desc", desc, 14, Theme.Cream, TextAnchor.UpperCenter, FontStyle.Normal, true);
            UI.Place(d.transform, w - 28, 68, new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(0.5f, 1));

            if (offer.Sold)
            {
                var sold = UI.Txt(tile.transform, "Sold", "SOLD", 16, Theme.CreamDim, TextAnchor.MiddleCenter, FontStyle.Bold, false);
                UI.Place(sold.transform, 120, 28, new Vector2(0.5f, 0), new Vector2(0, 16));
            }
            else
            {
                UI.Btn(tile.transform, "Buy", offer.Price + "g", 120, 34, new Vector2(0.5f, 0), new Vector2(0, 22), () => Buy(offer), 16);
                var tip = tile.AddComponent<TooltipHover>();
                tip.Title = name;
                tip.Body = desc;
            }
        }

        static void StampSold(RectTransform host)
        {
            var dim = UI.Img(host, "Sold", new Color(0, 0, 0, 0.55f));
            UI.Stretch(dim.transform);
            dim.raycastTarget = true;
            var t = UI.Txt(host, "SoldLabel", "SOLD", 22, Theme.Cream, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UI.Stretch(t.transform);
        }

        void Buy(ShopOffer o)
        {
            var run = AshTowerApp.I.Run;
            if (o.Sold || run.Gold < o.Price) { Sfx.Ui(); return; }
            run.Gold -= o.Price;
            o.Sold = true;
            if (o.Card != null) run.AddCard(o.Card);
            if (o.Relic != null) run.AddRelic(o.Relic);
            if (o.Potion != null) run.AddPotion(o.Potion);
            Sfx.Energy();
            Fill();
            AshTowerApp.I.hud?.Bind(run);
        }

        void OnRemove()
        {
            var run = AshTowerApp.I.Run;
            int removeCost = run.ShopPrice(run.RemoveCost);
            if (run.Gold < removeCost) { Sfx.Ui(); return; }
            AshTowerApp.I.OpenRemovePicker(() =>
            {
                run.Gold -= removeCost;
                AshTowerApp.I.Show(ScreenId.Shop);
            });
        }

        void OnLeave()
        {
            _stock = null;
            AshTowerApp.I.BackToMap();
        }
    }
}
