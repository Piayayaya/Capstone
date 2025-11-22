using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class GoogleWebLogin : MonoBehaviour
{
    private WebViewObject webView;
    private TaskCompletionSource<string> loginResult;

    [Header("WEB OAuth Client ID")]
    [SerializeField]
    private string clientId =
        "397904065396-71kqualnlvfa6sh17qf7h1hckjf780l0.apps.googleusercontent.com";

    [Header("Redirect URI (must be whitelisted in WEB client)")]
    [SerializeField] private string redirectUri = "https://localhost";

    private string BuildAuthUrl()
    {
        string scope = "openid email profile";

        return
            "https://accounts.google.com/o/oauth2/v2/auth" +
            "?client_id=" + clientId +
            "&redirect_uri=" + System.Uri.EscapeDataString(redirectUri) +
            "&response_type=id_token" +
            "&scope=" + System.Uri.EscapeDataString(scope) +
            "&nonce=unity_nonce" +
            "&prompt=select_account" +
            "&state=" + System.Guid.NewGuid().ToString("N");
    }

    public void StartGoogleLogin()
    {
        Debug.Log("[GoogleWebLogin] BUTTON CLICKED");

#if UNITY_EDITOR
        Application.OpenURL(BuildAuthUrl());
#else
        StartCoroutine(LoginFlow());
#endif
    }

    private IEnumerator LoginFlow()
    {
        var tokenTask = GetIdToken();
        while (!tokenTask.IsCompleted) yield return null;

        string idToken = tokenTask.Result;

        if (string.IsNullOrEmpty(idToken))
        {
            Debug.LogError("[GoogleWebLogin] Login failed: idToken null/empty.");
            yield break;
        }

        Debug.Log("[GoogleWebLogin] Login Success. idToken length=" + idToken.Length);
        // You can now use idToken with Firebase Auth if you want.
    }

    public Task<string> GetIdToken()
    {
        loginResult = new TaskCompletionSource<string>();

        string authUrl = BuildAuthUrl();
        Debug.Log("[GoogleWebLogin] Auth URL: " + authUrl);

        webView = (new GameObject("WebView")).AddComponent<WebViewObject>();
        webView.Init(
            cb: OnURLChanged,
            err: (err) =>
            {
                Debug.LogError("[GoogleWebLogin] WebView Error: " + err);
                loginResult.TrySetResult(null);
            },
            ld: (msg) => Debug.Log("[GoogleWebLogin] WebView Loaded: " + msg)
        );

        webView.SetMargins(0, 120, 0, 120);
        webView.SetVisibility(true);
        webView.LoadURL(authUrl);

        return loginResult.Task;
    }

    private void OnURLChanged(string url)
    {
        Debug.Log("[GoogleWebLogin] URL Changed: " + url);

        // normalize trailing slash
        string normRedirect = redirectUri.TrimEnd('/');

        if (url.StartsWith(normRedirect))
        {
            string idToken = ExtractIdToken(url);

            Debug.Log("[GoogleWebLogin] Extracted idToken: " +
                      (string.IsNullOrEmpty(idToken) ? "NULL" : "OK"));

            loginResult.TrySetResult(idToken);

            if (webView != null)
                Destroy(webView.gameObject);
        }
    }

    private string ExtractIdToken(string url)
    {
        int hashIndex = url.IndexOf('#');
        if (hashIndex < 0) return null;

        string fragment = url.Substring(hashIndex + 1);
        string[] parts = fragment.Split('&');

        foreach (var p in parts)
        {
            if (p.StartsWith("id_token="))
                return System.Uri.UnescapeDataString(p.Substring("id_token=".Length));
        }

        return null;
    }
}
