// Assets/Scripts/AndroidTextToSpeech.cs
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;

public class AndroidTextToSpeech : System.IDisposable
{
    AndroidJavaObject _plugin;
    AndroidJavaObject _activity;

    public bool IsAvailable => _plugin != null;

    public AndroidTextToSpeech()
    {
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                _activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            // If you don't have a Java TTS plugin, this call will fail and we’ll just no-op.
            using (var cls = new AndroidJavaClass("com.brainyme.speech.TextToSpeechPlugin"))
            {
                _plugin = cls.CallStatic<AndroidJavaObject>("create", _activity);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[TTY] TTS plugin not found (no-op): " + e.Message);
            _plugin = null;
        }
    }

    public void Speak(string text)
    {
        if (_plugin == null || string.IsNullOrEmpty(text)) return;
        RunOnUiThread(() => _plugin.Call("speak", text));
    }

    public void Dispose()
    {
        if (_plugin == null) return;
        RunOnUiThread(() =>
        {
            try { _plugin.Call("release"); } catch {}
            _plugin = null;
        });
    }

    void RunOnUiThread(AndroidJavaRunnable r)
    {
        if (_activity == null) return;
        _activity.Call("runOnUiThread", r);
    }
}
#endif
