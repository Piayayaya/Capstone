using System.Collections.Generic;
using UnityEngine;

public class CharacterManagerUI : MonoBehaviour
{
    [Header("UI")]
    public RectTransform contentRoot;          // parent where cards live
    public CharacterCardBinder cardPrefab;     // prefab with CharacterCardBinder attached

    [Header("Catalog")]
    public CharacterDefinition[] allCharacters; // all possible characters (including starter)

    void OnEnable()
    {
        // Subscribe to inventory events
        if (CharacterInventory.Instance != null)
        {
            CharacterInventory.Instance.OnInventoryChanged += RefreshCardsOnly;
            CharacterInventory.Instance.OnEquippedChanged += RefreshCardsOnly;
        }

        RefreshCardsOnly();
    }

    void OnDisable()
    {
        // Unsubscribe to prevent calling on destroyed objects
        if (CharacterInventory.Instance != null)
        {
            CharacterInventory.Instance.OnInventoryChanged -= RefreshCardsOnly;
            CharacterInventory.Instance.OnEquippedChanged -= RefreshCardsOnly;
        }
    }

    public void RefreshCardsOnly()
    {
        if (contentRoot == null || cardPrefab == null) return;
        if (CharacterInventory.Instance == null) return;

        // 1) Safely clear existing children
        //    Use for-loop from end instead of foreach to avoid MissingReferenceException.
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            var child = contentRoot.GetChild(i);
            if (child != null)
                Destroy(child.gameObject);
        }

        // 2) Rebuild cards from the catalog & inventory
        if (allCharacters == null) return;

        foreach (var def in allCharacters)
        {
            if (def == null) continue;

            // Only show if owned
            if (!CharacterInventory.Instance.IsOwned(def.id))
                continue;

            var card = Instantiate(cardPrefab, contentRoot);
            card.Bind(def);
        }
    }
}
