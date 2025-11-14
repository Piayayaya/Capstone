using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterCardBinder : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public TMP_Text nameText;
    public Button equipButton;
    public TMP_Text equipButtonLabel;
    public Button sellButton;
    public TMP_Text sellButtonLabel;

    [Header("Data (assigned at Bind)")]
    public CharacterDefinition def;

    void OnEnable()
    {
        // re-hook to avoid double registration in Editor play/stop
        equipButton.onClick.RemoveAllListeners();
        sellButton.onClick.RemoveAllListeners();

        equipButton.onClick.AddListener(OnEquipClicked);
        sellButton.onClick.AddListener(OnSellClicked);

        Refresh();
        // Also listen for external changes (equip elsewhere)
        if (CharacterInventory.Instance != null)
        {
            CharacterInventory.Instance.OnEquippedChanged -= Refresh;
            CharacterInventory.Instance.OnEquippedChanged += Refresh;
            CharacterInventory.Instance.OnInventoryChanged -= Refresh;
            CharacterInventory.Instance.OnInventoryChanged += Refresh;
        }
    }

    void OnDisable()
    {
        if (CharacterInventory.Instance != null)
        {
            CharacterInventory.Instance.OnEquippedChanged -= Refresh;
            CharacterInventory.Instance.OnInventoryChanged -= Refresh;
        }
    }

    public void Bind(CharacterDefinition d)
    {
        def = d;
        if (icon) icon.sprite = def.previewSprite;
        if (nameText) nameText.text = def.displayName;
        Refresh();
    }

    void Refresh()
    {
        if (def == null || CharacterInventory.Instance == null) return;

        bool isOwned = CharacterInventory.Instance.IsOwned(def.id);
        gameObject.SetActive(isOwned); // only show if owned (safety)
        if (!isOwned) return;

        bool isEquipped = CharacterInventory.Instance.GetEquipped() == def.id;

        // Equip/Used button
        equipButton.interactable = !isEquipped;
        equipButtonLabel.text = isEquipped ? "USED" : "EQUIP";

        // Sell button (don’t allow selling currently used)
        sellButton.interactable = !isEquipped;
        sellButtonLabel.text = $"Sell {def.GetSellPrice()}";
    }

    void OnEquipClicked()
    {
        if (def == null) return;
        CharacterInventory.Instance.Equip(def.id);
        Refresh();
    }

    void OnSellClicked()
    {
        if (def == null) return;
        var price = def.GetSellPrice();

        // Prompt via tiny popup
        ConfirmSellPopup.Show(
            message: $"Sell {def.displayName} for {price} coins?",
            confirmLabel: $"Sell {price}",
            onConfirm: () =>
            {
                if (CharacterInventory.Instance.Sell(def))
                {
                    // After selling, the card will auto-hide since it’s no longer owned.
                    // Optionally destroy the GO now for instant feedback:
                    Destroy(gameObject);
                }
            });
    }
}
