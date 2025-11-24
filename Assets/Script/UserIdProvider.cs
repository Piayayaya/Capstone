using System;
using UnityEngine;

public static class UserIdProvider
{
    // Stable per-device guest id (saved locally)
    private const string GuestIdKey = "guestId_v1";

    // Current "active" user (guest or google uid)
    private const string ActiveUserIdKey = "activeUserId_v1";

    // "guest" or "google"
    private const string LoginTypeKey = "loginType_v1";

    public static string GetOrCreateGuestId()
    {
        if (!PlayerPrefs.HasKey(GuestIdKey) || string.IsNullOrEmpty(PlayerPrefs.GetString(GuestIdKey)))
        {
            string id = Guid.NewGuid().ToString();
            PlayerPrefs.SetString(GuestIdKey, id);
            PlayerPrefs.Save();
        }
        return PlayerPrefs.GetString(GuestIdKey);
    }

    public static string ActiveUserId
    {
        get
        {
            // If not set yet, default to guest
            if (!PlayerPrefs.HasKey(ActiveUserIdKey) || string.IsNullOrEmpty(PlayerPrefs.GetString(ActiveUserIdKey)))
                SetActiveUserId(GetOrCreateGuestId());

            return PlayerPrefs.GetString(ActiveUserIdKey);
        }
    }

    public static void SetActiveUserId(string uid)
    {
        if (string.IsNullOrEmpty(uid))
            uid = GetOrCreateGuestId();

        PlayerPrefs.SetString(ActiveUserIdKey, uid);
        PlayerPrefs.Save();
    }

    public static void MarkGuestLogin()
    {
        SetActiveUserId(GetOrCreateGuestId());
        PlayerPrefs.SetString(LoginTypeKey, "guest");
        PlayerPrefs.Save();
    }

    // ✅ this fixes your overload error
    public static void MarkGoogleLogin(string googleUid)
    {
        SetActiveUserId(googleUid);
        PlayerPrefs.SetString(LoginTypeKey, "google");
        PlayerPrefs.Save();
    }

    public static bool IsGoogleLogin()
        => PlayerPrefs.GetString(LoginTypeKey, "guest") == "google";
}