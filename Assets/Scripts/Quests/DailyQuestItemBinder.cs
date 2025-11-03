using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyQuestItemBinder : MonoBehaviour
{
    [Header("Wiring")]
    public TMP_Text titleText;
    public TMP_Text descText;
    public TMP_Text progressText;
    public Slider progressBar;
    public Button claimButton;

    [Header("Runtime")]
    public string questId; // set by list binder

    void OnEnable()
    {
        if (DailyQuestService.I != null)
            DailyQuestService.I.OnDailyListChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (DailyQuestService.I != null)
            DailyQuestService.I.OnDailyListChanged -= Refresh;
    }

    public void Bind(string questId, QuestDefinition def)
    {
        this.questId = questId;
        if (def != null)
        {
            titleText.text = def.title;
            descText.text = def.description;
        }
        Refresh();
    }

    public void OnClickClaim()
    {
        DailyQuestService.I?.Claim(questId);
    }

    void Refresh()
    {
        var e = DailyQuestService.I?.GetEntryById(questId);
        if (e == null) return;

        progressText.text = $"{e.current}/{e.target}";
        if (progressBar != null)
        {
            progressBar.minValue = 0;
            progressBar.maxValue = e.target;
            progressBar.value = e.current;
        }

        bool canClaim = DailyQuestService.I.CanClaim(questId);
        claimButton.interactable = canClaim;
        claimButton.gameObject.SetActive(!e.isClaimed);
        // You can add a claimed badge if you like
    }
}
