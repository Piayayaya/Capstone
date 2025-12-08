using System;
using UnityEngine;

public class GoogleDeepLinkReceiver : MonoBehaviour
{
    [SerializeField] private GoogleWebLogin googleLogin;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // Fired when app is opened from a deep link
        Application.deepLinkActivated += OnDeepLinkActivated;

        // If app was *started* via deep link, handle it immediately
        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            OnDeepLinkActivated(Application.absoluteURL);
        }
    }

    private void OnDeepLinkActivated(string url)
    {
        Debug.Log("[DeepLink] Received URL: " + url);

        if (string.IsNullOrEmpty(url))
            return;

        int hashIndex = url.IndexOf('#');
        if (hashIndex < 0) return;

        string fragment = url.Substring(hashIndex + 1); // after '#'
        string[] parts = fragment.Split('&');

        string idToken = null;
        foreach (var p in parts)
        {
            if (p.StartsWith("id_token="))
            {
                idToken = Uri.UnescapeDataString(p.Substring("id_token=".Length));
                break;
            }
        }

        if (!string.IsNullOrEmpty(idToken) && googleLogin != null)
        {
            googleLogin.OnGotIdTokenFromDeepLink(idToken);
        }
        else
        {
            Debug.LogWarning("[DeepLink] No id_token found or googleLogin not set.");
        }
    }
}
