using System.Collections;
using UnityEngine;

public class CharacterManagerUI : MonoBehaviour
{
    public CharacterCatalog catalog;
    public Transform contentParent;   // ScrollView/Viewport/Content
    public GameObject cardPrefab;     // CharacterCard prefab
    public bool clearOnStart = true;

    [Header("Debug")]
    public bool seedTwoOwnedForTesting = false; // <-- turn ON in Inspector for first run

    void Start()
    {
        StartCoroutine(InitWhenInventoryReady());
    }

    IEnumerator InitWhenInventoryReady()
    {
        while (CharacterInventory.Instance == null) yield return null;

        if (seedTwoOwnedForTesting && catalog && catalog.characters.Count > 0)
        {
            CharacterInventory.Instance.AddOwned(catalog.characters[0].id);
            if (catalog.characters.Count > 1)
                CharacterInventory.Instance.AddOwned(catalog.characters[1].id);
        }

        Populate();

        CharacterInventory.Instance.OnInventoryChanged -= Populate;
        CharacterInventory.Instance.OnInventoryChanged += Populate;
        CharacterInventory.Instance.OnEquippedChanged -= RefreshCardsOnly;
        CharacterInventory.Instance.OnEquippedChanged += RefreshCardsOnly;
    }

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
        if (!catalog || !cardPrefab || !contentParent)
        {
            Debug.LogError("[CharacterManagerUI] Missing wiring (catalog/cardPrefab/contentParent).");
            return;
        }

        if (clearOnStart)
        {
            for (int i = contentParent.childCount - 1; i >= 0; i--)
                Destroy(contentParent.GetChild(i).gameObject);
        }

        int spawned = 0;
        foreach (var def in catalog.characters)
        {
            if (CharacterInventory.Instance.IsOwned(def.id))
            {
                var go = Instantiate(cardPrefab, contentParent);
                var binder = go.GetComponent<CharacterCardBinder>();
                if (!binder) { Debug.LogError("Card prefab missing CharacterCardBinder!"); continue; }
                binder.Bind(def);
                spawned++;
            }
        }
        Debug.Log($"[CharacterManagerUI] Spawned {spawned} owned character cards.");
        if (spawned == 0) Debug.LogWarning("[CharacterManagerUI] No owned characters. (IDs mismatch or empty PlayerPrefs)");
    }
}
