using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Firebase.Extensions;          // for ContinueWithOnMainThread
using Google;                       // Google Sign-In for Unity

public class GoogleNativeLogin : MonoBehaviour
{
    [Header("Scenes After Login")]
    public string dashboardScene = "Dashboard";
    public string profileScene = "Profile Scene";
    public bool goToDashboardAfterLogin = true;

    [Header("Sign-in Behaviour")]
    [Tooltip("If true, always show the Google account picker instead of auto-using last account.")]
    public bool forceAccountPicker = false;

    [Header("WEB OAuth Client ID (from Google Cloud / Firebase)")]
    [Tooltip("The Web client ID, NOT the Android client ID.")]
    public string webClientId =
        "397904065396-71kqualnlvfa6sh17qf7h1hckjf780l0.apps.googleusercontent.com";

    private FirebaseAuth auth;

    private void Awake()
    {
        // ❌ DO NOT touch Firebase here anymore.
        Debug.Log("[GoogleNativeLogin] Awake – will init FirebaseAuth after Google sign-in.");
    }

    // Hook this to your "Log in with Google" button
    public void OnClickGoogleLogin()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Debug.Log("[GoogleNativeLogin] OnClickGoogleLogin() on ANDROID build");

        var config = new GoogleSignInConfiguration
        {
            WebClientId   = webClientId,
            RequestIdToken = true,
            RequestEmail   = true
        };

        GoogleSignIn.Configuration = config;
        GoogleSignIn.Configuration.UseGameSignIn = false;

        if (forceAccountPicker)
        {
            Debug.Log("[GoogleNativeLogin] Forcing account picker (SignOut before SignIn).");
            GoogleSignIn.DefaultInstance.SignOut();
        }

        Debug.Log("[GoogleNativeLogin] Starting Google sign-in...");
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleSignInFinished);
#else
        Debug.LogWarning("[GoogleNativeLogin] Google sign-in only works on an Android device build.");
#endif
    }

    private void OnGoogleSignInFinished(System.Threading.Tasks.Task<GoogleSignInUser> task)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (task.IsFaulted)
        {
            Debug.LogError("[GoogleNativeLogin] Google sign-in failed: " + task.Exception);
            return;
        }
        if (task.IsCanceled)
        {
            Debug.LogWarning("[GoogleNativeLogin] Google sign-in cancelled.");
            return;
        }

        GoogleSignInUser gUser = task.Result;
        string idToken = gUser.IdToken;

        Debug.Log("[GoogleNativeLogin] Google user: " + gUser.Email);

        // ✅ NOW (after Firebase has had time to init) we grab FirebaseAuth
        if (auth == null)
        {
            try
            {
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("[GoogleNativeLogin] FirebaseAuth.DefaultInstance acquired.");
            }
            catch (System.SystemException e)
            {
                Debug.LogError("[GoogleNativeLogin] FAILED to get FirebaseAuth after Google sign-in: " + e);
                return;   // can't continue if auth is missing
            }
        }

        var credential = GoogleAuthProvider.GetCredential(idToken, null);

        // Run on Unity main thread so SceneManager + other Unity APIs are safe
        auth.SignInWithCredentialAsync(credential)
            .ContinueWithOnMainThread(OnFirebaseAuthFinished);
#else
        Debug.LogWarning("[GoogleNativeLogin] OnGoogleSignInFinished called in non-Android build.");
#endif
    }

    // Runs on main thread because of ContinueWithOnMainThread
    private void OnFirebaseAuthFinished(System.Threading.Tasks.Task<FirebaseUser> task)
    {
        if (task.IsFaulted)
        {
            Debug.LogError("[GoogleNativeLogin] Firebase sign-in error: " + task.Exception);
            return;
        }

        FirebaseUser fUser = task.Result;
        Debug.Log("[GoogleNativeLogin] Firebase user signed in: " +
                  fUser.UserId + " / " + fUser.Email);

        // Now do our own post-login logic
        FinalizeLogin(fUser);
    }

    /// <summary>
    /// Final post-login steps: set active user, fire off DB saves, bind CoinService,
    /// then immediately load the next scene.
    /// </summary>
    private void FinalizeLogin(FirebaseUser fUser)
    {
        string uid = fUser.UserId;
        string displayName = string.IsNullOrWhiteSpace(fUser.DisplayName)
            ? "PLAYER"
            : fUser.DisplayName.Trim();

        Debug.Log("[GoogleNativeLogin] FinalizeLogin for UID=" + uid);

        // 1) Set active user
        UserIdProvider.SetActiveUserId(uid);
        Debug.Log("[GoogleNativeLogin] Active user set.");

        // 2) Local profile name
        if (ProfileService.Instance != null)
        {
            ProfileService.Instance.SetName(displayName);
            Debug.Log("[GoogleNativeLogin] Profile name set: " + displayName);
        }
        else
        {
            Debug.LogWarning("[GoogleNativeLogin] ProfileService.Instance is null.");
        }

        // 3) Create / update user record in Realtime DB (Google user) - fire & forget
        if (DatabaseService.Instance != null)
        {
            Task dbTask = DatabaseService.Instance.CreateOrUpdateGoogleUser(uid, displayName, fUser.Email);
            dbTask.ContinueWithOnMainThread(t =>
            {
                if (t.IsFaulted)
                    Debug.LogError("[GoogleNativeLogin] Error saving user to Firebase DB: " + t.Exception);
                else
                    Debug.Log("[GoogleNativeLogin] User saved/updated in Firebase DB.");
            });
        }
        else
        {
            Debug.LogWarning("[GoogleNativeLogin] DatabaseService.Instance is null.");
        }

        // 4) Coins / progress for this user - fire & forget
        if (CoinService.Instance != null)
        {
            Task coinTask = CoinService.Instance.SetPlayer(uid);
            coinTask.ContinueWithOnMainThread(t =>
            {
                if (t.IsFaulted)
                    Debug.LogError("[GoogleNativeLogin] Error binding CoinService to player: " + t.Exception);
                else
                    Debug.Log("[GoogleNativeLogin] CoinService bound to player " + uid);
            });
        }
        else
        {
            Debug.LogWarning("[GoogleNativeLogin] CoinService.Instance is null.");
        }

        // 5) Load next scene (this will always run, not blocked by network)
        string targetScene = goToDashboardAfterLogin ? dashboardScene : profileScene;

        if (!string.IsNullOrEmpty(targetScene))
        {
            Debug.Log("[GoogleNativeLogin] Loading scene: " + targetScene);
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            Debug.LogWarning("[GoogleNativeLogin] targetScene is empty, not loading.");
        }
    }
}
