using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Central logic for dangerous actions in Settings:
/// - Reset Progress
/// - Delete Account
///
/// Works together with:
/// - DangerToggleOpener on each toggle
/// - One confirmation panel (panelRoot) with a message text + YES/NO buttons
/// </summary>
public class DangerActionsManager : MonoBehaviour
{
    public static DangerActionsManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Navigation")]
    [Tooltip("Scene to load after deleting account (e.g. CreateAccount).")]
    [SerializeField] private string createAccountSceneName = "CreateAccount";

    [Header("PlayerPrefs keys (MUST match your project)")]
    [Tooltip("Same key CoinService uses for the player node under /players.")]
    [SerializeField] private string playerIdPrefsKey = "DEVICE_PLAYER_ID";

    [Tooltip("Same key UserIdProvider uses for /users.")]
    [SerializeField] private string userIdPrefsKey = "activeUserId_v1";

    [Tooltip("If true, will call PlayerPrefs.DeleteAll() on Delete Account.")]
    [SerializeField] private bool wipeAllLocalPrefsOnDelete = true;

    private enum CurrentAction
    {
        None,
        ResetProgress,
        DeleteAccount
    }

    private CurrentAction _currentAction = CurrentAction.None;
    private DangerToggleOpener _currentToggleOpener;

    // cache Firebase root
    private DatabaseReference _root;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (panelRoot != null)
            panelRoot.SetActive(false);

        // root of your RTDB (where "players" and "users" live)
        _root = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // PUBLIC API – called from DangerToggleOpener ------------------------

    public void OpenReset(DangerToggleOpener opener)
    {
        Open(opener, CurrentAction.ResetProgress,
            "ARE YOU SURE YOU WANT TO RESET YOUR PROGRESS?");
    }

    public void OpenDelete(DangerToggleOpener opener)
    {
        Open(opener, CurrentAction.DeleteAccount,
            "ARE YOU SURE YOU WANT TO DELETE YOUR ACCOUNT?");
    }

    // --------------------------------------------------------------------

    private void Open(DangerToggleOpener opener, CurrentAction action, string message)
    {
        _currentToggleOpener = opener;
        _currentAction = action;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (messageText != null)
            messageText.text = message;

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
    }

