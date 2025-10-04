using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;

public class SmartLadderQuiz : MonoBehaviour
{
    [Header("Difficulty")]
    public LadderDifficulty difficulty = LadderDifficulty.Easy;

    [Header("Panels & UI")]
    public GameObject questionPanel;         // active while picking an answer
    public TMP_Text questionText;
    public Button[] optionButtons;           // size = 3
    public GameObject explanationPanel;      // active after picking, shows explanation
    public TMP_Text explanationText;

    [Header("Coins (optional UI)")]
    public TMP_Text coinsText;               // optional UI label to show total coins

    [Header("Events")]
    public UnityEvent<bool> onAnswered;      // true if correct
    public UnityEvent<int> onCoinsChanged;   // passes new coin total (optional)

    [Header("Mover Hook")]
    public Gameplay mover;                   // drag the object that has Gameplay

    // Data/provider
    IQuestionProvider _provider;
    HashSet<int> _asked = new HashSet<int>();
    Question _current;

    // State
    bool _inited;
    bool _lastAnswerCorrect = false;
    int _coins = 0;
    int _wrongStreakThisLevel = 0;           // wrong attempts on the *current level*

    void Awake()
    {
        if (questionPanel) questionPanel.SetActive(false);
        if (explanationPanel) explanationPanel.SetActive(false);
        UpdateCoinsUI();
    }

    void EnsureInit()
    {
        if (_inited) return;
        _provider = new InMemoryQuestionProvider();  // your in-memory pool
        _provider.Initialize();
        _inited = true;
    }

    // Called by Gameplay when the player arrives at a platform
    public void ShowNextQuestion()
    {
        EnsureInit();

        _current = _provider.GetNext(difficulty, _asked);
        if (_current == null)
        {
            if (questionPanel) questionPanel.SetActive(true);
            if (explanationPanel) explanationPanel.SetActive(false);
            if (questionText) questionText.text = "No questions available.";
            return;
        }

        _asked.Add(_current.Id);

        if (explanationPanel) explanationPanel.SetActive(false);
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

        _lastAnswerCorrect = (_current != null && choiceIndex == _current.CorrectIndex);

        if (explanationText)
        {
            string prefix = _lastAnswerCorrect ? "Correct! " : "Not quite. ";
            explanationText.text = prefix + (_current?.Explanation ?? "");
        }

        if (questionPanel) questionPanel.SetActive(false);
        if (explanationPanel) explanationPanel.SetActive(true);

        onAnswered?.Invoke(_lastAnswerCorrect);
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
            int reward = RewardForCurrentStreak();
            _coins += reward;
            UpdateCoinsUI();
            _wrongStreakThisLevel = 0;           // reset for next level

            if (mover != null) mover.MoveNext();
            else Debug.LogWarning("SmartLadderQuiz: mover not assigned.");
        }
        else
        {
            // stay on same level; increase streak and ask another question
            _wrongStreakThisLevel++;
            ShowNextQuestion();
        }
    }

    // --- Coins / Reward helpers ---

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
        if (coinsText) coinsText.text = $"{_coins}";
        onCoinsChanged?.Invoke(_coins);
    }

    // If you need to reset at the start of a run/difficulty:
    public void ResetRun()
    {
        _coins = 0;
        _wrongStreakThisLevel = 0;
        _asked.Clear();
        UpdateCoinsUI();
    }
}
