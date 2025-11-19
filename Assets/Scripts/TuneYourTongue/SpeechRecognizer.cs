using UnityEngine;

public class SpeechRecognizer : MonoBehaviour
{
    private static AndroidJavaObject activity;

    public delegate void SpeechResultDelegate(string text);
    public static event SpeechResultDelegate OnResult;

    void Awake()
    {
        gameObject.name = "SpeechRecognizer";  // Required for UnitySendMessage

#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
#endif
    }

    public void StartListening()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    {
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaClass bridge = new AndroidJavaClass("com.brainyme.tts.SpeechBridge");
        bridge.CallStatic("StartListening", activity);
    }
#else
        Debug.Log("StartListening called in Editor.");
#endif
    }



    // Called by Java
    public void OnSpeechResult(string recognizedText)
    {
        Debug.Log("[STT] Recognized: " + recognizedText);
        OnResult?.Invoke(recognizedText);
    }
}
