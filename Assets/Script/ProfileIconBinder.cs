using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ProfileIconBinder : MonoBehaviour
{
    [Tooltip("The Image to update (drag your 'profile' Image here). If empty, will use Image on this GameObject.")]
    public Image target;

    void Awake()
    {
        if (!target) target = GetComponent<Image>();
    }

    IEnumerator Start()
    {
        // wait a frame so AvatarService.Awake runs
        yield return null;

        if (!target)
        {
            Debug.LogError("[ProfileIconBinder] No target Image assigned and none on this GameObject.");
            yield break;
        }

        if (AvatarService.Instance == null)
        {
            Debug.Log("[ProfileIconBinder] Waiting for AvatarService...");
            while (AvatarService.Instance == null) yield return null;
        }

        // initial set
        target.sprite = AvatarService.Instance.CurrentAvatar;
        Debug.Log("[ProfileIconBinder] Initial sprite set to: " + (target.sprite ? target.sprite.name : "null"));

        // subscribe to live changes
        AvatarService.Instance.OnAvatarChanged += HandleChanged;
    }

    void OnDisable()
    {
        if (AvatarService.Instance != null)
            AvatarService.Instance.OnAvatarChanged -= HandleChanged;
    }

    void HandleChanged(Sprite s)
    {
        if (!target) return;
        target.sprite = s;
        Debug.Log("[ProfileIconBinder] Received avatar change -> " + (s ? s.name : "null"));
    }

    // Handy button in Inspector (during Play) to force a refresh
    [ContextMenu("Refresh Now")]
    void RefreshNow()
    {
        if (AvatarService.Instance && target)
        {
            target.sprite = AvatarService.Instance.CurrentAvatar;
            Debug.Log("[ProfileIconBinder] Manual refresh -> " + (target.sprite ? target.sprite.name : "null"));
        }
    }
}
