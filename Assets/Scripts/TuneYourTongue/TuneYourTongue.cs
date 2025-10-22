// Assets/Scripts/TuneYourTongue.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TuneYourTongue : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text wordText;
    public Button hearButton;
    public Button micButton;
    public TMP_Text coinsText;
    public Animator micButtonAnimator;

    [Header("Result Panels")]
    public GameObject successPanel;
    public GameObject tryAgainPanel;
    public TMP_Text successHeardText;
    public TMP_Text tryAgainHeardText;
    public float panelAutoHide = 1.2f;
    public bool autoAdvanceOnSuccess = true;

    [Header("Game")]
    [TextArea] public string[] wordList = { "hippopotamus", "elephant", "giraffe", "rhinoceros", "caterpillar", "astronaut", "dictionary" };
    [Range(0.5f, 1f)] public float passThreshold = 0.85f;
    public int coinsPerPass = 10;
    public float listenTimeout = 6f;
    public int minHeardLength = 3;

    [Header("Debug (optional)")]
    public TMP_Text debugText;
#if UNITY_EDITOR
    public bool simulateInEditor = true;
    public TMP_InputField editorInput;
    public string editorSimulatedSaid = "";
#endif

    [Header("Audio (optional)")]
    public AudioSource sfx;
    public AudioClip passSfx;
    public AudioClip failSfx;

    int _index;
    int _coins;
    bool _listening;

#if UNITY_ANDROID && !UNITY_EDITOR
    AndroidSpeechRecognizer _stt;
    AndroidTextToSpeech _tts;
    bool _ttsOk; // we’ll keep playing even if TTS is missing
#endif

    void Awake()
    {
        EnsureEventSystem();

        HideAllPanels();
        _index = 0; _coins = 0;
        RefreshCoins();
        SetWord(wordList[_index]);

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            _stt = new AndroidSpeechRecognizer(gameObject.name, nameof(OnSpeechFinalResult), nameof(OnSpeechError));
            Debug.Log("[TTY] STT initialized.");

            try
            {
                _tts = new AndroidTextToSpeech();   // safe wrapper; may no-op if plugin missing
                _ttsOk = _tts.IsAvailable;
                Debug.Log("[TTY] TTS initialized: " + _ttsOk);
            }
            catch (System.Exception tex)
            {
                _tts = null; _ttsOk = false;
                Debug.LogWarning("[TTY] TTS init failed (skipping): " + tex.Message);
            }
        }
        catch (System.Exception sex)
        {
            Debug.LogError("[TTY] STT init failed: " + sex.Message);
            _stt = null;
        }
#else
        Debug.Log("[TTY] Running in Editor / non-Android.");
#endif

        if (hearButton)
        {
            hearButton.onClick.RemoveAllListeners();
            hearButton.onClick.AddListener(SpeakCurrent);
        }
        if (micButton)
        {
            micButton.onClick.RemoveAllListeners();
            micButton.onClick.AddListener(ToggleMic);
        }

        Log("Ready.");
    }

    void OnDestroy()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try { _stt?.Dispose(); } catch {}
        try { _tts?.Dispose(); } catch {}
#endif
    }

    void OnApplicationPause(bool pause)
    {
        if (pause && _listening) StopListening();
    }

    void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(go);
            Log("[TTY] Created EventSystem.");
        }
    }

    void HideAllPanels()
    {
        if (successPanel) successPanel.SetActive(false);
        if (tryAgainPanel) tryAgainPanel.SetActive(false);
    }

    void SetWord(string w) => wordText.text = w.ToUpperInvariant();
    void NextWord() { _index = (_index + 1) % wordList.Length; SetWord(wordList[_index]); }
    void RefreshCoins() => coinsText.text = $"Coins: {_coins}";

    void SpeakCurrent()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_ttsOk) _tts.Speak(wordList[_index]);
        else Debug.Log("[TTY] TTS disabled; would say: " + wordList[_index]);
#else
        Debug.Log("[TTS] " + wordList[_index]);
#endif
    }

    public void ToggleMic()
    {
        Debug.LogError("[TTY] Mic Button clicked (ToggleMic).");
        if (_listening) StopListening();
        else StartListening();
    }

    void StartListening()
    {
        if (_listening) return;
        _listening = true;
        HideAllPanels();
        if (micButtonAnimator) micButtonAnimator.SetBool("isListening", true);
        if (micButton) micButton.interactable = false;

        Debug.LogError("[TTY] Mic pressed → StartListening()");

#if UNITY_ANDROID && !UNITY_EDITOR
        var micPerm = UnityEngine.Android.Permission.Microphone;
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(micPerm))
        {
            LogError("[TTY] Mic permission NOT granted.");
            ShowTryAgain("Microphone permission not granted.");
            StopListening();
            return;
        }

        if (_stt == null)
        {
            LogError("[TTY] Speech plugin not available.");
            ShowTryAgain("Speech engine not available.");
            StopListening();
            return;
        }

        Debug.LogError("[TTY] Calling _stt.StartListening()");
        _stt.StartListening("en-US", Mathf.RoundToInt(listenTimeout));
