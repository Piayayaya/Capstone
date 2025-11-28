using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeIntroLoader : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("Numeric id from LocalGamemode.id / Firebase key (e.g., 7001, 7002...)")]
    public int gameModeId = 7001;         // set per mode in Inspector
    public string targetSceneName = "SmartLadder";

    [Header("Fallback text (if SQLite has no gameInstruc)")]
    [TextArea(3, 8)]
    public string fallbackInstruc = "Get ready to learn with BrainyMe!";

    private string _cachedInstruc;
    private bool _loadedFromDb = false;

    private void Awake()
    {
        // Safety: make sure DB is ready even if MasterSqliteSync wasn’t in this scene.
        LocalDb.Init();
    }

    /// <summary>
    /// Call this from the game mode button OnClick.
    /// - Loads gameInstruc from SQLite (LocalGamemode)
    /// - Shows ModeIntroSimple panel with TTS
    /// - On Proceed -> loads targetSceneName
    /// </summary>
    public void OnClickShowIntro()
    {
        Debug.Log($"[GameModeIntroLoader] OnClickShowIntro for id={gameModeId}");

        EnsureLoadedFromSqlite();

        string msg = string.IsNullOrWhiteSpace(_cachedInstruc)
            ? fallbackInstruc
            : _cachedInstruc;

        // Use your ModeIntroSimple singleton
        if (ModeIntroSimple.Instance != null)
        {
            ModeIntroSimple.Instance.Open(
                msg,
                () =>
                {
                    Debug.Log($"[GameModeIntroLoader] Proceed -> load scene '{targetSceneName}'");
                    if (!string.IsNullOrEmpty(targetSceneName))
                    {
                        SceneManager.LoadScene(targetSceneName);
                    }
                });
        }
        else
        {
            Debug.LogWarning("[GameModeIntroLoader] ModeIntroSimple.Instance is null → going straight to scene.");
            if (!string.IsNullOrEmpty(targetSceneName))
                SceneManager.LoadScene(targetSceneName);
        }
    }

    /// <summary>
    /// One-time load from SQLite so we don’t keep hitting DB every click.
    /// </summary>
    private void EnsureLoadedFromSqlite()
    {
        if (_loadedFromDb) return;

        try
        {
            var db = LocalDb.DB;

            // SQLite4Unity3d: Find by primary key
            var row = db.Find<LocalGamemode>(gameModeId);

            if (row != null)
            {
                Debug.Log($"[GameModeIntroLoader] SQLite hit id={gameModeId}, name='{row.gameModeName}', instrucLen={row.gameInstruc?.Length ?? 0}");
                _cachedInstruc = row.gameInstruc;
            }
            else
            {
                Debug.LogWarning($"[GameModeIntroLoader] No LocalGamemode row for id={gameModeId} → using fallback.");
                _cachedInstruc = null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[GameModeIntroLoader] SQLite load failed\n" + ex);
            _cachedInstruc = null;
        }

        _loadedFromDb = true;
    }
}
