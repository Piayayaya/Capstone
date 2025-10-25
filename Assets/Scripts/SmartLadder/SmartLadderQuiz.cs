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

    [Header("Progress UI")]
    public Slider progressBar;                     // normalized 0..1
    public TMP_Text progressText;                  // "X/Y" (optional)
    public bool initProgressFromSavedLevel = true; // read saved level and pre-fill bar

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
    public GameObject completionPanel;             // “Mode Complete!” panel (inactive by default)
    public bool moveToChestOnComplete = true;
    public int chestLevel1Based = 11;              // e.g., 11 for Easy (10 levels + chest)

    [Header("Resume UI")]
    public GameObject resumeButton;

    // completion move temp flags
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
    HashSet<int> _asked = new HashSet<int>();
    Question _current;

    // State
    bool _inited;
    bool _lastAnswerCorrect = false;
    int _coins = 0;
    int _wrongStreakThisLevel = 0;                 // wrong attempts on the *current level*
    int _correctSoFar = 0;                         // total correct answers this run
    Coroutine _rewardCo;

    // -------- Lifecycle --------
    void Awake()
    {
        if (questionPanel) questionPanel.SetActive(false);
        if (explanationPanel) explanationPanel.SetActive(false);
        if (rewardPanel) rewardPanel.SetActive(false);

        // Guards
        if (explanationText == null) Debug.LogError("[Quiz] explanationText is not assigned.", this);
        if (coinsText == null) Debug.LogWarning("[Quiz] coinsText not assigned (OK if you don't display coins).", this);
        if (rewardText == null) Debug.LogWarning("[Quiz] rewardText not assigned (OK if you don't show popup).", this);

        if (coinsText != null && explanationText != null && ReferenceEquals(coinsText, explanationText))
        {
            Debug.LogError("[Quiz] coinsText and explanationText reference the SAME TMP object. Fix the Inspector.", this);
            coinsText = null; // disable coins UI until fixed
        }

        UpdateCoinsUI();
        UpdateProgressUI(); // ensure bar reflects initial state
    }

    void OnEnable()
    {
        if (useSessionDifficulty)
        {
            difficulty = SmartLadderSession.SelectedDifficulty;
            ResetRun();
        }

        _coins = ProgressStore.LoadCoins(EffectiveDifficulty, 0);
        UpdateCoinsUI();

        // Optionally initialize bar from saved level (resume)
        if (initProgressFromSavedLevel)
        {
            var diff = EffectiveDifficulty;
            int resumeLevel = ProgressStore.LoadLevel(diff, 1); // defaults to 1
            _correctSoFar = Mathf.Clamp(resumeLevel - 1, 0, _targetCorrect);
            UpdateProgressUI();
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

    // -------- Public API --------
    public void SetDifficulty(LadderDifficulty d)
    {
        difficulty = d;
        UpdateProgressUI();
    }

    public void SetTargetCorrect(int count)
    {
        _targetCorrect = Mathf.Max(1, count);
        UpdateProgressUI();
    }

    public bool ReachedTarget() => _correctSoFar >= _targetCorrect;

    public void ResetRun()
    {
        _coins = 0;
        _wrongStreakThisLevel = 0;
        _correctSoFar = 0;
        _asked.Clear();

        UpdateCoinsUI();
        UpdateProgressUI();

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
        _current = _provider.GetNext(EffectiveDifficulty, _asked);

        // If the provider ran out due to all wrong answers shown already,
        // allow it to recycle any (we exclude nothing). This prevents "No questions" stall.
        if (_current == null)
        {
            _current = _provider.GetNext(EffectiveDifficulty, new HashSet<int>()); // no excludes
        }

        if (_current == null)
        {
            if (questionPanel) questionPanel.SetActive(true);
            if (explanationPanel) explanationPanel.SetActive(false);
            if (questionText) questionText.text = "No questions available.";
            return;
        }

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

        if (explanationText)
        {
            string prefix = _lastAnswerCorrect ? "Correct! " : "Not quite. ";
            explanationText.text = prefix + (_current?.Explanation ?? "");
            explanationText.color = new Color(1f, 1f, 1f, 1f);
        }

        if (_lastAnswerCorrect)
        {
            int reward = RewardForCurrentStreak();
            _coins += reward;
            UpdateCoinsUI();

            ProgressStore.SaveCoins(EffectiveDifficulty, _coins);

            if (_rewardCo != null) StopCoroutine(_rewardCo);
            _rewardCo = StartCoroutine(CoShowRewardThenExplanation(reward));
        }
        else
        {
            if (questionPanel) questionPanel.SetActive(false);
            if (explanationPanel) explanationPanel.SetActive(true);
        }

        onAnswered?.Invoke(_lastAnswerCorrect);
    }

    IEnumerator CoShowRewardThenExplanation(int reward)
    {
        if (rewardPanel)
        {
            if (questionPanel && rewardPanel.transform.parent != questionPanel.transform)
                rewardPanel.transform.SetParent(questionPanel.transform, false);

            rewardPanel.transform.SetAsLastSibling();
            if (rewardText) rewardText.text = $"+{reward} COINS";
            rewardPanel.SetActive(true);
        }

        yield return new WaitForSeconds(rewardPopupDuration);

        if (rewardPanel) rewardPanel.SetActive(false);

        if (questionPanel) questionPanel.SetActive(false);
        if (explanationPanel) explanationPanel.SetActive(true);

        _rewardCo = null;
    }

    public void CloseExplanation()
    {
        if (explanationPanel) explanationPanel.SetActive(false);
    }

    // Hook this to the Explanation panel's "Next" (Continue) button
    public void NextQuestionButton()
    {
        CloseExplanation();

        if (_lastAnswerCorrect)
        {
            _wrongStreakThisLevel = 0;
            _correctSoFar++;
            UpdateProgressUI();

            // Optionally persist coins
            var diffForSave = EffectiveDifficulty;
            ProgressStore.SaveCoins(diffForSave, _coins);

            if (ReachedTarget())
            {
                onRunCompleted?.Invoke();

                if (moveToChestOnComplete && mover != null)
                {
                    _savedAutoShow = mover.autoShowOnArrive;
                    _savedCallQuiz = mover.callQuizOnArrive;
                    mover.autoShowOnArrive = false;
                    mover.callQuizOnArrive = false;

                    _awaitingChestArrival = true;
                    mover.MoveToLevel(chestLevel1Based);
                }
                else
                {
                    ShowCompletionPanel();
                }
                return;
            }

            // Save resume level = next platform
            if (mover != null)
            {
                int nextLevel1Based = (mover.CurrentIndex + 1) + 1; // current 1-based + 1
                ProgressStore.SaveLevel(EffectiveDifficulty, nextLevel1Based);
                mover.MoveNext();
            }
            else
            {
                Debug.LogWarning("SmartLadderQuiz: mover not assigned.");
            }
        }
        else
        {
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
            coinsText.text = $"{_coins}";
        onCoinsChanged?.Invoke(_coins);
    }

    void UpdateProgressUI()
    {
        // Clamp to avoid division issues
        int clampedCorrect = Mathf.Clamp(_correctSoFar, 0, Mathf.Max(1, _targetCorrect));

        if (progressBar)
            progressBar.value = (float)clampedCorrect / Mathf.Max(1, _targetCorrect);

        if (progressText)
            progressText.text = $"{clampedCorrect}/{_targetCorrect}";
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

            mover.autoShowOnArrive = _savedAutoShow;
            mover.callQuizOnArrive = _savedCallQuiz;

            ShowCompletionPanel();

            // Fill bar just in case
            _correctSoFar = _targetCorrect;
            UpdateProgressUI();
        }
    }

    void ShowCompletionPanel()
    {
        if (questionPanel) questionPanel.SetActive(false);
        if (explanationPanel) explanationPanel.SetActive(false);
        if (completionPanel) completionPanel.SetActive(true);
    }

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

    public bool HasCurrentQuestion => _current != null;
    public bool ReadyForManualContinue { get; private set; } = false;
}
