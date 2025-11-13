using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ProfileAvatarBinder : MonoBehaviour
{
    [Header("Target Image (avatar in this scene)")]
    public Image target;

    [Tooltip("Update automatically when avatar changes")]
    public bool listenToChanges = true;

    void Awake()
    {
        if (!target)
            target = GetComponent<Image>();
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

    void HandleAvatarChanged(Sprite s)
    {
        ApplyAvatar();
    }

    void ApplyAvatar()
    {
        if (!target) return;
        if (AvatarService.Instance == null) return;

        // use the property you already have
        var sprite = AvatarService.Instance.CurrentAvatar;
        target.sprite = sprite;
        target.preserveAspect = true;
        target.enabled = (sprite != null);
    }
}
