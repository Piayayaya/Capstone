using System.Collections;
using TMPro;
using UnityEngine;

public class Toast : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] CanvasGroup group;
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] RectTransform background;   // optional: a panel behind the text

    [Header("Layout")]
    [SerializeField] float maxWidth = 930f;
    [SerializeField] Vector2 padding = new Vector2(40f, 24f);

    [Header("Timings")]
    [SerializeField] float fadeIn = 0.15f;
    [SerializeField] float showForSeconds = 1.5f;
    [SerializeField] float fadeOut = 0.20f;

    public float TotalDuration => fadeIn + showForSeconds + fadeOut;

    Coroutine running;

    void Reset()
    {
        group = GetComponentInChildren<CanvasGroup>(true);
        if (!group) group = gameObject.AddComponent<CanvasGroup>();
        label = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    void Awake()
    {
        if (group)
        {
            group.alpha = 0f;
            group.gameObject.SetActive(true);
        }
    }

    public void Show(string msg)
    {
        if (!group || !label) return;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(CoShow(msg));
    }

    IEnumerator CoShow(string msg)
    {
        label.text = msg;
        label.textWrappingMode = TextWrappingModes.Normal; // replaces obsolete enableWordWrapping
        label.enableAutoSizing = true;
        label.ForceMeshUpdate();

        // size background
        Vector2 pref = label.GetPreferredValues(msg, maxWidth, 0f);
        float w = Mathf.Min(pref.x, maxWidth) + padding.x;
        float h = pref.y + padding.y;
        if (background)
        {
            background.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
            background.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        }

        // fade in
        group.blocksRaycasts = false;
        group.interactable = false;

        float t = 0f;
        while (t < fadeIn) { t += Time.unscaledDeltaTime; group.alpha = Mathf.Lerp(0f, 1f, t / fadeIn); yield return null; }
        group.alpha = 1f;

        yield return new WaitForSecondsRealtime(showForSeconds);

        // fade out
        t = 0f;
        while (t < fadeOut) { t += Time.unscaledDeltaTime; group.alpha = Mathf.Lerp(1f, 0f, t / fadeOut); yield return null; }
        group.alpha = 0f;

        running = null;
    }
}
