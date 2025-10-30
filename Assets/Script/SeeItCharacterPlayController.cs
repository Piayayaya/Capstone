using UnityEngine;

/// Controls the character’s idle/play motion using CharacterBouncer.
/// - If StartOnFirstTap = true, waits for the first tap/click before starting.
/// - StopPlay() cleanly freezes all motion (used by the timer on timeout).
[RequireComponent(typeof(RectTransform))]
public class SeeItCharacterPlayController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CharacterBouncer bouncer;   // drag the CharacterBouncer from the same object

    [Header("Behaviour")]
    [SerializeField] bool startOnFirstTap = true;
    [SerializeField] bool useUnscaledTime = true;

    bool started;
    bool waitingForTap;

    void Awake()
    {
        if (bouncer == null) bouncer = GetComponent<CharacterBouncer>();
    }

    void OnEnable()
    {
        // Default idle state
        if (bouncer != null) bouncer.Stop();
        started = false;
        waitingForTap = startOnFirstTap;
    }

    void Update()
    {
        if (!waitingForTap) return;

        // Any tap/click starts play
        bool tapped =
#if UNITY_EDITOR || UNITY_STANDALONE
            Input.GetMouseButtonDown(0);
#else
            Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
#endif

        if (tapped)
        {
            StartPlay();
        }
    }

    /// Call from your intro overlay “On Intro Hidden” if you want the game
    /// to be ready for the first tap (or start immediately if startOnFirstTap = false).
    public void OnIntroHidden()
    {
        waitingForTap = startOnFirstTap;
        if (!startOnFirstTap)
            StartPlay();
    }

    public void StartPlay()
    {
        waitingForTap = false;
        started = true;
        if (bouncer != null) bouncer.Play();
    }

    /// Called by timer on TIMEOUT or by your round controller when 3/3 is reached.
    public void StopPlay()
    {
        started = false;
        waitingForTap = false;
        if (bouncer != null) bouncer.Stop();

        // Optionally freeze this controller so it can’t be re-started accidentally.
        // enabled = false;
    }
}
