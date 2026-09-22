using AtomicSnake.Bootstrap;
using AtomicSnake.Field;
using AtomicSnake.Gameplay;
using AtomicSnake.Runtime;
using AtomicSnake.Snake;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AtomicSnake.EditorTools
{
    // Builds every persisted piece of the game (field, snake, UI, wiring)
    // into the scene and saves it. Re-run safe: it deletes any objects from
    // a previous build before recreating them. This is the only place that
    // constructs GameObjects/components from code — everything it builds is
    // then a normal, artist-editable scene object or prefab from then on.
    public static class AtomicSnakeSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/SampleScene.unity";

        static readonly string[] ManagedRootNames =
        {
            "FieldRoot", "Snake", "GameplayController", "Canvas", "EventSystem", "UIManager", "GameBootstrap"
        };

        public static void BuildAll()
        {
            AtomicSnakeArtGenerator.GenerateAll();
            AtomicSnakePrefabBuilder.BuildAll();

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemoveManagedObjects();

            ConfigureCamera();

            var fieldRoot = BuildFieldRoot();
            var snake = BuildSnake(fieldRoot.transform);
            var gameplay = new GameObject("GameplayController").AddComponent<GameplayController>();
            var wordTilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AtomicSnakePrefabBuilder.WordTilePath).GetComponent<WordTileView>();
            var mapPickupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AtomicSnakePrefabBuilder.MapPickupPath).GetComponent<PowerUpView>();
            var clockPickupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AtomicSnakePrefabBuilder.ClockPickupPath).GetComponent<PowerUpView>();
            var obstaclePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AtomicSnakePrefabBuilder.ObstaclePath).GetComponent<ObstacleView>();

            var canvasGo = AtomicSnakeUiBuilder.Build(out var uiManager);

            var bootstrapGo = new GameObject("GameBootstrap");
            var bootstrap = bootstrapGo.AddComponent<GameBootstrap>();

            var so = new SerializedObject(bootstrap);
            so.FindProperty("fieldRoot").objectReferenceValue = fieldRoot;
            so.FindProperty("snake").objectReferenceValue = snake;
            so.FindProperty("gameplay").objectReferenceValue = gameplay;
            so.FindProperty("ui").objectReferenceValue = uiManager;
            so.FindProperty("wordTilePrefab").objectReferenceValue = wordTilePrefab;
            so.FindProperty("mapPickupPrefab").objectReferenceValue = mapPickupPrefab;
            so.FindProperty("clockPickupPrefab").objectReferenceValue = clockPickupPrefab;
            so.FindProperty("obstaclePrefab").objectReferenceValue = obstaclePrefab;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("AtomicSnakeSceneBuilder: scene built.");
        }

        static void RemoveManagedObjects()
        {
            var scene = EditorSceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var name in ManagedRootNames)
                {
                    if (root.name == name)
                    {
                        Object.DestroyImmediate(root);
                        break;
                    }
                }
            }
        }

        static void ConfigureCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;

            cam.orthographic = true;
            cam.backgroundColor = new Color(0.10f, 0.08f, 0.05f);
            cam.transform.position = new Vector3(0f, 0f, -10f);

            float halfHeight = GridConfig.Rows * GridConfig.CellSize * 0.5f + GridConfig.CellSize;
            float halfWidthNeeded = GridConfig.Columns * GridConfig.CellSize * 0.5f + GridConfig.CellSize;
            float aspect = cam.aspect > 0f ? cam.aspect : 16f / 9f;
            cam.orthographicSize = Mathf.Max(halfHeight, halfWidthNeeded / aspect);
        }

        static GameObject BuildFieldRoot()
        {
            var fieldRoot = new GameObject("FieldRoot");
            var solidSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AtomicSnakeArtGenerator.SolidPath);
            var circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AtomicSnakeArtGenerator.CirclePath);

            var dirt = new Color(0.45f, 0.32f, 0.20f);
            var dirtBlotch = new Color(0.40f, 0.28f, 0.17f, 0.55f);
            var leafyBorder = new Color(0.30f, 0.60f, 0.22f);

            float width = GridConfig.Columns * GridConfig.CellSize;
            float height = GridConfig.Rows * GridConfig.CellSize;
            float border = GridConfig.CellSize * 0.8f;

            CreateQuad(fieldRoot.transform, "LeafyBorder", solidSprite, leafyBorder, new Vector3(0f, 0f, -0.02f), width + border * 2f, height + border * 2f, 1);
            CreateQuad(fieldRoot.transform, "Dirt", solidSprite, dirt, new Vector3(0f, 0f, -0.01f), width, height, 0);

            // A handful of soft, randomly-placed blotches to break up the flat dirt fill.
            var random = new System.Random(1);
            for (int i = 0; i < 14; i++)
            {
                float x = (float)(random.NextDouble() - 0.5) * (width - GridConfig.CellSize);
                float y = (float)(random.NextDouble() - 0.5) * (height - GridConfig.CellSize);
                float size = GridConfig.CellSize * (0.8f + (float)random.NextDouble() * 0.9f);
                var go = new GameObject($"DirtBlotch_{i}");
                go.transform.SetParent(fieldRoot.transform, false);
                go.transform.localPosition = new Vector3(x, y, -0.005f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = circleSprite;
                sr.color = dirtBlotch;
                sr.sortingOrder = 0;
                float scale = size / circleSprite.bounds.size.x;
                go.transform.localScale = new Vector3(scale, scale, 1f);
            }

            fieldRoot.SetActive(false);
            return fieldRoot;
        }

        static void CreateQuad(Transform parent, string name, Sprite sprite, Color color, Vector3 localPosition, float width, float height, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = sortingOrder;

            float spriteSize = sprite.bounds.size.x;
            go.transform.localScale = new Vector3(width / spriteSize, height / spriteSize, 1f);
        }

        static SnakeController BuildSnake(Transform fieldRoot)
        {
            var go = new GameObject("Snake");
            var snake = go.AddComponent<SnakeController>();
            go.AddComponent<PlayerInputReader>();

            var headPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AtomicSnakePrefabBuilder.SnakeHeadPath);
            var bodyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AtomicSnakePrefabBuilder.SnakeBodyPath).GetComponent<LineRenderer>();

            var so = new SerializedObject(snake);
            so.FindProperty("headPrefab").objectReferenceValue = headPrefab.transform;
            so.FindProperty("bodyPrefab").objectReferenceValue = bodyPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();

            return snake;
        }
    }
}
