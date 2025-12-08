using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DangerActionsManager : MonoBehaviour
{
    public static DangerActionsManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Navigation")]
    [SerializeField] private string createAccountSceneName = "CreateAccount";

    [Header("PlayerPrefs keys (MUST match your project)")]
    [SerializeField] private string playerIdPrefsKey = "DEVICE_PLAYER_ID";
    [SerializeField] private string userIdPrefsKey = "activeUserId_v1";
    [SerializeField] private bool wipeAllLocalPrefsOnDelete = true;

    private enum CurrentAction
    {
        None,
        ResetProgress,
        DeleteAccount
    }

    private CurrentAction _currentAction = CurrentAction.None;
    private DangerToggleOpener _currentToggleOpener;

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

#if UNITY_ANDROID && !UNITY_EDITOR
        _root = FirebaseDatabase.DefaultInstance.RootReference;
#else
        _root = null;
        Debug.LogWarning("[DangerActionsManager] Firebase disabled in Editor / non-Android. Remote reset/delete will be skipped.");
#endif
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
        switch (_currentAction)
        {
            case CurrentAction.ResetProgress:
                PerformResetProgress();
                break;

            case CurrentAction.DeleteAccount:
                PerformDeleteAccount();
                break;
        }

        if (_currentToggleOpener != null)
            _currentToggleOpener.ForceOff();

        ClearState();
    }

    private void OnNoClicked()
    {
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

        ResetLocalProgress();
        RefreshRuntimeAfterReset();
    }

    private Task ResetProgressInFirebase(string playerId)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_root == null)
            _root = FirebaseDatabase.DefaultInstance.RootReference;

        var updates = new Dictionary<string, object>();

        string basePath = $"players/{playerId}/coins";

        updates[$"{basePath}/byMode/DailyQuests"] = 0;
        updates[$"{basePath}/byMode/DailyRewards"] = 0;
        updates[$"{basePath}/byMode/DragAndDrop"] = 0;
        updates[$"{basePath}/byMode/NameTheFlag"] = 0;
        updates[$"{basePath}/byMode/SeeItOrLoseIt"] = 0;
        updates[$"{basePath}/byMode/SmartLadder"] = 0;
        updates[$"{basePath}/byMode/TuneYourTongue"] = 0;

        updates[$"{basePath}/total"] = 0;
        updates[$"{basePath}/updatedAt"] = DateTime.UtcNow.ToString("o");

        updates[$"players/{playerId}/dailyLogin"] = null;
        updates[$"players/{playerId}/dailyQuests"] = null;

        return _root.UpdateChildrenAsync(updates);
#else
        Debug.LogWarning("[DangerActionsManager] ResetProgressInFirebase skipped (no Firebase on this platform).");
        return Task.CompletedTask;
#endif
    }

    private void ResetLocalProgress()
    {
        PlayerPrefs.DeleteKey("BM_TotalCoins");
        PlayerPrefs.DeleteKey("BM_DailyLoginState_v1");
        PlayerPrefs.DeleteKey("BM_DailyQuestState_v1");
        PlayerPrefs.DeleteKey("BM_AchievementState_v1");

        PlayerPrefs.Save();

        ShopSave.ResetAll();

        if (CharacterInventory.Instance != null)
        {
            CharacterInventory.Instance.ResetAllLocalInventory();

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

    private void RefreshRuntimeAfterReset()
    {
        if (CoinService.Instance != null)
        {
            CoinService.Instance.ForceSetAllZeroLocal();
        }
    }

    #endregion

    #region Delete Account

    private async void PerformDeleteAccount()
    {
        Debug.Log("[DangerActionsManager] DELETE ACCOUNT confirmed.");

        string playerId = PlayerPrefs.GetString(playerIdPrefsKey, string.Empty);
        string userId = PlayerPrefs.GetString(userIdPrefsKey, string.Empty);

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

        WipeLocalAccountData();

        if (!string.IsNullOrEmpty(userId) && ProfileService.Instance != null)
        {
            ProfileService.Instance.ClearForUser(userId);
        }

        if (!string.IsNullOrEmpty(createAccountSceneName))
        {
            SceneManager.LoadScene(createAccountSceneName);
        }
    }

    private Task DeleteAccountInFirebase(string playerId, string userId)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_root == null)
            _root = FirebaseDatabase.DefaultInstance.RootReference;

        var updates = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(playerId))
        {
            updates[$"players/{playerId}"] = null;
        }

        if (!string.IsNullOrEmpty(userId))
        {
            updates[$"users/{userId}"] = null;
        }

        return _root.UpdateChildrenAsync(updates);
#else
        Debug.LogWarning("[DangerActionsManager] DeleteAccountInFirebase skipped (no Firebase on this platform).");
        return Task.CompletedTask;
#endif
    }

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

        ShopSave.ResetAll();

        if (CharacterInventory.Instance != null)
        {
            CharacterInventory.Instance.ResetAllLocalInventory();
        }

        if (CharacterSelectionService.Instance != null)
        {
            CharacterSelectionService.Instance.ResetSelectionToDefault();
        }

        if (CoinService.Instance != null)
        {
            CoinService.Instance.ForceSetAllZeroLocal();
        }
    }

    #endregion
}
