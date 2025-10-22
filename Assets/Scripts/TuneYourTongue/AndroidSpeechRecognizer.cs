// Assets/Scripts/AndroidSpeechRecognizer.cs
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;

public class AndroidSpeechRecognizer : System.IDisposable
{
    AndroidJavaObject _plugin;
    AndroidJavaObject _activity;

    readonly string _go;
    readonly string _resultMethod;
    readonly string _errorMethod;

    public AndroidSpeechRecognizer(string gameObjectName, string resultMethod, string errorMethod)
    {
        _go = gameObjectName;
        _resultMethod = resultMethod;
        _errorMethod = errorMethod;

        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            _activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        RunOnUiThread(() =>
        {
            using (var cls = new AndroidJavaClass("com.brainyme.speech.SpeechRecognizerPlugin"))
            {
                _plugin = cls.CallStatic<AndroidJavaObject>(
                    "create", _activity, _go, _resultMethod, _errorMethod);
            }
        });
    }

    public void StartListening(string lang, int timeoutSeconds)
    {
        if (_plugin == null) return;
        RunOnUiThread(() => _plugin.Call("startListening", lang, timeoutSeconds));
    }

    public void StopListening()
    {
        if (_plugin == null) return;
        RunOnUiThread(() => _plugin.Call("stopListening"));
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
