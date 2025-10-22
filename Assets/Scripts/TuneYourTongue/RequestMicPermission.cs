using UnityEngine;
public class RequestMicPermission : MonoBehaviour
{
    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var mic = UnityEngine.Android.Permission.Microphone;
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(mic))
            UnityEngine.Android.Permission.RequestUserPermission(mic);
#endif
    }
}
