using System.Collections.Generic;
using UnityEngine;

namespace AtomicSnake.Runtime
{
    // Builds simple placeholder 3D geometry (spheres, cubes, a ground quad)
    // with flat, unlit colors so the 2.5D scene needs no imported art or
    // scene lighting to look correct.
    public static class ShapeFactory
    {
        static readonly Dictionary<Color, Material> MaterialCache = new();
        static Shader _unlitShader;

        static Shader UnlitShader
        {
            get
            {
                if (_unlitShader == null)
                {
                    _unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
                    if (_unlitShader == null)
                        _unlitShader = Shader.Find("Unlit/Color");
                }
                return _unlitShader;
            }
        }

        public static Material GetMaterial(Color color)
        {
            if (MaterialCache.TryGetValue(color, out var cached)) return cached;

            var mat = new Material(UnlitShader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            MaterialCache[color] = mat;
            return mat;
        }

        public static GameObject CreateSphere(Transform parent, Color color, float diameter, string name = "Sphere")
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one * diameter;
            go.GetComponent<MeshRenderer>().sharedMaterial = GetMaterial(color);
            return go;
        }

        public static GameObject CreateBox(Transform parent, Color color, Vector3 size, string name = "Box")
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().sharedMaterial = GetMaterial(color);
            return go;
        }

        public static GameObject CreateGroundQuad(Transform parent, Color color, float width, float depth, string name = "Ground")
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            go.transform.localScale = new Vector3(width, depth, 1f);
            go.GetComponent<MeshRenderer>().sharedMaterial = GetMaterial(color);
            return go;
        }
    }
}
