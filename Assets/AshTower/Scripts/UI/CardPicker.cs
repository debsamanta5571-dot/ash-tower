using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AshTower
{
    public class CardPicker : MonoBehaviour
    {
        public Text title;
        public Transform content;
        public Button closeButton;
        Action<CardRuntime> _picked;

        public void Open(string heading, List<CardRuntime> cards, Action<CardRuntime> picked)
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            _picked = picked;
            UI.Ensure(transform, BuildChrome);
            Cache();
            if (title != null) title.text = heading;
            Fill(cards);
        }

        public void Close()
        {
            if (content != null) UI.Clear(content);
            gameObject.SetActive(false);
        }

        public void BuildChrome()
        {
            var veil = UI.Img(transform, "Veil", Theme.Overlay, Theme.White, true);
            UI.Stretch(veil.transform);
            var panel = ScreenChrome.Panel(transform, "Deck", 1400, 780);
            title = panel.Find("Title")?.GetComponent<Text>();
            var hint = UI.Txt(panel, "Hint", "Hold right-click on a card to see its upgrade.", 14, Theme.CreamDim, TextAnchor.MiddleCenter, FontStyle.Normal, false);
            UI.Place(hint.transform, 500, 20, new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(0.5f, 1));
            closeButton = UI.Btn(panel, "Close", "Close", 140, 40, new Vector2(1, 1), new Vector2(-90, -28), Close);
            content = ScreenChrome.Content(panel);
        }

        void Cache()
        {
            var panel = transform.Find("Panel");
            if (title == null && panel != null) title = panel.Find("Title")?.GetComponent<Text>();
            if (content == null && panel != null) content = panel.Find("Content");
            if (closeButton == null && panel != null) closeButton = panel.Find("Close")?.GetComponent<Button>();
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Close);
            }
        }

        void Fill(List<CardRuntime> cards)
        {
            if (content == null) content = ScreenChrome.Content(transform.Find("Panel"));
            else UI.Clear(content);

            float x = -600, y = 160;
            foreach (var c in cards)
            {
                var captured = c;
                var v = CardView.Create(content, captured, 170, 255);
                v.LiftOnHover = true;
                v.Interactable = true;
                var rt = v.transform as RectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(x, y);
                v.Clicked = _ =>
                {
                    Close();
                    _picked?.Invoke(captured);
                };
                x += 185;
                if (x > 620) { x = -600; y -= 280; }
            }
        }
    }
}
