#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;

public class VoskRecognizerAndroid : System.IDisposable
{
    AndroidJavaObject _plugin; AndroidJavaObject _activity;
    string _go, _result, _error;

    public VoskRecognizerAndroid(string gameObjectName, string resultMethod, string errorMethod)
    {
        _go = gameObjectName; _result = resultMethod; _error = errorMethod;
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            _activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        using (var cls = new AndroidJavaClass("com.brainyme.speech.VoskPlugin"))
            _plugin = cls.CallStatic<AndroidJavaObject>("create", _activity, _go, _result, _error);
    }

    public void LoadModel(string absolutePath) => _plugin?.Call("loadModel", absolutePath);
    public void StartListening() => _plugin?.Call("startListening");
    public void StopListening()  => _plugin?.Call("stopListening");
    public void Dispose()        => _plugin?.Call("release");
}
#endif
