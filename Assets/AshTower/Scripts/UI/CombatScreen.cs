using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AshTower
{
    public class CombatScreen : MonoBehaviour
    {
        Transform _root;
        public RectTransform handRoot;
        public RectTransform floatRoot;
        public RectTransform enemiesRoot;
        public Transform playerHpRoot;
        public Transform potionsRoot;
        RectTransform _handRoot, _floatRoot, _playerRt;
        Image _playerGlow;
        readonly List<CardView> _views = new List<CardView>();
        readonly Dictionary<string, RectTransform> _enemyRt = new Dictionary<string, RectTransform>();
        CardRuntime _pending;
        bool _busy, _done;
        Text _energy, _drawN, _discN, _exN, _banner;
        CombatState C => AshTowerApp.I.Combat;

        public void Open()
        {
            gameObject.SetActive(true);
            StopAllCoroutines();
            _root = transform;
            _busy = false;
            _done = false;
            _pending = null;
            if (GetComponent<RuntimeChrome>() == null || UI.Broken(transform) || transform.Find("EndTurn") == null)
            {
                UI.Wipe(transform);
                Build();
                if (GetComponent<RuntimeChrome>() == null)
                    gameObject.AddComponent<RuntimeChrome>();
            }
            else
                CacheBuilt();
            UI.Restore(transform);
            var help = transform.Find("Help");
            if (help != null) Destroy(help.gameObject);
            if (_playerRt != null)
            {
                var pimg = _playerRt.GetComponent<Image>();
                if (pimg != null)
                {
                    pimg.sprite = Art.Get("knight");
                    pimg.preserveAspect = true;
                }
            }
            Refresh();
        }

        public void BuildChrome()
        {
            _root = transform;
            if (transform.Find("EndTurn") == null)
                Build();
            else
                CacheBuilt();
        }

        public void Close() => gameObject.SetActive(false);

        void CacheBuilt()
        {
            var hand = transform.Find("Hand");
            _handRoot = handRoot != null ? handRoot : (hand != null ? UI.RT(hand.gameObject) : null);
            var flo = transform.Find("Float");
            _floatRoot = floatRoot != null ? floatRoot : (flo != null ? UI.RT(flo.gameObject) : null);
            var player = transform.Find("Player");
            if (player != null) _playerRt = player as RectTransform;
            var glow = transform.Find("PlayerGlow");
            if (glow != null) _playerGlow = glow.GetComponent<Image>();
            var en = transform.Find("Energy");
            if (en != null) _energy = en.GetComponent<Text>();
            var draw = transform.Find("Draw/Count");
            if (draw != null) _drawN = draw.GetComponent<Text>();
            var disc = transform.Find("Discard/Count");
            if (disc != null) _discN = disc.GetComponent<Text>();
            var ex = transform.Find("ExhaustCount");
            if (ex != null) _exN = ex.GetComponent<Text>();
            var banner = transform.Find("Banner");
            if (banner != null) _banner = banner.GetComponent<Text>();
            var end = transform.Find("EndTurn");
            if (end != null)
            {
                var b = end.GetComponent<Button>();
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(EndTurn);
            }
            var drawBtn = transform.Find("Draw")?.GetComponent<Button>();
            if (drawBtn != null)
            {
                drawBtn.onClick.RemoveAllListeners();
                drawBtn.onClick.AddListener(() => { Sfx.Ui(); AshTowerApp.I.ShowPile("Draw Pile", C.DrawPile); });
            }
            var discBtn = transform.Find("Discard")?.GetComponent<Button>();
            if (discBtn != null)
            {
                discBtn.onClick.RemoveAllListeners();
                discBtn.onClick.AddListener(() => { Sfx.Ui(); AshTowerApp.I.ShowPile("Discard", C.Discard); });
            }
            if (_playerRt != null)
            {
                var pb = _playerRt.GetComponent<Button>();
                if (pb != null)
                {
                    pb.onClick.RemoveAllListeners();
                    pb.onClick.AddListener(OnPlayerClicked);
                }
            }
            var enemies = transform.Find("Enemies");
            if (enemies != null) enemiesRoot = enemies as RectTransform;
            var php = transform.Find("PlayerHp");
            if (php != null) playerHpRoot = php;
            var pots = transform.Find("Potions");
            if (pots != null) potionsRoot = pots;
        }

        void Update()
        {
            if (AshTowerApp.I == null || AshTowerApp.I.Screen != ScreenId.Combat || C == null || _busy) return;
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
                EndTurn();
            if (Input.GetKeyDown(KeyCode.Escape)) { _pending = null; Refresh(); }
            for (int i = 0; i < 9; i++)
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) && i < C.Hand.Count)
                    TryPlay(_views.FirstOrDefault(v => v.Card == C.Hand[i]));
            UpdatePlayerDropHint();
        }

        void Build()
        {
            var bg = UI.Img(_root, "Background", new Color(0.07f, 0.07f, 0.08f, 1f));
            UI.Stretch(bg.transform);
            var glow = UI.Img(_root, "Glow", new Color(0.35f, 0.12f, 0.04f, 0.35f), Theme.Circle);
            UI.Place(glow.transform, 1400, 700, new Vector2(0.5f, 0), new Vector2(0, -80));
            var veil = UI.Img(_root, "Veil", new Color(0, 0, 0, 0.15f));
            UI.Stretch(veil.transform);

            var player = UI.Img(_root, "Player", Color.white, Art.Get("knight"));
            player.preserveAspect = true;
            player.raycastTarget = true;
            UI.Place(player.transform, 200, 300, new Vector2(0, 0), new Vector2(160, 200), new Vector2(0.5f, 0));
            _playerRt = player.rectTransform;
            var pb = player.gameObject.AddComponent<Button>();
            pb.onClick.AddListener(OnPlayerClicked);
            _playerGlow = UI.Img(_root, "PlayerGlow", new Color(Theme.Gold.r, Theme.Gold.g, Theme.Gold.b, 0f), Theme.Circle);
            UI.Place(_playerGlow.transform, 280, 280, new Vector2(0, 0), new Vector2(160, 300));

            _handRoot = UI.RT(UI.Go("Hand", _root));
            UI.Stretch(_handRoot);
            handRoot = _handRoot;
            _floatRoot = UI.RT(UI.Go("Float", _root));
            UI.Stretch(_floatRoot);
            floatRoot = _floatRoot;

            var enemies = UI.Go("Enemies", _root);
            UI.Place(enemies.transform, 1200, 520, new Vector2(0.5f, 0.62f), new Vector2(80, 0));
            enemiesRoot = UI.RT(enemies);

            var php = UI.Go("PlayerHp", _root);
            UI.Place(php.transform, 180, 20, new Vector2(0, 0), new Vector2(210, 168));
            playerHpRoot = php.transform;

            var pots = UI.Go("Potions", _root);
            UI.Place(pots.transform, 180, 36, new Vector2(1, 1), new Vector2(-120, -92));
            potionsRoot = pots.transform;

            UI.Btn(_root, "EndTurn", "END TURN", 180, 58, new Vector2(1, 0.5f), new Vector2(-130, 40), EndTurn, 22);

            var eorb = UI.Img(_root, "EnergyOrb", Theme.Energy, Theme.Circle);
            UI.Place(eorb.transform, 18, 18, new Vector2(0, 0), new Vector2(28, 168));
            _energy = UI.Txt(_root, "Energy", "3 / 3", 22, Theme.Energy, TextAnchor.MiddleLeft, FontStyle.Bold, false);
            UI.Place(_energy.transform, 140, 28, new Vector2(0, 0), new Vector2(44, 168), new Vector2(0, 0.5f));
            UI.Outline(_energy, Theme.Ink);

            _drawN = Pile(_root, "Draw", new Vector2(0, 0), () => AshTowerApp.I.ShowPile("Draw Pile", C.DrawPile), new Vector2(40, 40), new Vector2(0, 0));
            _discN = Pile(_root, "Discard", new Vector2(1, 0), () => AshTowerApp.I.ShowPile("Discard", C.Discard), new Vector2(-40, 40), new Vector2(1, 0));
            _exN = UI.Txt(_root, "ExhaustCount", "Ex 0", 16, Theme.CreamDim, TextAnchor.MiddleRight, FontStyle.Normal, false);
            UI.Place(_exN.transform, 80, 24, new Vector2(1, 0), new Vector2(-40, 88), new Vector2(1, 0));

            _banner = UI.Txt(_root, "Banner", "", 22, Theme.Gold, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UI.Place(_banner.transform, 500, 28, new Vector2(0.5f, 1), new Vector2(0, -88), new Vector2(0.5f, 1));
            UI.Outline(_banner, Theme.Ink);

        }

        Text Pile(Transform root, string label, Vector2 anchor, System.Action click, Vector2? pos = null, Vector2? pivot = null)
        {
            var g = UI.Go(label, root);
            UI.Place(g.transform, 90, 70, anchor, pos ?? new Vector2(40, 40), pivot ?? new Vector2(0, 0));
            var img = g.AddComponent<Image>();
            img.sprite = Theme.White;
            img.color = Theme.PanelHi;
            img.raycastTarget = true;
            var b = g.AddComponent<Button>();
            b.onClick.AddListener(() => { Sfx.Ui(); click(); });
            var t = UI.Txt(g.transform, "Count", "0", 18, Theme.Cream, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UI.Stretch(t.transform);
            var lb = UI.Txt(g.transform, "Label", label, 12, Theme.Gold, TextAnchor.LowerCenter, FontStyle.Normal, false);
            UI.Place(lb.transform, 90, 16, new Vector2(0.5f, 0), new Vector2(0, -14));
            return t;
        }

        public void Refresh()
        {
            if (C == null || _root == null) return;
            DrainFx();
            _energy.text = $"{C.Energy}/{C.EnergyMax}";
            _drawN.text = C.DrawPile.Count.ToString();
            _discN.text = C.Discard.Count.ToString();
            _exN.text = "Ex " + C.ExhaustPile.Count;
            _banner.text = _pending != null ? "Choose a target" : (C.PlayerTurn ? "Your turn" : "Enemy turn");

            var php = playerHpRoot != null ? playerHpRoot : _root.Find("PlayerHp");
            if (php == null)
            {
                var g = UI.Go("PlayerHp", _root);
                UI.Place(g.transform, 180, 20, new Vector2(0, 0), new Vector2(210, 168));
                php = g.transform;
                playerHpRoot = php;
            }
            UI.Clear(php);
            UI.HpBar(php, C.Player, 180, 18, new Vector2(0.5f, 0.5f), Vector2.zero);
            StatusChip.LayoutRow(php, C.Player, new Vector2(0, -26), 400f);

            RebuildEnemies();
            RebuildHand();
            RebuildPotions();
            if (_handRoot != null) _handRoot.SetAsLastSibling();
            if (_floatRoot != null) _floatRoot.SetAsLastSibling();

            if (C.Over && !_busy) StartCoroutine(FinishSoon());
        }

        void RebuildEnemies()
        {
            Transform hostT = enemiesRoot != null ? enemiesRoot : _root.Find("Enemies");
            if (hostT == null)
            {
                var hostGo = UI.Go("Enemies", _root);
                UI.Place(hostGo.transform, 1200, 520, new Vector2(0.5f, 0.62f), new Vector2(80, 0));
                hostT = hostGo.transform;
                enemiesRoot = hostT as RectTransform;
            }
            UI.Clear(hostT);
            _enemyRt.Clear();
            var host = hostT;
            var list = C.Enemies;
            int n = list.Count;
            for (int i = 0; i < n; i++)
            {
                var e = list[i];
                float x = (i - (n - 1) * 0.5f) * 300f;
                if (UiLibrary.HasEnemy)
                {
                    var view = Instantiate(UiLibrary.I.enemy, host);
                    UI.Restore(view.transform);
                    UI.Place(view.transform, 230, 360, new Vector2(0.5f, 0.5f), new Vector2(x, 10));
                    view.Bind(e, C, OnEnemy, _pending != null);
                    _enemyRt[e.Uid] = UI.RT(view.gameObject);
                    continue;
                }
                var g = UI.Go("Enemy" + i, host);
                UI.Place(g.transform, 230, 360, new Vector2(0.5f, 0.5f), new Vector2(x, 10));
                _enemyRt[e.Uid] = UI.RT(g);

                var name = UI.Txt(g.transform, "Name", e.Name, 16, Theme.Cream, TextAnchor.MiddleCenter, FontStyle.Bold, false);
                UI.Place(name.transform, 220, 22, new Vector2(0.5f, 1), new Vector2(0, -4), new Vector2(0.5f, 1));
                UI.Outline(name, Theme.Ink);

                if (e.Alive && e.CurrentMove != null)
                {
                    var row = UI.Go("Intent", g.transform);
                    UI.Place(row.transform, 200, 30, new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(0.5f, 1));
                    var rowBg = row.AddComponent<Image>();
                    rowBg.sprite = Theme.White;
                    rowBg.color = new Color(0, 0, 0, 0.01f);
                    rowBg.raycastTarget = true;
                    var ic = UI.Img(row.transform, "Icon", Color.white, Art.Intent(e.CurrentMove.Intent));
                    ic.preserveAspect = true;
                    UI.Place(ic.transform, 26, 26, new Vector2(0, 0.5f), new Vector2(16, 0));
                    int dmg = C.IntentDamage(e);
                    string lab = StatusUtil.IntentShort(e.CurrentMove, dmg);
                    var it = UI.Txt(row.transform, "Label", lab, 15, Theme.EmberHi, TextAnchor.MiddleLeft, FontStyle.Bold, false);
                    UI.Place(it.transform, 150, 26, new Vector2(0, 0.5f), new Vector2(36, 0), new Vector2(0, 0.5f));
                    UI.Outline(it, Theme.Ink);
                    var itip = row.AddComponent<TooltipHover>();
                    itip.Title = e.CurrentMove.Name;
                    itip.Body = IntentTip(e);
                }

                var art = UI.Img(g.transform, "art", e.Alive ? Color.white : new Color(0.3f, 0.3f, 0.3f, 0.5f), Art.Enemy(e.ArtKey));
                art.preserveAspect = true;
                UI.Place(art.transform, 150, 168, new Vector2(0.5f, 1), new Vector2(0, -64), new Vector2(0.5f, 1));
                art.raycastTarget = e.Alive;
                if (e.Alive)
                {
                    var btn = art.gameObject.AddComponent<Button>();
                    var captured = e;
                    btn.onClick.AddListener(() => OnEnemy(captured));
                    if (_pending != null)
                    {
                        var ring = UI.Img(g.transform, "TargetRing", Theme.Gold, Theme.Circle);
                        UI.Place(ring.transform, 170, 170, new Vector2(0.5f, 1), new Vector2(0, -64), new Vector2(0.5f, 1));
                        ring.color = new Color(Theme.Gold.r, Theme.Gold.g, Theme.Gold.b, 0.22f);
                    }
                }

                UI.HpBar(g.transform, e, 190, 16, new Vector2(0.5f, 0), new Vector2(0, 28));
                StatusChip.LayoutRow(g.transform, e, new Vector2(0, 54), 220f);
                if (!e.Alive)
                {
                    var dead = UI.Txt(g.transform, "Dead", "DEAD", 20, Theme.Blood, TextAnchor.MiddleCenter, FontStyle.Bold, false);
                    UI.Place(dead.transform, 180, 26, new Vector2(0.5f, 0.5f), new Vector2(0, 20));
                }
            }
        }

        string IntentTip(Combatant e)
        {
            if (e.CurrentMove == null) return "";
            var tip = StatusUtil.IntentTip(e.CurrentMove, C.IntentDamage(e));
            var m = e.CurrentMove;
            if (m.Intent == IntentKind.Attack || m.Intent == IntentKind.AttackDebuff)
            {
                if (C.Player.Get(StatusId.Brittle) > 0) tip += " Hits through Block.";
                if (e.Get(StatusId.Dulled) > 0) tip += " Recoil " + e.Get(StatusId.Dulled) + ".";
            }
            if (e.Get(StatusId.Heft) > 0) tip += " Then Heft burns you for " + e.Get(StatusId.Heft) + ".";
            return tip;
        }

        void RebuildHand()
        {
            UI.Clear(_handRoot);
            _views.Clear();
            int n = C.Hand.Count;
            for (int i = 0; i < n; i++)
            {
                var card = C.Hand[i];
                var v = CardView.Create(_handRoot, card, 196, 294);
                float t = n == 1 ? 0.5f : (float)i / (n - 1);
                float x = Mathf.Lerp(-(n - 1) * 78f, (n - 1) * 78f, t);
                float y = 18f - Mathf.Abs(t - 0.5f) * 36f;
                float rot = Mathf.Lerp(10f, -10f, t);
                v.Home = new Vector2(x, y);
                v.HomeRot = rot;
                v.HomeSibling = i;
                v.LiftOnHover = true;
                v.Interactable = C.PlayerTurn;
                v.Clicked = TryPlay;
                v.Dropped = OnDrop;
                var rt = v.transform as RectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0);
                rt.pivot = new Vector2(0.5f, 0);
                v.SnapHome(true);
                bool aff = C.CanPlay(card);
                v.SetAffordable(aff);
                _views.Add(v);
            }
        }

        void RebuildPotions()
        {
            var hostT = potionsRoot != null ? potionsRoot : _root.Find("Potions");
            if (hostT == null)
            {
                var g = UI.Go("Potions", _root);
                UI.Place(g.transform, 180, 36, new Vector2(1, 1), new Vector2(-120, -92));
                hostT = g.transform;
                potionsRoot = hostT;
            }
            UI.Clear(hostT);
            var host = hostT.gameObject;
            var pots = AshTowerApp.I.Run.Potions;
            for (int i = 0; i < RunState.PotionSlots; i++)
            {
                var g = UI.Go("Slot" + i, host.transform);
                UI.Place(g.transform, 42, 42, new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 48, 0));
                var img = g.AddComponent<Image>();
                img.sprite = Theme.Circle;
                img.color = i < pots.Count ? Theme.Ember : new Color(0.2f, 0.2f, 0.2f, 0.7f);
                img.raycastTarget = i < pots.Count;
                if (i < pots.Count)
                {
                    int idx = i;
                    var b = g.AddComponent<Button>();
                    b.onClick.AddListener(() => UsePotion(idx));
                    var th = g.AddComponent<TooltipHover>();
                    th.Title = pots[i].Name;
                    th.Body = pots[i].Desc;
                }
            }
        }

        void UsePotion(int idx)
        {
            if (_busy || !C.PlayerTurn) return;
            var run = AshTowerApp.I.Run;
            if (idx < 0 || idx >= run.Potions.Count) return;
            var p = run.Potions[idx];
            run.Potions.RemoveAt(idx);
            p.Use(C);
            Sfx.Energy();
            Refresh();
        }

        void TryPlay(CardView v)
        {
            if (v == null || _busy || !C.PlayerTurn || C.Over) return;
            var card = v.Card;
            if (!C.CanPlay(card)) { Sfx.Ui(); return; }
            if (C.NeedsTarget(card))
            {
                _pending = card;
                Refresh();
                return;
            }
            DoPlay(card, C.AliveEnemies.FirstOrDefault());
        }

        void OnEnemy(Combatant e)
        {
            if (_busy || !e.Alive) return;
            if (_pending != null)
            {
                var card = _pending;
                _pending = null;
                DoPlay(card, e);
            }
        }

        void OnPlayerClicked()
        {
            if (_busy || !C.PlayerTurn || C.Over) return;
            if (_pending != null && IsSelfCard(_pending))
            {
                var card = _pending;
                _pending = null;
                DoPlay(card, null);
            }
        }

        static bool IsSelfCard(CardRuntime card)
        {
            if (card?.Def == null) return false;
            return card.Def.Target == TargetMode.None;
        }

        bool OverPlayer(Vector2 screen)
        {
            if (_playerRt == null) return false;
            if (RectTransformUtility.RectangleContainsScreenPoint(_playerRt, screen, null)) return true;
            if (_playerGlow != null && RectTransformUtility.RectangleContainsScreenPoint(_playerGlow.rectTransform, screen, null))
                return true;
            return false;
        }

        Combatant EnemyUnder(Vector2 screen)
        {
            foreach (var kv in _enemyRt)
            {
                var e = C.Enemies.FirstOrDefault(x => x.Uid == kv.Key);
                if (e == null || !e.Alive || kv.Value == null) continue;
                if (RectTransformUtility.RectangleContainsScreenPoint(kv.Value, screen, null))
                    return e;
            }
            return null;
        }

        void UpdatePlayerDropHint()
        {
            if (_playerGlow == null) return;
            bool draggingSelf = _views.Any(v => v != null && v.Dragging && IsSelfCard(v.Card));
            float a = draggingSelf ? 0.35f : 0f;
            var c = _playerGlow.color;
            c.a = Mathf.MoveTowards(c.a, a, Time.deltaTime * 4f);
            _playerGlow.color = c;
        }

        void OnDrop(CardView v)
        {
            if (_busy || !C.PlayerTurn) { Refresh(); return; }
            var card = v.Card;
            if (!C.CanPlay(card)) { Refresh(); return; }
            var pos = (Vector2)Input.mousePosition;
            var hit = EnemyUnder(pos);

            if (card.Def.Target == TargetMode.Enemy)
            {
                if (hit == null && C.AliveCount == 1) hit = C.AliveEnemies.First();
                if (hit == null) { Refresh(); return; }
                DoPlay(card, hit);
                return;
            }

            if (card.Def.Target == TargetMode.AllEnemies || card.Def.Target == TargetMode.RandomEnemy)
            {
                if (hit != null || OverPlayer(pos) || InPlayArea(pos))
                    DoPlay(card, hit);
                else Refresh();
                return;
            }

            if (OverPlayer(pos) || InPlayArea(pos))
                DoPlay(card, null);
            else
                Refresh();
        }

        static bool InPlayArea(Vector2 screen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(UI.Root, screen, null, out var local);
            return local.y > -40f;
        }

        void DoPlay(CardRuntime card, Combatant t)
        {
            C.Play(card, t);
            Sfx.PlayCard();
            _pending = null;
            Refresh();
        }

        void DrainFx()
        {
            if (C.Fx.Count == 0) return;
            foreach (var fx in C.Fx)
            {
                if (fx.Type == CombatFx.Kind.Damage) Sfx.Hit();
                if (fx.Type == CombatFx.Kind.Block) Sfx.Block();
                if (fx.Type == CombatFx.Kind.Death) Sfx.Death();
                if (fx.Type == CombatFx.Kind.Draw) Sfx.Draw();
                SpawnFloater(fx);
            }
            C.Fx.Clear();
        }

        void SpawnFloater(CombatFx fx)
        {
            if (string.IsNullOrEmpty(fx.Msg)) return;
            Vector2 pos = new Vector2(0, 80);
            if (fx.Who != null && fx.Who.IsPlayer) pos = new Vector2(-700, -120);
            else if (fx.Who != null && _enemyRt.TryGetValue(fx.Who.Uid, out var rt))
                pos = rt.anchoredPosition + new Vector2(80, 80);
            var t = UI.Txt(_floatRoot, "Floater", fx.Msg, 26, fx.Type == CombatFx.Kind.Damage ? Theme.EmberHi : Theme.Cream, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UI.Place(t.transform, 220, 40, new Vector2(0.5f, 0.5f), pos);
            UI.Outline(t, Theme.Ink, 1.6f);
            StartCoroutine(FloatUp(t.rectTransform));
        }

        IEnumerator FloatUp(RectTransform rt)
        {
            var p = rt.anchoredPosition;
            float t = 0;
            var tx = rt.GetComponent<Text>();
            while (t < 0.8f && rt != null)
            {
                t += Time.deltaTime;
                rt.anchoredPosition = p + new Vector2(0, t * 70);
                if (tx != null) tx.color = new Color(tx.color.r, tx.color.g, tx.color.b, 1f - t / 0.8f);
                yield return null;
            }
            if (rt != null) Destroy(rt.gameObject);
        }

        void EndTurn()
        {
            if (_busy || C == null || !C.PlayerTurn || C.Over) return;
            StartCoroutine(EndTurnCo());
        }

        IEnumerator EndTurnCo()
        {
            _busy = true;
            _pending = null;
            if (UnityEngine.EventSystems.EventSystem.current != null)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            try
            {
                C.EndPlayerTurn();
                Refresh();
                yield return new WaitForSeconds(0.25f);
                foreach (var e in C.Enemies.ToList())
                {
                    if (!e.Alive || C.Over) continue;
                    if (_enemyRt.TryGetValue(e.Uid, out var rt) && rt != null)
                        yield return Punch(rt);
                    C.ExecuteEnemy(e);
                    Refresh();
                    yield return new WaitForSeconds(0.4f);
                }
                if (!C.Over)
                    C.StartPlayerTurn(false);
            }
            finally
            {
                _busy = false;
            }
            if (!C.Over) Refresh();
            else StartCoroutine(FinishSoon());
        }

        IEnumerator Punch(RectTransform rt)
        {
            if (rt == null) yield break;
            var s = rt.localScale;
            rt.localScale = s * 1.08f;
            yield return new WaitForSeconds(0.08f);
            if (rt != null) rt.localScale = s;
        }

        IEnumerator FinishSoon()
        {
            if (_done) yield break;
            _done = true;
            _busy = true;
            yield return new WaitForSeconds(0.9f);
            if (C.Won) Sfx.Win();
            AshTowerApp.I.CombatFinished(C.Won);
        }
    }
}
