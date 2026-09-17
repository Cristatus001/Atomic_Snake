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

            var soilLight = new Color(0.78f, 0.62f, 0.40f);
            var soilDark = new Color(0.71f, 0.55f, 0.35f);
            var hedge = new Color(0.24f, 0.47f, 0.27f);

            float width = GridConfig.Columns * GridConfig.CellSize;
            float height = GridConfig.Rows * GridConfig.CellSize;
            float border = GridConfig.CellSize * 0.5f;

            CreateQuad(fieldRoot.transform, "Hedge", solidSprite, hedge, new Vector3(0f, 0f, -0.02f), width + border * 2f, height + border * 2f, 1);

            for (int row = 0; row < GridConfig.Rows; row++)
            {
                var color = row % 2 == 0 ? soilLight : soilDark;
                var rowCenter = GridUtil.CellToWorld(new Vector2Int(0, row));
                var pos = new Vector3(0f, rowCenter.y, -0.01f);
                CreateQuad(fieldRoot.transform, $"Row_{row}", solidSprite, color, pos, width, GridConfig.CellSize, 0);
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
