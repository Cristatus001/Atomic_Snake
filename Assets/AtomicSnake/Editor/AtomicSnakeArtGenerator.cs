using System.IO;
using UnityEditor;
using UnityEngine;

namespace AtomicSnake.EditorTools
{
    // One-time placeholder art generation: writes real Sprite/Material assets
    // to disk so an artist can select them in the Project window and replace
    // them (swap the PNG, or drag a new sprite onto a prefab's SpriteRenderer)
    // without touching any code.
    public static class AtomicSnakeArtGenerator
    {
        public const string ArtDir = "Assets/AtomicSnake/Art";
        public const string CirclePath = ArtDir + "/circle_placeholder.png";
        public const string SolidPath = ArtDir + "/solid_placeholder.png";
        public const string BodyMaterialPath = ArtDir + "/SnakeBodyLine.mat";

        public static void GenerateAll()
        {
            Directory.CreateDirectory(ArtDir);

            WritePng(CirclePath, DrawCircle(128));
            WritePng(SolidPath, DrawSolid(8));
            AssetDatabase.Refresh();

            ConfigureAsSprite(CirclePath);
            ConfigureAsSprite(SolidPath);

            CreateBodyMaterial();

            AssetDatabase.SaveAssets();
            Debug.Log("AtomicSnakeArtGenerator: placeholder art generated.");
        }

        static Texture2D DrawCircle(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float radius = size / 2f;
            var center = new Vector2(radius, radius);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    float alpha = Mathf.Clamp01(radius - dist);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            return tex;
        }

        static Texture2D DrawSolid(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        static void WritePng(string path, Texture2D tex)
        {
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        static void ConfigureAsSprite(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        static void CreateBodyMaterial()
        {
            if (AssetDatabase.LoadAssetAtPath<Material>(BodyMaterialPath) != null) return;

            var shader = Shader.Find("Sprites/Default");
            var mat = new Material(shader) { color = new Color(0.30f, 0.68f, 0.36f) };
            var solidTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(SolidPath);
            if (solidTexture != null) mat.mainTexture = solidTexture;
            AssetDatabase.CreateAsset(mat, BodyMaterialPath);
        }
    }
}
