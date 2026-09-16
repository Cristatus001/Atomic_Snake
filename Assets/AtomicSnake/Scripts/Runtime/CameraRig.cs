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

            cam.orthographic = false;
            cam.fieldOfView = 48f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.06f, 0.08f);

            // Angled, over-the-shoulder view looking down the length of the field for a 2.5D look.
            float height = FieldConfig.Depth * 0.95f;
            float back = FieldConfig.Depth * 1.15f;
            cam.transform.position = new Vector3(0f, height, -back);
            cam.transform.rotation = Quaternion.Euler(52f, 0f, 0f);

            return cam;
        }
    }
}
