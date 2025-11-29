// LocalShopItem.cs
using System;
using SQLite4Unity3d;

[Serializable]
public class LocalShopItem
{
    [PrimaryKey]
    public string RefId { get; set; }         // "char_poppi" – the main id you use in code
    public string FirebaseKey { get; set; }   // "9001" (optional, for debugging)

    public string ItemName { get; set; }      // "Poppi"
    public string ItemType { get; set; }      // "character", "coinsPack", "subscription"
    public string ItemImage { get; set; }     // "char_poppi.png"

    public int PriceCoins { get; set; }
    public int PricePhp { get; set; }
    public int RewardCoins { get; set; }

    public bool IsActive { get; set; }
}
