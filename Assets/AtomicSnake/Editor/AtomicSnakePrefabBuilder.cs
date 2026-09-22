using AtomicSnake.Field;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AtomicSnake.EditorTools
{
    // Builds the SnakeHead, SnakeBody, WordTile, power-up and obstacle
    // prefabs as real, persisted assets with SpriteHolder/TextHolder child
    // objects, so an artist can open any of them and swap the sprite, font,
    // color or scale directly in the Inspector.
    public static class AtomicSnakePrefabBuilder
    {
        public const string PrefabDir = "Assets/AtomicSnake/Prefabs";
        public const string SnakeHeadPath = PrefabDir + "/SnakeHead.prefab";
        public const string SnakeBodyPath = PrefabDir + "/SnakeBody.prefab";
        public const string WordTilePath = PrefabDir + "/WordTile.prefab";
        public const string MapPickupPath = PrefabDir + "/MapPickup.prefab";
        public const string ClockPickupPath = PrefabDir + "/ClockPickup.prefab";
        public const string ObstaclePath = PrefabDir + "/Obstacle.prefab";
        public const string TileDotPath = PrefabDir + "/TileDot.prefab";
        public const string ObstacleDotPath = PrefabDir + "/ObstacleDot.prefab";

        static readonly Color HeadColor = new Color(0.36f, 0.60f, 0.24f);
        static readonly Color EyeColor = new Color(0.05f, 0.05f, 0.05f);
        static readonly Color LabelChipColor = new Color(0.12f, 0.10f, 0.10f, 0.72f);
        static readonly Color MapColor = new Color(0.20f, 0.55f, 0.85f); // matches the reference's blue/white map egg
        static readonly Color ClockColor = new Color(0.90f, 0.65f, 0.20f);
        static readonly Color ObstacleColor = new Color(0.40f, 0.36f, 0.34f);
        static readonly Color MiniMapMarkerColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);

        // Matches the reference design's varied egg colors (yellow/pink/blue/purple);
        // WordTileView picks one deterministically from each tile's text.
        static readonly Color[] TilePalette =
        {
            new Color(0.95f, 0.75f, 0.20f), // yellow
            new Color(0.92f, 0.45f, 0.55f), // pink
            new Color(0.30f, 0.55f, 0.85f), // blue
            new Color(0.65f, 0.45f, 0.80f), // purple
        };

        public static void BuildAll()
        {
            System.IO.Directory.CreateDirectory(PrefabDir);

            var circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AtomicSnakeArtGenerator.CirclePath);
            var eggSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AtomicSnakeArtGenerator.EggPath);
            var roundedRectSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AtomicSnakeArtGenerator.RoundedRectPath);
            var bodyMaterial = AssetDatabase.LoadAssetAtPath<Material>(AtomicSnakeArtGenerator.BodyMaterialPath);

            BuildSnakeHead(circleSprite);
            BuildSnakeBody(bodyMaterial);
            BuildWordTile(eggSprite, roundedRectSprite);
            BuildPowerUp(eggSprite, roundedRectSprite, MapPickupPath, "MAP", Field.PowerUpType.Map, MapColor);
            BuildPowerUp(eggSprite, roundedRectSprite, ClockPickupPath, "CLOCK", Field.PowerUpType.Clock, ClockColor);
            BuildObstacle(circleSprite);
            BuildMapDot(eggSprite, TileDotPath, MiniMapMarkerColor);
            BuildMapDot(eggSprite, ObstacleDotPath, ObstacleColor);

            AssetDatabase.SaveAssets();
            Debug.Log("AtomicSnakePrefabBuilder: prefabs built.");
        }

        static void BuildSnakeHead(Sprite sprite)
        {
            var root = new GameObject("SnakeHead");

            var holder = new GameObject("SpriteHolder");
            holder.transform.SetParent(root.transform, false);
            holder.transform.localScale = Vector3.one * ScaleForDiameter(sprite, 0.58f);
            var sr = holder.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = HeadColor;
            sr.sortingOrder = 12;

            // Two small eyes toward the default facing direction (+X) for character.
            CreateEye(root.transform, sprite, new Vector3(0.16f, 0.10f, -0.01f));
            CreateEye(root.transform, sprite, new Vector3(0.16f, -0.10f, -0.01f));

            SaveAndDestroy(root, SnakeHeadPath);
        }

        static void CreateEye(Transform parent, Sprite sprite, Vector3 localPosition)
        {
            var eye = new GameObject("Eye");
            eye.transform.SetParent(parent, false);
            eye.transform.localPosition = localPosition;
            eye.transform.localScale = Vector3.one * ScaleForDiameter(sprite, 0.12f);
            var sr = eye.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = EyeColor;
            sr.sortingOrder = 13;
        }

        static void BuildSnakeBody(Material material)
        {
            var root = new GameObject("SnakeBody");
            var lr = root.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.numCapVertices = 8;
            lr.numCornerVertices = 8;
            lr.widthMultiplier = 0.5f;
            lr.sharedMaterial = material;
            lr.sortingOrder = 10;
            lr.positionCount = 0;

            SaveAndDestroy(root, SnakeBodyPath);
        }

        static void BuildWordTile(Sprite eggSprite, Sprite roundedRectSprite)
        {
            var root = new GameObject("WordTile");

            var holder = new GameObject("SpriteHolder");
            holder.transform.SetParent(root.transform, false);
            holder.transform.localScale = Vector3.one * ScaleForDiameter(eggSprite, 0.56f);
            var sr = holder.AddComponent<SpriteRenderer>();
            sr.sprite = eggSprite;
            sr.color = TilePalette[0];
            sr.sortingOrder = 4;

            var chip = new GameObject("LabelChip");
            chip.transform.SetParent(root.transform, false);
            chip.transform.localPosition = new Vector3(0f, 0f, -0.005f);
            var chipRenderer = chip.AddComponent<SpriteRenderer>();
            chipRenderer.sprite = roundedRectSprite;
            chipRenderer.drawMode = SpriteDrawMode.Sliced;
            chipRenderer.size = new Vector2(0.46f, 0.20f);
            chipRenderer.color = LabelChipColor;
            chipRenderer.sortingOrder = 5;

            var textHolder = new GameObject("TextHolder");
            textHolder.transform.SetParent(root.transform, false);
            textHolder.transform.localPosition = new Vector3(0f, 0f, -0.01f);
            var mesh = textHolder.AddComponent<TextMesh>();
            mesh.text = "";
            mesh.characterSize = 0.08f;
            mesh.fontSize = 48;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = Color.white;
            textHolder.GetComponent<MeshRenderer>().sortingOrder = 6;

            var view = root.AddComponent<WordTileView>();
            var so = new SerializedObject(view);
            so.FindProperty("textHolder").objectReferenceValue = mesh;
            so.FindProperty("spriteHolder").objectReferenceValue = sr;
            var paletteProp = so.FindProperty("palette");
            paletteProp.arraySize = TilePalette.Length;
            for (int i = 0; i < TilePalette.Length; i++)
                paletteProp.GetArrayElementAtIndex(i).colorValue = TilePalette[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveAndDestroy(root, WordTilePath);
        }

        static void BuildPowerUp(Sprite eggSprite, Sprite roundedRectSprite, string path, string label, Field.PowerUpType type, Color color)
        {
            var root = new GameObject(label + "Pickup");

            var holder = new GameObject("SpriteHolder");
            holder.transform.SetParent(root.transform, false);
            holder.transform.localScale = Vector3.one * ScaleForDiameter(eggSprite, 0.5f);
            var sr = holder.AddComponent<SpriteRenderer>();
            sr.sprite = eggSprite;
            sr.color = color;
            sr.sortingOrder = 4;

            var chip = new GameObject("LabelChip");
            chip.transform.SetParent(root.transform, false);
            chip.transform.localPosition = new Vector3(0f, 0f, -0.005f);
            var chipRenderer = chip.AddComponent<SpriteRenderer>();
            chipRenderer.sprite = roundedRectSprite;
            chipRenderer.drawMode = SpriteDrawMode.Sliced;
            chipRenderer.size = new Vector2(0.42f, 0.20f);
            chipRenderer.color = LabelChipColor;
            chipRenderer.sortingOrder = 5;

            var textHolder = new GameObject("TextHolder");
            textHolder.transform.SetParent(root.transform, false);
            textHolder.transform.localPosition = new Vector3(0f, 0f, -0.01f);
            var mesh = textHolder.AddComponent<TextMesh>();
            mesh.text = label;
            mesh.characterSize = 0.06f;
            mesh.fontSize = 48;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = Color.white;
            textHolder.GetComponent<MeshRenderer>().sortingOrder = 6;

            var view = root.AddComponent<PowerUpView>();
            var so = new SerializedObject(view);
            so.FindProperty("type").enumValueIndex = (int)type;
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveAndDestroy(root, path);
        }

        static void BuildObstacle(Sprite sprite)
        {
            var root = new GameObject("Obstacle");

            var holder = new GameObject("SpriteHolder");
            holder.transform.SetParent(root.transform, false);
            holder.transform.localScale = Vector3.one * ScaleForDiameter(sprite, 0.6f);
            var sr = holder.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = ObstacleColor;
            sr.sortingOrder = 4;

            root.AddComponent<ObstacleView>();

            SaveAndDestroy(root, ObstaclePath);
        }

        static void BuildMapDot(Sprite eggSprite, string path, Color color)
        {
            var root = new GameObject("Dot", typeof(RectTransform));
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(12f, 15f);

            var image = root.AddComponent<Image>();
            image.sprite = eggSprite;
            image.color = color;

            SaveAndDestroy(root, path);
        }

        static float ScaleForDiameter(Sprite sprite, float desiredWorldDiameter)
        {
            float nativeDiameter = sprite.bounds.size.x;
            return nativeDiameter > 0f ? desiredWorldDiameter / nativeDiameter : 1f;
        }

        static void SaveAndDestroy(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }
    }
}
