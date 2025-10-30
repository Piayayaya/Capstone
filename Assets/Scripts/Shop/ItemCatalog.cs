using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BrainyMe/Shop/Catalog", fileName = "ShopCatalog")]
public class ItemCatalog : ScriptableObject
{
    public List<ItemDefinition> items = new();

    public ItemDefinition GetById(string id)
        => items.Find(i => i != null && i.id == id);

    public IEnumerable<ItemDefinition> GetByType(ItemType t)
        => items.FindAll(i => i != null && i.type == t);
}
