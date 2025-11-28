using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCardSelectUI : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("Unique id of this character (same id used in your data).")]
    public string characterId;

    [Header("UI")]
    public Button selectButton;              // button on the card (USE / USING)
    public TMP_Text selectButtonLabel;       // label inside the button
    public GameObject equippedTag;           // e.g. "USING" badge or glow (optional)

    private void OnEnable()
    {
        if (selectButton != null)
            selectButton.onClick.AddListener(OnSelectClicked);

        if (CharacterSelectionService.Instance != null)
        {
            CharacterSelectionService.Instance.OnSelectionChanged += RefreshState;
            // Initial refresh
            RefreshState(CharacterSelectionService.Instance.CurrentId);
        }
    }

    private void OnDisable()
    {
        if (selectButton != null)
            selectButton.onClick.RemoveListener(OnSelectClicked);

        if (CharacterSelectionService.Instance != null)
            CharacterSelectionService.Instance.OnSelectionChanged -= RefreshState;
    }

    private void OnSelectClicked()
    {
        if (CharacterSelectionService.Instance == null)
            return;

        CharacterSelectionService.Instance.SetSelection(characterId);
    }

    private void RefreshState(string currentId)
    {
        bool isEquipped = string.Equals(currentId, characterId, StringComparison.Ordinal);

        // Button label & interactable
        if (selectButtonLabel != null)
            selectButtonLabel.text = isEquipped ? "USING" : "USE";

        if (selectButton != null)
            selectButton.interactable = !isEquipped; // disable if this is already equipped

        // Optional badge / highlight
        if (equippedTag != null)
            equippedTag.SetActive(isEquipped);
    }
}
