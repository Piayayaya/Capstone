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
    [SerializeField] private string webClientId;

    [Header("Next Scene")]
    [SerializeField] private string nextScene = "Profile Scene";

    [Header("Optional")]
    [SerializeField] private bool forceAccountPicker = true;
    [SerializeField] private bool autoWriteUserToRTDB = true;

    private GoogleSignInConfiguration config;
    private bool signingIn;

    void Awake()
    {
        if (string.IsNullOrWhiteSpace(webClientId))
            Debug.LogWarning("[GoogleNativeLogin] webClientId is EMPTY. Paste your WEB client ID in Inspector.");

        config = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestEmail = true,
            RequestIdToken = true,
            UseGameSignIn = false
        };
    }

    public void SignInWithGoogle()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (signingIn) return;
        signingIn = true;

        try
        {
            GoogleSignIn.Configuration = config;

            if (forceAccountPicker)
                GoogleSignIn.DefaultInstance.SignOut();

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

        if (task.IsCanceled || task.IsFaulted)
        {
            Debug.LogError("[GoogleNativeLogin] Google Sign-In failed: " + task.Exception);
            return;
        }

        var gUser = task.Result;
        var cred = GoogleAuthProvider.GetCredential(gUser.IdToken, null);

        FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(cred)
            .ContinueWithOnMainThread(async (Task<FirebaseUser> authTask) =>
            {
                if (authTask.IsCanceled || authTask.IsFaulted)
                {
                    Debug.LogError("[GoogleNativeLogin] Firebase sign-in failed: " + authTask.Exception);
                    return;
                }

                FirebaseUser fUser = authTask.Result;
                string displayName = string.IsNullOrEmpty(gUser.DisplayName) ? "PLAYER" : gUser.DisplayName.Trim();

                // ✅ track google login + active uid
                UserIdProvider.MarkGoogleLogin(fUser.UserId);

                if (ProfileService.Instance != null)
                    ProfileService.Instance.SetName(displayName);

                if (autoWriteUserToRTDB && DatabaseService.Instance != null)
                {
                    await DatabaseService.Instance.CreateOrUpdateGoogleUser(
                        fUser.UserId,
                        displayName,
                        gUser.Email
                    );

                    // also map device -> this google user
                    string deviceKey = UserIdProvider.GetOrCreateGuestId();
                    await DatabaseService.Instance.ClaimDevice(deviceKey, fUser.UserId);
                }

                // ✅ connect CoinService to this Google player
                if (CoinService.Instance != null)
                {
                    await CoinService.Instance.SetPlayer(fUser.UserId);
                }

                SceneManager.LoadScene(nextScene);
            });
    }
}
