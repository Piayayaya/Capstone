using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RegisterUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField nameInput;
    public TMP_Text createButtonText;   // drag your "Create Text" TMP here

    [Header("Scenes")]
    public string profileScene = "Profile Scene";
    public string dashboardScene = "Dashboard";

    private enum Mode { CreateNew, LoginExisting }
    private Mode _mode = Mode.CreateNew;

    private async void Start()
    {
        // Always ensure guestId exists
        string deviceKey = UserIdProvider.GetOrCreateGuestId();

        // default state
        SetCreateMode();

        if (DatabaseService.Instance == null) return;

        // 1) check deviceUsers mapping
        string mappedUserId = await DatabaseService.Instance.GetMappedUserForDevice(deviceKey);

        if (!string.IsNullOrEmpty(mappedUserId))
        {
            // 2) verify user still exists in Firebase
            bool remoteExists = await DatabaseService.Instance.UserExists(mappedUserId);

            if (remoteExists)
            {
                // set active user to mapped uid (so Profile/Avatar load correctly)
                UserIdProvider.SetActiveUserId(mappedUserId);

                // load local name
                if (ProfileService.Instance != null)
                    ProfileService.Instance.LoadFromPrefs();

                string localName = ProfileService.Instance != null ? ProfileService.Instance.DisplayName : "";

                // if local empty, fetch remote name and save locally
                if (string.IsNullOrWhiteSpace(localName))
                {
                    string remoteName = await DatabaseService.Instance.GetUserName(mappedUserId);
                    if (!string.IsNullOrWhiteSpace(remoteName) && ProfileService.Instance != null)
                    {
                        ProfileService.Instance.SetName(remoteName);
                        localName = remoteName;
                    }
                }

                SetLoginMode(localName);
                return;
            }
            else
            {
                // ❌ Admin deleted user => clear mapping + local
                await DatabaseService.Instance.ReleaseDevice(deviceKey);

                if (ProfileService.Instance != null)
                    ProfileService.Instance.ClearForUser(mappedUserId);

                if (AvatarService.Instance != null)
                    AvatarService.Instance.ClearForUser(mappedUserId);

                // reset active to guest
                UserIdProvider.MarkGuestLogin();

                SetCreateMode();
                return;
            }
        }

        // no mapped user => fresh device / new user
        UserIdProvider.MarkGuestLogin();
        SetCreateMode();
    }

    private void SetCreateMode()
    {
        _mode = Mode.CreateNew;
        nameInput.interactable = true;
        nameInput.text = "";
        if (createButtonText != null)
            createButtonText.text = "CREATE";
    }

    private void SetLoginMode(string name)
    {
        _mode = Mode.LoginExisting;

        nameInput.text = name;
        nameInput.interactable = false; // avoid changing only here

        if (createButtonText != null)
            createButtonText.text = $"LOGIN AS {name}";
    }

    // Hook this to CREATE button OnClick()
    public async void OnClickCreate()
    {
        if (_mode == Mode.LoginExisting)
        {
            SceneManager.LoadScene(dashboardScene);
            return;
        }

        // ✅ username required
        string rawName = nameInput.text.Trim();
        if (string.IsNullOrEmpty(rawName))
        {
            Debug.LogWarning("[RegisterUI] Username is required.");
            return;
        }

        string userId = UserIdProvider.GetOrCreateGuestId();
        string deviceKey = userId; // we use guestId as deviceKey

        // save local
        if (ProfileService.Instance != null)
            ProfileService.Instance.SetName(rawName);

        // save firebase + claim device
        if (DatabaseService.Instance != null)
        {
            await DatabaseService.Instance.CreateUser(userId, rawName);
            await DatabaseService.Instance.ClaimDevice(deviceKey, userId);
        }

        SceneManager.LoadScene(profileScene);
    }
}
