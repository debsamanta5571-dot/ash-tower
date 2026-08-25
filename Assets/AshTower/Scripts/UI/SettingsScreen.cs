using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AshTower
{
    public class SettingsScreen : MonoBehaviour
    {
        public Image volumeFill;
        public Text volumeLabel;
        public Button menuButton;
        public Button backButton;
        public Button borderlessButton;
        public Button windowedButton;

        public void Open()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            UI.Ensure(transform, BuildChrome);
            if (transform.Find("Panel/Borderless") == null)
            {
                UI.Wipe(transform);
                BuildChrome();
                if (GetComponent<RuntimeChrome>() == null)
                    gameObject.AddComponent<RuntimeChrome>();
            }
            Cache();
            if (volumeFill != null) volumeFill.fillAmount = Sfx.Volume;
            if (volumeLabel != null) volumeLabel.text = Mathf.RoundToInt(Sfx.Volume * 100f) + "%";
            RefreshDisplayButtons();
        }

        public void Close() => gameObject.SetActive(false);

        public void BuildChrome()
        {
            var veil = UI.Img(transform, "Veil", Theme.Overlay, Theme.White, true);
            UI.Stretch(veil.transform);
            var panel = ScreenChrome.Panel(transform, "SETTINGS", 560, 480);

            var lab = UI.Txt(panel, "SoundLabel", "Sound", 20, Theme.Cream, TextAnchor.MiddleLeft, FontStyle.Bold, false);
            UI.Place(lab.transform, 120, 28, new Vector2(0.5f, 0.5f), new Vector2(-180, 90), new Vector2(0, 0.5f));

            volumeLabel = UI.Txt(panel, "VolumePercent", Mathf.RoundToInt(Sfx.Volume * 100f) + "%", 20, Theme.Gold, TextAnchor.MiddleRight, FontStyle.Bold, false);
            UI.Place(volumeLabel.transform, 80, 28, new Vector2(0.5f, 0.5f), new Vector2(200, 90), new Vector2(1, 0.5f));

            volumeFill = UI.Bar(panel, "Volume", Theme.Gold, 400, 22, new Vector2(0.5f, 0.5f), new Vector2(0, 42));
            volumeFill.fillAmount = Sfx.Volume;
            var host = volumeFill.transform.parent.gameObject;
            host.GetComponent<Image>().raycastTarget = true;
            var slider = host.GetComponent<VolumeSlider>() ?? host.AddComponent<VolumeSlider>();
            slider.Fill = volumeFill;
            slider.Label = volumeLabel;

            var hint = UI.Txt(panel, "Hint", "Drag the bar to change the volume.", 14, Theme.CreamDim, TextAnchor.MiddleCenter, FontStyle.Normal, false);
            UI.Place(hint.transform, 420, 22, new Vector2(0.5f, 0.5f), new Vector2(0, 8));

            var display = UI.Txt(panel, "DisplayLabel", "Display", 20, Theme.Cream, TextAnchor.MiddleLeft, FontStyle.Bold, false);
            UI.Place(display.transform, 160, 28, new Vector2(0.5f, 0.5f), new Vector2(-180, -40), new Vector2(0, 0.5f));
            borderlessButton = UI.Btn(panel, "Borderless", "Borderless", 190, 40, new Vector2(0.5f, 0.5f), new Vector2(-100, -88), () => SetDisplay(true), 16);
            windowedButton = UI.Btn(panel, "Windowed", "Windowed", 190, 40, new Vector2(0.5f, 0.5f), new Vector2(110, -88), () => SetDisplay(false), 16);

            menuButton = UI.Btn(panel, "MainMenu", "Main Menu", 180, 44, new Vector2(0.5f, 0), new Vector2(-110, 40), () => AshTowerApp.I.Show(ScreenId.Title));
            backButton = UI.Btn(panel, "Back", "Back", 160, 44, new Vector2(0.5f, 0), new Vector2(110, 40), Close);
        }

        void Cache()
        {
            var panel = transform.Find("Panel");
            if (volumeFill == null && panel != null)
            {
                var vol = panel.Find("Volume/fill") ?? panel.Find("Volume/Fill");
                if (vol != null) volumeFill = vol.GetComponent<Image>();
            }
            if (volumeLabel == null && panel != null) volumeLabel = panel.Find("VolumePercent")?.GetComponent<Text>();
            if (menuButton == null && panel != null) menuButton = panel.Find("MainMenu")?.GetComponent<Button>();
            if (backButton == null && panel != null) backButton = panel.Find("Back")?.GetComponent<Button>();
            if (borderlessButton == null && panel != null) borderlessButton = panel.Find("Borderless")?.GetComponent<Button>();
            if (windowedButton == null && panel != null) windowedButton = panel.Find("Windowed")?.GetComponent<Button>();
            if (menuButton != null)
            {
                menuButton.onClick.RemoveAllListeners();
                menuButton.onClick.AddListener(() => AshTowerApp.I.Show(ScreenId.Title));
            }
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(Close);
            }
            if (borderlessButton != null)
            {
                borderlessButton.onClick.RemoveAllListeners();
                borderlessButton.onClick.AddListener(() => SetDisplay(true));
            }
            if (windowedButton != null)
            {
                windowedButton.onClick.RemoveAllListeners();
                windowedButton.onClick.AddListener(() => SetDisplay(false));
            }
            RefreshDisplayButtons();
        }

        void SetDisplay(bool borderless)
        {
            GameDisplay.SetBorderless(borderless);
            RefreshDisplayButtons();
        }

        void RefreshDisplayButtons()
        {
            PaintChoice(borderlessButton, GameDisplay.Borderless);
            PaintChoice(windowedButton, !GameDisplay.Borderless);
        }

        static void PaintChoice(Button button, bool on)
        {
            if (button == null) return;
            var label = button.GetComponentInChildren<Text>();
            if (label != null) label.color = on ? Theme.Gold : Color.white;
            var img = button.GetComponent<Image>();
            if (img != null)
                img.color = on ? new Color(0.32f, 0.28f, 0.22f, 1f) : Theme.PanelHi;
        }
    }

    public static class GameDisplay
    {
        const string PrefKey = "ash_display";

        public static bool Borderless => PlayerPrefs.GetInt(PrefKey, 1) == 1;

        public static void SetBorderless(bool on)
        {
            PlayerPrefs.SetInt(PrefKey, on ? 1 : 0);
            PlayerPrefs.Save();
            Apply();
        }

        public static void Apply()
        {
            if (Borderless)
            {
                var r = Screen.currentResolution;
                Screen.SetResolution(r.width, r.height, FullScreenMode.FullScreenWindow);
            }
            else
                Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
        }
    }

    public class VolumeSlider : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        public Image Fill;
        public Text Label;
        RectTransform _rt;

        void Awake() => _rt = transform as RectTransform;

        public void OnPointerDown(PointerEventData e)
        {
            Set(e);
            Sfx.Ui();
        }

        public void OnDrag(PointerEventData e) => Set(e);

        void Set(PointerEventData e)
        {
            if (_rt == null) _rt = transform as RectTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, e.position, null, out var lp)) return;
            float t = Mathf.InverseLerp(-_rt.rect.width * 0.5f, _rt.rect.width * 0.5f, lp.x);
            Sfx.SetVolume(t);
            if (Fill != null) Fill.fillAmount = Sfx.Volume;
            if (Label != null) Label.text = Mathf.RoundToInt(Sfx.Volume * 100f) + "%";
        }
    }
}
