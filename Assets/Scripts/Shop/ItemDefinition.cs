using UnityEngine;

public enum ItemType { Character, CoinsPack, Subscription }

[CreateAssetMenu(menuName = "BrainyMe/Shop/Item", fileName = "NewShopItem")]
public class ItemDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;                // unique: e.g. "char_poppi", "coins_1000", "noads_30d"
    public string displayName;       // e.g. "Poppi", "x1000 COINS", "No Ads"
    public ItemType type;
    public Sprite icon;

    [Header("Pricing")]
    public bool usesGameCoins;       // true = buy with in-game coins
    public int coinCost;             // if usesGameCoins
    public string pesoDisplay;       // e.g. "₱49" - UI only for now
    public string pesoProductId;     // IAP id for later (optional)

    [Header("Optional labels")]
    public string subLabel;          // e.g. "30 DAYS" for subscriptions
    public int subscriptionDays;     // e.g. 30, 90, 365
}
