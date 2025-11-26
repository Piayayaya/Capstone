using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementsPanel : MonoBehaviour
{
    [Header("Wiring")]
    [Tooltip("ScrollView/Viewport/Content transform")]
    public Transform contentRoot;
    [Tooltip("Prefab with AchievementRowBinder on it")]
    public AchievementRowBinder rowPrefab;

    private readonly List<AchievementRowBinder> rows = new();

    void OnEnable()
    {
        Build();
    }

    void OnDisable()
    {
        Clear();
    }

    void Clear()
    {
        foreach (var r in rows)
        {
            if (r) Destroy(r.gameObject);
        }
        rows.Clear();
    }

    public void Build()
    {
        if (!contentRoot || !rowPrefab)
        {
            Debug.LogWarning("[AchievementsPanel] contentRoot or rowPrefab not assigned.");
            return;
        }

        Clear();

        var mgr = AchievementManager.I;
        if (mgr == null)
        {
            Debug.LogWarning("[AchievementsPanel] No AchievementManager instance.");
            return;
        }

        // Load all achievement rows from SQLite
        List<LocalAchievementDef> locals;
        try
        {
            locals = LocalDb.DB.Table<LocalAchievementDef>().ToList();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[AchievementsPanel] Failed to query LocalAchievementDef: " + ex);
            return;
        }

        if (locals == null || locals.Count == 0)
        {
            Debug.LogWarning("[AchievementsPanel] No LocalAchievementDef records found.");
            return;
        }

        foreach (var local in locals)
        {
            if (local == null || string.IsNullOrEmpty(local.id))
                continue;

            // Runtime def (for icon, flags, etc.)
            var runtimeDef = mgr.GetDef(local.id);
            // Progress data from PlayerPrefs
            var prog = mgr.GetProgress(local.id);

            var row = Instantiate(rowPrefab, contentRoot);
            row.Bind(local, runtimeDef, prog);
            rows.Add(row);
        }

        // Mark last row so we can hide bottom divider, etc.
        for (int i = 0; i < rows.Count; i++)
        {
            rows[i].SetIsLast(i == rows.Count - 1);
        }
    }
}
