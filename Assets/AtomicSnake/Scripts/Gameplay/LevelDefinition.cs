namespace AtomicSnake.Gameplay
{
    public class LevelDefinition
    {
        public string DisplayName;
        public int[] AtomicNumbers;
        public float TimeLimitSeconds;

        public float SnakeSpeed;
        public bool TilesDrift;
        public float DriftIntervalSeconds;
        public int DecoyTileCount;
        public float MapHintDuration;
        public int ObstacleCount;

        public int ElementCount => AtomicNumbers.Length;
    }

    public static class LevelCatalog
    {
        // Level 1 - Training Field / "Initial Run": stationary word parts,
        // normal snake speed, no decoys or obstacles, longer map hint.
        public static readonly LevelDefinition Level1 = new LevelDefinition
        {
            DisplayName = "Level 1: Training Field",
            AtomicNumbers = new[] { 103, 108, 110, 114 },
            TimeLimitSeconds = 90f,
            SnakeSpeed = 3.4f,
            TilesDrift = false,
            DriftIntervalSeconds = 0f,
            DecoyTileCount = 0,
            MapHintDuration = 5f,
            ObstacleCount = 0
        };

        // Level 2 - Element Anomaly / "Advanced Run": word parts disappear and
        // reappear elsewhere, faster snake, extra incorrect word parts,
        // obstacles, shorter map hint.
        public static readonly LevelDefinition Level2 = new LevelDefinition
        {
            DisplayName = "Level 2: Element Anomaly",
            AtomicNumbers = new[] { 101, 102, 105, 109, 112, 117 },
            TimeLimitSeconds = 120f,
            SnakeSpeed = 4.4f,
            TilesDrift = true,
            DriftIntervalSeconds = 4f,
            DecoyTileCount = 2,
            MapHintDuration = 2f,
            ObstacleCount = 3
        };

        public static readonly LevelDefinition[] All = { Level1, Level2 };
    }
}
