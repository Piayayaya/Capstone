using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class RewardToast : MonoBehaviour
{
    [Header("Wiring")] public TMP_Text amountText;
    public CanvasGroup canvasGroup;

    [Header("Timing")] public float showSeconds = 2f;
    public float fadeIn = 0.18f;
    public float fadeOut = 0.18f;

    Coroutine routine;
    Action onDone; // <-- NEW

    void Awake()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (!amountText) amountText = GetComponentInChildren<TMP_Text>(true);
        if (canvasGroup) canvasGroup.alpha = 0f;
    }

    // Accept a callback to run after the toast hides
    public void Show(int amount, Action onDone = null)
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        if (!enabled) enabled = true;
        transform.SetAsLastSibling();

        if (routine != null) StopCoroutine(routine);
        if (amountText) amountText.text = $"+{amount} Coins";
        if (canvasGroup) canvasGroup.alpha = 0f;

        this.onDone = onDone;                 // <-- NEW
        routine = StartCoroutine(CoShow());
    }

    IEnumerator CoShow()
    {
        if (canvasGroup && fadeIn > 0f) yield return FadeTo(1f, fadeIn);
        else if (canvasGroup) canvasGroup.alpha = 1f;

        yield return new WaitForSecondsRealtime(showSeconds);

        if (canvasGroup && fadeOut > 0f) yield return FadeTo(0f, fadeOut);

        gameObject.SetActive(false);
        routine = null;

        // Fire and clear callback
        var cb = onDone; onDone = null;
        cb?.Invoke();                          // <-- NEW
    }

    IEnumerator FadeTo(float target, float dur)
    {
        float start = canvasGroup ? canvasGroup.alpha : 1f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            if (canvasGroup) canvasGroup.alpha = Mathf.Lerp(start, target, t / dur);
            yield return null;
        }
        if (canvasGroup) canvasGroup.alpha = target;
    }
}
