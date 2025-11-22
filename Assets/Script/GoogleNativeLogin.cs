using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

using Firebase.Auth;
using Firebase.Extensions;
using Google;

public class GoogleNativeLogin : MonoBehaviour
{
    [Header("Google OAuth (WEB Client ID)")]
    [SerializeField] private string webClientId;  // WEB client id from Google Cloud / Firebase

    [Header("Next Scene")]
    [SerializeField] private string nextScene = "Profile Scene";

    [Header("Optional")]
    [SerializeField] private bool forceAccountPicker = true;
    [SerializeField] private bool autoWriteUserToRTDB = true; // turn off if you don't want auto-write

    private GoogleSignInConfiguration config;
    private bool signingIn;

    void Awake()
    {
        if (string.IsNullOrWhiteSpace(webClientId))
        {
            Debug.LogWarning("[GoogleNativeLogin] webClientId is EMPTY. Paste your WEB client ID in Inspector.");
        }

        config = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestEmail = true,
            RequestIdToken = true,
            UseGameSignIn = false
        };

        Debug.Log("[GoogleNativeLogin] Config prepared. WebClientId=" + webClientId);
    }

    public void SignInWithGoogle()
    {
        Debug.Log("[GoogleNativeLogin] Button pressed!");

#if UNITY_ANDROID && !UNITY_EDITOR
        if (signingIn)
        {
            Debug.Log("[GoogleNativeLogin] Already signing in.");
            return;
        }

        signingIn = true;

        try
        {
            GoogleSignIn.Configuration = config;

            if (forceAccountPicker)
                GoogleSignIn.DefaultInstance.SignOut();

            Debug.Log("[GoogleNativeLogin] Starting Google Sign-In...");
            GoogleSignIn.DefaultInstance.SignIn()
                .ContinueWithOnMainThread(OnGoogleSignedIn);
        }
        catch (Exception e)
        {
            signingIn = false;
            Debug.LogError("[GoogleNativeLogin] SignIn exception: " + e);
        }
#else
        Debug.LogWarning("[GoogleNativeLogin] Google Sign-In works only on Android build.");
#endif
    }

    private void OnGoogleSignedIn(Task<GoogleSignInUser> task)
    {
        signingIn = false;

        if (task.IsCanceled)
        {
            Debug.LogWarning("[GoogleNativeLogin] Google Sign-In canceled.");
            return;
        }

        if (task.IsFaulted)
        {
            Debug.LogError("[GoogleNativeLogin] Google Sign-In failed: " + task.Exception);
            return;
        }

        var gUser = task.Result;
        Debug.Log("[GoogleNativeLogin] Google signed in: " + gUser.Email);
        Debug.Log("[GoogleNativeLogin] ID Token length: " + (gUser.IdToken?.Length ?? 0));

        // Exchange Google token for Firebase credential
        var cred = GoogleAuthProvider.GetCredential(gUser.IdToken, null);

        FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(cred)
            .ContinueWithOnMainThread(async authTask =>
            {
                if (authTask.IsCanceled)
                {
                    Debug.LogWarning("[GoogleNativeLogin] Firebase sign-in canceled.");
                    return;
                }

                if (authTask.IsFaulted)
                {
                    Debug.LogError("[GoogleNativeLogin] Firebase sign-in failed: " + authTask.Exception);
                    return;
                }

                var fUser = authTask.Result;
                Debug.Log("[GoogleNativeLogin] Firebase signed in UID=" + fUser.UserId);

                // ✅ compute display name once
                string displayName = string.IsNullOrEmpty(gUser.DisplayName) ? "PLAYER" : gUser.DisplayName;

                // ✅ NEW: mark login type + save local per-user name
                UserIdProvider.MarkGoogleLogin();
                if (ProfileService.Instance != null)
                    ProfileService.Instance.SetName(displayName);

                // Optional write to RTDB
                if (autoWriteUserToRTDB && DatabaseService.Instance != null)
                {
                    await DatabaseService.Instance.CreateOrUpdateGoogleUser(
                        fUser.UserId,
                        displayName,
                        gUser.Email
                    );
                }

                SceneManager.LoadScene(nextScene);
            });
    }
}
