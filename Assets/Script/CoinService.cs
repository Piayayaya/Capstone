using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

public enum GameModeId
{
    SmartLadder,
    NameTheFlag,
    DragAndDrop,
    TuneYourTongue,
    SeeItOrLoseIt,
    DailyRewards,
    DailyQuests,
    Achievements
}

public class CoinService : MonoBehaviour
{
    public static CoinService Instance;

    private DatabaseReference db;
    private string playerId;

    public int TotalCoins { get; private set; }
    private readonly Dictionary<GameModeId, int> byMode = new();

    public event Action<int> OnTotalChanged;
    public event Action<GameModeId, int> OnModeChanged;

    // PlayerPrefs keys
    private const string LOCAL_TOTAL = "BM_LOCAL_TOTAL_COINS";
    private const string LOCAL_MODE_PREFIX = "BM_LOCAL_MODE_COINS_";
    private const string PLAYER_KEY = "DEVICE_PLAYER_ID";

    // ------------------------ UNITY LIFECYCLE ------------------------

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 1) Always load local first so coins survive app close
        LoadLocal();
        FireAllEvents();
        Debug.Log($"[CoinService] Awake. Local total={TotalCoins}");

        // 2) Try to hook up Firebase + player mapping
        await InitFirebaseAndPlayer();
    }

    // ------------------------ INIT & PLAYER ------------------------

    private async Task InitFirebaseAndPlayer()
    {
        // Wait (a bit) for FirebaseInit to say it's ready
        int guard = 0;
        while (!FirebaseInit.IsReady && guard < 50)
        {
            await Task.Delay(100); // up to ~5 seconds
            guard++;
        }

        try
        {
            db = FirebaseDatabase.DefaultInstance.RootReference;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[CoinService] Firebase Database init failed, staying local-only. " + e.Message);
            db = null;
        }

        // Restore playerId from prefs
        playerId = PlayerPrefs.GetString(PLAYER_KEY, "");
        Debug.Log($"[CoinService] InitFirebaseAndPlayer: stored playerId='{playerId}'");

        if (db != null && !string.IsNullOrEmpty(playerId))
        {
            await LoadFromFirebase();
        }
    }

    /// <summary>
    /// Called after you create/login a user (RegisterUI, Google login, etc.).
    /// Binds this device's coins to that /players/{userId}/coins node.
    /// </summary>
    public async Task SetPlayer(string newPlayerId)
    {
        if (string.IsNullOrEmpty(newPlayerId))
        {
            Debug.LogWarning("[CoinService] SetPlayer called with empty id.");
            return;
        }

        playerId = newPlayerId;
        PlayerPrefs.SetString(PLAYER_KEY, playerId);
        PlayerPrefs.Save();

        Debug.Log($"[CoinService] SetPlayer -> '{playerId}'");

        // Ensure we have a db reference
        if (db == null)
            await InitFirebaseAndPlayer();
        else
            await LoadFromFirebase();
    }

    // ------------------------ LOCAL SAVE ------------------------

    private void LoadLocal()
    {
        TotalCoins = PlayerPrefs.GetInt(LOCAL_TOTAL, 0);

        foreach (GameModeId id in Enum.GetValues(typeof(GameModeId)))
        {
            int v = PlayerPrefs.GetInt(LOCAL_MODE_PREFIX + id, 0);
            byMode[id] = v;
        }
    }

    private void SaveLocal()
    {
        PlayerPrefs.SetInt(LOCAL_TOTAL, TotalCoins);

        foreach (var kvp in byMode)
        {
            PlayerPrefs.SetInt(LOCAL_MODE_PREFIX + kvp.Key, kvp.Value);
        }

        PlayerPrefs.Save();
    }

    // ------------------------ FIREBASE SYNC ------------------------

    private async Task LoadFromFirebase()
    {
        if (db == null || string.IsNullOrEmpty(playerId))
            return;

        DataSnapshot snap;
        try
        {
            snap = await db.Child("players").Child(playerId).Child("coins").GetValueAsync();
        }
        catch (Exception e)
        {
            Debug.LogWarning("[CoinService] LoadFromFirebase failed, keeping local coins. " + e);
            return;
        }

        if (snap == null || !snap.Exists)
        {
            // First time this user has coins in Firebase – push local values up.
            Debug.Log("[CoinService] No coins node in Firebase; pushing local values.");
            await WriteFullCoinsToFirebase();
            return;
        }

        // Only override local if values exist on server
        if (snap.Child("total").Exists &&
            int.TryParse(snap.Child("total").Value.ToString(), out int serverTotal))
        {
            TotalCoins = serverTotal;
        }

        foreach (GameModeId id in Enum.GetValues(typeof(GameModeId)))
        {
            var modeSnap = snap.Child("byMode").Child(id.ToString());
            if (modeSnap.Exists &&
                int.TryParse(modeSnap.Value.ToString(), out int modeValue))
            {
                byMode[id] = modeValue;
            }
        }

        SaveLocal();
        FireAllEvents();

        Debug.Log($"[CoinService] Loaded from Firebase: total={TotalCoins}");
    }

    private async Task WriteFullCoinsToFirebase()
    {
        if (db == null || string.IsNullOrEmpty(playerId))
            return;

        var updates = new Dictionary<string, object>
        {
            [$"players/{playerId}/coins/total"] = TotalCoins,
            [$"players/{playerId}/coins/updatedAt"] = DateTime.UtcNow.ToString("o")
        };

        foreach (var kvp in byMode)
        {
            updates[$"players/{playerId}/coins/byMode/{kvp.Key}"] = kvp.Value;
        }

        try
        {
            await db.UpdateChildrenAsync(updates);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[CoinService] WriteFullCoinsToFirebase failed (will retry on next change). " + e);
        }
    }

    // ------------------------ EVENTS / READ API ------------------------

    private void FireAllEvents()
    {
        OnTotalChanged?.Invoke(TotalCoins);

        foreach (var kvp in byMode)
        {
            OnModeChanged?.Invoke(kvp.Key, kvp.Value);
        }
    }

    public int GetModeCoins(GameModeId mode)
        => byMode.TryGetValue(mode, out int v) ? v : 0;

    // ------------------------ COIN MUTATION CORE ------------------------

    private async void AddCoinsInternal(int amount, GameModeId mode)
    {
        if (amount <= 0) return;

        // Debug special case
        if (mode == GameModeId.NameTheFlag)
        {
            Debug.Log($"[CoinService] AddCoins NAME THE FLAG +{amount}, total BEFORE={TotalCoins}");
        }

        TotalCoins += amount;
        byMode[mode] = GetModeCoins(mode) + amount;

        SaveLocal();

        OnTotalChanged?.Invoke(TotalCoins);
        OnModeChanged?.Invoke(mode, byMode[mode]);

        // Fire-and-forget sync to Firebase
        if (db != null && !string.IsNullOrEmpty(playerId))
        {
            await WriteFullCoinsToFirebase();
        }
    }

    // ------------------------ PUBLIC HELPERS (USED BY OTHER SYSTEMS) ------------------------

    public void AddModeCoins(GameModeId mode, int amount)
    {
        AddCoinsInternal(amount, mode);
    }

    public void AddDailyRewardCoins(int amount)
    {
        AddCoinsInternal(amount, GameModeId.DailyRewards);
    }

    public void AddDailyQuestCoins(int amount)
    {
        AddCoinsInternal(amount, GameModeId.DailyQuests);
    }

    public void AddAchievementCoins(int amount)
    {
        AddCoinsInternal(amount, GameModeId.Achievements);
    }

    public void AddCharacterSellCoins(int amount)
    {
        // You can choose any bucket; Achievements is fine for "misc coins"
        AddCoinsInternal(amount, GameModeId.Achievements);
    }

    /// <summary>
    /// Used by Shop to spend coins.
    /// Updates local, fires events, and pushes to Firebase (if possible).
    /// </summary>
    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0) return true;

        Debug.Log($"[CoinService] TrySpendCoins amount={amount}, total BEFORE={TotalCoins}");

        if (TotalCoins < amount)
        {
            Debug.Log($"[CoinService] NOT ENOUGH COINS: need {amount}, have {TotalCoins}");
            return false;
        }

        TotalCoins -= amount;
        SaveLocal();
        OnTotalChanged?.Invoke(TotalCoins);

        // fire-and-forget sync to Firebase
        if (db != null && !string.IsNullOrEmpty(playerId))
        {
            _ = WriteFullCoinsToFirebase();
        }

        Debug.Log($"[CoinService] Spend OK. total AFTER={TotalCoins}");
        return true;
    }

    // ------------------------ HARD RESET (used by settings) ------------------------

    public void ForceSetAllZeroLocal()
    {
        TotalCoins = 0;

        foreach (GameModeId id in Enum.GetValues(typeof(GameModeId)))
        {
            byMode[id] = 0;
        }

        SaveLocal();
        FireAllEvents();
    }

    public void DebugForceZeroAndBroadcast()
    {
        ForceSetAllZeroLocal();
    }

    public void AddCoins(int amount, GameModeId mode)
    {
        AddCoinsInternal(amount, mode);
    }
}
