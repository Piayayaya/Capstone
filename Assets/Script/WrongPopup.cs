using UnityEngine;
using System.Collections;

public class WrongPopup : MonoBehaviour
{
    [Header("Refs")]
    public CanvasGroup group;

    [Header("Timing")]
    public float fadeTime = 0.25f;
    public float autoHideAfter = 1.6f;

    Coroutine routine;

    void Awake()
    {
        if (!group) group = GetComponent<CanvasGroup>();
        HideImmediate();
    }

    public void Show()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine());
    }

    void HideImmediate()
    {
        if (!group) return;
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    IEnumerator ShowRoutine()
    {
        if (!group) yield break;

        // fade in
        float t = 0f;
        group.blocksRaycasts = false;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
            yield return null;
        }
        group.alpha = 1f;

        // wait
        yield return new WaitForSecondsRealtime(autoHideAfter);

        // fade out
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            yield return null;
        }
        group.alpha = 0f;
    }
}
