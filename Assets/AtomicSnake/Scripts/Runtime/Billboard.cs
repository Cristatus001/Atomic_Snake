using UnityEngine;

namespace AtomicSnake.Runtime
{
    // Keeps a world-space label facing the camera under the angled 2.5D view.
    public class Billboard : MonoBehaviour
    {
        public Camera Target;

        void LateUpdate()
        {
            var cam = Target != null ? Target : Camera.main;
            if (cam == null) return;
            transform.rotation = cam.transform.rotation;
        }
    }
}
