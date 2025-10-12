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

        // --- Guard against bad wiring ---
        if (explanationText == null) Debug.LogError("[Quiz] explanationText is not assigned.", this);
        if (coinsText == null) Debug.LogWarning("[Quiz] coinsText is not assigned (OK if you don't display coins).", this);
        if (rewardText == null) Debug.LogWarning("[Quiz] rewardText is not assigned (OK if you don't show popup).", this);

        if (coinsText != null && explanationText != null && ReferenceEquals(coinsText, explanationText))
        {
            Debug.LogError("[Quiz] coinsText and explanationText reference the SAME TMP object. Fix the Inspector.", this);
            // To prevent damage at runtime:
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

        _current = _provider.GetNext(EffectiveDifficulty, _asked);

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

    // OnOption: change this part
    void OnOption(int choiceIndex)
    {
        foreach (var b in optionButtons) if (b) b.interactable = false;

        _lastAnswerCorrect = (_current != null && choiceIndex == _current.CorrectIndex);

        if (explanationText)
        {
            string prefix = _lastAnswerCorrect ? "Correct! " : "Not quite. ";
            explanationText.text = prefix + (_current?.Explanation ?? "");

            // Debug: tint the explanation so you can see you’re writing to the right label
            explanationText.color = new Color(1f, 1f, 1f, 1f); // solid white (or any color you want)
        }


        if (_lastAnswerCorrect)
        {
            // DO NOT hide the question panel yet – we want the popup to overlap it
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


    // CoShowRewardThenExplanation: put popup over the question panel, then switch to explanation
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
            // We already added coins; now finalize the level result
            _wrongStreakThisLevel = 0;
            _correctSoFar++;

            if (ReachedTarget())
            {
                onRunCompleted?.Invoke();
                return;
            }

            if (mover != null) mover.MoveNext();
            else Debug.LogWarning("SmartLadderQuiz: mover not assigned.");
        }
        else
        {
            // stay on same level; increase streak and ask another
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
        // If you had listeners, keep them; otherwise leave them out
        // onCoinsChanged?.Invoke(_coins);
    }


    // Optional close/reopen
    public void CloseQuestionPanel() { if (questionPanel) questionPanel.SetActive(false); }
    public void ReopenQuestionPanel() { if (questionPanel) questionPanel.SetActive(true); }
}
