using UnityEngine;
using UnityEngine.UI;

namespace AshTower
{
    public class MapNodeView : MonoBehaviour
    {
        public Image disc;
        public Image icon;
        public Text bossLabel;
        public Button button;

        MapNode _node;

        public void Bind(MapNode node, bool available, bool current, bool seen)
        {
            _node = node;
            if (disc == null) disc = GetComponent<Image>();
            if (button == null) button = GetComponent<Button>();
            if (icon == null)
            {
                var ic = transform.Find("Icon");
                if (ic != null) icon = ic.GetComponent<Image>();
            }
            if (bossLabel == null)
            {
                var b = transform.Find("BossLabel");
                if (b != null) bossLabel = b.GetComponent<Text>();
            }

            float sz = node.Type == RoomType.Boss ? 86 : 64;
            var rt = transform as RectTransform;
            rt.sizeDelta = new Vector2(sz, sz);

            if (Theme.Circle == null) Theme.Init();
            disc.sprite = Theme.Circle;
            disc.color = MapScreen.NodeColor(node.Type, available, current, seen);
            disc.raycastTarget = available;

            if (icon != null)
            {
                icon.sprite = MapScreen.IconFor(node.Type);
                icon.preserveAspect = true;
            }

            if (bossLabel != null)
                bossLabel.gameObject.SetActive(node.Type == RoomType.Boss);

            if (button != null)
            {
                button.enabled = available;
                button.onClick.RemoveAllListeners();
                if (available)
                {
                    var captured = node;
                    button.onClick.AddListener(() =>
                    {
                        Sfx.Ui();
                        AshTowerApp.I.EnterNode(captured);
                    });
                }
            }
        }
    }
}
