using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCardBinder : MonoBehaviour
{
    [Header("Data")]
    public ItemDefinition item;        // RECOMMENDED: drag the item asset here
    public ItemCatalog catalog;        // used only if item == null
    public string itemId;              // used only if item == null

    [Header("UI (only what you have)")]
    public Image icon;
    public TMP_Text title;
    public TMP_Text priceText;

    [Header("Optional")]
    public GameObject coinIcon;
    public TMP_Text subLabel;
    public TMP_Text footerPurchased;
    public Button button;

    [SerializeField] private ConfirmPanel confirmPanel; // assign, or auto-find

    void Start() => Refresh();

    public void OnClickBuy()
    {
        if (confirmPanel == null) confirmPanel = FindConfirmPanel();
        if (confirmPanel == null) { Debug.LogError("No ConfirmPanel found under Canvas."); return; }

        var id = EnsureItemLoaded() ? item.id : itemId;
        if (string.IsNullOrEmpty(id)) { Debug.LogError($"{name}: no item assigned."); return; }

        confirmPanel.Open(id);
    }

    public void Refresh()
    {
        if (!EnsureItemLoaded()) return;

        if (icon) icon.sprite = item.icon;
        if (title) title.text = item.displayName;
        if (subLabel) subLabel.text = item.subLabel ?? "";
        if (priceText) priceText.text = FormatPrice(item);
        if (coinIcon) coinIcon.SetActive(item.usesGameCoins);

        bool purchased = false;
        if (item.type == ItemType.Character) purchased = ShopSave.HasCharacter(item.id);
        if (item.type == ItemType.Subscription) purchased = ShopSave.IsNoAdsActive();

        if (footerPurchased) footerPurchased.gameObject.SetActive(purchased);
        if (button) button.interactable = !purchased;
    }

    private bool EnsureItemLoaded()
    {
        if (item != null) { itemId = item.id; return true; }
        if (catalog == null) { Debug.LogError($"{name}: catalog not assigned.", this); return false; }
        if (string.IsNullOrEmpty(itemId)) { Debug.LogError($"{name}: itemId is empty.", this); return false; }
        item = catalog.GetById(itemId);
        if (item == null) { Debug.LogError($"{name}: itemId '{itemId}' not found in catalog.", this); return false; }
        return true;
    }

    private string FormatPrice(ItemDefinition i)
    {
        if (i.usesGameCoins) return $"{i.coinCost:N0} COINS";
        return string.IsNullOrWhiteSpace(i.pesoDisplay) ? "PESOS" : $"{i.pesoDisplay} PESOS";
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying && item != null) itemId = item.id;
        if (!Application.isPlaying) Refresh();
    }
#endif

    private ConfirmPanel FindConfirmPanel()
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindFirstObjectByType<ConfirmPanel>(FindObjectsInactive.Include);
#else
        return Object.FindObjectOfType<ConfirmPanel>(true);
#endif
    }
}
