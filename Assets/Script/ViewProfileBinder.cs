using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ViewProfileBinder : MonoBehaviour
{
    [Header("UI References (drag in Inspector)")]
    public Image avatarImage;       // your "profile avatar" Image
    public TMP_Text usernameText;   // your "Username TMP"

    void Start()
    {
        RefreshUI();

        // live update if avatar changes while this scene is open
        if (AvatarService.Instance != null)
            AvatarService.Instance.OnAvatarChanged += OnAvatarChanged;
    }

    void OnDestroy()
    {
        if (AvatarService.Instance != null)
            AvatarService.Instance.OnAvatarChanged -= OnAvatarChanged;
    }

    private void OnAvatarChanged(Sprite s)
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        // Ensure services are loaded for active user
        if (ProfileService.Instance != null)
            ProfileService.Instance.LoadFromPrefs();

        if (AvatarService.Instance != null)
            AvatarService.Instance.LoadFromPrefs();

        // Set avatar
        if (avatarImage != null && AvatarService.Instance != null)
            avatarImage.sprite = AvatarService.Instance.CurrentAvatar;

        // Set username
        if (usernameText != null && ProfileService.Instance != null)
            usernameText.text = ProfileService.Instance.DisplayName;
    }
}
