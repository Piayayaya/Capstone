using UnityEngine;

public class CharacterManagerUI : MonoBehaviour
{
    public CharacterCatalog catalog;
    public Transform contentParent;       // ScrollView/Viewport/Content
    public GameObject cardPrefab;         // The CharacterCard prefab
    public bool clearOnStart = true;

    void Start()
    {
        Populate();
        if (CharacterInventory.Instance != null)
        {
            CharacterInventory.Instance.OnInventoryChanged -= Repopulate;
            CharacterInventory.Instance.OnInventoryChanged += Repopulate;
            CharacterInventory.Instance.OnEquippedChanged -= RefreshCardsOnly;
            CharacterInventory.Instance.OnEquippedChanged += RefreshCardsOnly;
        }
    }

    void Repopulate() { Populate(); }
    void RefreshCardsOnly()
    {
        foreach (Transform t in contentParent)
        {
            var binder = t.GetComponent<CharacterCardBinder>();
            if (binder) binder.SendMessage("Refresh", SendMessageOptions.DontRequireReceiver);
        }
    }

    public void Populate()
    {
        if (!catalog || !cardPrefab || !contentParent) { Debug.LogError("[CharacterManagerUI] Wiring missing."); return; }

        if (clearOnStart)
        {
            for (int i = contentParent.childCount - 1; i >= 0; i--)
                Destroy(contentParent.GetChild(i).gameObject);
        }

        // Only instantiate cards for owned characters
        foreach (var def in catalog.characters)
        {
            if (CharacterInventory.Instance != null && CharacterInventory.Instance.IsOwned(def.id))
            {
                var go = Instantiate(cardPrefab, contentParent);
                var binder = go.GetComponent<CharacterCardBinder>();
                binder.Bind(def);
            }
        }
    }
}
