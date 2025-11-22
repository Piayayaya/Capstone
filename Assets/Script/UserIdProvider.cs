// UserIdProvider.cs
using UnityEngine;
#if FIREBASE_AUTH
using Firebase.Auth;
#endif

public static class UserIdProvider
{
    const string GuestIdKey = "guest_userId";
    const string LoginTypeKey = "login_type"; // "guest" or "google"

    /// Guest id per device (created once)
    public static string GetOrCreateGuestId()
    {
        if (PlayerPrefs.HasKey(GuestIdKey))
            return PlayerPrefs.GetString(GuestIdKey);

        string id = System.Guid.NewGuid().ToString();
        PlayerPrefs.SetString(GuestIdKey, id);
        PlayerPrefs.SetString(LoginTypeKey, "guest");
        PlayerPrefs.Save();
        return id;
    }

    /// Active user id: google uid if logged in, else guest id
    public static string ActiveUserId
    {
        get
        {
#if FIREBASE_AUTH
            var auth = FirebaseAuth.DefaultInstance;
            if (auth != null && auth.CurrentUser != null)
                return auth.CurrentUser.UserId;
#endif
            return GetOrCreateGuestId();
        }
    }

    public static bool IsGoogleLogin()
        => PlayerPrefs.GetString(LoginTypeKey, "guest") == "google";

    public static void MarkGoogleLogin()
    {
        PlayerPrefs.SetString(LoginTypeKey, "google");
        PlayerPrefs.Save();
    }

    public static void MarkGuestLogin()
    {
        PlayerPrefs.SetString(LoginTypeKey, "guest");
        PlayerPrefs.Save();
    }
}
