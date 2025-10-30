using UnityEngine;

/// Bouncer used only in "See It or Lose It".
/// Safe to coexist with the global CharacterBouncer used by other modes.
[RequireComponent(typeof(RectTransform))]
public class SeeItCharacterBouncer : MonoBehaviour
{
    [Header("Bounce")]
    public float amplitude = 12f;        // pixels up/down
    public float frequency = 1.2f;       // cycles per second
    public float horizontalSway = 0f;    // optional pixels left/right

    [Header("Wiggle (optional)")]
    public float wiggleDegrees = 3f;     // Z-rotation in degrees
    public float wiggleFrequency = 1.6f; // cycles per second

    [Header("Misc")]
    public bool useUnscaledTime = true;

    [Tooltip("For testing: start bouncing automatically when enabled.")]
    [SerializeField] bool playOnEnable = false;

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
        baseAnchoredPos = rt.anchoredPosition;
        baseRotation = rt.localRotation;
        if (playOnEnable) Play();
    }

    void Update()
    {
        if (!playing) return;

        float t = useUnscaledTime ? Time.unscaledTime : Time.time;

        float y = Mathf.Sin(t * Mathf.PI * 2f * frequency) * amplitude;
        float x = (horizontalSway == 0f) ? 0f
                 : Mathf.Sin(t * Mathf.PI * 2f * frequency * 0.75f) * horizontalSway;

        rt.anchoredPosition = baseAnchoredPos + new Vector2(x, y);

        if (wiggleDegrees > 0f)
        {
            float wiggle = Mathf.Sin(t * Mathf.PI * 2f * wiggleFrequency) * wiggleDegrees;
            rt.localRotation = Quaternion.Euler(0f, 0f, wiggle);
        }
    }

    public void Play()
    {
        baseAnchoredPos = rt.anchoredPosition;
        baseRotation = rt.localRotation;
        playing = true;
    }

    public void Stop()
    {
        playing = false;
        rt.anchoredPosition = baseAnchoredPos;
        rt.localRotation = baseRotation;
    }

    public void SetPlaying(bool value)
    {
        if (value) Play(); else Stop();
    }
}
