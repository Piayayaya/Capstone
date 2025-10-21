using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CoinPopup_DragDrop : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform root;            // assign CoinPopup (RectTransform)
    public TextMeshProUGUI amountText;    // "+10 COINS"

    [Header("Anim")]
    public float rise = 120f;
    public float appearDuration = 0.7f;   // time to move/fade in
    public float holdTime = 3f;           // <<< keep visible this long
    public float fadeOutDuration = 0.35f; // time to fade out
    public float startAlpha = 0f;
    public float endAlpha = 1f;

    CanvasGroup cg;

    void Awake()
    {
        if (!root) root = (RectTransform)transform;
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void Show(int amount)
    {
        if (amountText) amountText.text = $"+{amount} COINS";
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        Vector2 start = root.anchoredPosition;
        Vector2 end = start + Vector2.up * rise;

        // Appear (move up + fade in)
        cg.alpha = startAlpha;
        float t = 0f;
        while (t < appearDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / appearDuration);
            root.anchoredPosition = Vector2.LerpUnclamped(start, end, k);
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, k);
            yield return null;
        }

        // Hold on screen
        cg.alpha = endAlpha;
        root.anchoredPosition = end;
        yield return new WaitForSecondsRealtime(holdTime);

        // Fade out
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeOutDuration);
            cg.alpha = Mathf.Lerp(endAlpha, 0f, k);
            yield return null;
        }

        // Reset + hide
        cg.alpha = 0f;
        root.anchoredPosition = start;
        gameObject.SetActive(false);
    }
}
