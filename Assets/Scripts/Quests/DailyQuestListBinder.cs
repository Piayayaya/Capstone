using System.Linq;
using TMPro;
using UnityEngine;

public class DailyQuestListBinder : MonoBehaviour
{
    [Header("Wiring")]
    public DailyQuestItemBinder itemPrefab;
    public Transform contentRoot;     // e.g., a vertical layout group parent
    public QuestCatalog catalog;

    void OnEnable()
    {
        if (DailyQuestService.I != null)
            DailyQuestService.I.OnDailyListChanged += Rebuild;
        Rebuild();
    }

    void OnDisable()
    {
        if (DailyQuestService.I != null)
            DailyQuestService.I.OnDailyListChanged -= Rebuild;
    }

    void Clear()
    {
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);
    }

    void Rebuild()
    {
        Clear();
        var entries = DailyQuestService.I?.GetEntries();
        if (entries == null) return;

        foreach (var e in entries)
        {
            var def = catalog.quests.FirstOrDefault(q => q.questId == e.questId);
            var item = Instantiate(itemPrefab, contentRoot);
            item.Bind(e.questId, def);
        }
    }
}
