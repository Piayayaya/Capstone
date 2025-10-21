using UnityEngine;

public static class ProgressStore
{
    // Namespace saves by difficulty so each mode has independent progress.
    static string Prefix(LadderDifficulty diff) => $"SL_{diff}_";

    public static void SaveLevel(LadderDifficulty diff, int level1Based)
    {
        PlayerPrefs.SetInt(Prefix(diff) + "LEVEL", Mathf.Max(1, level1Based));
        PlayerPrefs.Save();
    }

    public static int LoadLevel(LadderDifficulty diff, int defaultLevel = 1)
    {
        return PlayerPrefs.GetInt(Prefix(diff) + "LEVEL", Mathf.Max(1, defaultLevel));
    }

    public static void SaveCoins(LadderDifficulty diff, int coins)
    {
        PlayerPrefs.SetInt(Prefix(diff) + "COINS", Mathf.Max(0, coins));
        PlayerPrefs.Save();
    }

    public static int LoadCoins(LadderDifficulty diff, int defaultCoins = 0)
    {
        return PlayerPrefs.GetInt(Prefix(diff) + "COINS", Mathf.Max(0, defaultCoins));
    }

    // Optional: call if you want a “Reset progress” button
    public static void Clear(LadderDifficulty diff)
    {
        PlayerPrefs.DeleteKey(Prefix(diff) + "LEVEL");
        PlayerPrefs.DeleteKey(Prefix(diff) + "COINS");
        PlayerPrefs.Save();
    }
}
