// AutoLoginRouter.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoLoginRouter : MonoBehaviour
{
    [Header("Scenes")]
    public string createAccountScene = "CreateAccount";
    public string profileScene = "Profile Scene";
    public string dashboardScene = "Dashboard";

    static bool hasRouted = false;

    void Start()
    {
        if (hasRouted) return;
        hasRouted = true;

        // Make sure guest id exists even for new devices
        UserIdProvider.GetOrCreateGuestId();

        if (ProfileService.Instance != null) ProfileService.Instance.LoadFromPrefs();
        if (AvatarService.Instance != null) AvatarService.Instance.LoadFromPrefs();

        bool hasName = ProfileService.Instance != null && ProfileService.Instance.HasName();
        bool hasAvatar = AvatarService.Instance != null && AvatarService.Instance.HasAvatar();

        string current = SceneManager.GetActiveScene().name;

        if (!hasName)
        {
            if (current != createAccountScene)
                SceneManager.LoadScene(createAccountScene);
            return;
        }

        if (!hasAvatar)
        {
            if (current != profileScene)
                SceneManager.LoadScene(profileScene);
            return;
        }

        if (current != dashboardScene)
            SceneManager.LoadScene(dashboardScene);
    }
}
