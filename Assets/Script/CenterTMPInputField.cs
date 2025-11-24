using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class CenterTMPInputField : MonoBehaviour
{
    private TMP_InputField input;

    private void Awake()
    {
        input = GetComponent<TMP_InputField>();

        // Center the visible text + placeholder
        if (input.textComponent != null)
            input.textComponent.alignment = TextAlignmentOptions.Center;

        if (input.placeholder is TMP_Text ph)
            ph.alignment = TextAlignmentOptions.Center;
    }

    private void OnEnable()
    {
        // Keep centered even after scene reloads
        if (input != null && input.textComponent != null)
            input.textComponent.alignment = TextAlignmentOptions.Center;
    }
}
