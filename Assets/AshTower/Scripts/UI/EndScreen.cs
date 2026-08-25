using UnityEngine;
using UnityEngine.UI;

namespace AshTower
{
    public class EndScreen : MonoBehaviour
    {
        public Text title;
        public Text body;
        public Button againButton;
        public Button titleButton;
        public Image veil;
        bool _win;

        public void Open(bool win)
        {
            _win = win;
            gameObject.SetActive(true);
            UI.Ensure(transform, BuildChrome);
            Cache();
            Bind();
        }

        public void Close() => gameObject.SetActive(false);

        public void BuildChrome()
        {
            ScreenChrome.Background(transform);
            veil = UI.Img(transform, "Veil", new Color(0, 0, 0, 0.55f));
            UI.Stretch(veil.transform);
            title = UI.Txt(transform, "Title", "YOU DIED", 64, Theme.Blood, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UI.Place(title.transform, 1100, 80, new Vector2(0.5f, 0.65f), Vector2.zero);
            UI.Outline(title, Theme.Ink, 2.2f);
            body = UI.Txt(transform, "Body", "", 22, Theme.Cream, TextAnchor.MiddleCenter);
            UI.Place(body.transform, 800, 90, new Vector2(0.5f, 0.5f), new Vector2(0, 20));
            againButton = UI.Btn(transform, "Again", "Ascend again", 280, 56, new Vector2(0.5f, 0.35f), Vector2.zero, () => AshTowerApp.I.NewRun());
            titleButton = UI.Btn(transform, "TitleButton", "Title", 200, 48, new Vector2(0.5f, 0.35f), new Vector2(0, -70), () => AshTowerApp.I.Show(ScreenId.Title));
        }

        void Cache()
        {
            if (title == null) title = transform.Find("Title")?.GetComponent<Text>();
            if (body == null) body = transform.Find("Body")?.GetComponent<Text>();
            if (veil == null) veil = transform.Find("Veil")?.GetComponent<Image>();
            if (againButton == null) againButton = transform.Find("Again")?.GetComponent<Button>();
            if (titleButton == null) titleButton = transform.Find("TitleButton")?.GetComponent<Button>();
            if (againButton != null)
            {
                againButton.onClick.RemoveAllListeners();
                againButton.onClick.AddListener(() => AshTowerApp.I.NewRun());
            }
            if (titleButton != null)
            {
                titleButton.onClick.RemoveAllListeners();
                titleButton.onClick.AddListener(() => AshTowerApp.I.Show(ScreenId.Title));
            }
        }

        void Bind()
        {
            var run = AshTowerApp.I.Run;
            if (title != null)
            {
                title.text = _win ? "You win?" : "YOU DIED";
                title.color = _win ? Theme.Gold : Theme.Blood;
            }
            if (veil != null)
                veil.color = new Color(0, 0, 0, _win ? 0.35f : 0.55f);
            if (body != null && run != null)
            {
                body.text = _win
                    ? $"You climbed {run.Floor} floors with {run.Deck.Count} cards and {run.Relics.Count} relics."
                    : $"You died on floor {run.Floor}.";
            }
        }
    }
}
