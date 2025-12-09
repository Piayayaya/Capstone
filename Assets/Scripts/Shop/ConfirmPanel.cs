using System.Collections;
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

    [Header("Refs (SO flow)")]
    [SerializeField] private ItemCatalog catalog;   // used only for legacy SO-based items
    [SerializeField] private ShopAPI shopAPI;

    [Header("Not enough coins behaviour")]
    [SerializeField] private GameObject notEnoughPanel; // panel with “NOT ENOUGH COINS”
    [SerializeField] private float notEnoughHideDelay = 1.5f; // seconds

    [Header("Success panel")]
    public TimedPanel timedPanel;   // your SuccessPurchasePanel with timer

    [Header("Parent / OTP gate for PHP items")]
    [SerializeField] private ParentApprovalPanel parentApprovalPanel;   // 🔹 drag new panel here

    // ---- internal state ----
    private ItemDefinition _currentSoItem;   // ScriptableObject-based item
    private LocalShopItem _currentDbItem;    // SQLite-based item
    private bool _usingDbItem = false;
    private Coroutine _notEnoughRoutine;

    void Awake()
    {
        if (!shopAPI) shopAPI = FindAPI();
        if (!catalog && shopAPI) catalog = shopAPI.catalog;
    }

    // ============= OPEN FROM DB (NEW SHOP FLOW) =============
    /// <summary>
    /// Called by ShopCardBinderDb. Uses exactly the data shown on the card
    /// (DB row + sprite passed in) so there is no re-lookup or mismatch.
    /// </summary>
    public void OpenDb(LocalShopItem item, Sprite iconSprite)
    {
        if (item == null)
        {
            Debug.LogError("ConfirmPanel.OpenDb: item is null.");
            return;
        }

        _usingDbItem = true;
        _currentDbItem = item;
        _currentSoItem = null;

        if (icon) icon.sprite = iconSprite;
        if (nameText) nameText.text = item.ItemName ?? item.RefId;
        if (priceText) priceText.text = FormatPriceDb(item);
        if (coinIcon) coinIcon.SetActive(item.PriceCoins > 0);

        HideNotEnoughInstant();

        transform.SetAsLastSibling();
        gameObject.SetActive(true);
    }

    // ============= OPEN FROM SO (LEGACY FLOW) =============
    /// <summary>
    /// Legacy entry: used by old ShopCardBinder that still uses ItemDefinition.
    /// New DB-based cards should call OpenDb instead.
    /// </summary>
    public void Open(string itemId)
    {
        if (!catalog)
        {
            Debug.LogError("ConfirmPanel.Open: catalog not assigned.");
            return;
        }

        _usingDbItem = false;
        _currentDbItem = null;
        _currentSoItem = catalog.GetById(itemId);

        if (_currentSoItem == null)
        {
            Debug.LogError($"ConfirmPanel.Open: item '{itemId}' not found in catalog.");
            return;
        }

        SetupUiForSo(_currentSoItem);
        HideNotEnoughInstant();

        transform.SetAsLastSibling();
        gameObject.SetActive(true);
    }

    // =======================================================

    public void OnCancel()
    {
        HideNotEnoughInstant();
        gameObject.SetActive(false);
    }

    public void OnConfirm()
    {
        if (!shopAPI)
        {
            Debug.LogError("ConfirmPanel: ShopAPI missing.");
            gameObject.SetActive(false);
            return;
        }

        bool success = false;

        // ---------- DB-based purchase ----------
        if (_usingDbItem)
        {
            if (_currentDbItem == null)
            {
                gameObject.SetActive(false);
                return;
            }

            // 1) Coins (in-game currency)
            if (_currentDbItem.PriceCoins > 0)
            {
                success = shopAPI.BuyWithCoinsFromDb(_currentDbItem.RefId);
                if (!success)
                {
                    ShowNotEnough();
                    return;
                }
            }
            // 2) PHP (real-money style) -> require parent approval panel
            else if (_currentDbItem.PricePhp > 0)
            {
                if (parentApprovalPanel != null)
                {
                    parentApprovalPanel.StartDbPurchase(_currentDbItem, this);
                    // ParentApprovalPanel will call OnExternalPaymentSuccess()
                    // when OTP is correct, so we stop here.
                    return;
                }

                Debug.Log("[ConfirmPanel] Non-coin DB purchase (PHP) – parentApprovalPanel is null; auto-success.");
                success = true;
            }
            // 3) FREE items
            else
            {
                success = true;
            }
        }
        // ---------- ScriptableObject-based purchase (old flow) ----------
        else
        {
            if (_currentSoItem == null)
            {
                gameObject.SetActive(false);
                return;
            }

            if (_currentSoItem.usesGameCoins)
            {
                success = shopAPI.BuyWithCoins(_currentSoItem.id);
                if (!success)
                {
                    ShowNotEnough();
                    return;
                }
            }
            else
            {
                // Peso / IAP-style -> parent approval gate if available
                if (parentApprovalPanel != null)
                {
                    parentApprovalPanel.StartSoPurchase(_currentSoItem, this);
                    return;
                }

                // fallback to your original mock
                shopAPI.BuyPesoProductMock(_currentSoItem.id);
                success = true;
            }
        }

        // If we reach here with success = true (coins or free items), show success panel if assigned
        if (success && timedPanel != null)
        {
            timedPanel.ShowPanel();
        }

        HideNotEnoughInstant();
        gameObject.SetActive(false);

        // refresh UI (both old and new binders)
        foreach (var b in FindObjectsByType<ShopCardBinder>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            b.Refresh();

        foreach (var b in FindObjectsByType<ShopCardBinderDb>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            b.Refresh();

        var hud = FindFirstHud();
        if (hud) hud.Refresh();
    }

    /// <summary>
    /// Called by ParentApprovalPanel when OTP is correct and the mock payment is granted.
    /// </summary>
    public void OnExternalPaymentSuccess()
    {
        Debug.Log("[ConfirmPanel] External payment success, showing success panel and refreshing UI.");

        if (timedPanel != null)
        {
            timedPanel.ShowPanel();
        }

        HideNotEnoughInstant();
        gameObject.SetActive(false);

        foreach (var b in FindObjectsByType<ShopCardBinder>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            b.Refresh();

        foreach (var b in FindObjectsByType<ShopCardBinderDb>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            b.Refresh();

        var hud = FindFirstHud();
        if (hud) hud.Refresh();
    }

    public void OnGetCoins()
    {
        HideNotEnoughInstant();
        gameObject.SetActive(false);
        // navigate to Coins section here if you have that flow
    }

    // ---------- NOT ENOUGH COINS HELPERS ----------

    private void ShowNotEnough()
    {
        if (!notEnoughPanel) return;

        notEnoughPanel.SetActive(true);

        if (_notEnoughRoutine != null)
            StopCoroutine(_notEnoughRoutine);

        _notEnoughRoutine = StartCoroutine(HideNotEnoughAfterDelay());
    }

    private IEnumerator HideNotEnoughAfterDelay()
    {
        yield return new WaitForSeconds(notEnoughHideDelay);
        if (notEnoughPanel) notEnoughPanel.SetActive(false);
        _notEnoughRoutine = null;
    }

    private void HideNotEnoughInstant()
    {
        if (_notEnoughRoutine != null)
        {
            StopCoroutine(_notEnoughRoutine);
            _notEnoughRoutine = null;
        }
        if (notEnoughPanel) notEnoughPanel.SetActive(false);
    }

    // ---------- UI helpers ----------

    private void SetupUiForSo(ItemDefinition item)
    {
        if (icon) icon.sprite = item.icon;
        if (nameText) nameText.text = item.displayName;
        if (coinIcon) coinIcon.SetActive(item.usesGameCoins);
        if (priceText) priceText.text = FormatPriceSo(item);
    }

    private static string FormatPriceSo(ItemDefinition item)
        => item.usesGameCoins
            ? $"{item.coinCost:N0} COINS"
            : $"{(string.IsNullOrWhiteSpace(item.pesoDisplay) ? "" : item.pesoDisplay + " ")}PESOS";

    private static string FormatPriceDb(LocalShopItem item)
    {
        if (item.PriceCoins > 0) return $"{item.PriceCoins:N0} COINS";
        if (item.PricePhp > 0) return $"{item.PricePhp} PHP";
        return "FREE";
    }

#if UNITY_2023_1_OR_NEWER
    private static ShopAPI FindAPI() => Object.FindFirstObjectByType<ShopAPI>(FindObjectsInactive.Include);
    private static CoinHudBinder FindFirstHud() => Object.FindFirstObjectByType<CoinHudBinder>(FindObjectsInactive.Include);
#else
    private static ShopAPI FindAPI() => Object.FindObjectOfType<ShopAPI>(true);
    private static CoinHudBinder FindFirstHud() => Object.FindObjectOfType<CoinHudBinder>(true);
#endif
}
