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

        static readonly Color HeadColor = new Color(0.16f, 0.38f, 0.86f);
        static readonly Color TileColor = new Color(0.86f, 0.55f, 0.70f);
        static readonly Color MapColor = new Color(0.20f, 0.70f, 0.75f);
        static readonly Color ClockColor = new Color(0.90f, 0.65f, 0.20f);
        static readonly Color ObstacleColor = new Color(0.40f, 0.36f, 0.34f);

        public static void BuildAll()
        {
            System.IO.Directory.CreateDirectory(PrefabDir);

            var circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AtomicSnakeArtGenerator.CirclePath);
            var bodyMaterial = AssetDatabase.LoadAssetAtPath<Material>(AtomicSnakeArtGenerator.BodyMaterialPath);

            BuildSnakeHead(circleSprite);
            BuildSnakeBody(bodyMaterial);
            BuildWordTile(circleSprite);
            BuildPowerUp(circleSprite, MapPickupPath, "MAP", Field.PowerUpType.Map, MapColor);
            BuildPowerUp(circleSprite, ClockPickupPath, "CLOCK", Field.PowerUpType.Clock, ClockColor);
            BuildObstacle(circleSprite);
            BuildMapDot(circleSprite, TileDotPath, TileColor);
            BuildMapDot(circleSprite, ObstacleDotPath, ObstacleColor);

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

            SaveAndDestroy(root, SnakeHeadPath);
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

        static void BuildWordTile(Sprite sprite)
        {
            var root = new GameObject("WordTile");

            var holder = new GameObject("SpriteHolder");
            holder.transform.SetParent(root.transform, false);
            holder.transform.localScale = Vector3.one * ScaleForDiameter(sprite, 0.53f);
            var sr = holder.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = TileColor;
            sr.sortingOrder = 5;

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
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveAndDestroy(root, WordTilePath);
        }

        static void BuildPowerUp(Sprite sprite, string path, string label, Field.PowerUpType type, Color color)
        {
            var root = new GameObject(label + "Pickup");

            var holder = new GameObject("SpriteHolder");
            holder.transform.SetParent(root.transform, false);
            holder.transform.localScale = Vector3.one * ScaleForDiameter(sprite, 0.5f);
            var sr = holder.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = 5;

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

        static void BuildMapDot(Sprite sprite, string path, Color color)
        {
            var root = new GameObject("Dot", typeof(RectTransform));
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(10f, 10f);

            var image = root.AddComponent<Image>();
            image.sprite = sprite;
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
