using System.Text.RegularExpressions;
using UnityEngine;

public class ShopAPI : MonoBehaviour
{
    [Header("ScriptableObject Catalog (optional, old flow)")]
    public ItemCatalog catalog;

    // ----------------------------------------------------------------
    //  SO-BASED COIN PURCHASE, USING CoinService FOR COINS
    // ----------------------------------------------------------------
    public bool BuyWithCoins(string itemId)
    {
        var item = catalog ? catalog.GetById(itemId) : null;
        if (item == null || !item.usesGameCoins) return false;

        if (!TrySpendCoins(item.coinCost)) return false;

        // Ownership still stored via ShopSave
        if (item.type == ItemType.Character)
            ShopSave.UnlockCharacter(item.id);

        if (CharacterInventory.Instance != null)
            CharacterInventory.Instance.AddOwned(itemId);
        else
            Debug.LogWarning("[ShopAPI] CharacterInventory.Instance is null (SO flow).");

        Debug.Log($"Purchased (coins, SO): {item.displayName}");
        return true;
    }

    // ----------------------------------------------------------------
    //  DB-BASED PURCHASE (LocalShopItem) USING CoinService
    // ----------------------------------------------------------------
    public bool BuyWithCoinsFromDb(string refId)
    {
        var row = ShopDb.GetByRefId(refId);
        if (row == null)
        {
            Debug.LogError($"[ShopAPI] BuyWithCoinsFromDb: no LocalShopItem for refId='{refId}'");
            return false;
        }

        int price = Mathf.Max(0, row.PriceCoins);
        int totalBefore =
            CoinService.Instance != null
                ? CoinService.Instance.TotalCoins
                : ShopSave.Data.coinBalance;

        Debug.Log($"[ShopAPI] BuyFromDb refId='{refId}' price={price} totalBefore={totalBefore}");

        bool paid;

        // use CoinService when available
        if (CoinService.Instance != null)
        {
            paid = CoinService.Instance.TrySpendCoins(price);
        }
        else
        {
            // fallback to old ShopSave if CoinService not present
            paid = ShopSave.SpendCoins(price);
        }

        if (!paid)
        {
            Debug.LogWarning(
                $"[ShopAPI] NOT ENOUGH COINS for '{refId}'. price={price}, totalBefore={totalBefore}");
            return false;
        }

        // -------- SUCCESS: deliver item --------
        var type = (row.ItemType ?? "").ToLowerInvariant();

        if (type == "character")
        {
            // 1) mark as owned in the old save system
            ShopSave.UnlockCharacter(row.RefId);

            // 2) update runtime CharacterInventory so Characters scene sees it immediately
            if (CharacterInventory.Instance != null)
            {
                CharacterInventory.Instance.AddOwned(row.RefId);
            }

            Debug.Log($"[ShopAPI] Character purchased/unlocked: {row.RefId}");
        }
        else if (type == "coins_pack")
        {
            if (CoinService.Instance != null)
                CoinService.Instance.AddCoins(row.RewardCoins, GameModeId.DailyRewards);
            else
                ShopSave.AddCoins(row.RewardCoins);
        }

        Debug.Log($"[ShopAPI] Purchase SUCCESS for '{refId}'. New total=" +
                  (CoinService.Instance != null ? CoinService.Instance.TotalCoins : ShopSave.Data.coinBalance));

        return true;
    }



    // ----------------------------------------------------------------
    //  MOCK PESO PURCHASE (SO-based)
    // ----------------------------------------------------------------
    public void BuyPesoProductMock(string itemId)
    {
        var item = catalog ? catalog.GetById(itemId) : null;
        if (item == null) return;

        int grantedCoins = 0;

        if (item.type == ItemType.CoinsPack)
        {
            // Extract numeric part from something like "500 COINS"
            var numeric = Regex.Replace(item.displayName, "[^0-9]", "");
            if (int.TryParse(numeric, out int amount))
            {
                grantedCoins = amount;

                if (CoinService.Instance != null)
                {
                    CoinService.Instance.AddCoinsFromStore(amount);
                    Debug.Log($"[MOCK] Granted {amount} coins from peso product via CoinService.");
                }
                else
                {
                    ShopSave.AddCoins(amount);
                    Debug.Log($"[MOCK] Granted {amount} coins using ShopSave fallback.");
                }
            }
            else
            {
                Debug.LogWarning($"[MOCK] Could not parse coin amount from '{item.displayName}'.");
            }
        }
        else if (item.type == ItemType.Subscription && item.subscriptionDays > 0)
        {
            ShopSave.GrantNoAdsForDays(item.subscriptionDays);
        }

        // Optional: add a notification entry as a receipt
        if (grantedCoins > 0 && NotificationService.Instance != null)
        {
            NotificationService.Instance.Add(
                $"You purchased {grantedCoins} coins in the Store.");
        }

        Debug.Log($"[MOCK] Delivered peso product: {item.displayName}");
    }

    // ----------------------------------------------------------------
    //  HELPERS: Spend coins using CoinService if available
    // ----------------------------------------------------------------
    private bool TrySpendCoins(int amount)
    {
        // Prefer teammate's CoinService
        if (CoinService.Instance != null)
        {
            return CoinService.Instance.TrySpendCoins(amount);
        }

        // Fallback: old ShopSave, if CoinService is missing in this scene
        return ShopSave.SpendCoins(amount);
    }
}
