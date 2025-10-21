using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(PerLevelSelector))]
public class PerLevelFocusApplier : MonoBehaviour
{
    [Header("Refs")]
    public Gameplay gameplay;              // drag your Gameplay (raises onArrived)
    public ScrollRect scroll;              // drag your ScrollRect

    [Header("Smoothing")]
    public bool smoothScroll = true;       // <— tick this for smooth movement
    [Range(0.05f, 1.0f)]
    public float smoothDuration = 0.35f;   // how long the scroll tween takes
    public AnimationCurve ease =           // easing curve for the tween
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    PerLevelSelector selector;
    Coroutine tween;

    void Awake()
    {
        selector = GetComponent<PerLevelSelector>();

        // Optional: avoid fighting with Gameplay’s centering
        if (gameplay != null) gameplay.centerOnPlayer = false;
    }

    void OnEnable()
    {
        if (gameplay != null)
            gameplay.onArrived.AddListener(OnArrived);
    }

    void OnDisable()
    {
        if (gameplay != null)
            gameplay.onArrived.RemoveListener(OnArrived);
    }

    void Start()
    {
        // Apply once on start so the view is correct immediately
        int idx = (gameplay != null && gameplay.CurrentIndex >= 0)
                  ? gameplay.CurrentIndex
                  : Mathf.Max(0, (gameplay?.startLevel1Based ?? 1) - 1);
        OnArrived(idx);
    }

    void OnArrived(int arrivedIndex)
    {
        if (scroll == null || selector == null) return;

        var profile = selector.CurrentProfile;
        if (profile == null || profile.Count == 0) return;

        int clamped = Mathf.Clamp(arrivedIndex, 0, profile.Count - 1);
        float target = Mathf.Clamp01(profile.Get(clamped));

        if (!smoothScroll)
        {
            scroll.verticalNormalizedPosition = target;
            scroll.velocity = Vector2.zero;
            return;
        }

        if (tween != null) StopCoroutine(tween);
        tween = StartCoroutine(CoTweenTo(target, Mathf.Max(0.05f, smoothDuration)));
    }

    IEnumerator CoTweenTo(float target, float dur)
    {
        float start = scroll.verticalNormalizedPosition;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float k = ease.Evaluate(Mathf.Clamp01(t));
            scroll.verticalNormalizedPosition = Mathf.Lerp(start, target, k);
            yield return null;
        }

        scroll.verticalNormalizedPosition = target;
        scroll.velocity = Vector2.zero;
        tween = null;
    }
}
