using UnityEngine;

namespace AtomicSnake.Field
{
    public enum PowerUpType { Map, Clock }

    // A map or clock power-up pickup. Sprite/color presentation lives on the
    // MapPickup/ClockPickup prefabs (baked-in per prefab), so an artist can
    // restyle either without touching code.
    public class PowerUpView : MonoBehaviour
    {
        [SerializeField] PowerUpType type;

        public PowerUpType Type => type;
        public Vector2 Position { get; private set; }

        public static PowerUpView Create(PowerUpView prefab, Transform parent, Vector2 position)
        {
            var instance = Instantiate(prefab, parent);
            instance.transform.localPosition = position;
            instance.Position = position;
            return instance;
        }
    }
}
