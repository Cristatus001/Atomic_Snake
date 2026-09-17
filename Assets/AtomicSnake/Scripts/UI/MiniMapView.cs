using System.Collections.Generic;
using AtomicSnake.Runtime;
using UnityEngine;

namespace AtomicSnake.UI
{
    // Shows scaled-down dot markers for word-part/obstacle field positions
    // while the map power-up is active. Dot color/size presentation lives on
    // the TileDot/ObstacleDot prefabs, so an artist can restyle either.
    public class MiniMapView : MonoBehaviour
    {
        [SerializeField] RectTransform mapArea;
        [SerializeField] RectTransform tileDotPrefab;
        [SerializeField] RectTransform obstacleDotPrefab;

        readonly List<RectTransform> _activeDots = new();

        public void Show(IEnumerable<Vector2> tilePositions, IEnumerable<Vector2> obstaclePositions)
        {
            Clear();
            foreach (var pos in tilePositions) SpawnDot(tileDotPrefab, pos);
            foreach (var pos in obstaclePositions) SpawnDot(obstacleDotPrefab, pos);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            Clear();
        }

        void SpawnDot(RectTransform prefab, Vector2 worldPos)
        {
            var dot = Instantiate(prefab, mapArea);
            dot.anchoredPosition = WorldToMapPosition(worldPos);
            _activeDots.Add(dot);
        }

        Vector2 WorldToMapPosition(Vector2 worldPos)
        {
            float halfWidth = GridConfig.Columns * GridConfig.CellSize / 2f;
            float halfHeight = GridConfig.Rows * GridConfig.CellSize / 2f;
            float normalizedX = Mathf.InverseLerp(-halfWidth, halfWidth, worldPos.x);
            float normalizedY = Mathf.InverseLerp(-halfHeight, halfHeight, worldPos.y);

            var size = mapArea.rect.size;
            return new Vector2((normalizedX - 0.5f) * size.x, (normalizedY - 0.5f) * size.y);
        }

        void Clear()
        {
            foreach (var dot in _activeDots)
                if (dot != null) Destroy(dot.gameObject);
            _activeDots.Clear();
        }
    }
}
