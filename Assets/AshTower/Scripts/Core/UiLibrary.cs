using UnityEngine;
using UnityEngine.UI;

namespace AshTower
{
    // Holds the prefabs the screens instantiate. Assigned by the editor baker.
    [CreateAssetMenu(menuName = "Ash Tower/UI Library")]
    public class UiLibrary : ScriptableObject
    {
        public CardView card;
        public EnemyView enemy;
        public MapNodeView mapNode;
        public StatusChip statusChip;
        public GameObject potionSlot;

        static UiLibrary _instance;
        public static UiLibrary I
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<UiLibrary>("UiLibrary");
                if (_instance != null && _instance.card == null && _instance.enemy == null)
                    _instance = null;
                return _instance;
            }
        }

        public static bool HasCard => Ready(I != null ? I.card : null);
        public static bool HasEnemy => Ready(I != null ? I.enemy : null);
        public static bool HasMapNode => Ready(I != null ? I.mapNode : null);
        public static bool HasChip => Ready(I != null ? I.statusChip : null);

        // True when a prefab still has sprites and fonts assigned.
        static bool Ready(Component c)
        {
            if (c == null) return false;
            var imgs = c.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < imgs.Length; i++)
                if (imgs[i] != null && imgs[i].sprite == null) return false;
            var texts = c.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
                if (texts[i] != null && texts[i].font == null) return false;
            return true;
        }
    }
}
