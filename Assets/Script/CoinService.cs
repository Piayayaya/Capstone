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

    private const string LOCAL_TOTAL = "BM_LOCAL_TOTAL_COINS";
    private const string LOCAL_MODE_PREFIX = "BM_LOCAL_MODE_COINS_";
    private const string PLAYER_KEY = "DEVICE_PLAYER_ID";

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadLocal();
        FireAllEvents();
        Debug.Log($"[CoinService] Awake. Local total={TotalCoins}");

        await InitFirebaseAndPlayer();
    }

    private async Task InitFirebaseAndPlayer()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        int guard = 0;
        while (!FirebaseInit.IsReady && guard < 50)
        {
            await Task.Delay(100);
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

        playerId = PlayerPrefs.GetString(PLAYER_KEY, "");
        Debug.Log($"[CoinService] InitFirebaseAndPlayer: stored playerId='{playerId}'");

        if (db != null && !string.IsNullOrEmpty(playerId))
        {
            await LoadFromFirebase();
        }
#else
        // Editor / non-Android: no Firebase, just keep local coins
        playerId = PlayerPrefs.GetString(PLAYER_KEY, "");
        Debug.Log($"[CoinService] InitFirebaseAndPlayer (Editor/local-only). playerId='{playerId}'");
        await Task.CompletedTask;
#endif
    }

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

        if (db == null)
            await InitFirebaseAndPlayer();
        else
            await LoadFromFirebase();
    }

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
            Debug.Log("[CoinService] No coins node in Firebase; pushing local values.");
            await WriteFullCoinsToFirebase();
            return;
        }

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

    private async void AddCoinsInternal(int amount, GameModeId mode)
    {
        if (amount <= 0) return;

        if (mode == GameModeId.NameTheFlag)
        {
            Debug.Log($"[CoinService] AddCoins NAME THE FLAG +{amount}, total BEFORE={TotalCoins}");
        }

        if (NotificationService.Instance != null)
        {
            NotificationService.Instance.LogCoinsEarned(amount, mode);
        }

        TotalCoins += amount;
        byMode[mode] = GetModeCoins(mode) + amount;

        SaveLocal();

        OnTotalChanged?.Invoke(TotalCoins);
        OnModeChanged?.Invoke(mode, byMode[mode]);

        if (db != null && !string.IsNullOrEmpty(playerId))
        {
            await WriteFullCoinsToFirebase();
        }
    }

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
        AddCoinsInternal(amount, GameModeId.Achievements);
    }

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

        if (db != null && !string.IsNullOrEmpty(playerId))
        {
            _ = WriteFullCoinsToFirebase();
        }

        Debug.Log($"[CoinService] Spend OK. total AFTER={TotalCoins}");
        return true;
    }

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
