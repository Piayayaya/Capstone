// ProfileAvatarBinder.cs
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ProfileAvatarBinder : MonoBehaviour
{
    public Image target;
    public bool listenToChanges = true;

    void Awake()
    {
        if (!target) target = GetComponent<Image>();
    }

    void OnEnable()
    {
        ApplyAvatar();

        if (listenToChanges && AvatarService.Instance != null)
            AvatarService.Instance.OnAvatarChanged += HandleAvatarChanged;
    }

    void OnDisable()
    {
        if (listenToChanges && AvatarService.Instance != null)
            AvatarService.Instance.OnAvatarChanged -= HandleAvatarChanged;
    }

    void HandleAvatarChanged(Sprite s) => ApplyAvatar();

    void ApplyAvatar()
    {
        if (!target || AvatarService.Instance == null) return;

        var sprite = AvatarService.Instance.CurrentAvatar;
        target.sprite = sprite;
        target.preserveAspect = true;
        target.enabled = (sprite != null);
    }
}
