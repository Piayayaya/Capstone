using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class DailyQuestEntry
{
    public string questId;
    public int current;
    public int target;
    public bool isComplete;
    public bool isClaimed;
    public int coinReward;
    public string progressTag;
}

[Serializable]
public class DailyQuestState
{
    public string yyyymmdd;                     // which day this selection belongs to
    public List<DailyQuestEntry> entries = new();
}

public class DailyQuestService : MonoBehaviour
{
    public static DailyQuestService I { get; private set; }

    [Header("Catalog & UI hooks")]
    public QuestCatalog catalog;
    public RewardToast rewardToast;            // (optional) hook if you already have one
    public CoinWallet coinWallet;              // coin service you already use; simple example below

    [Header("Rules")]
    public int questsPerDay = 5;

    public event Action OnDailyListChanged;    // UI can refresh when this fires

    const string SAVE_KEY = "BM_DailyQuestState_v1";
    DailyQuestState _state;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        LoadOrGenerateForToday();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus) ResetIfNewDay();
    }

    void LoadOrGenerateForToday()
    {
        string today = DateTime.Now.ToString("yyyyMMdd");
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            var json = PlayerPrefs.GetString(SAVE_KEY);
            _state = JsonUtility.FromJson<DailyQuestState>(json);
            if (_state != null && _state.yyyymmdd == today)
            {
                OnDailyListChanged?.Invoke();
                return; // already today’s list
            }
        }
        GenerateNewDailyList(today);
    }

    void GenerateNewDailyList(string yyyymmdd)
    {
        // Pick from eligible quests (ignore ones with empty IDs)
        var pool = catalog.quests
            .Where(q => q != null && q.eligibleForDaily && !string.IsNullOrWhiteSpace(q.questId))
            .ToList();

        // Deterministic shuffle by day string so same selection persists if you don't save for some reason
        int seed = yyyymmdd.GetHashCode();
        System.Random rng = new System.Random(seed);
        pool = pool.OrderBy(_ => rng.Next()).ToList();

        var picked = pool.Take(Mathf.Min(questsPerDay, pool.Count)).ToList();

        _state = new DailyQuestState
        {
            yyyymmdd = yyyymmdd,
            entries = picked.Select(q => new DailyQuestEntry
            {
                questId = q.questId,
                current = 0,
                target = Mathf.Max(1, q.target),
                isComplete = false,
                isClaimed = false,
                coinReward = Mathf.Max(0, q.coinReward),
                progressTag = q.progressTag
            }).ToList()
        };

        Save();
        OnDailyListChanged?.Invoke();
    }

    void Save()
    {
        var json = JsonUtility.ToJson(_state);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    void ResetIfNewDay()
    {
        string today = DateTime.Now.ToString("yyyyMMdd");
        if (_state == null || _state.yyyymmdd != today)
        {
            GenerateNewDailyList(today);
        }
    }

    public IReadOnlyList<DailyQuestEntry> GetEntries() => _state?.entries;

    public DailyQuestEntry GetEntryById(string questId) =>
        _state?.entries?.FirstOrDefault(e => e.questId == questId);

    // Call this from gameplay: e.g., Report("answers_any", 1) when player answers a question
    public void Report(string progressTag, int amount = 1)
    {
        if (_state == null || amount <= 0) return;

        bool changed = false;
        foreach (var e in _state.entries)
        {
            if (e.progressTag == progressTag && !e.isComplete)
            {
                e.current = Mathf.Clamp(e.current + amount, 0, e.target);
                if (e.current >= e.target) e.isComplete = true;
                changed = true;
            }
        }
        if (changed)
        {
            Save();
            OnDailyListChanged?.Invoke();
        }
    }

    public bool CanClaim(string questId)
    {
        var e = GetEntryById(questId);
        return e != null && e.isComplete && !e.isClaimed;
    }

    public void Claim(string questId)
    {
        var e = GetEntryById(questId);
        if (e == null || !e.isComplete || e.isClaimed) return;

        e.isClaimed = true;
        Save();
        OnDailyListChanged?.Invoke();

        // Give coins
        if (coinWallet != null) coinWallet.Add(e.coinReward);

        // Optional toast integration
        if (rewardToast != null)
        {
            rewardToast.Show(e.coinReward, null); // your RewardToast(int amount, Action onDone)
        }
    }
}
