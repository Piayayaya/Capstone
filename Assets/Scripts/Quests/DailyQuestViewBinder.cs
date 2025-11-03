using UnityEngine;

public class DailyQuestViewBinder : MonoBehaviour
{
    public DailyQuestSimple.QuestRow[] rows;

    void OnEnable()
    {
        if (DailyQuestSimple.Instance != null)
            DailyQuestSimple.Instance.AttachView(rows);
    }

    void OnDisable()
    {
        if (DailyQuestSimple.Instance != null)
            DailyQuestSimple.Instance.DetachView();
    }
}
