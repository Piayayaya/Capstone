using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plays the global button click SFX when this UI Button is pressed.
/// Attach this to any Button in any scene.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (btn != null)
            btn.onClick.AddListener(OnButtonClicked);
    }

    private void OnDisable()
    {
        if (btn != null)
            btn.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (SfxManager.Instance != null)
            SfxManager.Instance.PlayButtonClick();
    }
}
