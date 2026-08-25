using UnityEngine;
using UnityEngine.UI;

namespace AshTower
{
    public class TreasureScreen : MonoBehaviour
    {
        public Transform content;
        RelicDef _relic;

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
            var panel = ScreenChrome.Panel(transform, "CHEST", 640, 420);
            content = ScreenChrome.Content(panel);
        }

        void Fill()
        {
            var run = AshTowerApp.I.Run;
            _relic = run.RandomRelic(CardRarity.Uncommon) ?? run.RandomRelic();
            var panel = transform.Find("Panel");
            if (content == null) content = ScreenChrome.Content(panel);
            else UI.Clear(content);

            var t = UI.Txt(content, "Name", _relic != null ? _relic.Name : "Dust", 26, Theme.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UI.Place(t.transform, 500, 36, new Vector2(0.5f, 0.5f), new Vector2(0, 40));
            var d = UI.Txt(content, "Desc", _relic != null ? _relic.Desc : "There's nothing in it but ash.", 16, Theme.Cream, TextAnchor.MiddleCenter);
            UI.Place(d.transform, 500, 80, new Vector2(0.5f, 0.5f), new Vector2(0, -20));
            UI.Btn(content, "Take", "Take", 200, 48, new Vector2(0.5f, 0), new Vector2(0, 50), () =>
            {
                if (_relic != null) run.AddRelic(_relic);
                AshTowerApp.I.BackToMap();
            });
        }
    }
}
