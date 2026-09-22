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
        public const string EggPath = ArtDir + "/egg_placeholder.png";
        public const string RoundedRectPath = ArtDir + "/rounded_rect_placeholder.png";
        public const string BodyMaterialPath = ArtDir + "/SnakeBodyLine.mat";

        public static void GenerateAll()
        {
            Directory.CreateDirectory(ArtDir);

            WritePng(CirclePath, DrawCircle(128));
            WritePng(SolidPath, DrawSolid(8));
            WritePng(EggPath, DrawEgg(128, 160));
            WritePng(RoundedRectPath, DrawRoundedRect(128, 128, 28));
            AssetDatabase.Refresh();

            ConfigureAsSprite(CirclePath);
            ConfigureAsSprite(SolidPath);
            ConfigureAsSprite(EggPath);
            ConfigureAsSprite(RoundedRectPath, border: new Vector4(28, 28, 28, 28)); // 9-slice so it can stretch to any badge/button size

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

        // An egg silhouette: narrower near the top, wider near the bottom —
        // used for the word-part collectible tiles, matching the reference design.
        static Texture2D DrawEgg(int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            float cx = width / 2f;
            float cy = height / 2f;
            float topRadiusX = width * 0.32f;
            float bottomRadiusX = width * 0.47f;
            const float edgeSoftness = 0.08f;

            for (int y = 0; y < height; y++)
            {
                float t = y / (float)(height - 1); // 0 at bottom row, 1 at top row
                float rx = Mathf.Lerp(bottomRadiusX, topRadiusX, t);
                float ny = (y + 0.5f - cy) / cy;

                for (int x = 0; x < width; x++)
                {
                    float nx = (x + 0.5f - cx) / rx;
                    float dist = nx * nx + ny * ny;
                    float alpha = Mathf.Clamp01((1f - dist) / edgeSoftness);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            return tex;
        }

        // A rounded-rectangle mask — used for HUD badges, label chips and pill buttons.
        static Texture2D DrawRoundedRect(int width, int height, float cornerRadius)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    float dx = Mathf.Max(0f, Mathf.Max(cornerRadius - px, px - (width - cornerRadius)));
                    float dy = Mathf.Max(0f, Mathf.Max(cornerRadius - py, py - (height - cornerRadius)));
                    float cornerDist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = cornerDist <= cornerRadius ? 1f : Mathf.Clamp01(1f - (cornerDist - cornerRadius));
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

        static void ConfigureAsSprite(string path, Vector4? border = null)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true;
            if (border.HasValue)
                importer.spriteBorder = border.Value;
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
