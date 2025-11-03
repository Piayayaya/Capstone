using TMPro;
using UnityEngine;

public class ProfileNameBinder : MonoBehaviour
{
    public TMP_Text target;

    // One line (non-breaking space keeps phrase + name together)
    public string format = "WELCOME TO BRAINYME\u00A0{0}!";

    void Start()
    {
        if (!target) return;

        var n = ProfileService.Instance ? ProfileService.Instance.Current.displayName : "Player";

        // One-line, auto-fit
        target.enableAutoSizing = true;
        target.fontSizeMax = 96;  // match TMP inspector
        target.fontSizeMin = 36;
        target.enableWordWrapping = false;
        target.overflowMode = TextOverflowModes.Overflow;

        target.alignment = TextAlignmentOptions.Center;
        target.text = string.Format(format, n);
        target.ForceMeshUpdate();
    }
}
