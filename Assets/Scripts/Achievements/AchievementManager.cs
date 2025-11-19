using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager I { get; private set; }

    [Header("Catalog")]
    public AchievementsCatalog catalog; // drag your AchievementsCatalog asset here

    [Header("Optional hooks")]
    public RewardToast rewardToast;     // optional, if you want a toast on completion
    public CoinWallet coinWallet;       // optional, in case you reward coins later

    [Header("Debug")]
    public bool logVerbose = true;

    const string PREFS_KEY = "BM_ACHIEVEMENTS_V1";
    AchievementsSave save = new();

    // quick lookup
    Dictionary<string, AchievementDef> defs = new();

    public event Action<string, AchievementProgressData> OnProgressChanged;  // id, data
    public event Action<string> OnCompleted;                                 // id

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        defs.Clear();
        if (catalog)
            foreach (var d in catalog.items)
                if (d && !string.IsNullOrEmpty(d.id))
                    defs[d.id] = d;

        Load();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            save = new AchievementsSave();
            Save();
            if (logVerbose) Debug.Log("[Achievements] ResetAll");
            foreach (var kv in defs)
                OnProgressChanged?.Invoke(kv.Key, save.GetOrCreate(kv.Key));
        }
    }

    void Load()
    {
        var json = PlayerPrefs.GetString(PREFS_KEY, "");
        if (!string.IsNullOrEmpty(json))
        {
            try { save = JsonUtility.FromJson<AchievementsSave>(json) ?? new AchievementsSave(); }
            catch { save = new AchievementsSave(); }
        }
        else save = new AchievementsSave();
    }

    void Save()
    {
        var json = JsonUtility.ToJson(save);
        PlayerPrefs.SetString(PREFS_KEY, json);
        PlayerPrefs.Save();
    }

    public AchievementProgressData GetProgress(string id) => save.GetOrCreate(id);

    /// <summary>Report progress. Example: Report("answers_any", 1)</summary>
    public void Report(string id, int amount = 1)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (!defs.TryGetValue(id, out var def))
        {
            if (logVerbose) Debug.LogWarning($"[Achievements] Unknown id '{id}'.");
            return;
        }

        var p = save.GetOrCreate(id);
        if (p.completed) return;

        p.value += Mathf.Max(0, amount);

        if (p.value >= def.target)
        {
            p.value = def.target;
            p.completed = true;
            p.completedAtIso = DateTime.UtcNow.ToString("o");

            if (logVerbose) Debug.Log($"[Achievements] Completed: {def.displayName} ({id})");

            // Optional: toast
            if (rewardToast)
                rewardToast.Show($"+ Achievement: {def.displayName}", null);

            OnCompleted?.Invoke(id);
        }

        Save();
        OnProgressChanged?.Invoke(id, p);
    }

    public bool IsCompleted(string id) => save.GetOrCreate(id).completed;

    public AchievementDef GetDef(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        // Fast path: use the lookup dictionary we already build in Awake()
        if (defs != null && defs.TryGetValue(id, out var def))
            return def;

        // Fallback: scan catalog in case the dict wasn’t populated for some reason
        if (catalog)
        {
            foreach (var d in catalog.items)
                if (d && d.id == id) return d;
        }
        return null;
    }

    public bool CanClaim(string id)
    {
        var def = GetDef(id);
        if (def == null) return false;
        var p = GetProgress(id);
        return p.completed && !p.rewardGranted && def.coinReward > 0 && !def.autoGrantReward;
    }

    public bool Claim(string id)
    {
        var def = GetDef(id);
        if (def == null) return false;
        var p = GetProgress(id);

        if (!p.completed || p.rewardGranted || def.coinReward <= 0 || def.autoGrantReward)
            return false;

        // Pay coins
        if (coinWallet) coinWallet.Add(def.coinReward);

        p.rewardGranted = true;
        Save();

        // Toast (numeric overload)
        if (rewardToast) rewardToast.Show(def.coinReward);

        if (logVerbose) Debug.Log($"[Achievements] Claimed reward for {id}: +{def.coinReward}");

        // refresh UI rows
        OnProgressChanged?.Invoke(id, p);
        return true;
    }

}
