using AtomicSnake.Field;
using UnityEditor;
using UnityEngine;

namespace AtomicSnake.EditorTools
{
    // Builds the SnakeHead, SnakeBody and WordTile prefabs as real, persisted
    // assets with SpriteHolder/TextHolder child objects, so an artist can
    // open any of them and swap the sprite, font, color or scale directly
    // in the Inspector.
    public static class AtomicSnakePrefabBuilder
    {
        public const string PrefabDir = "Assets/AtomicSnake/Prefabs";
        public const string SnakeHeadPath = PrefabDir + "/SnakeHead.prefab";
        public const string SnakeBodyPath = PrefabDir + "/SnakeBody.prefab";
        public const string WordTilePath = PrefabDir + "/WordTile.prefab";

        static readonly Color HeadColor = new Color(0.16f, 0.38f, 0.86f);
        static readonly Color TileColor = new Color(0.86f, 0.55f, 0.70f);

        public static void BuildAll()
        {
            System.IO.Directory.CreateDirectory(PrefabDir);

            var circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AtomicSnakeArtGenerator.CirclePath);
            var bodyMaterial = AssetDatabase.LoadAssetAtPath<Material>(AtomicSnakeArtGenerator.BodyMaterialPath);

            BuildSnakeHead(circleSprite);
            BuildSnakeBody(bodyMaterial);
            BuildWordTile(circleSprite);

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
