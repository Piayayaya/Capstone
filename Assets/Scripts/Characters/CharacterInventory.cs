using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using SQLite4Unity3d;

// Make sure LocalCharacterInventoryRow exists somewhere else:
// public class LocalCharacterInventoryRow { ... }

public class CharacterInventory : MonoBehaviour
{
    public static CharacterInventory Instance { get; private set; }

    private HashSet<string> owned = new();
    private string equippedId = "";

    [Header("Defaults")]
    [Tooltip("Id of the free starter character (must match CharacterDefinition.id / Firebase refId).")]
    public string defaultCharacterId = "char_starter";   // <<< change to your starter's id

    [Header("Optional")]
    public CoinWallet wallet; // only used for adding coins when selling

    public event Action OnInventoryChanged;
    public event Action OnEquippedChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure DB is ready
        var _ = LocalDb.DB;

        LoadFromSqlite();
        EnsureDefaultCharacter();
    }

    // ---- Public API ----
    public bool IsOwned(string id) => owned.Contains(id);
    public string GetEquipped() => equippedId;
    public IEnumerable<string> GetOwnedIds() => owned;

    /// Call this from your Shop after a purchase (id should match refId / CharacterDefinition.id)
    public void AddOwned(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (owned.Add(id))
        {
            // Auto-equip first character if none equipped yet
            if (string.IsNullOrEmpty(equippedId))
                equippedId = id;

            SaveToSqlite();
            OnInventoryChanged?.Invoke();
        }
    }

    public bool Equip(string id)
    {
        if (!IsOwned(id)) return false;
        if (equippedId == id) return true;

        equippedId = id;
        SaveToSqlite();
        OnEquippedChanged?.Invoke();
        return true;
    }

    public bool Sell(CharacterDefinition def)
    {
        if (def == null) return false;

        // 🚫 never allow selling the default starter
        if (!string.IsNullOrEmpty(defaultCharacterId) && def.id == defaultCharacterId)
            return false;

        // must be owned and not currently equipped
        if (!owned.Contains(def.id)) return false;
        if (equippedId == def.id) return false; // disallow selling equipped

        owned.Remove(def.id);

        // --- pay the refund via CoinService so TotalCoins & HUD update ---
        int refund = def.GetSellPrice();
        if (refund > 0)
        {
            if (CoinService.Instance != null)
            {
                // you can change the helper name if you used a different one
                CoinService.Instance.AddCharacterSellCoins(refund);
            }
            else if (wallet != null)
            {
                // fallback to old wallet system if CoinService is missing
                wallet.Add(refund);
            }
        }

        // if we accidentally sold the equipped (shouldn't happen), clear or pick another
        if (!owned.Contains(equippedId))
            equippedId = GetAnyOwnedOrEmpty();

        SaveToSqlite();
        OnInventoryChanged?.Invoke();
        return true;
    }



    string GetAnyOwnedOrEmpty()
    {
        foreach (var id in owned)
            return id;
        return "";
    }

    // =========================================================
    //                    SQLITE SAVE / LOAD
    // =========================================================

    void SaveToSqlite()
    {
        LocalDb.DB.RunInTransaction(() =>
        {
            // Clear previous snapshot
            LocalDb.DB.DeleteAll<LocalCharacterInventoryRow>();

            // Insert current state
            foreach (var id in owned)
            {
                var row = new LocalCharacterInventoryRow
                {
                    charId = id,
                    isOwned = 1,
                    isEquipped = (id == equippedId) ? 1 : 0
                };

                LocalDb.DB.InsertOrReplace(row);
            }
        });
    }

    void LoadFromSqlite()
    {
        owned.Clear();
        equippedId = "";

        // Make sure table exists
        LocalDb.DB.CreateTable<LocalCharacterInventoryRow>();

        var rows = LocalDb.DB.Table<LocalCharacterInventoryRow>().ToList();

        foreach (var row in rows)
        {
            if (row == null) continue;

            if (row.isOwned != 0 && !string.IsNullOrEmpty(row.charId))
                owned.Add(row.charId);

            if (row.isEquipped != 0 && !string.IsNullOrEmpty(row.charId))
                equippedId = row.charId;
        }

        // Safety: if we own something but nothing is marked equipped, pick one
        if (string.IsNullOrEmpty(equippedId))
            equippedId = GetAnyOwnedOrEmpty();
    }

    void EnsureDefaultCharacter()
    {
        // Only grant if we have NOTHING yet and a default is configured
        if (owned.Count > 0) return;
        if (string.IsNullOrEmpty(defaultCharacterId)) return;

        owned.Add(defaultCharacterId);
        equippedId = defaultCharacterId;

        SaveToSqlite();
        OnInventoryChanged?.Invoke();
        OnEquippedChanged?.Invoke();

        Debug.Log("[CharacterInventory] Granted default character: " + defaultCharacterId);
    }
}

// 1 row per character the player owns
public class LocalCharacterInventoryRow
{
    [PrimaryKey]
    public string charId { get; set; }   // ex: "char_poppi"  (same as refId / CharacterDefinition.id)

    public int isOwned { get; set; }     // 1 = owned, 0 = not owned (we’ll only store 1s)
    public int isEquipped { get; set; }  // 1 = currently selected, 0 = not selected
}