#else
        if (simulateInEditor) StartCoroutine(EditorSimulate());
        else Log("Editor cannot capture mic. Build to Android.");
#endif
    }

    void StopListening()
    {
        if (!_listening) return;
        _listening = false;
        if (micButtonAnimator) micButtonAnimator.SetBool("isListening", false);
        if (micButton) micButton.interactable = true;

#if UNITY_ANDROID && !UNITY_EDITOR
        try { _stt?.StopListening(); } catch {}
#endif
    }

#if UNITY_EDITOR
    IEnumerator EditorSimulate()
    {
        yield return new WaitForSeconds(0.4f);
        string said = editorInput ? editorInput.text : editorSimulatedSaid;
        Debug.LogError($"[TTY] [EditorSim] said=\"{said}\"");
        OnSpeechFinalResult(said);
        StopListening();
    }
#endif

    // ====== Callbacks from recognizer ======
    public void OnSpeechFinalResult(string text)
    {
        Debug.LogError("[TTY] OnSpeechFinalResult: " + (text ?? "<null>"));
        StopListening();

        string target = wordList[_index].Trim().ToLowerInvariant();
        string said = (text ?? "").Trim().ToLowerInvariant();

        Log($"Heard: \"{said}\"  | Target: \"{target}\"");

        if (string.IsNullOrWhiteSpace(said) || said.Length < minHeardLength)
        {
            ShowTryAgain("I didn't catch anything.");
            return;
        }

        float score = Similarity(target, said);
        bool pass = score >= passThreshold;

        if (pass) OnPass(said);
        else OnFail(said);
    }

    public void OnSpeechError(string message)
    {
        Debug.LogError("[TTY] OnSpeechError: " + message);
        StopListening();
        ShowTryAgain("I didn't catch that.");
    }

    // ====== UI helpers ======
    void OnPass(string said)
    {
        HideAllPanels();
        _coins += coinsPerPass; RefreshCoins();

        if (successHeardText) successHeardText.text = $"You said: {said}";
        if (successPanel) successPanel.SetActive(true);
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_ttsOk) _tts.Speak("Great job!");
#endif
        if (sfx && passSfx) sfx.PlayOneShot(passSfx);
        if (autoAdvanceOnSuccess) StartCoroutine(HideThenNext(panelAutoHide));
    }

    void OnFail(string said)
    {
        HideAllPanels();
        if (tryAgainHeardText) tryAgainHeardText.text = $"I heard: {said}";
        if (tryAgainPanel) tryAgainPanel.SetActive(true);
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_ttsOk) _tts.Speak("Try again. You can do it!");
#endif
        if (sfx && failSfx) sfx.PlayOneShot(failSfx);
        if (panelAutoHide > 0f) StartCoroutine(HidePanelAfter(tryAgainPanel, panelAutoHide));
    }

    void ShowTryAgain(string msg)
    {
        HideAllPanels();
        if (tryAgainHeardText) tryAgainHeardText.text = msg;
        if (tryAgainPanel) tryAgainPanel.SetActive(true);
        if (panelAutoHide > 0f) StartCoroutine(HidePanelAfter(tryAgainPanel, panelAutoHide));
    }

    IEnumerator HideThenNext(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (successPanel) successPanel.SetActive(false);
        NextWord();
    }

    IEnumerator HidePanelAfter(GameObject panel, float delay)
    {
        if (!panel) yield break;
        yield return new WaitForSeconds(delay);
        panel.SetActive(false);
    }

    // ====== Fuzzy matching ======
    static float Similarity(string a, string b)
    {
        if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b)) return 1f;
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0f;
        int dist = Levenshtein(a, b);
        int maxLen = Mathf.Max(a.Length, b.Length);
        return 1f - (float)dist / Mathf.Max(1, maxLen);
    }

    static int Levenshtein(string s, string t)
    {
        int n = s.Length, m = t.Length;
        int[,] d = new int[n + 1, m + 1];
        for (int i = 0; i <= n; i++) d[i, 0] = i;
        for (int j = 0; j <= m; j++) d[0, j] = j;
        for (int i = 1; i <= n; i++)
            for (int j = 1; j <= m; j++)
            {
                int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                d[i, j] = Mathf.Min(
                    d[i - 1, j] + 1,
                    Mathf.Min(d[i, j - 1] + 1, d[i - 1, j - 1] + cost)
                );
            }
        return d[n, m];
    }

    // ====== Logging ======
    void Log(string msg)
    {
        Debug.Log($"[TuneYourTongue] {msg}");
        if (debugText) debugText.text = msg;
    }

    void LogError(string msg)
    {
        Debug.LogError(msg);
        if (debugText) debugText.text = msg;
    }
}
