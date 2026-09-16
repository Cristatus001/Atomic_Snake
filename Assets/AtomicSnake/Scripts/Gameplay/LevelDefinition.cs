namespace AtomicSnake.Gameplay
{
    public class LevelDefinition
    {
        public string DisplayName;
        public int[] AtomicNumbers;
        public float TimeLimitSeconds;

        public int ElementCount => AtomicNumbers.Length;
    }

    public static class LevelCatalog
    {
        public static readonly LevelDefinition Level1 = new LevelDefinition
        {
            DisplayName = "Level 1: Training Field",
            AtomicNumbers = new[] { 103, 108, 110, 114 },
            TimeLimitSeconds = 90f
        };

        public static readonly LevelDefinition Level2 = new LevelDefinition
        {
            DisplayName = "Level 2: Element Anomaly",
            AtomicNumbers = new[] { 101, 102, 105, 109, 112, 117 },
            TimeLimitSeconds = 120f
        };

        public static readonly LevelDefinition[] All = { Level1, Level2 };
    }
}
