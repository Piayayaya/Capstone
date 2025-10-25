using UnityEngine;

public static class ProgressStore
{
    const string KEY_PREFIX = "SL_";

    static string LevelKey(LadderDifficulty d) => $"{KEY_PREFIX}{d}_Level";
    static string CorrectKey(LadderDifficulty d) => $"{KEY_PREFIX}{d}_Correct";
    static string CoinsKey(LadderDifficulty d) => $"{KEY_PREFIX}{d}_Coins"; // NEW

    // --- Level (1-based) ---
    public static void SaveLevel(LadderDifficulty d, int level1Based)
    {
        PlayerPrefs.SetInt(LevelKey(d), Mathf.Max(1, level1Based));
        PlayerPrefs.Save();
    }

    public static int LoadLevel(LadderDifficulty d, int defaultLevel = 1)
    {
        return PlayerPrefs.GetInt(LevelKey(d), defaultLevel);
    }

    // --- Correct count (optional, if you use it) ---
    public static void SaveCorrectCount(LadderDifficulty d, int correct)
    {
        PlayerPrefs.SetInt(CorrectKey(d), Mathf.Max(0, correct));
        PlayerPrefs.Save();
    }

    public static int LoadCorrectCount(LadderDifficulty d, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(CorrectKey(d), defaultValue);
    }

    // --- Coins (NEW) ---
    public static void SaveCoins(LadderDifficulty d, int coins)
    {
        PlayerPrefs.SetInt(CoinsKey(d), Mathf.Max(0, coins));
        PlayerPrefs.Save();
    }

    public static int LoadCoins(LadderDifficulty d, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(CoinsKey(d), defaultValue);
    }

    // --- Reset helpers (updated to clear coins too) ---
    public static void ClearDifficulty(LadderDifficulty d)
    {
        PlayerPrefs.DeleteKey(LevelKey(d));
        PlayerPrefs.DeleteKey(CorrectKey(d));
        PlayerPrefs.DeleteKey(CoinsKey(d)); // clear coins
        PlayerPrefs.Save();
    }

    public static void ClearAll()
    {
        foreach (LadderDifficulty d in System.Enum.GetValues(typeof(LadderDifficulty)))
            ClearDifficulty(d);
        PlayerPrefs.Save();
    }
}
