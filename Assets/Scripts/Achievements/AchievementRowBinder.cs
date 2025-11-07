using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementRowBinder : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public TMP_Text titleText;
    public TMP_Text descText;
    public TMP_Text progressText;
    public Slider progressBar;  // min=0, max=1
    public GameObject divider;
    public Button claimButton;
    public TMP_Text claimLabel;

    [Header("State")]
    public string id;

    public void Bind(AchievementDef def, AchievementProgressData prog)
    {
        id = def.id;
        if (icon) icon.sprite = def.icon;
        if (titleText) titleText.text = def.displayName;
        if (descText) descText.text = def.description;

        var t = Mathf.Max(1, def.target);
        var ratio = Mathf.Clamp01((float)prog.value / t);
        if (progressBar)
        {
            progressBar.minValue = 0;
            progressBar.maxValue = 1;
            progressBar.value = ratio;
        }

        if (progressText)
        {
            if (prog.completed && !def.showAsCounter)
                progressText.text = "Completed";
            else
                progressText.text = $"{prog.value}/{def.target}";
        }

        // Optional: tint when completed
        if (prog.completed && icon) icon.color = Color.white;
        RefreshClaimUI(def, prog);
    }

    void RefreshClaimUI(AchievementDef def, AchievementProgressData prog)
    {
        if (!claimButton) return;

        bool show = def.coinReward > 0 && prog.completed && !prog.rewardGranted && !def.autoGrantReward;
        claimButton.gameObject.SetActive(show);

        if (show)
        {
            if (claimLabel) claimLabel.text = $"CLAIM +{def.coinReward}";
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(() =>
            {
                claimButton.interactable = false; // debounce
                bool ok = AchievementManager.I && AchievementManager.I.Claim(def.id);
                // After Claim, manager triggers OnProgressChanged → Bind runs again and hides the button
            });
        }
    }

    void OnEnable()
    {
        if (AchievementManager.I)
            AchievementManager.I.OnProgressChanged += HandleUpdate;
    }

    void OnDisable()
    {
        if (AchievementManager.I)
            AchievementManager.I.OnProgressChanged -= HandleUpdate;
    }

    void HandleUpdate(string changedId, AchievementProgressData data)
    {
        if (changedId != id) return;
        var def = AchievementManager.I ? AchievementManager.I.GetDef(id) : null;
        if (def != null) Bind(def, data);
    }

    public void SetIsLast(bool isLast)
    {
        if (divider) divider.SetActive(!isLast); // hide divider on last row
    }
}

// Small helper extension so binder can fetch def safely
public static class AchievementManagerExt
{
    public static AchievementDef GetDef(this AchievementManager m, string id)
    {
        foreach (var d in m.catalog.items)
            if (d && d.id == id) return d;
        return null;
    }

}
