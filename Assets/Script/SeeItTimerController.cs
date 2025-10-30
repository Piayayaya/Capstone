using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Simple countdown timer for See It Or Lose It.
/// - Shows mm:ss while running
/// - On timeout: freezes, swaps to "TIME'S UP!" text, stops character (optional),
///   and shows the Play Again panel (optional).
/// - Exposes Pause/Resume so other flows (e.g., win flow) can freeze the time display.
/// </summary>
public class SeeItTimerController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text timerText;            // e.g., "1:30"
    [SerializeField] private TMP_Text timesUpText;          // optional; will be set to "TIME'S UP!"
    [SerializeField] private CanvasGroup playAgainPanel;    // optional; shown on timeout

    [Header("Timing")]
    [SerializeField] private int startSeconds = 90;         // 1:30 default
    [SerializeField] private bool autoStart = false;        // start on Intro Hidden via event
    [SerializeField] private bool useUnscaledTime = true;   // recommended for UI/game pause safety

    [Header("Optional motion control")]
    [SerializeField] private CharacterBouncer characterBouncer; // optional; stopped on timeout

    [Header("Events")]
    public UnityEvent OnTimeout;                            // raised once when time reaches 0

    // --- runtime state ---
    private float _remain;          // seconds left
    private bool _running;          // whether countdown is ticking
    private bool _firedTimeout;     // prevents double-firing timeout

    private void Awake()
    {
        // Ensure initial panel/text states are clean
        if (timesUpText != null)
        {
            timesUpText.gameObject.SetActive(false);
        }

        if (playAgainPanel != null)
        {
            playAgainPanel.alpha = 0f;
            playAgainPanel.interactable = false;
            playAgainPanel.blocksRaycasts = false;
        }
    }

    private void Start()
    {
        ResetTimer(startSeconds);

        if (autoStart)
            StartTimer();
        else
            RefreshTimerLabel(); // show initial mm:ss
    }

    private void Update()
    {
        if (!_running) return;
        if (_remain <= 0f) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _remain -= dt;

        if (_remain <= 0f)
        {
            _remain = 0f;
            RefreshTimerLabel();   // show 0:00
            Timeout();
            return;
        }

        RefreshTimerLabel();
    }

    // -------- Public API --------

    /// <summary>Set the starting duration (in seconds) without starting it.</summary>
    public void SetDuration(int seconds)
    {
        startSeconds = Mathf.Max(0, seconds);
        ResetTimer(startSeconds);
        RefreshTimerLabel();
    }

    /// <summary>Reset internal clock to the provided seconds (or startSeconds if omitted).</summary>
    public void ResetTimer(int? seconds = null)
    {
        _remain = Mathf.Max(0, seconds ?? startSeconds);
        _running = false;
        _firedTimeout = false;

        // UI state: show timer, hide TIME'S UP
        if (timerText != null) timerText.gameObject.SetActive(true);
        if (timesUpText != null) timesUpText.gameObject.SetActive(false);

        // Keep Play Again hidden
        HidePlayAgainPanel();

        RefreshTimerLabel();
    }

    /// <summary>Begin counting down.</summary>
    public void StartTimer()
    {
        _running = true;
        _firedTimeout = false;

        if (timerText != null) timerText.gameObject.SetActive(true);
        if (timesUpText != null) timesUpText.gameObject.SetActive(false);
    }

    /// <summary>Pause the countdown but keep the current displayed time.</summary>
    public void Pause()
    {
        _running = false; // We intentionally do not change the label.
    }

    /// <summary>Continue the countdown from the current remaining time.</summary>
    public void Resume()
    {
        if (_remain > 0f)
            _running = true;
    }

    /// <summary>Force a timeout right now (useful if you need to end the round early).</summary>
    public void ForceTimeout()
    {
        if (_firedTimeout) return;
        _remain = 0f;
        RefreshTimerLabel();
        Timeout();
    }

    // -------- Internals --------

    private void Timeout()
    {
        if (_firedTimeout) return;
        _running = false;
        _firedTimeout = true;

        // Stop character motion if provided
        if (characterBouncer != null)
        {
            // Your CharacterBouncer already has Stop() in your project.
            characterBouncer.Stop();
        }

        // Swap to TIME'S UP! UI
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (timesUpText != null)
        {
            timesUpText.text = "TIME'S UP!";
            timesUpText.gameObject.SetActive(true);
        }

        // Show Play Again dialog if provided
        ShowPlayAgainPanel();

        // Raise event for any additional listeners
        OnTimeout?.Invoke();
    }

    private void RefreshTimerLabel()
    {
        if (timerText == null) return;

        int total = Mathf.CeilToInt(_remain);
        int m = total / 60;
        int s = total % 60;
        timerText.text = $"{m}:{s:00}";
    }

    private void ShowPlayAgainPanel()
    {
        if (playAgainPanel == null) return;

        playAgainPanel.alpha = 1f;
        playAgainPanel.interactable = true;
        playAgainPanel.blocksRaycasts = true;
    }

    private void HidePlayAgainPanel()
    {
        if (playAgainPanel == null) return;

        playAgainPanel.alpha = 0f;
        playAgainPanel.interactable = false;
        playAgainPanel.blocksRaycasts = false;
    }
}
