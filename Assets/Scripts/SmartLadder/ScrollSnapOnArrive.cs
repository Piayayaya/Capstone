using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// Snap the ScrollRect to a preset position (0..1) whenever the mover reports arrival.
/// Easiest, most reliable way to "focus" the player without doing layout math.
public class ScrollSnapOnArrive : MonoBehaviour
{
    [Header("Refs")]
    public Gameplay mover;          // your mover script (the one that has onArrived)
    public ScrollRect scroll;       // your Scroll View

    [Header("Per-level focus positions")]
    [Tooltip("One value per level (0..1). 1 = top, 0 = bottom. Fill in the Inspector.")]
    [Range(0f, 1f)] public float[] normalizedPerLevel;

    [Header("Behavior")]
    public bool smooth = true;
    [Range(0.05f, 1.0f)] public float duration = 0.25f;

    Coroutine co;

    void OnEnable()
    {
        if (mover != null) mover.onArrived.AddListener(OnArrived);
    }

    void OnDisable()
    {
        if (mover != null) mover.onArrived.RemoveListener(OnArrived);
        if (co != null) StopCoroutine(co);
    }

    void OnArrived(int idx)
    {
        if (!scroll || normalizedPerLevel == null || idx < 0 || idx >= normalizedPerLevel.Length)
            return;

        float target = Mathf.Clamp01(normalizedPerLevel[idx]);

        // kill inertia so it doesn't drift
        scroll.velocity = Vector2.zero;

        if (!smooth)
        {
            scroll.verticalNormalizedPosition = target;
        }
        else
        {
            if (co != null) StopCoroutine(co);
            co = StartCoroutine(CoLerpNormalized(scroll.verticalNormalizedPosition, target, duration));
        }
    }

    IEnumerator CoLerpNormalized(float from, float to, float dur)
    {
        float t = 0f;
        dur = Mathf.Max(0.01f, dur);
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            scroll.verticalNormalizedPosition = Mathf.Lerp(from, to, t);
            yield return null;
        }
        co = null;
    }

    // Optional helper if you want to capture current position at runtime via button:
    public void CaptureCurrentForIndex(int idx)
    {
        if (!scroll || normalizedPerLevel == null || idx < 0 || idx >= normalizedPerLevel.Length) return;
        normalizedPerLevel[idx] = Mathf.Clamp01(scroll.verticalNormalizedPosition);
        Debug.Log($"Captured level {idx + 1} focus = {normalizedPerLevel[idx]:0.000}");
    }
}
