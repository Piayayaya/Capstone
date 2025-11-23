using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class SeeItTimerController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text timesUpText;
    [SerializeField] private CanvasGroup playAgainPanel;

    [Header("Timing")]
    [SerializeField] private int startSeconds = 90;
    [SerializeField] private bool autoStart = false;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Optional motion control")]
    [SerializeField] private CharacterBouncer characterBouncer;

    [Header("Events")]
    public UnityEvent OnTimeout;

    private float _remain;
    private bool _running;
    private bool _firedTimeout;

    private void Awake()
    {
        if (timesUpText != null)
            timesUpText.gameObject.SetActive(false);

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

        if (autoStart) StartTimer();
        else RefreshTimerLabel();
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
            RefreshTimerLabel();
            Timeout();
            return;
        }

        RefreshTimerLabel();
    }

    // -------- Public API --------

    public void SetDuration(int seconds)
    {
        startSeconds = Mathf.Max(0, seconds);
        ResetTimer(startSeconds);
        RefreshTimerLabel();
    }

    public void ResetTimer(int? seconds = null)
    {
        _remain = Mathf.Max(0, seconds ?? startSeconds);
        _running = false;
        _firedTimeout = false;

        if (timerText != null) timerText.gameObject.SetActive(true);
        if (timesUpText != null) timesUpText.gameObject.SetActive(false);

        HidePlayAgainPanel();
        RefreshTimerLabel();
    }

    public void StartTimer()
    {
        _running = true;
        _firedTimeout = false;

        if (timerText != null) timerText.gameObject.SetActive(true);
        if (timesUpText != null) timesUpText.gameObject.SetActive(false);
    }

    public void Pause()
    {
        _running = false;
    }

    public void Resume()
    {
        if (_remain > 0f)
            _running = true;
    }

    // ✅ NEW: used by win flow
    public void StopTimer()
    {
        _running = false;
    }

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

        if (characterBouncer != null)
            characterBouncer.Stop();

        if (timerText != null) timerText.gameObject.SetActive(false);

        if (timesUpText != null)
        {
            timesUpText.text = "TIME'S UP!";
            timesUpText.gameObject.SetActive(true);
        }

        ShowPlayAgainPanel();
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
