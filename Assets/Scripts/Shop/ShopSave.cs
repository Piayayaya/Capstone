using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShopSaveData
{
    public int coinBalance = 0;
    public List<string> ownedCharacterIds = new();
    public long noAdsUntilUnix = 0; // for subscription “No Ads”
}

public static class ShopSave
{
    private const string Key = "BM_SHOP_SAVE";
    private static ShopSaveData _cache;

    public static ShopSaveData Data
    {
        get
        {
            if (_cache == null)
            {
                _cache = PlayerPrefs.HasKey(Key)
                    ? JsonUtility.FromJson<ShopSaveData>(PlayerPrefs.GetString(Key))
                    : new ShopSaveData();
            }
            return _cache;
        }
    }

    public static void Save()
    {
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(Data));
        PlayerPrefs.Save();
    }

    public static void AddCoins(int amount) { Data.coinBalance += amount; Save(); }

    public static bool SpendCoins(int amount)
    {
        if (Data.coinBalance < amount) return false;
        Data.coinBalance -= amount; Save(); return true;
    }

    public static bool HasCharacter(string id) => Data.ownedCharacterIds.Contains(id);

    public static void UnlockCharacter(string id)
    {
        if (!Data.ownedCharacterIds.Contains(id)) Data.ownedCharacterIds.Add(id);
        Save();
    }

    public static bool IsNoAdsActive()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return now < Data.noAdsUntilUnix;
    }

    public static void GrantNoAdsForDays(int days)
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long baseTime = Math.Max(now, Data.noAdsUntilUnix);
        Data.noAdsUntilUnix = baseTime + days * 86400L;
        Save();
    }

    public static void ResetAll()
    {
        // Clear in-memory data
        _cache = new ShopSaveData();

        // Wipe the saved JSON on disk
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }
}
