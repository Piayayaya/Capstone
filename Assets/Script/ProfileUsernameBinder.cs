// ProfileUsernameBinder.cs
using TMPro;
using UnityEngine;

public class ProfileUsernameBinder : MonoBehaviour
{
    public TMP_Text target;
    public string format = "{0}";
    public string defaultName = "PLAYER";

    void Awake()
    {
        if (!target) target = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        if (!target) return;

        string nameToUse = defaultName;

        if (ProfileService.Instance != null && ProfileService.Instance.HasName())
            nameToUse = ProfileService.Instance.DisplayName;

        target.text = string.Format(format, nameToUse);
    }
}
