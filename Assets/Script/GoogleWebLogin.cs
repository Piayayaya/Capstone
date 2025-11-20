using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class GoogleWebLogin : MonoBehaviour
{
    private WebViewObject webView;
    private TaskCompletionSource<string> loginResult;

    private const string CLIENT_ID =
        "397904065396-71kqualnlvfa6sh17qf7h1hckjf780l0.apps.googleusercontent.com";

    private string GOOGLE_AUTH_URL =
        "https://accounts.google.com/o/oauth2/v2/auth" +
        "?client_id=" + CLIENT_ID +
        "&redirect_uri=https://localhost" +
        "&response_type=id_token" +
        "&scope=profile%20email" +
        "&nonce=unity_nonce";

    // This method appears in the Button
    public void StartGoogleLogin()
    {
        Debug.Log("Google Login Started");
        StartCoroutine(LoginFlow());
    }

    private IEnumerator LoginFlow()
    {
        var tokenTask = GetIdToken();

        while (!tokenTask.IsCompleted)
            yield return null;

        string idToken = tokenTask.Result;

        Debug.Log("Google Login Success. Token: " + idToken);

        // TODO: Validate token with Firebase
        // TODO: Load profile scene
    }

    public Task<string> GetIdToken()
    {
        loginResult = new TaskCompletionSource<string>();

        webView = (new GameObject("WebView")).AddComponent<WebViewObject>();
        webView.Init(
            cb: url => OnURLChanged(url),
            err: err => Debug.Log("WebView Error: " + err),
            ld: msg => Debug.Log("WebView Loaded")
        );

        webView.SetMargins(0, 200, 0, 200);
        webView.SetVisibility(true);

        webView.LoadURL(GOOGLE_AUTH_URL);

        return loginResult.Task;
    }

    private void OnURLChanged(string url)
    {
        Debug.Log("URL: " + url);

        if (url.StartsWith("https://localhost"))
        {
            string idToken = ExtractIdToken(url);
            loginResult.TrySetResult(idToken);

            Destroy(webView.gameObject);
        }
    }

    private string ExtractIdToken(string url)
    {
        if (!url.Contains("#")) return null;

        string[] parts = url.Split('#');
        string[] fragment = parts[1].Split('&');

        foreach (var item in fragment)
        {
            if (item.StartsWith("id_token="))
                return item.Substring("id_token=".Length);
        }

        return null;
    }
}
