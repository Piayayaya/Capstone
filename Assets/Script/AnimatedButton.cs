using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// Add this to any UI button object (same GameObject as the Button).
/// It plays a quick "pop" animation, then calls onClickAfterAnimation.
[RequireComponent(typeof(RectTransform))]
public class AnimatedButton : MonoBehaviour, IPointerClickHandler
{
    [Header("Pop Animation Settings")]
    [Tooltip("How big the button grows when clicked (1.1 = 10% bigger).")]
    public float scaleUpMultiplier = 1.15f;

    [Tooltip("Total time of the animation (up + down).")]
    public float totalDuration = 0.18f;

    [Tooltip("Disable extra clicks while animating.")]
    public bool blockWhileAnimating = true;

    [Header("Event called AFTER animation finishes")]
    public UnityEvent onClickAfterAnimation;

    private RectTransform rect;
    private Vector3 originalScale;
    private bool isAnimating;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (blockWhileAnimating && isAnimating)
            return;

        StartCoroutine(ClickRoutine());
    }

    private IEnumerator ClickRoutine()
    {
        isAnimating = true;

        float half = totalDuration * 0.5f;
        float t = 0f;

        // --------- scale UP ----------
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(t / half);
            float scale = Mathf.Lerp(1f, scaleUpMultiplier, normalized);
            rect.localScale = originalScale * scale;
            yield return null;
        }

        // --------- scale BACK ----------
        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(t / half);
            float scale = Mathf.Lerp(scaleUpMultiplier, 1f, normalized);
            rect.localScale = originalScale * scale;
            yield return null;
        }

        rect.localScale = originalScale;
        isAnimating = false;

        // now do the real action (load scene, call your script, etc.)
        onClickAfterAnimation?.Invoke();
    }
}
