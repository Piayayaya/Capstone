using UnityEngine;
using System;
using System.Collections.Generic;

public class CharacterInventory : MonoBehaviour
{
    public static CharacterInventory Instance { get; private set; }

    private const string KEY_OWNED = "OwnedChars";
    private const string KEY_EQUIPPED = "EquippedChar";

    private HashSet<string> owned = new();
    private string equippedId = "";

    [Header("Optional")]
    public CoinWallet wallet; // only used for adding coins when selling

    public event Action OnInventoryChanged;
    public event Action OnEquippedChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    // ---- Public API ----
    public bool IsOwned(string id) => owned.Contains(id);
    public string GetEquipped() => equippedId;
    public IEnumerable<string> GetOwnedIds() => owned;

    /// Call this from your Shop after a purchase:
    public void AddOwned(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (owned.Add(id))
        {
            // Auto-equip first character if none equipped
            if (string.IsNullOrEmpty(equippedId)) equippedId = id;
            Save();
            OnInventoryChanged?.Invoke();
        }
    }

    public bool Equip(string id)
    {
        if (!IsOwned(id)) return false;
        if (equippedId == id) return true;
        equippedId = id;
        Save();
        OnEquippedChanged?.Invoke();
        return true;
    }

    public bool Sell(CharacterDefinition def)
    {
        if (def == null) return false;
        if (!owned.Contains(def.id)) return false;
        if (equippedId == def.id) return false; // disallow selling equipped

        owned.Remove(def.id);

        // pay the refund
        if (wallet) wallet.Add(def.GetSellPrice());

        // if inventory is now empty or we sold something else, keep equipped as-is
        // if we accidentally sold the equipped (shouldn't happen), clear it
        if (!owned.Contains(equippedId)) equippedId = GetAnyOwnedOrEmpty();

        Save();
        OnInventoryChanged?.Invoke();
        return true;
    }

    string GetAnyOwnedOrEmpty()
    {
        foreach (var id in owned) return id;
        return "";
    }

    // ---- Save/Load ----
    void Save()
    {
        PlayerPrefs.SetString(KEY_OWNED, string.Join(",", owned));
        PlayerPrefs.SetString(KEY_EQUIPPED, equippedId ?? "");
        PlayerPrefs.Save();
    }

    void Load()
    {
        owned.Clear();
        var ownedStr = PlayerPrefs.GetString(KEY_OWNED, "");
        if (!string.IsNullOrWhiteSpace(ownedStr))
        {
            foreach (var id in ownedStr.Split(','))
            {
                var trimmed = id.Trim();
                if (!string.IsNullOrEmpty(trimmed)) owned.Add(trimmed);
            }
        }
        equippedId = PlayerPrefs.GetString(KEY_EQUIPPED, "");
    }
}
