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
    // OLD global key (kept for migration)
    private const string Key = "BM_SHOP_SAVE";

    // per-user cache
    private static ShopSaveData _cache;
    private static string _loadedUserId;  // which user the cache belongs to

    // ------------------------------------------------------------
    //  INTERNAL HELPERS: per-user key + lazy load
    // ------------------------------------------------------------

    private static string CurrentUserId
    {
        get
        {
            // This uses your existing user system
            // guestId_v1 / google uid etc.
            return UserIdProvider.ActiveUserId;
        }
    }

    private static string KeyFor(string uid)
    {
        // If somehow uid is empty, still fall back to old key prefix
        return string.IsNullOrEmpty(uid) ? Key : $"{Key}_{uid}";
    }

    /// <summary>
    /// Makes sure _cache is loaded for the *current* active user.
    /// Also migrates old global BM_SHOP_SAVE if it exists.
    /// </summary>
    private static void EnsureLoaded()
    {
        string uid = CurrentUserId;

        // If we already loaded for this user, nothing to do.
        if (_cache != null && _loadedUserId == uid)
            return;

        _loadedUserId = uid;
        string perUserKey = KeyFor(uid);

        // 1) Prefer per-user key (new format)
        if (PlayerPrefs.HasKey(perUserKey))
        {
            string json = PlayerPrefs.GetString(perUserKey);
            _cache = JsonUtility.FromJson<ShopSaveData>(json) ?? new ShopSaveData();
            return;
        }

        // 2) If no per-user key but old global key exists, migrate it
        if (PlayerPrefs.HasKey(Key))
        {
            string oldJson = PlayerPrefs.GetString(Key);
            _cache = JsonUtility.FromJson<ShopSaveData>(oldJson) ?? new ShopSaveData();

            // write under per-user key
            PlayerPrefs.SetString(perUserKey, oldJson);
            PlayerPrefs.DeleteKey(Key);   // remove old shared data
            PlayerPrefs.Save();
            return;
        }

        // 3) Nothing saved yet → start fresh for this user
        _cache = new ShopSaveData();
    }

    // ------------------------------------------------------------
    //  PUBLIC API (same methods you already use)
    // ------------------------------------------------------------

    public static ShopSaveData Data
    {
        get
        {
            EnsureLoaded();
            return _cache;
        }
    }

    public static void Save()
    {
        EnsureLoaded();

        string uid = _loadedUserId ?? CurrentUserId;
        string key = KeyFor(uid);

        PlayerPrefs.SetString(key, JsonUtility.ToJson(_cache));
        PlayerPrefs.Save();
    }

    public static void AddCoins(int amount)
    {
        EnsureLoaded();
        Data.coinBalance += amount;
        Save();
    }

    public static bool SpendCoins(int amount)
    {
        EnsureLoaded();
        if (Data.coinBalance < amount) return false;
        Data.coinBalance -= amount;
        Save();
        return true;
    }

    public static bool HasCharacter(string id)
    {
        EnsureLoaded();
        return Data.ownedCharacterIds.Contains(id);
    }

    public static void UnlockCharacter(string id)
    {
        EnsureLoaded();
        if (!Data.ownedCharacterIds.Contains(id))
            Data.ownedCharacterIds.Add(id);
        Save();
    }

    public static bool IsNoAdsActive()
    {
        EnsureLoaded();
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return now < Data.noAdsUntilUnix;
    }

    public static void GrantNoAdsForDays(int days)
    {
        EnsureLoaded();
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long baseTime = Math.Max(now, Data.noAdsUntilUnix);
        Data.noAdsUntilUnix = baseTime + days * 86400L;
        Save();
    }

    /// <summary>
    /// Clears shop data ONLY for the *current* active user.
    /// Use this when deleting account or doing a full reset.
    /// </summary>
    public static void ResetAllForActiveUser()
    {
        string uid = CurrentUserId;
        string key = KeyFor(uid);

        _cache = new ShopSaveData();
        _loadedUserId = uid;

        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Keeps old name for backward compatibility.
    /// This now just calls ResetAllForActiveUser().
    /// </summary>
    public static void ResetAll()
    {
        ResetAllForActiveUser();
    }

    public static int RemainingDays()
    {
        EnsureLoaded();
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long remaining = Data.noAdsUntilUnix - now;
        if (remaining <= 0) return 0;
        return Mathf.CeilToInt(remaining / 86400f);
    }

    public static DateTimeOffset? ExpiryUtc()
    {
        EnsureLoaded();
        if (Data.noAdsUntilUnix <= 0) return null;
        return DateTimeOffset.FromUnixTimeSeconds(Data.noAdsUntilUnix);
    }

    // Optional dev helpers
    public static void ExpireNow()
    {
        EnsureLoaded();
        Data.noAdsUntilUnix = 0;
        Save();
    }

    public static void ExtendDays(int days)
    {
        GrantNoAdsForDays(days);
    }
}
