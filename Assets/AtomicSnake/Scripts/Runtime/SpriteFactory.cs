using System.Collections.Generic;
using UnityEngine;

namespace AtomicSnake.Runtime
{
    // Generates simple placeholder circle/solid sprites at runtime so the
    // game needs no imported art to be playable.
    public static class SpriteFactory
    {
        const float PixelsPerUnit = 100f;
        const int CircleSize = 64;

        static readonly Dictionary<Color, Sprite> CircleCache = new();
        static readonly Dictionary<Color, Sprite> SolidCache = new();

        public static Sprite GetCircle(Color color)
        {
            if (CircleCache.TryGetValue(color, out var cached)) return cached;

            var tex = new Texture2D(CircleSize, CircleSize, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            float radius = CircleSize / 2f;
            var center = new Vector2(radius, radius);
            for (int y = 0; y < CircleSize; y++)
            {
                for (int x = 0; x < CircleSize; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    float alpha = Mathf.Clamp01(radius - dist);
                    tex.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * alpha));
                }
            }
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, CircleSize, CircleSize), new Vector2(0.5f, 0.5f), PixelsPerUnit);
            CircleCache[color] = sprite;
            return sprite;
        }

        public static Sprite GetSolid(Color color)
        {
            if (SolidCache.TryGetValue(color, out var cached)) return cached;

            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), PixelsPerUnit);
            SolidCache[color] = sprite;
            return sprite;
        }

        // Scales a sprite renderer so its rendered width equals worldSize, regardless of source texture size.
        public static void SetWorldSize(SpriteRenderer renderer, float worldSize)
        {
            SetWorldSize(renderer, worldSize, worldSize);
        }

        // Scales a sprite renderer to an exact world-space width/height, regardless of source texture size.
        public static void SetWorldSize(SpriteRenderer renderer, float worldWidth, float worldHeight)
        {
            var spriteSize = renderer.sprite.bounds.size;
            float scaleX = spriteSize.x > 0f ? worldWidth / spriteSize.x : 1f;
            float scaleY = spriteSize.y > 0f ? worldHeight / spriteSize.y : 1f;
            renderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
    }
}
