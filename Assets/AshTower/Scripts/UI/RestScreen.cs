using UnityEngine;
using UnityEngine.UI;

namespace AshTower
{
    public class RestScreen : MonoBehaviour
    {
        public Transform content;

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
            var panel = ScreenChrome.Panel(transform, "CAMP", 720, 520);
            content = ScreenChrome.Content(panel);
        }

        void Fill()
        {
            var run = AshTowerApp.I.Run;
            var panel = transform.Find("Panel");
            if (content == null) content = ScreenChrome.Content(panel);
            else UI.Clear(content);

            var body = UI.Txt(content, "Body", "There's a fire going. You can sleep, or you can hold a card in the coals until it changes.", 16, Theme.Cream, TextAnchor.MiddleCenter, FontStyle.Normal, true);
            UI.Place(body.transform, 580, 48, new Vector2(0.5f, 1), new Vector2(0, -68), new Vector2(0.5f, 1));

            int heal = Mathf.RoundToInt(run.MaxHp * 0.3f);
            UI.Btn(content, "Sleep", $"Rest   (+{heal} HP)", 360, 56, new Vector2(0.5f, 0.5f), new Vector2(0, 40), () =>
            {
                run.Heal(heal);
                AshTowerApp.I.BackToMap();
            });
            UI.Btn(content, "Smith", "Smith   (upgrade a card)", 360, 56, new Vector2(0.5f, 0.5f), new Vector2(0, -30), () =>
            {
                AshTowerApp.I.OpenUpgradePicker(ShowSmithed);
            });
            if (run.HasRelic("anvil_rite") && run.Girya < 2)
            {
                UI.Btn(content, "Stoke", $"Stoke   (+2 Heft, -5 Max HP, {run.Girya}/2)", 360, 48, new Vector2(0.5f, 0.5f), new Vector2(0, -100), () =>
                {
                    run.Girya++;
                    run.MaxHp = Mathf.Max(1, run.MaxHp - 5);
                    run.Hp = Mathf.Min(run.Hp, run.MaxHp);
                    run.Relics.Add(new RelicDef
                    {
                        Id = "stoke_str_" + run.Girya,
                        Name = "Stoked Coals",
                        Desc = "At the start of each fight, gain 2 Heft.",
                        CombatStart = c => c.Player.Add(StatusId.Heft, 2),
                        Rarity = CardRarity.Basic
                    });
                    AshTowerApp.I.BackToMap();
                }, 16);
            }
            UI.Btn(content, "Leave", "Leave", 160, 40, new Vector2(0.5f, 0), new Vector2(0, 36), () => AshTowerApp.I.BackToMap());
        }

        void ShowSmithed(CardRuntime card)
        {
            if (card == null)
            {
                AshTowerApp.I.BackToMap();
                return;
            }
            var panel = transform.Find("Panel");
            if (content == null && panel != null) content = ScreenChrome.Content(panel);
            else UI.Clear(content);

            var body = UI.Txt(content, "Body", "The coals take. The card comes back different.", 16, Theme.Cream, TextAnchor.MiddleCenter, FontStyle.Normal, true);
            UI.Place(body.transform, 580, 40, new Vector2(0.5f, 1), new Vector2(0, -56), new Vector2(0.5f, 1));
            var tag = UI.Txt(content, "Kind", "UPGRADED", 13, Theme.Gold, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UI.Place(tag.transform, 200, 20, new Vector2(0.5f, 0.5f), new Vector2(0, 150));
            var v = CardView.Create(content, card, 176, 248);
            v.LiftOnHover = true;
            v.Interactable = true;
            var rt = v.transform as RectTransform;
            UI.Place(rt, 176, 248, new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(0.5f, 0.5f));
            v.Home = rt.anchoredPosition;
            UI.Btn(content, "Continue", "Continue", 200, 44, new Vector2(0.5f, 0), new Vector2(0, 36), () => AshTowerApp.I.BackToMap());
        }
    }
}
