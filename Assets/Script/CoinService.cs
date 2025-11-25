using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using System.Threading.Tasks;

public enum GameModeId
{
    SmartLadder,
    NameTheFlag,
    DragAndDrop,
    TuneYourTongue,
    SeeItOrLoseIt,
    DailyRewards,
    DailyQuests
}

public class CoinService : MonoBehaviour
{
    public static CoinService Instance;

    private DatabaseReference db;
    private string playerId;

    public int TotalCoins { get; private set; }
    private Dictionary<GameModeId, int> byMode = new();

    public event Action<int> OnTotalChanged;
    public event Action<GameModeId, int> OnModeChanged;

    private const string LOCAL_TOTAL = "LOCAL_TOTAL_COINS";
    private const string LOCAL_MODE_PREFIX = "LOCAL_MODE_COINS_";

    private async void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        await Init();
    }

    private async Task Init()
    {
        await Task.Delay(300); // wait Firebase ready
        db = FirebaseDatabase.DefaultInstance.RootReference;

        playerId = PlayerPrefs.GetString("DEVICE_PLAYER_ID", "");
        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogWarning("No playerId yet. CoinService will use local only until login/create.");
            LoadLocal();
            return;
        }

        await LoadFromFirebase();
    }

    // Called after you create/login player
    public async Task SetPlayer(string newPlayerId)
    {
        playerId = newPlayerId;
        PlayerPrefs.SetString("DEVICE_PLAYER_ID", playerId);

        await LoadFromFirebase();
    }

    private void LoadLocal()
    {
        TotalCoins = PlayerPrefs.GetInt(LOCAL_TOTAL, 0);
        foreach (GameModeId id in Enum.GetValues(typeof(GameModeId)))
        {
            byMode[id] = PlayerPrefs.GetInt(LOCAL_MODE_PREFIX + id, 0);
        }
    }

    private void SaveLocal()
    {
        PlayerPrefs.SetInt(LOCAL_TOTAL, TotalCoins);
        foreach (var kvp in byMode)
            PlayerPrefs.SetInt(LOCAL_MODE_PREFIX + kvp.Key, kvp.Value);

        PlayerPrefs.Save();
    }

    private async Task LoadFromFirebase()
    {
        // fallback to local first so UI not empty
        LoadLocal();
        FireAllEvents();

        var snap = await db.Child("players").Child(playerId).Child("coins").GetValueAsync();
        if (!snap.Exists)
        {
            // create if missing
            await WriteFullCoinsToFirebase();
            return;
        }

        TotalCoins = snap.Child("total").Exists ? int.Parse(snap.Child("total").Value.ToString()) : 0;

        foreach (GameModeId id in Enum.GetValues(typeof(GameModeId)))
        {
            var modeSnap = snap.Child("byMode").Child(id.ToString());
            byMode[id] = modeSnap.Exists ? int.Parse(modeSnap.Value.ToString()) : 0;
        }

        SaveLocal();
        FireAllEvents();
    }

    private void FireAllEvents()
    {
        OnTotalChanged?.Invoke(TotalCoins);
        foreach (var kvp in byMode)
            OnModeChanged?.Invoke(kvp.Key, kvp.Value);
    }

    public int GetModeCoins(GameModeId mode)
        => byMode.TryGetValue(mode, out int v) ? v : 0;

    public async void AddCoins(int amount, GameModeId mode)
    {
        if (amount <= 0) return;

        // update local immediately
        TotalCoins += amount;
        byMode[mode] = GetModeCoins(mode) + amount;
        SaveLocal();

        OnTotalChanged?.Invoke(TotalCoins);
        OnModeChanged?.Invoke(mode, byMode[mode]);

        // if no player logged in yet, stop here
        if (string.IsNullOrEmpty(playerId) || db == null) return;

        // write updated values to firebase
        await WriteFullCoinsToFirebase();
    }

    // --- convenience wrappers (optional, but nice) ---
    public void AddDailyRewardCoins(int amount)
    {
        AddCoins(amount, GameModeId.DailyRewards);
    }

    public void AddDailyQuestCoins(int amount)
    {
        AddCoins(amount, GameModeId.DailyQuests);
    }

    private async Task WriteFullCoinsToFirebase()
    {
        var updates = new Dictionary<string, object>
        {
            [$"players/{playerId}/coins/total"] = TotalCoins,
            [$"players/{playerId}/coins/updatedAt"] = DateTime.UtcNow.ToString("o")
        };

        foreach (var kvp in byMode)
            updates[$"players/{playerId}/coins/byMode/{kvp.Key}"] = kvp.Value;

        await db.UpdateChildrenAsync(updates);
    }
}
