using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DailyQuestPanelController : MonoBehaviour
{
    [Header("Panel root with CanvasGroup")]
    public CanvasGroup panelRoot;      // add CanvasGroup on the panel root
    public Button blockerButton;       // optional: tap outside to close
    public float fadeTime = 0.15f;

    void Awake()
    {
        if (blockerButton) blockerButton.onClick.AddListener(Close);
        HideImmediate();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(Fade(0f, 1f, true));
    }

    public void Close()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(panelRoot.alpha, 0f, false));
    }

    IEnumerator Fade(float from, float to, bool enableAtEnd)
    {
        panelRoot.blocksRaycasts = true;
        panelRoot.interactable = false;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            panelRoot.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        panelRoot.alpha = to;
        panelRoot.interactable = to > 0.99f;
        panelRoot.blocksRaycasts = to > 0.01f;

        if (!panelRoot.blocksRaycasts) gameObject.SetActive(false);
    }

    void HideImmediate()
    {
        panelRoot.alpha = 0f;
        panelRoot.blocksRaycasts = false;
        panelRoot.interactable = false;
        gameObject.SetActive(false);
    }
}
