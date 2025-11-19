using System.Text.RegularExpressions;
using UnityEngine;

public class ShopAPI : MonoBehaviour
{
    public ItemCatalog catalog;

    // Try a coin purchase. Returns true if success.
    public bool BuyWithCoins(string itemId)
    {
        var item = catalog ? catalog.GetById(itemId) : null;
        if (item == null || !item.usesGameCoins) return false;

        if (!ShopSave.SpendCoins(item.coinCost)) return false;

        if (item.type == ItemType.Character)
            ShopSave.UnlockCharacter(item.id);

        CharacterInventory.Instance.AddOwned(itemId);


        Debug.Log($"Purchased (coins): {item.displayName}");
        return true;
    }

    // Mock pesos purchase (deliver instantly). Replace with real IAP later.
    public void BuyPesoProductMock(string itemId)
    {
        var item = catalog ? catalog.GetById(itemId) : null;
        if (item == null) return;

        if (item.type == ItemType.CoinsPack)
        {
            // Try to parse numeric amount from displayName like "x69,999 COINS"
            var numeric = Regex.Replace(item.displayName, "[^0-9]", "");
            if (int.TryParse(numeric, out int amount)) ShopSave.AddCoins(amount);
        }
        else if (item.type == ItemType.Subscription && item.subscriptionDays > 0)
        {
            ShopSave.GrantNoAdsForDays(item.subscriptionDays);
        }

        Debug.Log($"[MOCK] Delivered peso product: {item.displayName}");
    }


}
