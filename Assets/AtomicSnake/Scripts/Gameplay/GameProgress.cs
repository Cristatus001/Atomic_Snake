using UnityEngine;

namespace AtomicSnake.Gameplay
{
    public static class GameProgress
    {
        const string ClearedKeyPrefix = "AtomicSnake.LevelCleared.";

        public static bool IsLevelUnlocked(int levelIndex)
        {
            return levelIndex == 0 || IsLevelCleared(levelIndex - 1);
        }

        public static bool IsLevelCleared(int levelIndex)
        {
            return PlayerPrefs.GetInt(ClearedKeyPrefix + levelIndex, 0) == 1;
        }

        public static void MarkLevelCleared(int levelIndex)
        {
            PlayerPrefs.SetInt(ClearedKeyPrefix + levelIndex, 1);
            PlayerPrefs.Save();
        }
    }
}
