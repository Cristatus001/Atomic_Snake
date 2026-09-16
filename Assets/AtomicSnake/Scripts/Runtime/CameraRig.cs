using UnityEngine;

namespace AtomicSnake.Runtime
{
    public static class CameraRig
    {
        public static Camera Setup()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            cam.backgroundColor = new Color(0.10f, 0.08f, 0.05f);
            cam.transform.position = new Vector3(0f, 0f, -10f);

            float halfHeight = GridConfig.Rows * GridConfig.CellSize * 0.5f + GridConfig.CellSize;
            float halfWidthNeeded = GridConfig.Columns * GridConfig.CellSize * 0.5f + GridConfig.CellSize;
            float aspect = cam.aspect > 0f ? cam.aspect : 16f / 9f;
            cam.orthographicSize = Mathf.Max(halfHeight, halfWidthNeeded / aspect);

            return cam;
        }
    }
}
