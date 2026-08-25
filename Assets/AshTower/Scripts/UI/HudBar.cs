using UnityEngine;
using UnityEngine.UI;

namespace AshTower
{
    public class HudBar : MonoBehaviour
    {
        public Text hp;
        public Text gold;
        public Text floor;
        public Transform relics;
        public Button deckButton;
        public Button settingsButton;

        public void Show(RunState run)
        {
            gameObject.SetActive(true);
            if (GetComponent<RuntimeChrome>() == null || UI.Broken(transform) || hp == null)
            {
                if (GetComponent<RuntimeChrome>() == null || UI.Broken(transform))
                {
                    UI.Wipe(transform);
                    hp = gold = floor = null;
                    relics = null;
                    deckButton = settingsButton = null;
                    if (GetComponent<RuntimeChrome>() == null)
                        gameObject.AddComponent<RuntimeChrome>();
                }
                else Cache();
                if (hp == null) Build();
            }
            UI.Restore(transform);
            Bind(run);
        }

        public void Hide() => gameObject.SetActive(false);

        public void Cache()
        {
            if (hp == null) hp = transform.Find("Hp")?.GetComponent<Text>();
            if (gold == null) gold = transform.Find("Gold")?.GetComponent<Text>();
            if (floor == null) floor = transform.Find("Floor")?.GetComponent<Text>();
            if (relics == null) relics = transform.Find("Relics");
            if (deckButton == null) deckButton = transform.Find("Deck")?.GetComponent<Button>();
            if (settingsButton == null) settingsButton = transform.Find("Settings")?.GetComponent<Button>();
            WireButtons();
        }

        public void Build()
        {
            var bar = UI.Img(transform, "Bar", new Color(0, 0, 0, 0.55f));
            var br = bar.rectTransform;
            br.anchorMin = new Vector2(0, 1);
            br.anchorMax = new Vector2(1, 1);
            br.pivot = new Vector2(0.5f, 1);
            br.sizeDelta = new Vector2(0, 70);
            br.anchoredPosition = Vector2.zero;
            bar.raycastTarget = false;

            hp = UI.Txt(transform, "Hp", "HP  0/0", 22, Theme.Cream, TextAnchor.MiddleLeft, FontStyle.Bold, false);
            UI.Place(hp.transform, 220, 40, new Vector2(0, 1), new Vector2(40, -36), new Vector2(0, 0.5f));
            UI.Outline(hp, Theme.Ink);

            gold = UI.Txt(transform, "Gold", "Gold  0", 22, Theme.Gold, TextAnchor.MiddleLeft, FontStyle.Bold, false);
            UI.Place(gold.transform, 200, 40, new Vector2(0, 1), new Vector2(280, -36), new Vector2(0, 0.5f));
            UI.Outline(gold, Theme.Ink);

            floor = UI.Txt(transform, "Floor", "Floor  1 / 15", 20, Theme.CreamDim, TextAnchor.MiddleLeft, FontStyle.Normal, false);
            UI.Place(floor.transform, 200, 40, new Vector2(0, 1), new Vector2(500, -36), new Vector2(0, 0.5f));

            deckButton = UI.Btn(transform, "Deck", "Deck", 90, 36, new Vector2(0, 1), new Vector2(760, -36), () => AshTowerApp.I.ShowDeck(), 16);
            settingsButton = UI.Btn(transform, "Settings", "Settings", 110, 36, new Vector2(0, 1), new Vector2(870, -36), () => AshTowerApp.I.OpenSettings(), 16);

            var rel = UI.Go("Relics", transform);
            UI.Place(rel.transform, 420, 42, new Vector2(1, 1), new Vector2(-40, -36), new Vector2(1, 0.5f));
            relics = rel.transform;
        }

        public void Bind(RunState run)
        {
            if (run == null) return;
            if (hp != null) hp.text = $"HP  {run.Hp}/{run.MaxHp}";
            if (gold != null) gold.text = $"Gold  {run.Gold}";
            if (floor != null) floor.text = $"Floor  {Mathf.Max(1, run.Floor)} / 15";

            if (relics == null) return;
            UI.Clear(relics);
            float x = 0;
            foreach (var r in run.Relics)
            {
                var sprite = Art.Get(r.Art);
                if (sprite == Theme.White) sprite = Art.Get("relic_heart");
                var ic = UI.Img(relics, r.Id, Color.white, sprite, true);
                ic.preserveAspect = true;
                UI.Place(ic.transform, 42, 42, new Vector2(1, 0.5f), new Vector2(x, 0), new Vector2(1, 0.5f));
                var tip = ic.gameObject.AddComponent<TooltipHover>();
                tip.Title = r.Name;
                tip.Body = r.Desc;
                x -= 48;
            }
        }

        void WireButtons()
        {
            if (deckButton != null)
            {
                deckButton.onClick.RemoveAllListeners();
                deckButton.onClick.AddListener(() => AshTowerApp.I.ShowDeck());
            }
            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveAllListeners();
                settingsButton.onClick.AddListener(() => AshTowerApp.I.OpenSettings());
            }
        }
    }
}
