using AtomicSnake.Bootstrap;
using AtomicSnake.Chemistry;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AtomicSnake.EditorTools
{
    // Batch-mode helpers: -executeMethod AtomicSnake.EditorTools.AtomicSnakeSetup.<Method>
    public static class AtomicSnakeSetup
    {
        const string ScenePath = "Assets/Scenes/SampleScene.unity";

        public static void SetupScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var existing = Object.FindFirstObjectByType<GameBootstrap>();
            if (existing == null)
            {
                var go = new GameObject("GameBootstrap");
                go.AddComponent<GameBootstrap>();
                Undo.RegisterCreatedObjectUndo(go, "Create GameBootstrap");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("AtomicSnakeSetup.SetupScene: done.");
        }

        public static void VerifyChemistry()
        {
            void Check(int atomicNumber, string expected)
            {
                string actual = IupacNaming.GetDisplayName(atomicNumber);
                if (actual != expected)
                    throw new System.Exception($"IupacNaming mismatch for {atomicNumber}: expected '{expected}', got '{actual}'");
                Debug.Log($"OK  {atomicNumber} -> {actual}");
            }

            Check(101, "Unnilunium");
            Check(102, "Unnilbium");
            Check(103, "Unniltrium");
            Check(104, "Unnilquadium");
            Check(108, "Unniloctium");
            Check(109, "Unnilennium");
            Check(110, "Ununnilium");
            Check(111, "Unununium");
            Check(112, "Ununbium");
            Check(113, "Ununtrium");
            Check(114, "Ununquadium");
            Check(118, "Ununoctium");

            Debug.Log("AtomicSnakeSetup.VerifyChemistry: all checks passed.");
        }
    }
}
