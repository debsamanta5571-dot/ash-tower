using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AshTower.Editor
{
    public static class UiBaker
    {
        const string PrefabFolder = "Assets/AshTower/Prefabs";
        const string ResourcesFolder = "Assets/AshTower/Resources";
        const string ScenePath = "Assets/Scenes/AshTower.unity";

        [MenuItem("Ash Tower/Bake UI Prefabs And Scene")]
        public static void Bake()
        {
            Theme.Init();
            Catalog.Build();
            if (!AssetDatabase.IsValidFolder("Assets/AshTower"))
                AssetDatabase.CreateFolder("Assets", "AshTower");
            if (!AssetDatabase.IsValidFolder("Assets/AshTower/Prefabs"))
                AssetDatabase.CreateFolder("Assets/AshTower", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/AshTower/Resources"))
                AssetDatabase.CreateFolder("Assets/AshTower", "Resources");

            AssetDatabase.DeleteAsset(ResourcesFolder + "/UiLibrary.asset");

            var lib = ScriptableObject.CreateInstance<UiLibrary>();
            lib.card = BakeCard();
            lib.enemy = BakeEnemy();
            lib.mapNode = BakeMapNode();
            lib.statusChip = BakeChip();
            AssetDatabase.CreateAsset(lib, ResourcesFolder + "/UiLibrary.asset");

            BakeScene(lib);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Ash Tower UI baked into the scene and prefabs.");
            if (Application.isBatchMode)
                EditorApplication.Exit(0);
        }

        static CardView BakeCard()
        {
            var canvas = TempCanvas();
            var dummy = new CardRuntime { Def = Catalog.Card("ember_cut") };
            var cv = CardView.Create(canvas.transform, dummy);
            cv.gameObject.name = "Card";
            var prefab = PrefabUtility.SaveAsPrefabAsset(cv.gameObject, PrefabFolder + "/Card.prefab").GetComponent<CardView>();
            Object.DestroyImmediate(canvas);
            return prefab;
        }

        static EnemyView BakeEnemy()
        {
            var canvas = TempCanvas();
            var g = UI.Go("Enemy", canvas.transform);
            UI.Place(g.transform, 230, 360, new Vector2(0.5f, 0.5f), Vector2.zero);
            var view = g.AddComponent<EnemyView>();
            var name = UI.Txt(g.transform, "Name", "Enemy", 16, Theme.Cream, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UI.Place(name.transform, 220, 22, new Vector2(0.5f, 1), new Vector2(0, -4), new Vector2(0.5f, 1));
            view.nameLabel = name;
            var row = UI.Go("Intent", g.transform);
            UI.Place(row.transform, 200, 30, new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(0.5f, 1));
            var rowBg = row.AddComponent<Image>();
            rowBg.color = new Color(0, 0, 0, 0.01f);
            rowBg.raycastTarget = true;
            view.intentRow = row;
            var ic = UI.Img(row.transform, "Icon", Color.white);
            UI.Place(ic.transform, 26, 26, new Vector2(0, 0.5f), new Vector2(16, 0));
            view.intentIcon = ic;
            var it = UI.Txt(row.transform, "Label", "", 15, Theme.EmberHi, TextAnchor.MiddleLeft, FontStyle.Bold, false);
            UI.Place(it.transform, 150, 26, new Vector2(0, 0.5f), new Vector2(36, 0), new Vector2(0, 0.5f));
            view.intentLabel = it;
            var art = UI.Img(g.transform, "Art", Color.white);
            art.preserveAspect = true;
            UI.Place(art.transform, 150, 168, new Vector2(0.5f, 1), new Vector2(0, -64), new Vector2(0.5f, 1));
            art.raycastTarget = true;
            view.art = art;
            view.artButton = art.gameObject.AddComponent<Button>();
            var ring = UI.Img(g.transform, "TargetRing", Theme.Gold, Theme.Circle);
            UI.Place(ring.transform, 170, 170, new Vector2(0.5f, 1), new Vector2(0, -64), new Vector2(0.5f, 1));
            view.targetRing = ring;
            var hp = UI.Go("Hp", g.transform);
            UI.Place(hp.transform, 190, 20, new Vector2(0.5f, 0), new Vector2(0, 28));
            view.hpRoot = hp.transform;
            var st = UI.Go("Status", g.transform);
            UI.Place(st.transform, 200, 24, new Vector2(0.5f, 0), new Vector2(0, 50));
            view.statusRoot = st.transform;
            var dead = UI.Txt(g.transform, "Dead", "DEAD", 20, Theme.Blood, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UI.Place(dead.transform, 180, 26, new Vector2(0.5f, 0.5f), new Vector2(0, 20));
            view.deadLabel = dead;
            var prefab = PrefabUtility.SaveAsPrefabAsset(g, PrefabFolder + "/Enemy.prefab").GetComponent<EnemyView>();
            Object.DestroyImmediate(canvas);
            return prefab;
        }

        static MapNodeView BakeMapNode()
        {
            var canvas = TempCanvas();
            var g = UI.Go("MapNode", canvas.transform);
            UI.Place(g.transform, 64, 64, new Vector2(0.5f, 0.5f), Vector2.zero);
            var view = g.AddComponent<MapNodeView>();
            var img = g.AddComponent<Image>();
            img.sprite = Theme.Circle;
            view.disc = img;
            view.button = g.AddComponent<Button>();
            var ico = UI.Img(g.transform, "Icon", Color.white);
            ico.preserveAspect = true;
            UI.Stretch(ico.transform, 10);
            view.icon = ico;
            var lb = UI.Txt(g.transform, "BossLabel", "BOSS", 12, Theme.Cream, TextAnchor.LowerCenter, FontStyle.Bold, false);
            UI.Place(lb.transform, 80, 18, new Vector2(0.5f, 0), new Vector2(0, -16));
            view.bossLabel = lb;
            var prefab = PrefabUtility.SaveAsPrefabAsset(g, PrefabFolder + "/MapNode.prefab").GetComponent<MapNodeView>();
            Object.DestroyImmediate(canvas);
            return prefab;
        }

        static StatusChip BakeChip()
        {
            var canvas = TempCanvas();
            var chip = StatusChip.Create(canvas.transform, StatusId.Heft, 1, Vector2.zero);
            chip.gameObject.name = "StatusChip";
            var prefab = PrefabUtility.SaveAsPrefabAsset(chip.gameObject, PrefabFolder + "/StatusChip.prefab").GetComponent<StatusChip>();
            Object.DestroyImmediate(canvas);
            return prefab;
        }

        static GameObject TempCanvas()
        {
            var cgo = new GameObject("BakeCanvas", typeof(RectTransform));
            var canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            return cgo;
        }

        static void BakeScene(UiLibrary lib)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var old = GameObject.Find("Ash Tower");
            if (old != null) Object.DestroyImmediate(old);

            var root = new GameObject("Ash Tower");
            var app = root.AddComponent<AshTowerApp>();

            var es = new GameObject("EventSystem");
            es.transform.SetParent(root.transform, false);
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();

            var cgo = new GameObject("Canvas", typeof(RectTransform));
            cgo.transform.SetParent(root.transform, false);
            var canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var sc = cgo.AddComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1920, 1080);
            sc.matchWidthOrHeight = 1f;
            cgo.AddComponent<GraphicRaycaster>();
            UI.Canvas = canvas;
            UI.Root = cgo.GetComponent<RectTransform>();

            var overlay = UI.Go("Overlay", UI.Root);
            UI.Stretch(overlay.transform);

            var title = ScreenGo("Title").AddComponent<TitleScreen>();
            title.Build();

            var mapGo = ScreenGo("Map");
            var map = mapGo.AddComponent<MapScreen>();
            map.BuildChrome();
            mapGo.SetActive(false);

            var combatGo = ScreenGo("Combat");
            var combat = combatGo.AddComponent<CombatScreen>();
            combat.BuildChrome();
            combatGo.SetActive(false);

            var shop = Screen<ShopScreen>("Shop");
            shop.BuildChrome();
            var rest = Screen<RestScreen>("Rest");
            rest.BuildChrome();
            var ev = Screen<EventScreen>("Event");
            ev.BuildChrome();
            var rewards = Screen<RewardsScreen>("Rewards");
            rewards.BuildChrome();
            var treasure = Screen<TreasureScreen>("Treasure");
            treasure.BuildChrome();
            var end = Screen<EndScreen>("End");
            end.BuildChrome();

            var hudGo = ScreenGo("Hud");
            var hud = hudGo.AddComponent<HudBar>();
            hud.Build();
            hudGo.SetActive(false);

            var settingsGo = ScreenGo("Settings");
            var settings = settingsGo.AddComponent<SettingsScreen>();
            settings.BuildChrome();
            settingsGo.SetActive(false);

            var pickerGo = ScreenGo("Picker");
            var picker = pickerGo.AddComponent<CardPicker>();
            picker.BuildChrome();
            pickerGo.SetActive(false);

            app.titleScreen = title;
            app.mapScreen = map;
            app.combatScreen = combat;
            app.shopScreen = shop;
            app.restScreen = rest;
            app.eventScreen = ev;
            app.rewardsScreen = rewards;
            app.treasureScreen = treasure;
            app.endScreen = end;
            app.settingsScreen = settings;
            app.cardPicker = picker;
            app.hud = hud;
            app.Overlay = overlay.GetComponent<RectTransform>();

            var cam = GameObject.Find("Main Camera");
            if (cam != null) cam.name = "Camera";
            var light2d = GameObject.Find("Global Light 2D");
            if (light2d != null) light2d.name = "Light2D";

            EditorUtility.SetDirty(app);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static GameObject ScreenGo(string name)
        {
            var g = UI.Go(name, UI.Root);
            UI.Stretch(g.transform);
            return g;
        }

        static T Screen<T>(string name) where T : MonoBehaviour
        {
            var g = ScreenGo(name);
            g.SetActive(false);
            return g.AddComponent<T>();
        }
    }
}
