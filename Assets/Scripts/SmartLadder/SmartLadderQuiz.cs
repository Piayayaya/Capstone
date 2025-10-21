using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class SmartLadderQuiz : MonoBehaviour
{
    [Header("Difficulty")]
    public LadderDifficulty difficulty = LadderDifficulty.Easy;

    [Header("Goal / Completion")]
    [Tooltip("How many correct answers are needed to finish this run (e.g. number of platforms).")]
    [SerializeField] int _targetCorrect = 10;
    [SerializeField] UnityEvent onRunCompleted;    // optional hook when target reached

    [Header("Panels & UI")]
    public GameObject questionPanel;               // active while picking an answer
    public TMP_Text questionText;
    public Button[] optionButtons;                 // size = 3
    public GameObject explanationPanel;            // active after picking, shows explanation
    public TMP_Text explanationText;

    [Header("Reward Popup")]
    public GameObject rewardPanel;                 // small "+10 COINS" bubble
    public TMP_Text rewardText;                    // text inside the bubble
    public float rewardPopupDuration = 1.5f;       // seconds the bubble is visible

    [Header("Coins (optional UI)")]
    public TMP_Text coinsText;                     // optional UI label to show total coins

    [Header("Events")]
    public UnityEvent<bool> onAnswered;            // true if correct
    public UnityEvent<int> onCoinsChanged;         // passes new coin total (optional)

    [Header("Mover Hook")]
    public Gameplay mover;                         // drag the object that has Gameplay

    [Header("Completion")]
    public GameObject completionPanel;           // “Mode Complete!” panel (inactive by default)
    public bool moveToChestOnComplete = true;
    public int chestLevel1Based = 11;            // e.g., 11 for Easy (10 levels + chest)

    [Header("Resume UI")]
    public GameObject resumeButton;

    bool _awaitingChestArrival = false;
    bool _savedAutoShow;
    bool _savedCallQuiz;

    [Header("Difficulty Source")]
    public bool useSessionDifficulty = true;       // if true, override inspector value from session

    // Always fetch using the chosen difficulty
    LadderDifficulty EffectiveDifficulty
        => useSessionDifficulty ? SmartLadderSession.SelectedDifficulty : difficulty;

    // Data/provider
    IQuestionProvider _provider;

    // Tracking (per run)
    // _asked            = shown at least once (right OR wrong)
    // _answeredCorrect  = answered correctly (never ask again this run)
    HashSet<int> _asked = new HashSet<int>();
    HashSet<int> _answeredCorrect = new HashSet<int>();

    Question _current;

    // State
    bool _inited;
    bool _lastAnswerCorrect = false;
    int _coins = 0;
    int _wrongStreakThisLevel = 0;                 // wrong attempts on the *current level*
    int _correctSoFar = 0;                         // total correct answers this run
    Coroutine _rewardCo;

    // Exposed flags
    public bool HasCurrentQuestion => _current != null;
    public bool ReadyForManualContinue { get; private set; } = false;

    // -------- Lifecycle --------
    void Awake()
    {
        if (questionPanel) questionPanel.SetActive(false);
        if (explanationPanel) explanationPanel.SetActive(false);
        if (rewardPanel) rewardPanel.SetActive(false);

        // --- Guard against bad wiring ---
        if (explanationText == null) Debug.LogError("[Quiz] explanationText is not assigned.", this);
        if (coinsText == null) Debug.LogWarning("[Quiz] coinsText is not assigned (OK if you don't display coins).", this);
        if (rewardText == null) Debug.LogWarning("[Quiz] rewardText is not assigned (OK if you don't show popup).", this);

        if (coinsText != null && explanationText != null && ReferenceEquals(coinsText, explanationText))
        {
            Debug.LogError("[Quiz] coinsText and explanationText reference the SAME TMP object. Fix the Inspector.", this);
            coinsText = null; // disable coins UI until you fix the binding
        }

        UpdateCoinsUI();
    }

    void OnEnable()
    {
        if (useSessionDifficulty)
        {
            difficulty = SmartLadderSession.SelectedDifficulty;
            ResetRun();
        }

        if (mover != null)
            mover.onArrived.AddListener(OnMoverArrived);
    }

    void OnDisable()
    {
        if (mover != null)
            mover.onArrived.RemoveListener(OnMoverArrived);
    }

    void EnsureInit()
    {
        if (_inited) return;
        _provider = new InMemoryQuestionProvider();    // your in-memory pool
        _provider.Initialize();
        _inited = true;
    }

    // -------- Public API (called by scene setup) --------
    public void SetDifficulty(LadderDifficulty d) => difficulty = d;
    public void SetTargetCorrect(int count) => _targetCorrect = Mathf.Max(1, count);
    public bool ReachedTarget() => _correctSoFar >= _targetCorrect;

    public void ResetRun()
    {
        _coins = 0;
        _wrongStreakThisLevel = 0;
        _correctSoFar = 0;
        _asked.Clear();
        _answeredCorrect.Clear();
        UpdateCoinsUI();

        if (questionPanel) questionPanel.SetActive(false);
        if (explanationPanel) explanationPanel.SetActive(false);
        if (rewardPanel) rewardPanel.SetActive(false);
        if (_rewardCo != null) { StopCoroutine(_rewardCo); _rewardCo = null; }
    }

    // -------- Gameplay flow --------
    // Called by Gameplay when the player arrives at a platform
    public void ShowNextQuestion()
    {
        EnsureInit();

        ReadyForManualContinue = false;

        // 1) Prefer NEVER-SEEN questions: exclude everything in _asked and everything correct
        var preferNewExclude = new HashSet<int>(_answeredCorrect);
        foreach (var id in _asked) preferNewExclude.Add(id);

        _current = _provider.GetNext(EffectiveDifficulty, preferNewExclude);

        // 2) If none left, fall back to re-ask previously-wrong ones:
        //    exclude ONLY the ones already answered correct
        if (_current == null)
        {
            _current = _provider.GetNext(EffectiveDifficulty, _answeredCorrect);
        }

        // 3) If still none, truly no questions available right now
        if (_current == null)
        {
            if (questionPanel) questionPanel.SetActive(true);
            if (explanationPanel) explanationPanel.SetActive(false);
            if (questionText) questionText.text = "No questions available.";
            return;
        }

        // Mark as asked as soon as we pick it
        _asked.Add(_current.Id);

        if (explanationPanel) explanationPanel.SetActive(false);
        if (rewardPanel) rewardPanel.SetActive(false);
        if (questionPanel) questionPanel.SetActive(true);

        if (questionText) questionText.text = _current.Text;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            var btn = optionButtons[i];
            if (!btn) continue;

            var label = btn.GetComponentInChildren<TMP_Text>(true);
            if (label && _current.Choices != null && i < _current.Choices.Length)
                label.text = _current.Choices[i];

            int idx = i;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnOption(idx));
            btn.interactable = true;
        }
    }

    void OnOption(int choiceIndex)
    {
        foreach (var b in optionButtons) if (b) b.interactable = false;
        ReadyForManualContinue = false;

        _lastAnswerCorrect = (_current != null && choiceIndex == _current.CorrectIndex);

        if (_lastAnswerCorrect && _current != null)
        {
            // Permanently exclude this question for the rest of the run
            _answeredCorrect.Add(_current.Id);
        }

        if (explanationText)
        {
            string prefix = _lastAnswerCorrect ? "Correct! " : "Not quite. ";
            explanationText.text = prefix + (_current?.Explanation ?? "");
            // (optional) ensure normal color
            explanationText.color = new Color(1f, 1f, 1f, 1f);
        }

        if (_lastAnswerCorrect)
        {
            // Show reward popup ON TOP of the question panel, then go to explanation
            int reward = RewardForCurrentStreak();
            _coins += reward;
            UpdateCoinsUI();

            if (_rewardCo != null) StopCoroutine(_rewardCo);
            _rewardCo = StartCoroutine(CoShowRewardThenExplanation(reward));
        }
        else
        {
            // Wrong: hide question immediately and show explanation
            if (questionPanel) questionPanel.SetActive(false);
            if (explanationPanel) explanationPanel.SetActive(true);
        }

        onAnswered?.Invoke(_lastAnswerCorrect);
    }

    IEnumerator CoShowRewardThenExplanation(int reward)
    {
        if (rewardPanel)
        {
            // make sure the popup is a child of the question panel so it overlaps it
            if (questionPanel && rewardPanel.transform.parent != questionPanel.transform)
                rewardPanel.transform.SetParent(questionPanel.transform, false);

            rewardPanel.transform.SetAsLastSibling(); // draw on top of question contents
            if (rewardText) rewardText.text = $"+{reward} COINS";
            rewardPanel.SetActive(true);
        }

        yield return new WaitForSeconds(rewardPopupDuration);

        if (rewardPanel) rewardPanel.SetActive(false);

        // now swap panels: hide question, show explanation
        if (questionPanel) questionPanel.SetActive(false);
        if (explanationPanel) explanationPanel.SetActive(true);

        _rewardCo = null;
    }

    public void CloseExplanation()
    {
        if (explanationPanel) explanationPanel.SetActive(false);
    }

    // Hook this to the Explanation panel's "Next" button
    public void NextQuestionButton()
    {
        CloseExplanation();

        if (_lastAnswerCorrect)
        {
            // Finalize this level result
            _wrongStreakThisLevel = 0;
            _correctSoFar++;

            if (ReachedTarget())
            {
                onRunCompleted?.Invoke();

                if (moveToChestOnComplete && mover != null)
                {
                    // Temporarily stop auto-questions during the “walk to chest”
                    _savedAutoShow = mover.autoShowOnArrive;
                    _savedCallQuiz = mover.callQuizOnArrive;
                    mover.autoShowOnArrive = false;
                    mover.callQuizOnArrive = false;

                    _awaitingChestArrival = true;
                    mover.MoveToLevel(chestLevel1Based);   // go to chest
                }
                else
                {
                    ShowCompletionPanel();
                }
                return;
            }

            if (mover != null) mover.MoveNext();
            else Debug.LogWarning("SmartLadderQuiz: mover not assigned.");
        }
        else
        {
            // stay on same level; increase streak and ask another (prefer new, fallback to wrong)
            _wrongStreakThisLevel++;
            ShowNextQuestion();
        }
    }

    // -------- Helpers --------
    int RewardForCurrentStreak()
    {
        // 0 wrong so far -> 10; 1 -> 7; 2 -> 5; 3+ -> 3
        if (_wrongStreakThisLevel <= 0) return 10;
        if (_wrongStreakThisLevel == 1) return 7;
        if (_wrongStreakThisLevel == 2) return 5;
        return 3;
    }

    void UpdateCoinsUI()
    {
        if (coinsText != null)
        {
            coinsText.text = $"{_coins}";
        }
        // If you wire listeners, you can emit this:
        // onCoinsChanged?.Invoke(_coins);
    }

    // Optional close/reopen
    public void CloseQuestionPanel() { ReadyForManualContinue = true; if (questionPanel) questionPanel.SetActive(false); }
    public void ReopenQuestionPanel() { if (questionPanel) questionPanel.SetActive(true); }

    void OnMoverArrived(int arrivedIndex)
    {
        if (!_awaitingChestArrival) return;

        int chestIdx = Mathf.Max(0, chestLevel1Based - 1);
        if (arrivedIndex == chestIdx)
        {
            _awaitingChestArrival = false;

            // restore defaults
            mover.autoShowOnArrive = _savedAutoShow;
            mover.callQuizOnArrive = _savedCallQuiz;

            ShowCompletionPanel();
        }
    }

    void ShowCompletionPanel()
    {
        // Hide gameplay panels
        if (questionPanel) questionPanel.SetActive(false);
        if (explanationPanel) explanationPanel.SetActive(false);
        if (completionPanel) completionPanel.SetActive(true);
    }

    // Reopen the currently loaded question (if any)
    public void ReopenCurrentQuestion()
    {
        if (_current == null) return;

        if (explanationPanel) explanationPanel.SetActive(false);

        if (optionButtons != null)
        {
            foreach (var b in optionButtons)
                if (b) b.interactable = true;
        }

        if (questionPanel) questionPanel.SetActive(true);
    }
}
