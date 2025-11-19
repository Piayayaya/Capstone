using UnityEngine;
using System;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

/// <summary>
/// Simple cross-platform TTS facade:
///   TTSManager.Speak("Hello!");
///   TTSManager.Stop();
///   TTSManager.SetRate(1.0f);  // 0.5–2.0
///   TTSManager.SetPitch(1.0f); // 0.5–2.0
/// </summary>
public class TTSManager : MonoBehaviour
{
    public static TTSManager Instance { get; private set; }

    [Header("Defaults")]
    [Tooltip("BCP-47 like 'en-US', 'en-PH', 'fil-PH' etc.")]
    public string languageTag = "en-US";
    [Range(0.5f, 2f)] public float speechRate = 1.0f;
    [Range(0.5f, 2f)] public float pitch = 1.0f;

#if UNITY_ANDROID && !UNITY_EDITOR
    AndroidJavaObject tts;
    AndroidJavaObject activity;
    bool ready = false;
#endif

    void Awake()
    {
        if (Instance) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    public void Initialize()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, new OnInitListener(OnTTSReady));
        }));
#endif
        // iOS is initialized lazily in Speak via native call (added in Step 4).
    }

    void OnDestroy()
    {
        Stop();
#if UNITY_ANDROID && !UNITY_EDITOR
        if (tts != null) { tts.Call("shutdown"); tts.Dispose(); tts = null; }
#endif
    }

    // ----------------- Public API -----------------
    public static void Speak(string text)
    {
        if (!Instance) { Debug.LogWarning("[TTS] No instance. Creating one."); new GameObject("TTSManager").AddComponent<TTSManager>(); }
        Instance._Speak(text);
    }

    public static void Stop() => Instance?._Stop();

    public static void SetRate(float rate)
    {
        if (!Instance) return;
        Instance.speechRate = Mathf.Clamp(rate, 0.5f, 2f);
#if UNITY_ANDROID && !UNITY_EDITOR
        if (Instance.ready) Instance.tts.Call<int>("setSpeechRate", Instance.speechRate);
#endif
#if UNITY_IOS && !UNITY_EDITOR
        _iosSetRate(Instance.speechRate);
#endif
    }

    public static void SetPitch(float p)
    {
        if (!Instance) return;
        Instance.pitch = Mathf.Clamp(p, 0.5f, 2f);
#if UNITY_ANDROID && !UNITY_EDITOR
        if (Instance.ready) Instance.tts.Call<int>("setPitch", Instance.pitch);
#endif
#if UNITY_IOS && !UNITY_EDITOR
        _iosSetPitch(Instance.pitch);
#endif
    }

    public static void SetLanguage(string bcp47)
    {
        if (!Instance) return;
        Instance.languageTag = bcp47;
#if UNITY_ANDROID && !UNITY_EDITOR
        if (Instance.ready) Instance._ApplyAndroidLanguage();
#endif
#if UNITY_IOS && !UNITY_EDITOR
        _iosSetLanguage(bcp47);
#endif
    }

    // ----------------- Platform impl -----------------
    void _Speak(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!ready) { Debug.LogWarning("[TTS] Not ready yet."); return; }
        var paramsBundle = new AndroidJavaObject("android.os.Bundle");
        // Optional: route as media
        paramsBundle.Call("putInt", "streamType", 3 /* AudioManager.STREAM_MUSIC */);

        // QUEUE_FLUSH = 0, QUEUE_ADD = 1
        tts.Call<int>("speak", text, 0, paramsBundle, Guid.NewGuid().ToString());
#elif UNITY_IOS && !UNITY_EDITOR
        _iosSpeak(text, languageTag, speechRate, pitch);
#else
        Debug.Log($"[TTS] (Editor stub) {text}");
#endif
    }

    void _Stop()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (ready) tts.Call<int>("stop");
#elif UNITY_IOS && !UNITY_EDITOR
        _iosStop();