    private void ClosePanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();
    }

    private void OnYesClicked()
    {
        // Do the requested action
        switch (_currentAction)
        {
            case CurrentAction.ResetProgress:
                PerformResetProgress();
                break;

            case CurrentAction.DeleteAccount:
                PerformDeleteAccount();
                break;
        }

        // Always put toggle back to OFF (left)
        if (_currentToggleOpener != null)
            _currentToggleOpener.ForceOff();

        ClearState();
    }

    private void OnNoClicked()
    {
        // User cancelled → just put toggle back OFF
        if (_currentToggleOpener != null)
            _currentToggleOpener.ForceOff();

        ClearState();
    }

    private void ClearState()
    {
        ClosePanel();
        _currentAction = CurrentAction.None;
        _currentToggleOpener = null;
    }

    // =====================================================================
    // ===============   ACTUAL DANGER ACTIONS   ===========================
    // =====================================================================

    #region Reset Progress

    private async void PerformResetProgress()
    {
        Debug.Log("[DangerActionsManager] RESET PROGRESS confirmed.");

        string playerId = PlayerPrefs.GetString(playerIdPrefsKey, string.Empty);

        // 1) Reset remote data in Firebase
        if (!string.IsNullOrEmpty(playerId))
        {
            try
            {
                await ResetProgressInFirebase(playerId);
                Debug.Log("[DangerActionsManager] Firebase progress reset for player " + playerId);
            }
            catch (Exception ex)
            {
                Debug.LogError("[DangerActionsManager] Error resetting progress in Firebase: " + ex);
            }
        }
        else
        {
            Debug.LogWarning($"[DangerActionsManager] No playerId in PlayerPrefs (key={playerIdPrefsKey}), skipped Firebase reset.");
        }

        // 2) Reset local progress (coins, daily login/quests, achievements, shop, characters)
        ResetLocalProgress();

        // 3) Make sure runtime systems show the reset state immediately
        RefreshRuntimeAfterReset();
    }

    /// <summary>
    /// Resets the player's coins, byMode, daily login/quest etc. in Firebase.
    /// </summary>
    private Task ResetProgressInFirebase(string playerId)
    {
        if (_root == null)
            _root = FirebaseDatabase.DefaultInstance.RootReference;

        var updates = new Dictionary<string, object>();

        string basePath = $"players/{playerId}/coins";

        // byMode entries → 0
        updates[$"{basePath}/byMode/DailyQuests"] = 0;
        updates[$"{basePath}/byMode/DailyRewards"] = 0;
        updates[$"{basePath}/byMode/DragAndDrop"] = 0;
        updates[$"{basePath}/byMode/NameTheFlag"] = 0;
        updates[$"{basePath}/byMode/SeeItOrLoseIt"] = 0;
        updates[$"{basePath}/byMode/SmartLadder"] = 0;
        updates[$"{basePath}/byMode/TuneYourTongue"] = 0;

        // total coins → 0
        updates[$"{basePath}/total"] = 0;

        // timestamp
        updates[$"{basePath}/updatedAt"] = DateTime.UtcNow.ToString("o");

        // Optional: wipe any daily login / quest progress nodes if you created them
        updates[$"players/{playerId}/dailyLogin"] = null;
        updates[$"players/{playerId}/dailyQuests"] = null;

        return _root.UpdateChildrenAsync(updates);
    }

    /// <summary>
    /// Clears local progress data but keeps the account itself.
    /// (Only PlayerPrefs keys – adjust to your actual ones.)
    /// ALSO clears shop purchases + character inventory.
    /// </summary>
    private void ResetLocalProgress()
    {
        // old keys you used in previous logic; safe to leave:
        PlayerPrefs.DeleteKey("BM_TotalCoins");
        PlayerPrefs.DeleteKey("BM_DailyLoginState_v1");
        PlayerPrefs.DeleteKey("BM_DailyQuestState_v1");
        PlayerPrefs.DeleteKey("BM_AchievementState_v1");

        PlayerPrefs.Save();

        // --- NEW: clear local shop data (coins + owned characters in ShopSave) ---
        ShopSave.ResetAll();

        // --- NEW: clear CharacterInventory + selection (local SQLite + equipped) ---
        if (CharacterInventory.Instance != null)
        {
            CharacterInventory.Instance.ResetAllLocalInventory();

            // re-grant starter so UI not empty (optional but nice)
            if (!string.IsNullOrEmpty(CharacterInventory.Instance.defaultCharacterId))
            {
                string starter = CharacterInventory.Instance.defaultCharacterId;
                CharacterInventory.Instance.AddOwned(starter);
                CharacterInventory.Instance.Equip(starter);
            }
        }

        if (CharacterSelectionService.Instance != null)
        {
            CharacterSelectionService.Instance.ResetSelectionToDefault();
        }
    }

    /// <summary>
    /// Makes sure runtime systems show the reset state immediately.
    /// </summary>
    private void RefreshRuntimeAfterReset()
    {
        // ✅ Use your CoinService instead of CoinWallet
        if (CoinService.Instance != null)
        {
            CoinService.Instance.ForceSetAllZeroLocal();
        }

        // If you later add public reset methods for quests / login / achievements,
        // you can also call them here.
    }

    #endregion

    #region Delete Account

    private async void PerformDeleteAccount()
    {
        Debug.Log("[DangerActionsManager] DELETE ACCOUNT confirmed.");

        string playerId = PlayerPrefs.GetString(playerIdPrefsKey, string.Empty);
        string userId = PlayerPrefs.GetString(userIdPrefsKey, string.Empty);

        // 1) Delete remote data in Firebase
        if (!string.IsNullOrEmpty(playerId) || !string.IsNullOrEmpty(userId))
        {
            try
            {
                await DeleteAccountInFirebase(playerId, userId);
                Debug.Log("[DangerActionsManager] Firebase player/user nodes deleted.");
            }
            catch (Exception ex)
            {
                Debug.LogError("[DangerActionsManager] Error deleting account in Firebase: " + ex);
            }
        }
        else
        {
            Debug.LogWarning($"[DangerActionsManager] No playerId/userId in PlayerPrefs (keys={playerIdPrefsKey}/{userIdPrefsKey}), skipped Firebase delete.");
        }

        // 2) Wipe local data (including shop + character inventory)
        WipeLocalAccountData();

        // 3) (optional) clear profile name
        if (!string.IsNullOrEmpty(userId) && ProfileService.Instance != null)
        {
            ProfileService.Instance.ClearForUser(userId);
        }

        // 4) Go back to CreateAccount scene so the user must create a new account
        if (!string.IsNullOrEmpty(createAccountSceneName))
        {
            SceneManager.LoadScene(createAccountSceneName);
        }
    }

    /// <summary>
    /// Deletes /players/{playerId} and /users/{userId} from Firebase.
    /// </summary>
    private Task DeleteAccountInFirebase(string playerId, string userId)
    {
        if (_root == null)
            _root = FirebaseDatabase.DefaultInstance.RootReference;

        var updates = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(playerId))
        {
            // delete entire player node (coins, progress, etc.)
            updates[$"players/{playerId}"] = null;
        }

        if (!string.IsNullOrEmpty(userId))
        {
            // delete user record
            updates[$"users/{userId}"] = null;
        }

        // If you have deviceUsers mapping and a known device key, you can also clear it here.
        // Example (ONLY if you actually use deviceUsers + deviceKey):
        // string deviceKey = SystemInfo.deviceUniqueIdentifier;
        // updates[$"deviceUsers/{deviceKey}"] = null;

        return _root.UpdateChildrenAsync(updates);
    }

    /// <summary>
    /// Clears local PlayerPrefs so the app behaves like a fresh install.
    /// Also resets shop + characters in memory & SQLite.
    /// </summary>
    private void WipeLocalAccountData()
    {
        if (wipeAllLocalPrefsOnDelete)
        {
            PlayerPrefs.DeleteAll();
        }
        else
        {
            PlayerPrefs.DeleteKey(playerIdPrefsKey);
            PlayerPrefs.DeleteKey(userIdPrefsKey);
            PlayerPrefs.DeleteKey("BM_TotalCoins");
            PlayerPrefs.DeleteKey("BM_DailyLoginState_v1");
            PlayerPrefs.DeleteKey("BM_DailyQuestState_v1");
            PlayerPrefs.DeleteKey("BM_AchievementState_v1");
        }

        PlayerPrefs.Save();

        // --- NEW: also wipe shop + characters for this session ---

        // reset ShopSave JSON
        ShopSave.ResetAll();

        // clear character inventory table + in-memory owned list
        if (CharacterInventory.Instance != null)
        {
            CharacterInventory.Instance.ResetAllLocalInventory();
        }

        // clear selected character id
        if (CharacterSelectionService.Instance != null)
        {
            CharacterSelectionService.Instance.ResetSelectionToDefault();
        }

        // Also clear coin UI in the current session
        if (CoinService.Instance != null)
        {
            CoinService.Instance.ForceSetAllZeroLocal();
        }
    }

    #endregion
}
