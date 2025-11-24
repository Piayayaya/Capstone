using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EditProfileController : MonoBehaviour
{
    [Header("UI References")]
    public Image avatarImage;            // Profile (Image)
    public TMP_Text uidText;             // UID TEXT (TMP_Text)
    public TMP_Text currentUsernameText; // Current Username Text (TMP_Text)
    public TMP_InputField newUsernameIF; // New Username Input Field (TMP_InputField)

    [Header("Scenes")]
    public string backScene = "View Profile";

    private async void Start()
    {
        // center new username input + placeholder
        CenterInput(newUsernameIF);

        await RefreshUI();
    }

    private void CenterInput(TMP_InputField input)
    {
        if (input == null) return;

        if (input.textComponent != null)
            input.textComponent.alignment = TextAlignmentOptions.Center;

        if (input.placeholder is TMP_Text ph)
            ph.alignment = TextAlignmentOptions.Center;
    }

    public async Task RefreshUI()
    {
        string uid = UserIdProvider.ActiveUserId;

        // load local caches
        if (ProfileService.Instance != null)
            ProfileService.Instance.LoadFromPrefs();

        if (AvatarService.Instance != null)
            AvatarService.Instance.LoadFromPrefs();

        // avatar
        if (avatarImage != null && AvatarService.Instance != null)
            avatarImage.sprite = AvatarService.Instance.CurrentAvatar;

        // UID text
        if (uidText != null)
            uidText.text = uid;

        // Current username: prefer local then Firebase
        string name = ProfileService.Instance != null ? ProfileService.Instance.DisplayName : "";

        if (string.IsNullOrWhiteSpace(name) && DatabaseService.Instance != null)
        {
            string remoteName = await DatabaseService.Instance.GetUserName(uid);
            if (!string.IsNullOrWhiteSpace(remoteName))
            {
                name = remoteName;
                if (ProfileService.Instance != null)
                    ProfileService.Instance.SetName(remoteName);
            }
        }

        // show current username (LOCKED because it's TMP_Text)
        if (currentUsernameText != null)
            currentUsernameText.text = name;

        // clear new username input
        if (newUsernameIF != null)
            newUsernameIF.text = "";
    }

    // Hook to SAVE button OnClick()
    public async void OnClickSave()
    {
        if (newUsernameIF == null) return;

        string newName = newUsernameIF.text.Trim();
        if (string.IsNullOrEmpty(newName))
        {
            Debug.LogWarning("[EditProfileController] New username is required.");
            return;
        }

        string uid = UserIdProvider.ActiveUserId;

        // 1) local update
        if (ProfileService.Instance != null)
            ProfileService.Instance.SetName(newName);

        // 2) firebase update name only
        if (DatabaseService.Instance != null)
            await DatabaseService.Instance.UpdateUserName(uid, newName);

        // 3) update current text on screen
        if (currentUsernameText != null)
            currentUsernameText.text = newName;

        // 4) go back
        if (!string.IsNullOrEmpty(backScene))
            SceneManager.LoadScene(backScene);
    }

    // Hook to arrow/back button OnClick()
    public void OnClickBack()
    {
        if (!string.IsNullOrEmpty(backScene))
            SceneManager.LoadScene(backScene);
    }
}
