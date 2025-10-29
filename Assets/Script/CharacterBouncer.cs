using UnityEngine;

/// Simple UI bounce (and optional wiggle) for a RectTransform.
/// Call Play() to start animation, Stop() to freeze & reset.
[RequireComponent(typeof(RectTransform))]
public class CharacterBouncer : MonoBehaviour
{
    [Header("Bounce")]
    public float amplitude = 12f;       // vertical pixels
    public float frequency = 1.2f;      // cycles/sec
    public float horizontalSway = 0f;   // horizontal pixels (optional)

    [Header("Wiggle (optional)")]
    public float wiggleDegrees = 3f;
    public float wiggleFrequency = 1.6f;

    [Header("Misc")]
    public bool useUnscaledTime = true;

    RectTransform rt;
    Vector2 baseAnchoredPos;
    Quaternion baseRotation;
    bool playing;

    void CacheRect()
    {
        if (rt == null)
        {
            rt = GetComponent<RectTransform>();   // will exist on any UI object
            if (rt != null)
            {
                baseAnchoredPos = rt.anchoredPosition;
                baseRotation = rt.localRotation;
            }
        }
    }

    void Awake()
    {
        CacheRect();
    }

    void OnEnable()
    {
        CacheRect();
        if (rt != null)
        {
            baseAnchoredPos = rt.anchoredPosition;
            baseRotation = rt.localRotation;
        }
    }

    void Update()
    {
        if (!playing || rt == null) return;

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
        CacheRect();
        if (rt == null) return;

        baseAnchoredPos = rt.anchoredPosition;
        baseRotation = rt.localRotation;
        playing = true;
    }

    public void Stop()
    {
        // Be safe even if someone removed components at runtime
        CacheRect();
        playing = false;
        if (rt != null)
        {
            rt.anchoredPosition = baseAnchoredPos;
            rt.localRotation = baseRotation;
        }
    }
}
