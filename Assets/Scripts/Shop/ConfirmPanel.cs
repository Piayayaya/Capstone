using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [Tooltip("Optional. Leave null if you don't show a coin icon.")]
    [SerializeField] private GameObject coinIcon;   // OPTIONAL

    [Header("Refs")]
    [SerializeField] private ItemCatalog catalog;
    [SerializeField] private ShopAPI shopAPI;

    [Header("Optional")]
    [Tooltip("Panel shown when balance is insufficient.")]
    [SerializeField] private GameObject notEnoughPanel; // OPTIONAL

    private ItemDefinition _current;

    void Awake()
    {
        if (!shopAPI) shopAPI = FindAPI();
        if (!catalog && shopAPI) catalog = shopAPI.catalog;
    }

    public void Open(string itemId)
    {
        if (!catalog) { Debug.LogError("ConfirmPanel: catalog not assigned."); return; }

        _current = catalog.GetById(itemId);
        if (_current == null) { Debug.LogError($"ConfirmPanel: item '{itemId}' not found."); return; }

        if (icon) icon.sprite = _current.icon;
        if (nameText) nameText.text = _current.displayName;
        if (coinIcon) coinIcon.SetActive(_current.usesGameCoins);
        if (priceText) priceText.text = FormatPrice(_current);

        if (notEnoughPanel) notEnoughPanel.SetActive(false);

        // ensure it shows above everything
        transform.SetAsLastSibling();
        gameObject.SetActive(true);
    }

    public void OnCancel() => gameObject.SetActive(false);

    public void OnConfirm()
    {
        if (_current == null) { gameObject.SetActive(false); return; }
        if (!shopAPI) { Debug.LogError("ConfirmPanel: ShopAPI missing."); return; }

        if (_current.usesGameCoins)
        {
            bool ok = shopAPI.BuyWithCoins(_current.id);
            if (!ok)
            {
                if (notEnoughPanel) { notEnoughPanel.SetActive(true); return; }
                Debug.Log("Not enough coins.");
                return;
            }
        }
        else
        {
            shopAPI.BuyPesoProductMock(_current.id);
        }

        gameObject.SetActive(false);

        // refresh UI
        foreach (var b in FindObjectsByType<ShopCardBinder>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            b.Refresh();
        var hud = FindFirstHud();
        if (hud) hud.Refresh();
    }

    public void OnGetCoins() { gameObject.SetActive(false); /* navigate to Coins section here */ }

    private static string FormatPrice(ItemDefinition item)
        => item.usesGameCoins ? $"{item.coinCost:N0} COINS"
                              : $"{(string.IsNullOrWhiteSpace(item.pesoDisplay) ? "" : item.pesoDisplay + " ")}PESOS";

#if UNITY_2023_1_OR_NEWER
    private static ShopAPI FindAPI() => Object.FindFirstObjectByType<ShopAPI>(FindObjectsInactive.Include);
    private static CoinHudBinder FindFirstHud() => Object.FindFirstObjectByType<CoinHudBinder>(FindObjectsInactive.Include);
#else
    private static ShopAPI FindAPI() => Object.FindObjectOfType<ShopAPI>(true);
    private static CoinHudBinder FindFirstHud() => Object.FindObjectOfType<CoinHudBinder>(true);
#endif
}
