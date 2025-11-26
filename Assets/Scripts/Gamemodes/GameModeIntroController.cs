using System;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeIntroLoader : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("Firebase key under /Gamemodes, e.g. 7001, 7002…")]
    public string gameModeId = "7001";

    [Tooltip("Scene to load when the player taps PROCEED.")]
    public string targetSceneName = "SmartLadder";

    [Header("Fallback text (if Firebase fails)")]
    [TextArea(4, 12)]
    public string fallbackDescription =
        "This is the Smart Ladder game. (Fallback text if Firebase is not available.)";

    // cached data
    private bool _hasLoadedOnce;
    private string _loadedDescription;

    /// <summary>
    /// Hook this to your Game Mode button's OnClick event.
    /// </summary>
    public void OnClickShowIntro()
    {
        // fire-and-forget async
        _ = ShowIntroAsync();
    }

    private async Task ShowIntroAsync()
    {
        // 1) Ensure we have the description cached at least once
        if (!_hasLoadedOnce)
            await LoadFromFirebase();

        // 2) Decide which text to show
        string messageToShow = string.IsNullOrEmpty(_loadedDescription)
            ? fallbackDescription
            : _loadedDescription;

        // 3) Use your existing ModeIntroSimple panel
        if (ModeIntroSimple.Instance == null)
        {
            Debug.LogError("GameModeIntroLoader: ModeIntroSimple.Instance is null. " +
                           "Make sure the ModeIntro panel is in the scene.");
            return;
        }

        ModeIntroSimple.Instance.Open(
            messageToShow,
            () =>
            {
                // PROCEED callback -> load the target scene
                if (!string.IsNullOrEmpty(targetSceneName))
                    SceneManager.LoadScene(targetSceneName);
            });
    }

    private async Task LoadFromFirebase()
    {
        _loadedDescription = null;

        if (string.IsNullOrEmpty(gameModeId))
        {
            Debug.LogWarning("GameModeIntroLoader: gameModeId is empty.");
            return;
        }

        try
        {
            var db = FirebaseDatabase.DefaultInstance;
            var snapshot = await db
                .RootReference
                .Child("Gamemodes")
                .Child(gameModeId)
                .GetValueAsync();

            if (!snapshot.Exists)
            {
                Debug.LogWarning($"GameModeIntroLoader: /Gamemodes/{gameModeId} not found.");
                return;
            }

            string descr = snapshot.Child("gameInstruc").Value?.ToString();
            _loadedDescription = descr;
            _hasLoadedOnce = true;
        }
        catch (Exception ex)
        {
            Debug.LogError("GameModeIntroLoader: failed to load from Firebase\n" + ex);
        }
    }
}
