using System.Collections.Generic;
using System.Linq;

public static class ShopDb
{
    public static List<LocalShopItem> GetActiveItems()
    {
        var db = LocalDb.DB;                 // use singleton connection
        db.CreateTable<LocalShopItem>();     // safe if already exists

        return db.Table<LocalShopItem>()
                 .Where(i => i.IsActive)
                 .ToList();
    }

    public static LocalShopItem GetByRefId(string refId)
    {
        var db = LocalDb.DB;
        db.CreateTable<LocalShopItem>();

        // PrimaryKey is RefId, so Find<T>(pk) works
        return db.Find<LocalShopItem>(refId);
    }
}
