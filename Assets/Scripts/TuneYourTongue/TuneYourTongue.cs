using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TuneYourTongue : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text wordLabel;

    public GameObject successPanel;
    public TMP_Text successMessage;

    public GameObject tryAgainPanel;
    public TMP_Text tryAgainMessage;

    [Header("GAME COMPLETE UI")]
    public GameObject sessionCompletePanel;
    public TMP_Text sessionCompleteCoinsText;

    [Header("Words")]
    public string[] words;

    [Header("Systems")]
    public CoinWallet coinWallet;

    [Header("Progress (SLIDER)")]
    public Slider progressSlider;    // 🔥 NEW
    public int wordsPerSession = 5;
    private int wordsCompleted = 0;

    private string currentWord;


    void OnEnable()
    {
        wordsCompleted = 0;
        SetupProgressBar();   // 🔥 NEW
        sessionCompletePanel.SetActive(false);

        SpeechRecognizer.OnResult += OnSpeechResult;
        NextWord();
    }

    void OnDisable()
    {
        SpeechRecognizer.OnResult -= OnSpeechResult;
    }


    // -----------------------
    //  WORD HANDLING
    // -----------------------

    void NextWord()
    {
        successPanel.SetActive(false);
        tryAgainPanel.SetActive(false);

        currentWord = words[Random.Range(0, words.Length)];
        wordLabel.text = currentWord.ToUpper();

        TTSManager.Speak("Say " + currentWord);
    }


    public void StartListening()
    {
        FindObjectOfType<SpeechRecognizer>().StartListening();
        Debug.Log("[UI] Mic button pressed");
    }


    void OnSpeechResult(string text)
    {
        bool correct = text.Trim().ToLower() == currentWord.ToLower();

        if (correct)
            HandleCorrect(text);
        else
            HandleIncorrect(text);
    }


    // -----------------------
    //  CORRECT / WRONG HANDLERS
    // -----------------------

    void HandleCorrect(string text)
    {
        coinWallet.Add(5);

        TTSManager.Speak("Good job!");

        successMessage.text = "Good job!";
        successPanel.SetActive(true);

        wordsCompleted++;
        UpdateProgressBar();      // 🔥 NEW

        if (wordsCompleted >= wordsPerSession)
        {
            ShowSessionComplete();
        }
    }


    void HandleIncorrect(string text)
    {
        TTSManager.Speak("Try again");

        tryAgainMessage.text = "Too bad!";
        tryAgainPanel.SetActive(true);
    }


    // -----------------------
    //  PROGRESS BAR
    // -----------------------

    void SetupProgressBar()
    {
        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = wordsPerSession;
            progressSlider.value = 0;
        }
    }

    void UpdateProgressBar()
    {
        if (progressSlider != null)
        {
            progressSlider.value = wordsCompleted;
        }
    }


    // -----------------------
    //  ADVANCE / RETRY
    // -----------------------

    public void OnNextQuestion()
    {
        NextWord();
    }

    public void OnRetry()
    {
        TTSManager.Speak("Say " + currentWord);
        StartListening();
        tryAgainPanel.SetActive(false);
    }


    // -----------------------
    //  SESSION COMPLETE
    // -----------------------

    void ShowSessionComplete()
    {
        successPanel.SetActive(false);
        tryAgainPanel.SetActive(false);

        int totalCoinsEarned = wordsCompleted * 5;
        sessionCompleteCoinsText.text = "You earned " + totalCoinsEarned + " coins!";

        sessionCompletePanel.SetActive(true);

        TTSManager.Speak("Great job! You finished the session");
    }

    public void OnPlayAgain()
    {
        sessionCompletePanel.SetActive(false);
        OnEnable();
    }

    public void OnExitToDashboard()
    {
        // SceneManager.LoadScene("Dashboard");
    }
}
