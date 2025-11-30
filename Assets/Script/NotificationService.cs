using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NotificationEntry
{
    public string id;          // unique id (guid)
    public string message;     // "You earned 10 coins from NameTheFlag."
    public string createdAt;   // "2025-12-01 13:45"
}

[Serializable]
class NotificationSave
{
    public List<NotificationEntry> entries = new();
}

public class NotificationService : MonoBehaviour
{
    public static NotificationService Instance { get; private set; }

    const string PREF_KEY = "BM_NotificationLog_v1";
    const int MAX_ENTRIES = 50;      // keep last 50 notifications

    public event Action OnLogChanged;

    NotificationSave _state;

    /// <summary>Read-only list for UI.</summary>
    public IReadOnlyList<NotificationEntry> Entries => _state.entries;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    void Load()
    {
        if (PlayerPrefs.HasKey(PREF_KEY))
        {
            try
            {
                var json = PlayerPrefs.GetString(PREF_KEY);
                _state = JsonUtility.FromJson<NotificationSave>(json);
                if (_state == null || _state.entries == null)
                    _state = new NotificationSave();
            }
            catch
            {
                _state = new NotificationSave();
            }
        }
        else
        {
            _state = new NotificationSave();
        }
    }

    void Save()
    {
        var json = JsonUtility.ToJson(_state);
        PlayerPrefs.SetString(PREF_KEY, json);
        PlayerPrefs.Save();
    }

    /// <summary>Add any custom message to the log.</summary>
    public void Add(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        var entry = new NotificationEntry
        {
            id = Guid.NewGuid().ToString(),
            message = message.Trim(),
            createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
        };

        // newest first
        _state.entries.Insert(0, entry);

        // limit list size
        if (_state.entries.Count > MAX_ENTRIES)
        {
            _state.entries.RemoveRange(MAX_ENTRIES, _state.entries.Count - MAX_ENTRIES);
        }

        Save();
        OnLogChanged?.Invoke();
    }

    /// <summary>Remove all notifications (e.g., Reset Progress / Delete Account).</summary>
    public void ClearAll()
    {
        _state.entries.Clear();
        Save();
        OnLogChanged?.Invoke();
    }

    // -------- Helper wrappers for common cases --------

    public void LogDailyLogin(int dayIndexOneBased, int coins)
    {
        Add($"You have successfully claimed {coins} coins for Day {dayIndexOneBased} login.");
    }

    public void LogCoinsEarned(int coins, GameModeId mode)
    {
        // You can prettify this switch if you want nicer names
        string modeName = mode.ToString().Replace("_", " ");
        Add($"You earned {coins} coins from {modeName}.");
    }
}
