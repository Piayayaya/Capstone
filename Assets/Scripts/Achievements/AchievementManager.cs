using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager I { get; private set; }

    [Header("Catalog (for icons / flags)")]
    public AchievementsCatalog catalog; // drag your AchievementsCatalog asset here

    [Header("Optional hooks")]
    public RewardToast rewardToast;     // optional, if you want a toast on completion
    public CoinWallet coinWallet;       // optional, in case you reward coins later

    [Header("Debug")]
    public bool logVerbose = true;

    const string PREFS_KEY = "BM_ACHIEVEMENTS_V1";
    AchievementsSave save = new();

    // ---- LOOKUPS ----
    // ScriptableObject defs (for visuals)
    Dictionary<string, AchievementDef> soDefs = new();
    // SQLite defs (for actual logic / numbers)
    Dictionary<string, LocalAchievementDef> localDefs = new();
    // key: progressTag -> list of local defs that listen to that tag
    Dictionary<string, List<LocalAchievementDef>> byTag = new();

    public event Action<string, AchievementProgressData> OnProgressChanged;  // id, data
    public event Action<string> OnCompleted;                                 // id

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        BuildDefinitionsFromSqliteAndCatalog();
        Load();
    }

    void Update()
    {
        // Debug reset (optional)
        if (Input.GetKeyDown(KeyCode.R))
        {
            save = new AchievementsSave();
            Save();
            if (logVerbose) Debug.Log("[Achievements] ResetAll");
            foreach (var kv in localDefs)
                OnProgressChanged?.Invoke(kv.Key, save.GetOrCreate(kv.Key));
        }
    }

    // ----------------------------------------------------------------------
    // LOAD / SAVE PROGRESS
    // ----------------------------------------------------------------------
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

    // ----------------------------------------------------------------------
    // DEFINITION BUILDING (SQLite + ScriptableObject)
    // ----------------------------------------------------------------------
    void BuildDefinitionsFromSqliteAndCatalog()
    {
        soDefs.Clear();
        localDefs.Clear();
        byTag.Clear();

        // 1) ScriptableObject catalog for visuals / flags
        if (catalog && catalog.items != null)
        {
            foreach (var d in catalog.items)
            {
                if (!d || string.IsNullOrEmpty(d.id)) continue;
                soDefs[d.id] = d;
            }
        }

        // 2) SQLite master data (from MasterSqliteSync)
        try
        {
            LocalDb.Init(); // safe if already initialized
            var rows = LocalDb.DB.Table<LocalAchievementDef>().ToList();

            foreach (var row in rows)
            {
                if (row == null || string.IsNullOrWhiteSpace(row.id)) continue;

                localDefs[row.id] = row;

                if (!string.IsNullOrEmpty(row.progressTag))
                {
                    if (!byTag.TryGetValue(row.progressTag, out var list))
                    {
                        list = new List<LocalAchievementDef>();
                        byTag[row.progressTag] = list;
                    }
                    list.Add(row);
                }
            }

            if (logVerbose)
                Debug.Log($"[Achievements] Loaded {localDefs.Count} achievements from SQLite.");
        }
        catch (Exception ex)
        {
            if (logVerbose)
                Debug.LogError("[Achievements] Failed to load LocalAchievementDef from SQLite: " + ex);
        }
    }

    // ----------------------------------------------------------------------
    // REPORTING PROGRESS
    // ----------------------------------------------------------------------
    /// <summary>
    /// Report progress.
    /// - If idOrTag matches an achievementId (from SQLite), only that achievement is updated.
    /// - Otherwise, if it matches a progressTag, ALL achievements with that tag are updated.
    /// Example: Report("answers_any", 1);
    /// </summary>
    public void Report(string idOrTag, int amount = 1)
    {
        if (string.IsNullOrEmpty(idOrTag)) return;
        if (amount <= 0) return;

        bool any = false;

        // 1) Try direct by achievementId (SQLite)
        if (localDefs.TryGetValue(idOrTag, out var defById))
        {
            ApplyProgressToDef(defById, amount);
            any = true;
        }
        // 2) Treat as progressTag
        else if (byTag.TryGetValue(idOrTag, out var list) && list != null)
        {
            foreach (var def in list)
            {
                if (def == null) continue;
                ApplyProgressToDef(def, amount);
                any = true;
            }
        }

        if (!any && logVerbose)
            Debug.LogWarning($"[Achievements] Unknown id/tag '{idOrTag}'.");
    }

    /// <summary>
    /// Internal helper: apply progress to a single achievement definition (SQLite data).
    /// </summary>
    void ApplyProgressToDef(LocalAchievementDef local, int amount)
    {
        if (local == null) return;

        var id = local.id;
        var p = save.GetOrCreate(id);
        if (p.completed) return;

        int target = local.target > 0 ? local.target : 1;
        p.value += Mathf.Max(0, amount);

        if (p.value >= target)
        {
            p.value = target;
            p.completed = true;
            p.completedAtIso = DateTime.UtcNow.ToString("o");

            // Resolve display name (prefer SQLite title, then SO, then id)
            string displayName = !string.IsNullOrEmpty(local.title) ? local.title : id;
            if (soDefs.TryGetValue(id, out var so) && !string.IsNullOrEmpty(so.displayName))
                displayName = so.displayName;

            if (logVerbose) Debug.Log($"[Achievements] Completed: {displayName} ({id})");

            // Optional: toast (string overload)
            if (rewardToast)
                rewardToast.Show($"+ Achievement: {displayName}", null);

            OnCompleted?.Invoke(id);

            // Optional auto-grant reward (flag comes from SO)
            bool autoGrant = so != null && so.autoGrantReward;
            int coinReward = local.coinReward > 0 ? local.coinReward : (so != null ? so.coinReward : 0);

            if (autoGrant && coinReward > 0 && !p.rewardGranted)
            {
                if (coinWallet) coinWallet.Add(coinReward);
                p.rewardGranted = true;

                // numeric toast if you like
                if (rewardToast) rewardToast.Show(coinReward);
            }
        }

        Save();
        OnProgressChanged?.Invoke(id, p);
    }

    // ----------------------------------------------------------------------
    // QUERY / CLAIM
    // ----------------------------------------------------------------------
    public bool IsCompleted(string id) => save.GetOrCreate(id).completed;

    /// <summary>
    /// Returns the ScriptableObject definition (for icon / visuals).
    /// NOTE: Logic uses SQLite (LocalAchievementDef), not this.
    /// </summary>
    public AchievementDef GetDef(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        // Fast path: use SO lookup dictionary
        if (soDefs != null && soDefs.TryGetValue(id, out var def))
            return def;

        // Fallback: scan catalog in case something wasn't in the dict
        if (catalog && catalog.items != null)
        {
            foreach (var d in catalog.items)
                if (d && d.id == id) return d;
        }
        return null;
    }

    LocalAchievementDef GetLocalDef(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        localDefs.TryGetValue(id, out var d);
        return d;
    }

    public bool CanClaim(string id)
    {
        var local = GetLocalDef(id);
        var so = GetDef(id); // visuals/flags

        if (local == null && so == null) return false;

        var p = GetProgress(id);

        int coinReward = local?.coinReward ?? (so != null ? so.coinReward : 0);
        bool autoGrant = so != null && so.autoGrantReward;

        return p.completed && !p.rewardGranted && coinReward > 0 && !autoGrant;
    }

    public bool Claim(string id)
    {
        var local = GetLocalDef(id);
        var so = GetDef(id);

        if (local == null && so == null) return false;

        var p = GetProgress(id);

        int coinReward = local?.coinReward ?? (so != null ? so.coinReward : 0);
        bool autoGrant = so != null && so.autoGrantReward;

        if (!p.completed || p.rewardGranted || coinReward <= 0 || autoGrant)
            return false;

        // Pay coins
        if (coinWallet) coinWallet.Add(coinReward);

        p.rewardGranted = true;
        Save();

        // Toast (numeric overload)
        if (rewardToast) rewardToast.Show(coinReward);

        if (logVerbose) Debug.Log($"[Achievements] Claimed reward for {id}: +{coinReward}");

        // refresh UI rows
        OnProgressChanged?.Invoke(id, p);
        return true;
    }
}
