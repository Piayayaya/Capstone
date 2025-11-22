// ProfileService.cs
using UnityEngine;

public class ProfileService : MonoBehaviour
{
    public static ProfileService Instance { get; private set; }

    [Header("Prefs")]
    [SerializeField] string prefsKey = "profile_name";
    [SerializeField] string defaultName = "PLAYER";

    public string DisplayName { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadFromPrefs();
    }

    string KeyFor(string uid)
        => string.IsNullOrEmpty(uid) ? prefsKey : $"{prefsKey}_{uid}";

    public void LoadFromPrefs()
    {
        string uid = UserIdProvider.ActiveUserId;
        string perUserKey = KeyFor(uid);

        // 1) per-user key
        if (PlayerPrefs.HasKey(perUserKey))
            DisplayName = PlayerPrefs.GetString(perUserKey, defaultName).Trim();
        // 2) backward compat global key
        else if (PlayerPrefs.HasKey(prefsKey))
            DisplayName = PlayerPrefs.GetString(prefsKey, defaultName).Trim();
        else
            DisplayName = "";

        if (string.IsNullOrWhiteSpace(DisplayName))
            DisplayName = "";
    }

    public bool HasName()
        => !string.IsNullOrWhiteSpace(DisplayName);

    public void SetName(string name)
    {
        string uid = UserIdProvider.ActiveUserId;
        string perUserKey = KeyFor(uid);

        DisplayName = string.IsNullOrWhiteSpace(name) ? defaultName : name.Trim();

        PlayerPrefs.SetString(perUserKey, DisplayName);
        PlayerPrefs.Save();
    }
}
