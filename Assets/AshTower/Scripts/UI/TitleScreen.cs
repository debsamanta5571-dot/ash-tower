using UnityEngine;
using UnityEngine.UI;

namespace AshTower
{
    public class TitleScreen : MonoBehaviour
    {
        public Image knight;
        public Button playButton;
        public Button settingsButton;
        public Button quitButton;

        public void Open()
        {
            gameObject.SetActive(true);
            UI.Ensure(transform, Build);
            Cache();
            if (knight != null)
            {
                knight.sprite = Art.Get("knight");
                knight.preserveAspect = true;
            }
            var hint = transform.Find("Hint")?.GetComponent<Text>();
            if (hint != null) hint.text = "Can you survive the climb?";
        }

        public void Close() => gameObject.SetActive(false);

        public void Build()
        {
            var bg = UI.Img(transform, "Background", new Color(0.08f, 0.08f, 0.09f, 1f));
            UI.Stretch(bg.transform);
            var glow = UI.Img(transform, "Glow", new Color(0.40f, 0.14f, 0.05f, 0.4f), Theme.Circle);
            UI.Place(glow.transform, 1600, 900, new Vector2(0.5f, 0), new Vector2(0, -120));

            knight = UI.Img(transform, "Knight", Color.white, Art.Get("knight"));
            knight.preserveAspect = true;
            UI.Place(knight.transform, 420, 680, new Vector2(1, 0.5f), new Vector2(-260, -20));

            var title = UI.Txt(transform, "Wordmark", "ASH TOWER", 72, Theme.Gold, TextAnchor.MiddleLeft, FontStyle.Bold, false);
            UI.Place(title.transform, 720, 80, new Vector2(0, 1), new Vector2(80, -80), new Vector2(0, 1));
            UI.Outline(title, Theme.Ink, 2f);
            UI.Shadow(title);

            var sub = UI.Txt(transform, "Tagline", "The stairs only go one way.", 24, Theme.CreamDim, TextAnchor.MiddleLeft, FontStyle.Italic, false);
            UI.Place(sub.transform, 720, 36, new Vector2(0, 1), new Vector2(84, -172), new Vector2(0, 1));

            playButton = UI.Btn(transform, "Play", "ASCEND", 300, 56, new Vector2(0, 1), new Vector2(230, -280), () => AshTowerApp.I.NewRun(), 26);
            settingsButton = UI.Btn(transform, "Settings", "SETTINGS", 300, 48, new Vector2(0, 1), new Vector2(230, -350), () => AshTowerApp.I.OpenSettings(), 20);
            quitButton = UI.Btn(transform, "Quit", "QUIT", 300, 48, new Vector2(0, 1), new Vector2(230, -418), Quit, 20);

            var hint = UI.Txt(transform, "Hint", "Can you survive the climb?", 16, Theme.CreamDim, TextAnchor.LowerLeft, FontStyle.Normal, false);
            UI.Place(hint.transform, 900, 28, new Vector2(0, 0), new Vector2(40, 28), new Vector2(0, 0));
        }

        void Cache()
        {
            if (knight == null)
            {
                var t = transform.Find("Knight");
                if (t != null) knight = t.GetComponent<Image>();
            }
            playButton = playButton ?? transform.Find("Play")?.GetComponent<Button>();
            settingsButton = settingsButton ?? transform.Find("Settings")?.GetComponent<Button>();
            quitButton = quitButton ?? transform.Find("Quit")?.GetComponent<Button>();
            Wire(playButton, () => AshTowerApp.I.NewRun());
            Wire(settingsButton, () => AshTowerApp.I.OpenSettings());
            Wire(quitButton, Quit);
        }

        static void Wire(Button button, UnityEngine.Events.UnityAction click)
        {
            if (button == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(click);
        }

        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
