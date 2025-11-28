using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CharacterAvatarFromInventory : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Image where the current character icon will be shown.")]
    public Image avatarImage;

    [Header("Characters this scene supports")]
    [Tooltip("Drag ALL CharacterDefinition assets here (or at least the ones you want usable in this scene).")]
    public CharacterDefinition[] characterDefs;

    private void Awake()
    {
        if (avatarImage == null)
            avatarImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (CharacterInventory.Instance != null)
        {
            CharacterInventory.Instance.OnEquippedChanged += RefreshAvatar;
            RefreshAvatar(); // initial update
        }
        else
        {
            Debug.LogWarning("CharacterAvatarFromInventory: No CharacterInventory.Instance found. " +
                             "Make sure there is a CharacterInventory GameObject in a bootstrap scene.");
        }
    }

    private void OnDisable()
    {
        if (CharacterInventory.Instance != null)
            CharacterInventory.Instance.OnEquippedChanged -= RefreshAvatar;
    }

    private void RefreshAvatar()
    {
        if (avatarImage == null || CharacterInventory.Instance == null) return;
        if (characterDefs == null || characterDefs.Length == 0) return;

        string equippedId = CharacterInventory.Instance.GetEquipped();

        // Try to find the matching CharacterDefinition by id
        CharacterDefinition def = null;

        if (!string.IsNullOrEmpty(equippedId))
        {
            def = characterDefs.FirstOrDefault(d => d != null && d.id == equippedId);
        }

        // Fallback: first definition that has a sprite
        if (def == null)
            def = characterDefs.FirstOrDefault(d => d != null && d.previewSprite != null);

        if (def != null && def.previewSprite != null)
            avatarImage.sprite = def.previewSprite;
    }
}
