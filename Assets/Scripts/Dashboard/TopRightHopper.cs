using UnityEngine;

/// Attach to a UI Image (or any RectTransform) anchored top-right.
/// Makes a cute hop left-right with a parabolic arc.
[RequireComponent(typeof(RectTransform))]
public class TopRightHopper : MonoBehaviour
{
    [Header("Motion (UI pixels)")]
    [Tooltip("Total left↔right travel distance in pixels.")]
    public float horizontalDistance = 180f;

    [Tooltip("Seconds for a full left→right→left cycle.")]
    public float cycleDuration = 1.2f;

    [Tooltip("Peak jump height in pixels.")]
    public float jumpHeight = 40f;

    [Header("Pause")]
    [Tooltip("If assigned, hopping pauses whenever this object is active in the hierarchy.")]
    public GameObject pausePopup;  // e.g., your DailyLoginPopup.popupRoot

    RectTransform rect;
    Vector2 startAnchored;
    float t;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        startAnchored = rect.anchoredPosition;
        t = Random.Range(0f, cycleDuration); // pleasant desync
    }

    void OnEnable()
    {
        // Reset to starting spot when re-enabled
        rect.anchoredPosition = startAnchored;
    }

    void Update()
    {
        if (pausePopup && pausePopup.activeInHierarchy) return;
        if (cycleDuration <= 0.01f) return;

        t += Time.unscaledDeltaTime;                 // keeps moving if timeScale=0
        float u = Mathf.PingPong(t / cycleDuration, 1f); // 0→1→0
        float lr = Mathf.Lerp(-1f, 1f, u);           // -1..+1 sideways

        float x = lr * (horizontalDistance * 0.5f);
        float y = 4f * jumpHeight * u * (1f - u);    // simple parabola

        rect.anchoredPosition = startAnchored + new Vector2(x, y);
    }

    // Optional helpers you can call from buttons:
    public void StopHopping(bool resetToStart = true)
    {
        enabled = false;
        if (resetToStart) rect.anchoredPosition = startAnchored;
    }

    public void StartHopping()
    {
        startAnchored = rect.anchoredPosition;
        enabled = true;
    }
}
