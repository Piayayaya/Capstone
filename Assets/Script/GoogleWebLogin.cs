using System;
using System.Collections;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoogleWebLogin : MonoBehaviour
{
    [Header("Scenes After Login")]
    public string dashboardScene = "Dashboard";
    public string profileScene = "Profile Scene";
    public bool goToDashboardAfterLogin = true;

    [Header("WEB OAuth Client ID (from Google Cloud)")]
    [SerializeField]
    private string clientId =
        "397904065396-71kqualnlvfa6sh17qf7h1hckjf780l0.apps.googleusercontent.com";

    [Header("Redirect URI (must match Web client)")]
    [SerializeField]
    private string redirectUri =
        "https://brainyme-firebase.web.app/oauth2redirect.html";

    // ---------------- BUTTON CLICK ----------------

    public void OnClickGoogleLogin()
    {
        string scope = "openid email profile";

        string authUrl =
            "https://accounts.google.com/o/oauth2/v2/auth" +
            "?client_id=" + clientId +
            "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
            "&response_type=id_token" +
            "&scope=" + Uri.EscapeDataString(scope) +
            "&nonce=unity_nonce" +
            "&prompt=select_account";

        Debug.Log("[GoogleWebLogin] Opening browser: " + authUrl);
        Application.OpenURL(authUrl);   // system browser
    }

    // ------ Called from GoogleDeepLinkReceiver once we’re back in the app ------

    public void OnGotIdTokenFromDeepLink(string idToken)
    {
        if (string.IsNullOrEmpty(idToken))
        {
            Debug.LogError("[GoogleWebLogin] idToken from deep link is null/empty");
            return;
        }

        Debug.Log("[GoogleWebLogin] Got id_token from deep link. len=" + idToken.Length);
        StartCoroutine(LoginFlow(idToken));
    }

    private IEnumerator LoginFlow(string idToken)
    {
        // 1) Sign into Firebase
        var signInTask = SignInWithFirebase(idToken);
        while (!signInTask.IsCompleted)
            yield return null;

        if (signInTask.Exception != null)
        {
            Debug.LogError("[GoogleWebLogin] Firebase sign-in error: " + signInTask.Exception);
            yield break;
        }

        FirebaseUser fUser = signInTask.Result;
        Debug.Log("[GoogleWebLogin] Firebase user signed in: " +
                  fUser.UserId + " / " + fUser.Email);

        // 2) Finalize login
        var postTask = FinalizeLogin(fUser);
        while (!postTask.IsCompleted)
            yield return null;

        if (postTask.Exception != null)
        {
            Debug.LogError("[GoogleWebLogin] FinalizeLogin error: " + postTask.Exception);
            yield break;
        }

        Debug.Log("[GoogleWebLogin] Google login flow complete.");
    }

    private async Task<FirebaseUser> SignInWithFirebase(string idToken)
    {
        var cred = GoogleAuthProvider.GetCredential(idToken, null);
        var auth = FirebaseAuth.DefaultInstance;
        return await auth.SignInWithCredentialAsync(cred);
    }

    private async Task FinalizeLogin(FirebaseUser fUser)
    {
        string uid = fUser.UserId;
        string displayName = string.IsNullOrWhiteSpace(fUser.DisplayName)
            ? "PLAYER"
            : fUser.DisplayName.Trim();

        UserIdProvider.SetActiveUserId(uid);

        if (ProfileService.Instance != null)
            ProfileService.Instance.SetName(displayName);

        if (DatabaseService.Instance != null)
            await DatabaseService.Instance.CreateUser(uid, displayName);

        if (CoinService.Instance != null)
            await CoinService.Instance.SetPlayer(uid);

        string targetScene = goToDashboardAfterLogin ? dashboardScene : profileScene;
        if (!string.IsNullOrEmpty(targetScene))
        {
            Debug.Log("[GoogleWebLogin] Loading scene: " + targetScene);
            SceneManager.LoadScene(targetScene);
        }
    }
}
