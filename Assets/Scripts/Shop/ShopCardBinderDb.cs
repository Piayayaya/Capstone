using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCardBinderDb : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("RefId from SQLite / Firebase, e.g. 'char_hootie' or 'coins_100'.")]
    public string itemId;

    [Header("UI (only what you have)")]
    public Image icon;
    public TMP_Text title;
    public TMP_Text priceText;

    [Header("Optional")]
    public GameObject coinIcon;
    public TMP_Text footerPurchased;
    public Button button;

    [SerializeField] private ConfirmPanel confirmPanel; // assign in inspector or auto-find

    private LocalShopItem _row;

    // ----------------- LIFECYCLE -----------------

    private void OnEnable()
    {
        Refresh();

        // Auto-refresh whenever inventory changes (e.g., after buying / selling)
        if (CharacterInventory.Instance != null)
            CharacterInventory.Instance.OnInventoryChanged += OnInventoryChanged;
    }

    private void OnDisable()
    {
        if (CharacterInventory.Instance != null)
            CharacterInventory.Instance.OnInventoryChanged -= OnInventoryChanged;
    }

    private void OnInventoryChanged()
    {
        // inventory changed elsewhere -> update purchased state
        Refresh();
    }

    // ----------------- MAIN BIND -----------------

    public void Refresh()
    {
        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogError($"{name}: itemId is empty for ShopCardBinderDb.");
            return;
        }

        _row = ShopDb.GetByRefId(itemId);
        if (_row == null)
        {
            Debug.LogError($"{name}: ShopDb has no row for refId '{itemId}'.");
            return;
        }

        // Texts from DB
        if (title) title.text = _row.ItemName ?? _row.RefId;
        if (priceText) priceText.text = FormatPrice(_row);

        if (coinIcon) coinIcon.SetActive(_row.PriceCoins > 0);

        // IMPORTANT: do NOT touch icon.sprite here.
        // Whatever sprite is assigned on the prefab stays as the card's icon.

        // -------- Purchased state (case-insensitive + CharacterInventory) --------
        bool purchased = false;

        // Normalize type to avoid "Character" vs "character" issues
        string type = (_row.ItemType ?? "").ToLowerInvariant();

        if (type == "character")
        {
            // Prefer new CharacterInventory
            if (CharacterInventory.Instance != null)
            {
                purchased = CharacterInventory.Instance.IsOwned(_row.RefId);
            }
            else
            {
                // Fallback to old ShopSave if needed
                purchased = ShopSave.HasCharacter(_row.RefId);
            }
        }

        if (footerPurchased) footerPurchased.gameObject.SetActive(purchased);
        if (button) button.interactable = !purchased;
    }

    // ----------------- BUTTON -----------------

    public void OnClickBuy()
    {
        if (_row == null)
        {
            Debug.LogError($"{name}: no LocalShopItem loaded; call Refresh first.");
            return;
        }

        if (confirmPanel == null) confirmPanel = FindConfirmPanel();
        if (confirmPanel == null)
        {
            Debug.LogError("ShopCardBinderDb: no ConfirmPanel found in scene.");
            return;
        }

        // Pass EXACT data used by the card (row + its current icon sprite)
        confirmPanel.OpenDb(_row, icon ? icon.sprite : null);
    }

    // ----------------- HELPERS -----------------

    private string FormatPrice(LocalShopItem row)
    {
        if (row.PriceCoins > 0) return $"{row.PriceCoins:N0} COINS";
        if (row.PricePhp > 0) return $"{row.PricePhp} PHP";
        return "FREE";
    }

    private ConfirmPanel FindConfirmPanel()
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindFirstObjectByType<ConfirmPanel>(FindObjectsInactive.Include);
#else
        return Object.FindObjectOfType<ConfirmPanel>(true);
#endif
    }
}
