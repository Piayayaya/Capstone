using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupFader : MonoBehaviour
{
    public float fade = 0.2f;
    CanvasGroup cg;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        // Start hidden
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    public void Show()
    {
        StopAllCoroutines();
        StartCoroutine(FadeTo(1f, true));
    }

    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(FadeTo(0f, false));
    }

    IEnumerator FadeTo(float target, bool enableInput)
    {
        float start = cg.alpha;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, fade);
            cg.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }
        cg.alpha = target;
        cg.interactable = enableInput;
        cg.blocksRaycasts = enableInput;
    }
}
