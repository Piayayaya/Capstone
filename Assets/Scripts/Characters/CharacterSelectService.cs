using System;
using UnityEngine;

public class CharacterSelectionService : MonoBehaviour
{
    public static CharacterSelectionService Instance { get; private set; }

    [Header("Default character (id) if nothing saved yet")]
    public string defaultCharacterId = "owl";   // <<< change to your real default

    private const string PREF_KEY = "BM_SelectedCharacterId_v1";

    // NEW: remember which user this selection belongs to
    private const string PREF_LAST_USER_KEY = "BM_SelectedCharacter_LastUser_v1";

    /// <summary>Current selected character ID.</summary>
    public string CurrentId => _currentId;
    private string _currentId;

    /// <summary>Fires whenever the selected character changes.</summary>
    public event Action<string> OnSelectionChanged;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        HandleUserChangeAndLoad();
    }

    // ------------------------------------------------------
    // NEW: Per-user selection handling
    // ------------------------------------------------------
    private void HandleUserChangeAndLoad()
    {
        string currentUserId = UserIdProvider.ActiveUserId;
        string lastUserId = PlayerPrefs.GetString(PREF_LAST_USER_KEY, "");

        if (!string.IsNullOrEmpty(lastUserId) && lastUserId != currentUserId)
        {
            // Different user -> forget previous selection
            Debug.Log($"[CharacterSelectionService] User changed {lastUserId} -> {currentUserId}. Clearing selection key.");
            PlayerPrefs.DeleteKey(PREF_KEY);
        }

        PlayerPrefs.SetString(PREF_LAST_USER_KEY, currentUserId);
        PlayerPrefs.Save();

        // Load saved selection or default
        _currentId = PlayerPrefs.GetString(PREF_KEY, defaultCharacterId);
        if (string.IsNullOrEmpty(_currentId))
            _currentId = defaultCharacterId;
    }

    /// <summary>Change the selected character and save it.</summary>
    public void SetSelection(string characterId, bool save = true)
    {
        if (string.IsNullOrEmpty(characterId))
            return;

        if (_currentId == characterId)
            return; // no change

        _currentId = characterId;

        if (save)
        {
            PlayerPrefs.SetString(PREF_KEY, _currentId);
            PlayerPrefs.Save();
        }

        OnSelectionChanged?.Invoke(_currentId);

        // (Optional / later) also push this to Firebase/SQLite user data
        // e.g. UserProgress.activePlayer.selectedCharacterId = _currentId;
    }

    /// <summary>
    /// Optional helper you can call from your Delete Account flow
    /// to reset selection immediately for the current user.
    /// </summary>
    public void ResetSelectionToDefault()
    {
        _currentId = defaultCharacterId;
        PlayerPrefs.DeleteKey(PREF_KEY);
        PlayerPrefs.Save();
        OnSelectionChanged?.Invoke(_currentId);
    }
}
