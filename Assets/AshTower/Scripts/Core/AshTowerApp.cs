using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AshTower
{
    public class AshTowerApp : MonoBehaviour
    {
        public static AshTowerApp I;
        public RunState Run;
        public CombatState Combat;
        public ScreenId Screen;
        public RectTransform Overlay;

        public TitleScreen titleScreen;
        public MapScreen mapScreen;
        public CombatScreen combatScreen;
        public ShopScreen shopScreen;
        public RestScreen restScreen;
        public EventScreen eventScreen;
        public RewardsScreen rewardsScreen;
        public TreasureScreen treasureScreen;
        public EndScreen endScreen;
        public SettingsScreen settingsScreen;
        public CardPicker cardPicker;
        public HudBar hud;
        public GameEvent CurrentEvent;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<AshTowerApp>() != null) return;
            Catalog.Build();
            Theme.Init();
            var go = new GameObject("Ash Tower");
            DontDestroyOnLoad(go);
            go.AddComponent<AshTowerApp>();
        }

        void Awake()
        {
            I = this;
            Catalog.Build();
            Theme.Init();
            GameDisplay.Apply();
            EnsureEventSystem();
            if (!BindScene())
                BuildCanvas();
            Sfx.Init(transform);
            HideScreens();
            Show(ScreenId.Title);
        }

        void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.transform.SetParent(transform, false);
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        bool BindScene()
        {
            UI.Canvas = GetComponentInChildren<Canvas>(true);
            if (UI.Canvas == null) return false;
            UI.Root = UI.Canvas.transform as RectTransform;
            var canvas = UI.Canvas.transform;

            var overlayT = canvas.Find("Overlay");
            if (overlayT == null)
            {
                var g = UI.Go("Overlay", canvas);
                UI.Stretch(g.transform);
                overlayT = g.transform;
            }
            Overlay = overlayT as RectTransform;

            titleScreen = titleScreen ?? canvas.GetComponentInChildren<TitleScreen>(true);
            mapScreen = mapScreen ?? canvas.GetComponentInChildren<MapScreen>(true);
            combatScreen = combatScreen ?? canvas.GetComponentInChildren<CombatScreen>(true);
            shopScreen = Bind<ShopScreen>(canvas, "Shop", shopScreen);
            restScreen = Bind<RestScreen>(canvas, "Rest", restScreen);
            eventScreen = Bind<EventScreen>(canvas, "Event", eventScreen);
            rewardsScreen = Bind<RewardsScreen>(canvas, "Rewards", rewardsScreen);
            treasureScreen = Bind<TreasureScreen>(canvas, "Treasure", treasureScreen);
            endScreen = Bind<EndScreen>(canvas, "End", endScreen);
            settingsScreen = Bind<SettingsScreen>(canvas, "Settings", settingsScreen);
            cardPicker = Bind<CardPicker>(canvas, "Picker", cardPicker);
            hud = Bind<HudBar>(canvas, "Hud", hud);
            if (settingsScreen != null) settingsScreen.gameObject.SetActive(false);
            if (cardPicker != null) cardPicker.gameObject.SetActive(false);
            if (hud != null) hud.gameObject.SetActive(false);
            return titleScreen != null;
        }

        void BuildCanvas()
        {
            var cgo = new GameObject("Canvas", typeof(RectTransform));
            cgo.transform.SetParent(transform, false);
            UI.Canvas = cgo.AddComponent<Canvas>();
            UI.Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            UI.Canvas.sortingOrder = 100;
            var sc = cgo.AddComponent<UnityEngine.UI.CanvasScaler>();
            sc.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1920, 1080);
            sc.matchWidthOrHeight = 1f;
            cgo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            UI.Root = cgo.GetComponent<RectTransform>();

            Overlay = UI.RT(UI.Go("Overlay", UI.Root));
            UI.Stretch(Overlay);

            titleScreen = ScreenGo<TitleScreen>("Title", true);
            mapScreen = ScreenGo<MapScreen>("Map", false);
            combatScreen = ScreenGo<CombatScreen>("Combat", false);
            shopScreen = ScreenGo<ShopScreen>("Shop", false);
            restScreen = ScreenGo<RestScreen>("Rest", false);
            eventScreen = ScreenGo<EventScreen>("Event", false);
            rewardsScreen = ScreenGo<RewardsScreen>("Rewards", false);
            treasureScreen = ScreenGo<TreasureScreen>("Treasure", false);
            endScreen = ScreenGo<EndScreen>("End", false);
            settingsScreen = ScreenGo<SettingsScreen>("Settings", false);
            cardPicker = ScreenGo<CardPicker>("Picker", false);
            hud = ScreenGo<HudBar>("Hud", false);
        }

        T ScreenGo<T>(string name, bool on) where T : MonoBehaviour
        {
            var g = UI.Go(name, UI.Root);
            UI.Stretch(g.transform);
            g.SetActive(on);
            return g.AddComponent<T>();
        }

        static T Bind<T>(Transform canvas, string name, T existing) where T : MonoBehaviour
        {
            if (existing != null) return existing;
            var t = canvas.Find(name);
            if (t == null)
            {
                var g = UI.Go(name, canvas);
                UI.Stretch(g.transform);
                g.SetActive(false);
                t = g.transform;
            }
            var c = t.GetComponent<T>();
            return c != null ? c : t.gameObject.AddComponent<T>();
        }

        void HideScreens()
        {
            titleScreen?.Close();
            mapScreen?.Close();
            combatScreen?.Close();
            shopScreen?.Close();
            restScreen?.Close();
            eventScreen?.Close();
            rewardsScreen?.Close();
            treasureScreen?.Close();
            endScreen?.Close();
            settingsScreen?.Close();
            cardPicker?.Close();
            hud?.Hide();
            ClearOverlay();
        }

        void ClearOverlay()
        {
            if (Overlay == null) return;
            for (int i = Overlay.childCount - 1; i >= 0; i--)
                Destroy(Overlay.GetChild(i).gameObject);
        }

        public void Show(ScreenId id)
        {
            Screen = id;
            if (UI.Root != null) UI.Root.anchoredPosition = Vector2.zero;
            HideScreens();
            bool showHud = id != ScreenId.Title && id != ScreenId.GameOver && id != ScreenId.Victory;
            if (showHud && hud != null && Run != null) hud.Show(Run);

            switch (id)
            {
                case ScreenId.Title: titleScreen.Open(); break;
                case ScreenId.Map: mapScreen.Open(); break;
                case ScreenId.Combat: combatScreen.Open(); break;
                case ScreenId.Rewards: rewardsScreen.Open(); break;
                case ScreenId.Shop: shopScreen.Open(); break;
                case ScreenId.Rest: restScreen.Open(); break;
                case ScreenId.Event: eventScreen.Open(); break;
                case ScreenId.Treasure: treasureScreen.Open(); break;
                case ScreenId.GameOver: endScreen.Open(false); break;
                case ScreenId.Victory: endScreen.Open(true); break;
            }
        }

        void Update()
        {
            if (Screen != ScreenId.Title) return;
            if (Input.GetKeyDown(KeyCode.L))
                CheatBoss();
        }

        public void NewRun()
        {
            Run = new RunState();
            Run.NewRun(Environment.TickCount);
            Show(ScreenId.Map);
        }

        void CheatBoss()
        {
            Run = new RunState();
            Run.NewRun(Environment.TickCount);
            Run.Floor = 15;
            Combat = new CombatState();
            Combat.Begin(Run, new Encounter { Id = "boss", Boss = true, EnemyIds = { "ash_warden" } });
            foreach (var e in Combat.Enemies)
                e.Hp = 1;
            Show(ScreenId.Combat);
        }

        public void EnterNode(MapNode n)
        {
            if (!Run.CanEnter(n)) return;
            Run.Enter(n);
            switch (n.Type)
            {
                case RoomType.Monster:
                case RoomType.Elite:
                case RoomType.Boss:
                    StartCombat(Catalog.EncounterFor(n.Type, n.Row, Run.Rng));
                    break;
                case RoomType.Rest:
                    if (Run.HasRelic("rest_ember")) Run.Heal(8);
                    Show(ScreenId.Rest);
                    break;
                case RoomType.Shop: Show(ScreenId.Shop); break;
                case RoomType.Event:
                    PrepareEvent();
                    Show(ScreenId.Event);
                    break;
                case RoomType.Treasure: Show(ScreenId.Treasure); break;
            }
        }

        public void StartCombat(Encounter enc)
        {
            Combat = new CombatState();
            Combat.Begin(Run, enc);
            Show(ScreenId.Combat);
        }

        public void CombatFinished(bool won)
        {
            if (!won) { Show(ScreenId.GameOver); return; }
            if (Combat.Boss)
            {
                Run.BossDefeated = true;
                Run.Hp = Combat.Player.Hp;
                Show(ScreenId.Victory);
                return;
            }
            if (Combat.Elite) Run.ElitesKilled++;
            else Run.MonstersKilled++;
            Run.Hp = Combat.Player.Hp;
            int gold = Combat.Elite ? Run.Rng.Next(25, 36) : Run.Rng.Next(10, 21);
            Run.Gold += gold;
            foreach (var r in Run.Relics) r.AfterCombat?.Invoke(Run, Combat.Elite);
            Show(ScreenId.Rewards);
        }

        public void BackToMap() => Show(ScreenId.Map);

        public void OpenUpgradePicker(Action after = null) => OpenUpgradePicker(_ => after?.Invoke());

        public void OpenUpgradePicker(Action<CardRuntime> after)
        {
            var list = Run.Deck.Where(c => !c.Upgraded && c.Def.Type != CardType.Status && c.Def.Type != CardType.Curse).ToList();
            if (list.Count == 0) { after?.Invoke(null); return; }
            cardPicker.Open("Upgrade", list, pick =>
            {
                pick.Upgraded = true;
                after?.Invoke(pick);
            });
        }

        public void OpenRemovePicker(Action after = null)
        {
            cardPicker.Open("Remove a card", Run.Deck.ToList(), pick =>
            {
                Run.RemoveCard(pick);
                after?.Invoke();
            });
        }

        public void PrepareEvent()
        {
            CurrentEvent = Catalog.AllEvents[Run.Rng.Next(Catalog.AllEvents.Count)];
        }

        public void ShowDeck()
        {
            if (Run == null) return;
            cardPicker.Open("Deck", Run.Deck.ToList(), _ => { });
        }

        public void OpenSettings() => settingsScreen.Open();

        public void ShowPile(string title, System.Collections.Generic.List<CardRuntime> cards)
        {
            cardPicker.Open(title, cards, _ => { });
        }
    }
}
