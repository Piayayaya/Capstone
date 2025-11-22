// ProfileNameBinder.cs
using TMPro;
using UnityEngine;

public class ProfileNameBinder : MonoBehaviour
{
    public TMP_Text target;
    public string format = "WELCOME TO BRAINYME\u00A0{0}!";

    void Start()
    {
        if (!target) target = GetComponent<TMP_Text>();
        if (!target) return;

        string n = (ProfileService.Instance != null && ProfileService.Instance.HasName())
            ? ProfileService.Instance.DisplayName
            : "PLAYER";

        target.enableAutoSizing = true;
        target.fontSizeMax = 96;
        target.fontSizeMin = 36;
        target.enableWordWrapping = false;
        target.overflowMode = TextOverflowModes.Overflow;
        target.alignment = TextAlignmentOptions.Center;

        target.text = string.Format(format, n);
        target.ForceMeshUpdate();
    }
}
