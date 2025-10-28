using UnityEngine;

/// <summary>
/// Simple UI bounce (and optional wiggle) for a RectTransform.
/// Call Play() to start animation, Stop() to freeze & reset.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CharacterBouncer : MonoBehaviour
{
    [Header("Bounce")]
    [Tooltip("Pixels to move up/down from the base position.")]
    public float amplitude = 12f;

    [Tooltip("Cycles per second.")]
    public float frequency = 1.2f;

    [Tooltip("Horizontal sway in pixels (optional).")]
    public float horizontalSway = 0f;

    [Header("Wiggle (optional)")]
    [Tooltip("Small rotation wiggle in degrees. 0 = none.")]
    public float wiggleDegrees = 3f;

    [Tooltip("Wiggle cycles per second.")]
    public float wiggleFrequency = 1.6f;

    [Header("Misc")]
    [Tooltip("Use unscaled time so it ignores timescale changes.")]
    public bool useUnscaledTime = true;

    RectTransform rt;
    Vector2 baseAnchoredPos;
    Quaternion baseRotation;
    bool playing;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        baseAnchoredPos = rt.anchoredPosition;
        baseRotation = rt.localRotation;
    }

    void OnEnable()
    {
        // Keep the base position if this object gets enabled/disabled
        baseAnchoredPos = rt.anchoredPosition;
        baseRotation = rt.localRotation;
    }

    void Update()
    {
        if (!playing) return;

        float t = useUnscaledTime ? Time.unscaledTime : Time.time;

        float y = Mathf.Sin(t * Mathf.PI * 2f * frequency) * amplitude;
        float x = horizontalSway == 0f ? 0f : Mathf.Sin(t * Mathf.PI * 2f * frequency * 0.75f) * horizontalSway;

        rt.anchoredPosition = baseAnchoredPos + new Vector2(x, y);

        if (wiggleDegrees > 0f)
        {
            float wiggle = Mathf.Sin(t * Mathf.PI * 2f * wiggleFrequency) * wiggleDegrees;
            rt.localRotation = Quaternion.Euler(0f, 0f, wiggle);
        }
    }

    public void Play()
    {
        // Re-sample base in case layout changed
        baseAnchoredPos = rt.anchoredPosition;
        baseRotation = rt.localRotation;
        playing = true;
    }

    public void Stop()
    {
        playing = false;
        // Reset to clean pose
        rt.anchoredPosition = baseAnchoredPos;
        rt.localRotation = baseRotation;
    }
}
