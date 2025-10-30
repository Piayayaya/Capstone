using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class SeeItIntroOverlay : MonoBehaviour
{
    [Header("Overlay UI")]
    [SerializeField] CanvasGroup overlay;
    [SerializeField] TMP_Text messageText;
    [SerializeField] GameObject tapHint;

    [Header("Text (optional)")]
    [TextArea(2, 4)][SerializeField] string introText = "";

    [Header("Behaviour")]
    [SerializeField] bool pauseTime = true;
    [SerializeField] float fadeInDuration = 0.25f;
    [SerializeField] float fadeOutDuration = 0.25f;
    [SerializeField] bool requireTapToContinue = true;
    [SerializeField] float autoContinueDelay = 2f;

    [Header("Events")]
    public UnityEvent OnIntroShown;    // <- you will hook listeners here
    public UnityEvent OnIntroHidden;   // <- and here

    void Start()
    {
        if (overlay) overlay.gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(introText) && messageText) messageText.text = introText;
        if (pauseTime) Time.timeScale = 0f;

        // show overlay instantly (keep it simple for now)
        if (overlay) overlay.alpha = 1f;

        OnIntroShown?.Invoke();

        if (requireTapToContinue)
            tapHint?.SetActive(true);
        else
            Invoke(nameof(Hide), autoContinueDelay);
    }

    void Update()
    {
        if (!requireTapToContinue) return;

        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            Hide();
    }

    void Hide()
    {
        tapHint?.SetActive(false);
        if (pauseTime) Time.timeScale = 1f;
        if (overlay) overlay.alpha = 0f;
        if (overlay) overlay.gameObject.SetActive(false);

        OnIntroHidden?.Invoke();
        enabled = false;
    }
}
