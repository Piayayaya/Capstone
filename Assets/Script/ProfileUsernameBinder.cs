using TMPro;
using UnityEngine;

public class ProfileUsernameBinder : MonoBehaviour
{
    [Header("Text to update")]
    public TMP_Text target;

    [Header("Where the name is stored")]
    public string prefsKey = "profile_name";

    [Header("Formatting")]
    [Tooltip("Use {0} where the name should appear, e.g. \"{0}\" or \"PLAYER {0}\"")]
    public string format = "{0}";

    [Tooltip("Name used if nothing has been saved yet")]
    public string defaultName = "PLAYER";

    void Awake()
    {
        // If you forget to wire it, grab the local TMP_Text
        if (!target)
            target = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        if (!target) return;

        string nameToUse = defaultName;

        // 1) Prefer ProfileService (same one used by Profile Scene)
        if (ProfileService.Instance != null &&
            !string.IsNullOrWhiteSpace(ProfileService.Instance.Current.displayName))
        {
            nameToUse = ProfileService.Instance.Current.displayName.Trim();
        }
        // 2) Fallback to PlayerPrefs (in case ProfileService isn't ready)
        else if (PlayerPrefs.HasKey(prefsKey))
        {
            var fromPrefs = PlayerPrefs.GetString(prefsKey, defaultName);
            if (!string.IsNullOrWhiteSpace(fromPrefs))
                nameToUse = fromPrefs.Trim();
        }

        // Safety
        if (string.IsNullOrWhiteSpace(nameToUse))
            nameToUse = defaultName;

        target.text = string.Format(format, nameToUse);
    }
}
