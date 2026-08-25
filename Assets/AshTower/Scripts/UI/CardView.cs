using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AshTower
{
    public class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler,
        IInitializePotentialDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public CardRuntime Card;
        public Action<CardView> Clicked, Dropped;
        public bool Interactable = true;
        public bool LiftOnHover = true;
        public Vector2 Home;
        public float HomeRot;
        public int HomeSibling;
        public bool Hovering { get; private set; }
        public bool Dragging { get; private set; }
        bool CanDrag => Dropped != null;

        RectTransform _rt;
        CanvasGroup _cg;
        Text _cost, _name, _body, _type;
        Image _affordDim, _stripe, _artBg, _art, _rare, _gem, _bg;
        bool _homeSet;
        bool _previewUpgrade;
        Vector3 _homePos;
        Vector2 _pressScreen;
        Vector2 _grabLocal;
        bool _passedDragSlop;
        const float DragSlop = 18f;

        public static CardView Create(Transform parent, CardRuntime card, float w = 200, float h = 300)
        {
            CardView cv = null;
            if (UiLibrary.HasCard)
            {
                cv = Instantiate(UiLibrary.I.card, parent);
                UI.Place(cv.transform, w, h, new Vector2(0.5f, 0), Vector2.zero, new Vector2(0.5f, 0));
                cv.CacheParts();
                UI.Restore(cv.transform);
                if (cv._cost == null)
                {
                    UnityEngine.Object.Destroy(cv.gameObject);
                    cv = null;
                }
            }
            if (cv == null)
            {
                var g = UI.Go("Card", parent);
                UI.Place(g.transform, w, h, new Vector2(0.5f, 0), Vector2.zero, new Vector2(0.5f, 0));
                cv = g.AddComponent<CardView>();
                cv._rt = UI.RT(g);
                cv._cg = g.AddComponent<CanvasGroup>();
                cv.Card = card;
                cv.Build(w, h);
            }
            cv.Card = card;
            cv.Refresh();
            return cv;
        }

        public void CacheParts()
        {
            _rt = transform as RectTransform;
            _cg = GetComponent<CanvasGroup>();
            if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
            if (_cost == null)
            {
                var t = transform.Find("Cost") ?? transform.Find("cost");
                if (t != null) _cost = t.GetComponent<Text>();
            }
            if (_name == null)
            {
                var t = transform.Find("Name") ?? transform.Find("name");
                if (t != null) _name = t.GetComponent<Text>();
            }
            if (_body == null)
            {
                var t = transform.Find("Body") ?? transform.Find("body");
                if (t != null) _body = t.GetComponent<Text>();
            }
            if (_type == null)
            {
                var t = transform.Find("Type") ?? transform.Find("type");
                if (t != null) _type = t.GetComponent<Text>();
            }
            if (_affordDim == null)
            {
                var t = transform.Find("Dim") ?? transform.Find("dim");
                if (t != null) _affordDim = t.GetComponent<Image>();
            }
            if (_stripe == null)
            {
                var t = transform.Find("Stripe");
                if (t != null) _stripe = t.GetComponent<Image>();
            }
            if (_artBg == null)
            {
                var t = transform.Find("ArtBackground");
                if (t != null) _artBg = t.GetComponent<Image>();
            }
            if (_art == null)
            {
                var t = transform.Find("Art");
                if (t != null) _art = t.GetComponent<Image>();
            }
            if (_rare == null)
            {
                var t = transform.Find("Rare");
                if (t != null) _rare = t.GetComponent<Image>();
            }
            if (_gem == null)
            {
                var t = transform.Find("Gem");
                if (t != null) _gem = t.GetComponent<Image>();
            }
            if (_bg == null) _bg = GetComponent<Image>();
        }

        void Build(float w, float h)
        {
            _bg = gameObject.AddComponent<Image>();
            _bg.sprite = Theme.White;
            _bg.color = new Color(0.10f, 0.10f, 0.11f, 0.96f);
            _bg.raycastTarget = true;

            _stripe = UI.Img(transform, "Stripe", Card.Def.TypeColor);
            var sr = _stripe.rectTransform;
            sr.anchorMin = new Vector2(0, 0);
            sr.anchorMax = new Vector2(0, 1);
            sr.offsetMin = Vector2.zero;
            sr.offsetMax = new Vector2(8, 0);

            _gem = UI.Img(transform, "Gem", Theme.Energy, Theme.Circle);
            UI.Place(_gem.transform, 34, 34, new Vector2(0, 1), new Vector2(30, -22));
            _cost = UI.Txt(transform, "Cost", "0", 20, Theme.Ink, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UI.Place(_cost.transform, 34, 34, new Vector2(0, 1), new Vector2(30, -22));

            _name = UI.Txt(transform, "Name", "", 16, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold, false);
            UI.Place(_name.transform, w - 58, 28, new Vector2(0, 1), new Vector2(52, -22), new Vector2(0, 0.5f));

            _artBg = UI.Img(transform, "ArtBackground", Card.Def.TypeColor * 0.35f);
            UI.Place(_artBg.transform, w - 28, 72, new Vector2(0.5f, 1), new Vector2(4, -78));
            _art = UI.Img(transform, "Art", Card.Def.TypeColor, Theme.Circle);
            UI.Place(_art.transform, 40, 40, new Vector2(0.5f, 1), new Vector2(4, -78));

            _type = UI.Txt(transform, "Type", "", 13, Card.Def.TypeColor, TextAnchor.MiddleCenter, FontStyle.Bold, false);
            UI.Place(_type.transform, w - 24, 20, new Vector2(0.5f, 1), new Vector2(4, -128));

            var bodyBg = UI.Img(transform, "BodyBackground", new Color(0.07f, 0.07f, 0.08f, 1f));
            UI.Place(bodyBg.transform, w - 24, h - 160, new Vector2(0.5f, 0), new Vector2(4, 14), new Vector2(0.5f, 0));
            _body = UI.Txt(transform, "Body", "", 16, new Color(0.92f, 0.90f, 0.86f), TextAnchor.UpperLeft, FontStyle.Normal, true);
            UI.Place(_body.transform, w - 40, h - 176, new Vector2(0.5f, 0), new Vector2(4, 22), new Vector2(0.5f, 0));

            _rare = UI.Img(transform, "Rare", Theme.RarityColor(Card.Def.Rarity), Theme.Circle);
            UI.Place(_rare.transform, 10, 10, new Vector2(1, 1), new Vector2(-14, -14));

            _affordDim = UI.Img(transform, "Dim", new Color(0, 0, 0, 0.5f));
            UI.Stretch(_affordDim.transform);
            _affordDim.gameObject.SetActive(false);

            UI.Border(transform, new Color(0.75f, 0.75f, 0.75f, 0.35f), 1f);
        }

        public void Refresh()
        {
            if (Card?.Def == null) return;
            if (Theme.Circle == null) Theme.Init();
            if (_name == null) CacheParts();
            if (_name == null || _type == null || _body == null || _cost == null) return;
            var d = Card.Def;
            bool up = Card.Upgraded || _previewUpgrade;
            _name.text = d.DisplayName(up);
            _name.color = _previewUpgrade && !Card.Upgraded ? Theme.Gold : Color.white;
            _type.text = d.Type.ToString().ToUpper();
            _type.color = d.TypeColor;
            _body.text = d.DisplayText(up);
            if (d.Unplayable) _cost.text = "*";
            else if (d.XCost) _cost.text = "X";
            else _cost.text = AshTowerApp.I?.Combat != null && !_previewUpgrade
                ? Card.GetCost(AshTowerApp.I.Combat).ToString()
                : d.BaseCost(up).ToString();

            if (_bg != null)
            {
                _bg.sprite = Theme.White;
                _bg.color = new Color(0.10f, 0.10f, 0.11f, 0.96f);
            }
            if (_stripe != null) _stripe.color = d.TypeColor;
            if (_artBg != null) _artBg.color = d.TypeColor * 0.35f;
            if (_art != null)
            {
                _art.sprite = Theme.Circle;
                _art.color = d.TypeColor;
            }
            if (_rare != null)
            {
                _rare.sprite = Theme.Circle;
                _rare.color = Theme.RarityColor(d.Rarity);
            }
            if (_gem != null)
            {
                _gem.sprite = Theme.Circle;
                _gem.color = Theme.Energy;
            }
        }

        public void SetAffordable(bool yes)
        {
            if (_affordDim != null) _affordDim.gameObject.SetActive(!yes && Interactable);
            if (_cg != null) _cg.alpha = 1f;
        }

        public void SnapHome(bool instant = false)
        {
            if (_rt == null) return;
            _homeSet = true;
            _rt.anchoredPosition = Home;
            _rt.localRotation = Quaternion.Euler(0, 0, HomeRot);
            _rt.localScale = Vector3.one;
            _homePos = _rt.position;
            transform.SetSiblingIndex(Mathf.Clamp(HomeSibling, 0, transform.parent.childCount - 1));
        }

        void RememberLayout()
        {
            if (_homeSet || _rt == null) return;
            Home = _rt.anchoredPosition;
            HomeRot = _rt.localEulerAngles.z;
            if (HomeRot > 180f) HomeRot -= 360f;
            HomeSibling = transform.GetSiblingIndex();
            _homePos = _rt.position;
            _homeSet = true;
        }

        bool PointerOverMe()
        {
            if (_rt == null) return false;
            return RectTransformUtility.RectangleContainsScreenPoint(_rt, Input.mousePosition, null);
        }

        void ApplyHover(bool on)
        {
            if (_rt == null) return;
            if (on)
            {
                RememberLayout();
                transform.SetAsLastSibling();
                _rt.localScale = Vector3.one * 1.08f;
            }
            else
            {
                _rt.localScale = Vector3.one;
                _rt.anchoredPosition = Home;
                _rt.localRotation = Quaternion.Euler(0, 0, HomeRot);
                if (transform.parent != null)
                    transform.SetSiblingIndex(Mathf.Clamp(HomeSibling, 0, transform.parent.childCount - 1));
            }
        }

        public void OnPointerEnter(PointerEventData e)
        {
            if (!Interactable || Dragging) return;
            if (Hovering) return;
            Hovering = true;
            Sfx.Hover();
            if (LiftOnHover) ApplyHover(true);
        }

        public void OnPointerExit(PointerEventData e)
        {
            if (Dragging) return;
            if (PointerOverMe()) return;
            if (!Hovering) return;
            Hovering = false;
            if (_previewUpgrade)
            {
                _previewUpgrade = false;
                Refresh();
            }
            if (LiftOnHover) ApplyHover(false);
        }

        void LateUpdate()
        {
            if (Dragging || !Hovering || !LiftOnHover) return;
            if (!PointerOverMe())
            {
                Hovering = false;
                ApplyHover(false);
            }
        }

        public void OnPointerDown(PointerEventData e)
        {
            if (e.button != PointerEventData.InputButton.Right) return;
            if (Card?.Def == null || Card.Upgraded) return;
            _previewUpgrade = true;
            Refresh();
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.button != PointerEventData.InputButton.Right) return;
            if (!_previewUpgrade) return;
            _previewUpgrade = false;
            Refresh();
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (!Interactable || Dragging || _passedDragSlop) return;
            if (e.button == PointerEventData.InputButton.Left) Clicked?.Invoke(this);
        }

        public void OnInitializePotentialDrag(PointerEventData e)
        {
            if (!CanDrag) e.pointerDrag = null;
        }

        public void OnBeginDrag(PointerEventData e)
        {
            if (!Interactable || !CanDrag)
            {
                e.pointerDrag = null;
                return;
            }
            RememberLayout();
            _pressScreen = e.position;
            _passedDragSlop = false;
        }

        public void OnDrag(PointerEventData e)
        {
            if (!Interactable || !CanDrag) return;
            var parent = _rt.parent as RectTransform;
            if (parent == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, e.position, null, out var local)) return;
            if (!_passedDragSlop)
            {
                if ((e.position - _pressScreen).sqrMagnitude < DragSlop * DragSlop) return;
                _passedDragSlop = true;
                Dragging = true;
                Hovering = false;
                _cg.blocksRaycasts = false;
                transform.SetAsLastSibling();
                _rt.localRotation = Quaternion.identity;
                _rt.localScale = Vector3.one * 1.04f;
                _grabLocal = _rt.anchoredPosition - local;
            }
            _rt.anchoredPosition = local + _grabLocal;
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (!CanDrag) return;
            bool did = _passedDragSlop;
            _passedDragSlop = false;
            Dragging = false;
            _cg.blocksRaycasts = true;
            if (did) Dropped?.Invoke(this);
            else ApplyHover(Hovering && LiftOnHover);
        }
    }
}
