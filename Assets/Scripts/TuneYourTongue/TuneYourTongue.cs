using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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
    public float listenTimeout = 6f; // not used by Vosk but we keep it
    public int minHeardLength = 3;

    [Header("Debug")]
    public TMP_Text debugText;

    [Header("Audio (optional)")]
    public AudioSource sfx;
    public AudioClip passSfx;
    public AudioClip failSfx;

    int _index, _coins;
    bool _listening;

#if UNITY_ANDROID && !UNITY_EDITOR
    VoskRecognizerAndroid _vosk;
#endif

    // Simple Vosk JSON structs
    [System.Serializable] class VoskPartial { public string partial; }
    [System.Serializable] class VoskFinal { public string text; }

    void Awake()
    {
        HideAllPanels();
        _index = 0; _coins = 0;
        RefreshCoins();
        SetWord(wordList[_index]);

#if UNITY_ANDROID && !UNITY_EDITOR
        // 1) Ensure model is extracted
        string modelPath = VoskModelInstaller.EnsureExtracted("vosk-model-small-en-us-0.15");
        if (string.IsNullOrEmpty(modelPath))
            Log("Model not ready (first boot?).");

        // 2) Init Vosk bridge and load model
        _vosk = new VoskRecognizerAndroid(gameObject.name, nameof(OnVoskMessage), nameof(OnSpeechError));
        if (!string.IsNullOrEmpty(modelPath))
            _vosk.LoadModel(modelPath);
#else
        Log("Editor mode: Vosk only runs on device in this setup.");
#endif
        if (hearButton) hearButton.onClick.AddListener(SpeakCurrent);
        if (micButton) micButton.onClick.AddListener(ToggleMic);
        Log("Ready.");
    }

    void OnDestroy()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try { _vosk?.Dispose(); } catch { }
#endif
    }

    void HideAllPanels() { if (successPanel) successPanel.SetActive(false); if (tryAgainPanel) tryAgainPanel.SetActive(false); }
    void SetWord(string w) => wordText.text = w.ToUpperInvariant();
    void NextWord() { _index = (_index + 1) % wordList.Length; SetWord(wordList[_index]); }
    void RefreshCoins() => coinsText.text = $"Coins: {_coins}";

    void SpeakCurrent() { Debug.Log($"[TTS stub] {wordList[_index]}"); } // keep or hook to your TTS

    void ToggleMic() { if (_listening) StopListening(); else StartListening(); }

    void StartListening()
    {
        if (_listening) return;
        _listening = true; HideAllPanels();

        if (micButtonAnimator) micButtonAnimator.SetBool("isListening", true);
        if (micButton) micButton.interactable = false;

#if UNITY_ANDROID && !UNITY_EDITOR
        Log("Mic pressed → Vosk start");
        _vosk?.StartListening();
#else
        Log("Build to Android to test Vosk.");
#endif
    }

    void StopListening()
    {
        if (!_listening) return;
        _listening = false;
        if (micButtonAnimator) micButtonAnimator.SetBool("isListening", false);
        if (micButton) micButton.interactable = true;

#if UNITY_ANDROID && !UNITY_EDITOR
        _vosk?.StopListening();
#endif
    }

    // ====== Vosk → Unity messages ======
    public void OnVoskMessage(string json)
    {
        // Called frequently (partials) and once (final)
        if (string.IsNullOrEmpty(json)) return;

        if (json.Contains("\"text\"")) // final
        {
            var f = JsonUtility.FromJson<VoskFinal>(json);
            string said = (f?.text ?? "").Trim().ToLowerInvariant();
            Log($"Final: {said}");
            OnSpeechFinalResult(said);
        }
        else if (json.Contains("\"partial\"") || json.Contains("\"Partial\""))
        {
            var p = JsonUtility.FromJson<VoskPartial>(json);
            string part = (p?.partial ?? "");
            if (debugText) debugText.text = "… " + part;
        }
    }

    // Keep your evaluation code
    void OnSpeechFinalResult(string said)
    {
        StopListening();

        string target = wordList[_index].Trim().ToLowerInvariant();
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
        StopListening();
        Log($"ASR error: {message}");
        ShowTryAgain("Recognizer error.");
    }

    // ====== UI helpers ======
    void OnPass(string said)
    {
        HideAllPanels();
        _coins += coinsPerPass; RefreshCoins();
        if (successHeardText) successHeardText.text = $"You said: {said}";
        if (successPanel) successPanel.SetActive(true);
        if (sfx && passSfx) sfx.PlayOneShot(passSfx);
        if (autoAdvanceOnSuccess) StartCoroutine(HideThenNext(panelAutoHide));
    }
    void OnFail(string said)
    {
        HideAllPanels();
        if (tryAgainHeardText) tryAgainHeardText.text = $"I heard: {said}";
        if (tryAgainPanel) tryAgainPanel.SetActive(true);
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

    IEnumerator HideThenNext(float t) { yield return new WaitForSeconds(t); if (successPanel) successPanel.SetActive(false); NextWord(); }
    IEnumerator HidePanelAfter(GameObject panel, float t) { if (!panel) yield break; yield return new WaitForSeconds(t); panel.SetActive(false); }

    // ====== Similarity ======
    static float Similarity(string a, string b) { if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0f; int dist = Levenshtein(a, b); int max = Mathf.Max(a.Length, b.Length); return 1f - (float)dist / Mathf.Max(1, max); }
    static int Levenshtein(string s, string t) { int n = s.Length, m = t.Length; int[,] d = new int[n + 1, m + 1]; for (int i = 0; i <= n; i++) d[i, 0] = i; for (int j = 0; j <= m; j++) d[0, j] = j; for (int i = 1; i <= n; i++) for (int j = 1; j <= m; j++) { int cost = (s[i - 1] == t[j - 1]) ? 0 : 1; d[i, j] = Mathf.Min(d[i - 1, j] + 1, Mathf.Min(d[i, j - 1] + 1, d[i - 1, j - 1] + cost)); } return d[n, m]; }

    void Log(string msg) { Debug.Log($"[TuneYourTongue] {msg}"); if (debugText) debugText.text = msg; }
}