#else
        // editor: nothing
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    void TrySelectMaleVoice()
    {
        try
        {
            if (tts == null) return;

            // Get all available voices from the current TTS engine
            using (var voices = tts.Call<AndroidJavaObject>("getVoices"))
            {
                if (voices == null) return;

                using (var iterator = voices.Call<AndroidJavaObject>("iterator"))
                {
                    AndroidJavaObject chosen = null;

                    while (iterator.Call<bool>("hasNext"))
                    {
                        using (var v = iterator.Call<AndroidJavaObject>("next"))
                        {
                            if (v == null) continue;

                            string name = v.Call<string>("getName");
                            using (var locale = v.Call<AndroidJavaObject>("getLocale"))
                            {
                                string localeTag = locale.Call<string>("toLanguageTag"); // e.g. "en-US"

                                bool hasMaleInName = !string.IsNullOrEmpty(name) &&
                                                     name.ToLower().Contains("male");

                                bool matchesLanguage =
                                    string.IsNullOrEmpty(languageTag) ||
                                    (!string.IsNullOrEmpty(localeTag) &&
                                     localeTag.StartsWith(languageTag.Substring(0, 2),
                                                          StringComparison.OrdinalIgnoreCase));

                                if (hasMaleInName && matchesLanguage)
                                {
                                    chosen = v;
                                    break;
                                }
                            }
                        }
                    }

                    if (chosen != null)
                    {
                        tts.Call<int>("setVoice", chosen);
                        string chosenName = chosen.Call<string>("getName");
                        Debug.Log("[TTS] Selected male voice: " + chosenName);
                    }
                    else
                    {
                        Debug.Log("[TTS] No explicit male voice found; using default.");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[TTS] Failed to select male voice: " + e.Message);
        }
    }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    void OnTTSReady(int status)
    {
        ready = status == 0; // SUCCESS
        if (!ready) { Debug.LogError("[TTS] Init failed."); return; }

        // Rate & pitch
        tts.Call<int>("setSpeechRate", speechRate);
        tts.Call<int>("setPitch", pitch);

        // Language
        _ApplyAndroidLanguage();

        // 🔊 Try to select a male voice that matches our language
        TrySelectMaleVoice();

        // Optional: install missing voice data
        int avail = tts.Call<int>("isLanguageAvailable", MakeLocale(languageTag));
        if (avail < 0) // LANG_MISSING_DATA or LANG_NOT_SUPPORTED
        {
            try
            {
                using var intent = new AndroidJavaObject("android.content.Intent", "android.speech.tts.engine.ACTION_INSTALL_TTS_DATA");
                activity.Call("startActivity", intent);
            }
            catch (Exception e) { Debug.LogWarning("[TTS] Could not open TTS installer: " + e.Message); }
        }
    }

    void _ApplyAndroidLanguage()
    {
        var loc = MakeLocale(languageTag);
        int res = tts.Call<int>("setLanguage", loc);
        // res can be LANG_AVAILABLE / COUNTRY_AVAILABLE / VARIANT_AVAILABLE / MISSING_DATA / NOT_SUPPORTED
    }

    AndroidJavaObject MakeLocale(string tag)
    {
        // tag like "en-US" or "fil-PH"
        string[] parts = tag.Split('-');
        if (parts.Length == 1) return new AndroidJavaObject("java.util.Locale", parts[0]);
        return new AndroidJavaObject("java.util.Locale", parts[0], parts[1]);
    }

    // Android OnInitListener
    class OnInitListener : AndroidJavaProxy
    {
        readonly Action<int> cb;
        public OnInitListener(Action<int> cb) : base("android.speech.tts.TextToSpeech$OnInitListener") { this.cb = cb; }
        public void onInit(int status) => cb?.Invoke(status);
    }
#endif

#if UNITY_IOS && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")] static extern void _iosSpeak(string text, string locale, float rate, float pitch);
    [System.Runtime.InteropServices.DllImport("__Internal")] static extern void _iosStop();
    [System.Runtime.InteropServices.DllImport("__Internal")] static extern void _iosSetRate(float rate);
    [System.Runtime.InteropServices.DllImport("__Internal")] static extern void _iosSetPitch(float pitch);
    [System.Runtime.InteropServices.DllImport("__Internal")] static extern void _iosSetLanguage(string locale);
#endif
}
