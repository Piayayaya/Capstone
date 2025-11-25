using System;
using UnityEngine;

/// <summary>
/// Global audio settings singleton.
/// - Lives in the first scene (CreateAccount / Home).
/// - Persists across scenes.
/// - Stores Music / SFX on/off in PlayerPrefs.
/// - Other scripts subscribe to OnMusicChanged / OnSfxChanged.
/// </summary>
public class GlobalAudioSettings : MonoBehaviour
{
    public static GlobalAudioSettings Instance { get; private set; }

    private const string KEY_MUSIC = "SETTING_MUSIC_ON";
    private const string KEY_SFX = "SETTING_SFX_ON";

    public bool MusicOn { get; private set; } = true;
    public bool SfxOn { get; private set; } = true;

    public event Action<bool> OnMusicChanged;
    public event Action<bool> OnSfxChanged;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load saved values (default = ON)
        MusicOn = PlayerPrefs.GetInt(KEY_MUSIC, 1) == 1;
        SfxOn = PlayerPrefs.GetInt(KEY_SFX, 1) == 1;
    }

    public void SetMusic(bool on)
    {
        if (MusicOn == on) return;

        MusicOn = on;
        PlayerPrefs.SetInt(KEY_MUSIC, on ? 1 : 0);
        PlayerPrefs.Save();

        OnMusicChanged?.Invoke(on);
    }

    public void SetSfx(bool on)
    {
        if (SfxOn == on) return;

        SfxOn = on;
        PlayerPrefs.SetInt(KEY_SFX, on ? 1 : 0);
        PlayerPrefs.Save();

        OnSfxChanged?.Invoke(on);
    }
}
