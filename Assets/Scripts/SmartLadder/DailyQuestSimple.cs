using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyQuestSimple : MonoBehaviour
{
    // ---------- UI: wire your 5 rows here ----------
    [Header("UI Rows (drag your 5 Quest objects here)")]
    public QuestRow[] rows = new QuestRow[5];

    // ---------- Catalog Options ----------
    [Header("EITHER fill Inline OR assign a ScriptableObject (SO)")]
    [Tooltip("Use this if you want to type quests directly in the Inspector.")]
    public List<QuestDef> catalogInline = new List<QuestDef>();
    [Tooltip("Or drag a QuestCatalog asset here.")]
    public QuestCatalog catalogSO;

    [Header("Catalog Auto-Load (Resources)")]
    [SerializeField] bool autoLoadCatalogFromResources = true;
    [SerializeField] string catalogResourcePath = "Quests/QuestCatalog";

    [Header("Optional Services")]
    public CoinWallet coinWallet;                  // existing wallet (optional)
    public RewardToast rewardToast;                // if present in scene, you can assign (optional)

    [Header("Toast (auto-find/spawn, optional)")]
    public RewardToast rewardToastPrefab;          // prefab to instantiate if none found
    public Transform toastParent;                  // parent for spawned toast (usually your top Canvas)

    [Header("Debug")]
    public bool logVerbose = true;

    // ---------- Internals ----------
    const string SAVE_KEY = "BM_DailyQuestSimple_v1";
    const int QUESTS_PER_DAY = 5;

    public static DailyQuestSimple Instance { get; private set; }

    [Serializable]
    public class QuestDef
    {
        public string id;                  // "answer10_any"
        public string title;               // "Answer 10 Questions"
        [TextArea] public string description;
        public string progressTag;         // "answers_any"
        public int target = 10;
        public int coinReward = 100;
    }

    [Serializable]
    public class QuestRow
    {
        public TMP_Text title;             // Quest/Title text
        public TMP_Text description;       // Quest/Description text
        public Button actionButton;        // Right-side button
        public TMP_Text actionLabel;       // TMP on the button
    }

    [Serializable]
    class SavedEntry
    {
        public string id;
        public int current;
        public bool complete;
        public bool claimed;
    }

    [Serializable]
    class SavedState
    {
        public string yyyymmdd;
        public List<SavedEntry> entries = new();
    }

    SavedState _state;
    QuestDef[] _todayDefs = new QuestDef[QUESTS_PER_DAY];

    // ----------------- Unity -----------------
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureCatalog();
        LoadOrRollToday();
        BindAll();
        WireButtons();

        if (logVerbose) Debug.Log("[DailyQuest] Manager alive in scene: " + gameObject.scene.name);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;  // clear stale reference
    }

    void OnEnable()
    {
        // Rebind in case the day rolled while panel was closed
        BindAll();
    }

    // ----------------- Public: gameplay reporting -----------------
    /// <summary>
    /// Call from gameplay when player makes progress.
    /// Example: DailyQuestSimple.Report("answers_any", 1);
    /// </summary>
    public static void Report(string progressTag, int amount = 1)
    {
        // Resolve instance with Unity-style null checks (handles destroyed objects)
        DailyQuestSimple inst = Instance;
        if (inst == null) inst = FindObjectOfType<DailyQuestSimple>();
        if (inst == null) inst = CreateHeadlessManager();   // last resort

        if (inst == null) { Debug.LogError("DailyQuest.Report: no instance and auto-create failed."); return; }
        if (amount <= 0) return;

        bool changed = false, anyMatch = false;

        for (int i = 0; i < inst._todayDefs.Length; i++)
        {
            var def = inst._todayDefs[i];
            if (def == null) continue;

            var e = inst._state.entries[i];
            if (def.progressTag == progressTag && !e.complete && !e.claimed)
            {
                anyMatch = true;
                int before = e.current;
                e.current = Mathf.Clamp(e.current + amount, 0, def.target);
                if (e.current >= def.target) e.complete = true;
                changed = true;

                if (inst.logVerbose)
                    Debug.Log($"[DailyQuest] MATCH '{def.title}': {before}->{e.current}/{def.target}");
            }
        }

        if (!anyMatch && inst.logVerbose)
            Debug.Log($"[DailyQuest] No quest matched tag '{progressTag}' (not in today's 5, or already complete/claimed).");

        if (changed)
        {
            inst.Save();
            inst.BindAll(); // updates UI if a view is attached
        }
    }

    // ----------------- UI wiring -----------------
    void WireButtons()
    {
        for (int i = 0; i < rows.Length; i++)
        {
            int idx = i;
            if (rows[i]?.actionButton == null) continue;
            rows[i].actionButton.onClick.RemoveAllListeners();
            rows[i].actionButton.onClick.AddListener(() => OnClickRow(idx));
        }
    }

    void OnClickRow(int i)
    {
        if (i < 0 || i >= _todayDefs.Length) return;
        var def = _todayDefs[i];
        if (def == null) return;

        var e = _state.entries[i];

        if (e.complete && !e.claimed)
        {
            // Claim reward
            e.claimed = true;
            Save();

            // Coins
            var wallet = GetWallet();
            if (wallet != null) wallet.Add(def.coinReward);

            // Toast (auto-find or spawn)
            var toast = GetToast();
            if (toast != null)
            {
                // Adjust this call if your RewardToast signature differs
                toast.Show(def.coinReward, null);
            }
            else
            {
                Debug.LogWarning("[DailyQuest] No RewardToast found or prefab not assigned; skipping toast.");
            }

            BindRow(i);
        }
        else
        {
            // Not complete; do nothing or navigate to a relevant mode if you want.
            if (logVerbose) Debug.Log($"[DailyQuest] '{def.title}' not complete yet. Progress {e.current}/{def.target}");
        }
    }

    // ----------------- Binding -----------------
    void BindAll()
    {
        for (int i = 0; i < rows.Length; i++) BindRow(i);
    }

    void BindRow(int i)
    {
        var r = rows[i];
        if (r == null) return;

        var def = i < _todayDefs.Length ? _todayDefs[i] : null;
        if (def == null)
        {
            SafeSet(r.title, "");
            SafeSet(r.description, "");
            SetButtonState(r, false, "");
            return;
        }

        var e = _state.entries[i];

        SafeSet(r.title, def.title);
        SafeSet(r.description, def.description);

        if (e.claimed)
        {
            SetButtonState(r, false, "CLAIMED");
        }
        else if (e.complete)
        {
            SetButtonState(r, true, $"+{def.coinReward}");
        }
        else
        {
            SetButtonState(r, true, $"{e.current}/{def.target}");
        }
    }

    void SafeSet(TMP_Text t, string val)
    {
        if (t) t.text = val;
    }

    void SetButtonState(QuestRow r, bool interactable, string label)
    {
        if (r.actionButton) r.actionButton.interactable = interactable;
        if (r.actionLabel) r.actionLabel.text = label;
    }

    // ----------------- Selection / Save -----------------
    void LoadOrRollToday()
    {
        string today = DateTime.Now.ToString("yyyyMMdd");

        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            _state = JsonUtility.FromJson<SavedState>(PlayerPrefs.GetString(SAVE_KEY));
        }

        if (_state == null || _state.yyyymmdd != today)
        {
            RollNew(today);
        }
        else
        {
            // Rehydrate today's quest definitions from saved IDs
            var pool = BuildPool();
            for (int i = 0; i < _todayDefs.Length; i++)
            {
                var id = _state.entries.ElementAtOrDefault(i)?.id;
                _todayDefs[i] = pool.FirstOrDefault(q => q.id == id);
            }
        }
    }

    void RollNew(string yyyymmdd)
    {
        var pool = BuildPool();
        if (pool.Count == 0)
        {
            Debug.LogWarning("[DailyQuest] No quests found in Inline or SO catalog.");
            _state = new SavedState { yyyymmdd = yyyymmdd, entries = new List<SavedEntry>() };
            for (int i = 0; i < _todayDefs.Length; i++)
            {
                _todayDefs[i] = null;
                _state.entries.Add(new SavedEntry());
            }
            Save();
            return;
        }

        // Deterministic shuffle by date so selection is stable for the day
        var rng = new System.Random(yyyymmdd.GetHashCode());
        pool = pool.OrderBy(_ => rng.Next()).ToList();

        for (int i = 0; i < _todayDefs.Length; i++)
            _todayDefs[i] = i < pool.Count ? pool[i] : null;

        _state = new SavedState { yyyymmdd = yyyymmdd, entries = new List<SavedEntry>() };
        for (int i = 0; i < _todayDefs.Length; i++)
        {
            var def = _todayDefs[i];
            if (def == null) { _state.entries.Add(new SavedEntry()); continue; }
            _state.entries.Add(new SavedEntry { id = def.id, current = 0, complete = false, claimed = false });
        }
        Save();
    }

    List<QuestDef> BuildPool()
    {
        EnsureCatalog();

        var pool = new List<QuestDef>();

        // Prefer ScriptableObject if assigned and has content
        if (catalogSO != null && catalogSO.quests != null && catalogSO.quests.Count > 0)
        {
            foreach (var q in catalogSO.quests)
            {
                if (q == null || !q.eligibleForDaily || string.IsNullOrWhiteSpace(q.questId)) continue;
                pool.Add(FromSO(q));
            }
        }
        else
        {
            // Fallback to inline
            foreach (var q in catalogInline)
            {
                if (q == null || string.IsNullOrWhiteSpace(q.id)) continue;
                pool.Add(q);
            }
        }

        return pool;
    }

    QuestDef FromSO(QuestDefinition d) => new QuestDef
    {
        id = d.questId,
        title = d.title,
        description = d.description,
        progressTag = d.progressTag,
        target = d.target,
        coinReward = d.coinReward
    };

    void Save()
    {
        PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(_state));
        PlayerPrefs.Save();
    }

    // ----------------- Catalog & Services helpers -----------------
    void EnsureCatalog()
    {
        if (catalogSO == null && autoLoadCatalogFromResources)
        {
            catalogSO = Resources.Load<QuestCatalog>(catalogResourcePath);
            if (catalogSO == null)
                Debug.LogWarning($"[DailyQuest] Could not load catalog at Resources/{catalogResourcePath}.asset");
        }
    }

    CoinWallet GetWallet()
    {
        if (coinWallet == null) coinWallet = FindObjectOfType<CoinWallet>(true);
        return coinWallet;
    }

    RewardToast GetToast()
    {
        if (rewardToast != null) return rewardToast;

        // Try to find one in the active scene (even if inactive)
        rewardToast = FindObjectOfType<RewardToast>(true);
        if (rewardToast != null) return rewardToast;

        // Spawn from prefab if assigned
        if (rewardToastPrefab != null)
        {
            Transform parent = toastParent != null ? toastParent : GetTopCanvasTransform();
            rewardToast = Instantiate(rewardToastPrefab, parent, false);
            return rewardToast;
        }
        return null;
    }

    Transform GetTopCanvasTransform()
    {
        Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>(true);
        if (canvases != null && canvases.Length > 0)
        {
            Canvas best = canvases[0];
            for (int i = 1; i < canvases.Length; i++)
                if (canvases[i].sortingOrder > best.sortingOrder) best = canvases[i];
            return best.transform;
        }
        return this.transform;
    }

    // ----------------- View attach/detach (panel binder calls these) -----------------
    public void AttachView(QuestRow[] newRows)
    {
        rows = newRows ?? Array.Empty<QuestRow>();
        BindAll();
        WireButtons();
    }

    public void DetachView()
    {
        rows = Array.Empty<QuestRow>();   // drop UI refs when panel closes
    }

    // ----------------- Debug helpers -----------------
    [ContextMenu("DEBUG: Print Today Quests")]
    public void DebugPrintToday()
    {
        if (_todayDefs == null) { Debug.Log("No today defs."); return; }
        for (int i = 0; i < _todayDefs.Length; i++)
        {
            var def = _todayDefs[i];
            if (def == null) { Debug.Log($"[{i}] <empty>"); continue; }
            var e = _state.entries[i];
            Debug.Log($"[{i}] id='{def.id}', tag='{def.progressTag}', progress={e.current}/{def.target}, complete={e.complete}, claimed={e.claimed}");
        }
    }

    [ContextMenu("DEBUG: Reset Daily State")]
    public void DebugResetDaily()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        LoadOrRollToday();
        BindAll();
        Debug.Log("[DailyQuest] Daily state reset and re-rolled.");
    }

    // ----------------- Auto-creator -----------------
    static DailyQuestSimple CreateHeadlessManager()
    {
        var go = new GameObject("DailyQuestManager (Auto)");
        var inst = go.AddComponent<DailyQuestSimple>();
        DontDestroyOnLoad(go);

        // Make sure a real catalog exists even if spawned mid-game
        inst.EnsureCatalog();
        inst.LoadOrRollToday();

        return inst;
    }
}
