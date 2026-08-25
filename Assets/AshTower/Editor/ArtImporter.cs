using UnityEditor;
using UnityEngine;

namespace AshTower.Editor
{
    public class ArtImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').Contains("AshTower/Resources/Art")) return;
            var t = (TextureImporter)assetImporter;
            t.textureType = TextureImporterType.Sprite;
            t.spriteImportMode = SpriteImportMode.Single;
            t.spritePixelsPerUnit = 100;
            t.alphaIsTransparency = true;
            t.mipmapEnabled = false;
            t.filterMode = FilterMode.Bilinear;
            t.anisoLevel = 0;
            t.npotScale = TextureImporterNPOTScale.None;
            t.maxTextureSize = 4096;
            t.textureCompression = TextureImporterCompression.Uncompressed;
            t.sRGBTexture = true;
        }

        [InitializeOnLoadMethod]
        static void SharpenExistingArt()
        {
            EditorApplication.delayCall += () =>
            {
                var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/AshTower/Resources/Art" });
                var dirty = false;
                foreach (var g in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(g);
                    var t = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (t == null) continue;
                    if (!t.mipmapEnabled && t.textureCompression == TextureImporterCompression.Uncompressed
                        && t.maxTextureSize >= 2048) continue;
                    t.mipmapEnabled = false;
                    t.textureCompression = TextureImporterCompression.Uncompressed;
                    t.maxTextureSize = 4096;
                    t.filterMode = FilterMode.Bilinear;
                    t.npotScale = TextureImporterNPOTScale.None;
                    t.SaveAndReimport();
                    dirty = true;
                }
                if (dirty) AssetDatabase.Refresh();
            };
        }
    }
}
