using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementRowBinder : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text progressText;
    public Image iconImage;

    [Header("Claim UI")]
    public Button claimButton;
    public TMP_Text claimButtonLabel;      // <- drag the Button's TMP child here
    public GameObject completedCheck;      // optional tick/check image
    public GameObject bottomDivider;       // optional line between rows

    private LocalAchievementDef _local;
    private AchievementDef _def;
    private AchievementProgressData _progress;

    [Header("Progress Bar")]
    public Slider progressSlider;         // <-- add this

    public void Bind(LocalAchievementDef local, AchievementDef def, AchievementProgressData progress)
    {
        _local = local;
        _def = def;
        _progress = progress ?? new AchievementProgressData();

        // Title/description: prefer SQLite text, fall back to ScriptableObject text
        string title = !string.IsNullOrEmpty(local?.title) ? local.title : def?.displayName;
        string desc = !string.IsNullOrEmpty(local?.description) ? local.description : def?.description;

        if (titleText) titleText.text = title ?? "";
        if (descriptionText) descriptionText.text = desc ?? "";

        // Icon comes from the ScriptableObject runtime def
        if (iconImage)
        {
            if (def != null && def.icon != null)
            {
                iconImage.sprite = def.icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }

        RefreshUI();
    }

    void RefreshUI()
    {
        int target = _def != null ? _def.target : _local?.target ?? 0;
        bool showCounter = _def != null ? _def.showAsCounter : true;

        if (progressSlider)
        {
            if (target > 0)
            {
                progressSlider.gameObject.SetActive(true);
                progressSlider.wholeNumbers = true;
                progressSlider.minValue = 0;
                progressSlider.maxValue = target;
                progressSlider.value = Mathf.Clamp(_progress.value, 0, target);
                progressSlider.interactable = false;   // just a display bar
            }
            else
            {
                progressSlider.gameObject.SetActive(false);
            }
        }

        // Default: hide claim button
        if (claimButton)
        {
            claimButton.interactable = false;
            claimButton.gameObject.SetActive(false);
        }
        if (claimButtonLabel)
            claimButtonLabel.text = "";

        if (_progress.completed)
        {
            // “Completed” text
            if (progressText)
                progressText.text = "Completed";

            // Can we claim a reward?
            bool canClaim = false;
            var id = _local?.id ?? _def?.id;

            if (!string.IsNullOrEmpty(id) && AchievementManager.I != null)
                canClaim = AchievementManager.I.CanClaim(id);

            if (claimButton)
            {
                claimButton.gameObject.SetActive(canClaim);  // only show when claimable
                claimButton.interactable = canClaim;
            }
            if (claimButtonLabel && canClaim)
                claimButtonLabel.text = "CLAIM";
        }
        else
        {
            // Not completed yet – show progress, hide button
            if (showCounter && target > 0)
            {
                if (progressText) progressText.text = $"{_progress.value}/{target}";
            }
            else if (progressText)
            {
                progressText.text = "";
            }

            if (claimButton)
            {
                claimButton.gameObject.SetActive(false);
                claimButton.interactable = false;
            }
        }

        if (completedCheck)
            completedCheck.SetActive(_progress.completed);
    }

    // Hook this to the Claim button OnClick
    public void OnClickClaim()
    {
        var id = _local?.id ?? _def?.id;
        if (string.IsNullOrEmpty(id) || AchievementManager.I == null) return;

        if (AchievementManager.I.Claim(id))
        {
            // Refresh our cached progress and UI (button will disappear)
            _progress = AchievementManager.I.GetProgress(id);
            RefreshUI();
        }
    }

    // Called by the panel so we can hide the last divider
    public void SetIsLast(bool isLast)
    {
        if (bottomDivider)
            bottomDivider.SetActive(!isLast);
    }
}
