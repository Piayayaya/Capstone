using System.Collections;
using UnityEngine;

public class AnswerCoach : MonoBehaviour
{
    [Header("Fade settings")]
    public CanvasGroup group;     // drag the AnswerCoach CanvasGroup here
    public float fade = 0.25f;
    public bool showOnStart = true;

    void Reset()
    {
        group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
    }

    void Awake()
    {
        if (group == null) group = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        // force visible on start if requested
        if (showOnStart)
        {
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }
        else
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
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

    IEnumerator FadeTo(float target, bool raycastable)
    {
        float start = group.alpha;
        float t = 0f;
        while (t < fade)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, target, t / fade);
            yield return null;
        }
        group.alpha = target;
        group.interactable = raycastable;
        group.blocksRaycasts = raycastable;
    }
}
