using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public enum NarrationCategory { System, Guide, Question, Choices, Hint, Explanation, Result }

[DisallowMultipleComponent]
public class NarrationCenter : MonoBehaviour
{
    public static NarrationCenter I;

    [Header("Global Settings")]
    public bool narrationEnabled = true;       // user toggle
    [Range(0.5f, 2f)] public float rate = 0.95f;
    [Range(0.5f, 2f)] public float pitch = 1.08f;
    [Tooltip("Default language tag, e.g., en-US, fil-PH")]
    public string languageTag = "en-US";

    [Header("Timing")]
    [Tooltip("Used to estimate speech duration when no callbacks available.")]
    public float wordsPerMinute = 160f;
    [Tooltip("Extra time added after each utterance (seconds).")]
    public float postPause = 0.20f;

    public Action<NarrationItem> OnSpeakStart;
    public Action<NarrationItem> OnSpeakEnd;

    readonly Queue<NarrationItem> _queue = new();
    Coroutine _runner;
    NarrationItem _current;

    [Serializable]
    public class NarrationItem
    {
        public string text;
        public NarrationCategory category;
        public bool interrupt;   // if true, clears current + queue and plays now
        public string voiceTag;  // optional override locale (e.g., "fil-PH")
        public float? rate;      // optional per-utterance rate
        public float? pitch;     // optional per-utterance pitch
        public NarrationItem(string t, NarrationCategory c, bool interrupt = false) { text = t; category = c; this.interrupt = interrupt; }
    }

    void Awake()
    {
        if (I) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        // prime TTS defaults
        TTSManager.SetLanguage(languageTag);
        TTSManager.SetRate(rate);
        TTSManager.SetPitch(pitch);
    }

    // ---------- Public API ----------
    public static void Enqueue(string text, NarrationCategory cat = NarrationCategory.Guide, bool interrupt = false, string voiceTag = null, float? rate = null, float? pitch = null)
    {
        if (!I) new GameObject("NarrationCenter").AddComponent<NarrationCenter>();
        var item = new NarrationItem(Clean(text), cat, interrupt) { voiceTag = voiceTag, rate = rate, pitch = pitch };
        I._Enqueue(item);
    }

    public static void ReadQuestion(string text, bool interrupt = true) => Enqueue(text, NarrationCategory.Question, interrupt);
    public static void ReadChoices(string text) => Enqueue(text, NarrationCategory.Choices);
    public static void ReadHint(string text) => Enqueue(text, NarrationCategory.Hint);
    public static void ReadExplanation(string text, bool interrupt = false) => Enqueue(text, NarrationCategory.Explanation, interrupt);
    public static void ReadResult(string text, bool interrupt = true) => Enqueue(text, NarrationCategory.Result, interrupt);

    public static void StopAll()
    {
        if (!I) return;
        I._queue.Clear();
        I._StopCurrent();
    }

    public static void Skip()
    {
        if (!I) return;
        I._StopCurrent(); // runner will continue to next in queue
    }

    public static void SetEnabled(bool enabled)
    {
        if (!I) return;
        I.narrationEnabled = enabled;
        if (!enabled) StopAll();
    }

    public static void SetLanguage(string bcp47)
    {
        if (!I) return;
        I.languageTag = bcp47;
        TTSManager.SetLanguage(bcp47);
    }

    public static void SetRate(float r)
    {
        if (!I) return;
        I.rate = Mathf.Clamp(r, 0.5f, 2f);
        TTSManager.SetRate(I.rate);
    }

    public static void SetPitch(float p)
    {
        if (!I) return;
        I.pitch = Mathf.Clamp(p, 0.5f, 2f);
        TTSManager.SetPitch(I.pitch);
    }

    // ---------- Internals ----------
    void _Enqueue(NarrationItem item)
    {
        if (item.interrupt)
        {
            _queue.Clear();
            _StopCurrent();
        }
        _queue.Enqueue(item);
        if (_runner == null) _runner = StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        while (true)
        {
            if (!narrationEnabled)
            {
                // mute mode: drop everything and exit
                _queue.Clear();
                _current = null;
                _runner = null;
                yield break;
            }

            if (_current == null)
            {
                if (_queue.Count == 0) { _runner = null; yield break; }
                _current = _queue.Dequeue();

                // per-utterance overrides
                if (!string.IsNullOrEmpty(_current.voiceTag)) TTSManager.SetLanguage(_current.voiceTag);
                if (_current.rate.HasValue) TTSManager.SetRate(_current.rate.Value);
                if (_current.pitch.HasValue) TTSManager.SetPitch(_current.pitch.Value);

                OnSpeakStart?.Invoke(_current);
                TTSManager.Speak(_current.text);

                // estimate duration (WPM heuristic)
                float secs = EstimateSeconds(_current.text, wordsPerMinute) + postPause;
                yield return new WaitForSeconds(secs);

                OnSpeakEnd?.Invoke(_current);

                // restore defaults if we changed them
                if (!string.IsNullOrEmpty(_current.voiceTag)) TTSManager.SetLanguage(languageTag);
                if (_current.rate.HasValue) TTSManager.SetRate(rate);
                if (_current.pitch.HasValue) TTSManager.SetPitch(pitch);

                _current = null;
            }
            else
            {
                yield return null;
            }
        }
    }

    void _StopCurrent()
    {
        TTSManager.Stop();
        _current = null;
    }

    static float EstimateSeconds(string text, float wpm)
    {
        // rough but works well for UX pacing
        int words = Mathf.Max(1, Regex.Matches(text.Trim(), @"\b[\p{L}\p{N}’']+\b").Count);
        return (words / Mathf.Max(60f, wpm)) * 60f;
    }

    static string Clean(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";
        // Remove TMP rich tags and collapse whitespace
        string s = Regex.Replace(input, "<.*?>", "");
        s = Regex.Replace(s, @"\s+", " ").Trim();
        return s;
    }
}
