using System.Collections.Generic;
using UnityEngine;

public class AchievementsPanel : MonoBehaviour
{
    [Header("Wiring")]
    public AchievementsCatalog catalog;         // same as manager or a subset
    public Transform contentRoot;               // ScrollView/Viewport/Content
    public AchievementRowBinder rowPrefab;      // a prefab with the binder script

    readonly List<AchievementRowBinder> rows = new();

    void OnEnable() { Build(); }
    void OnDisable() { Clear(); }

    void Clear()
    {
        foreach (var r in rows) if (r) Destroy(r.gameObject);
        rows.Clear();
    }

    public void Build()
    {
        if (!catalog || !rowPrefab || !contentRoot || !AchievementManager.I) return;
        Clear();

        foreach (var def in catalog.items)
        {
            if (!def || string.IsNullOrEmpty(def.id)) continue;

            // Hide until started?
            if (def.hiddenUntilStarted)
            {
                var prog = AchievementManager.I.GetProgress(def.id);
                if (prog.value <= 0 && !prog.completed) continue;
            }

            var row = Instantiate(rowPrefab, contentRoot);
            var data = AchievementManager.I.GetProgress(def.id);
            row.Bind(def, data);
            rows.Add(row);

            for (int i = 0; i < rows.Count; i++)
                rows[i].SetIsLast(i == rows.Count - 1);
        }
    }
}
