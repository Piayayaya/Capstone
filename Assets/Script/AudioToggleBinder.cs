using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Binds a UI Toggle to GlobalAudioSettings.
/// Use Type = Music for music toggle, Type = Sfx for sounds toggle.
/// Works together with your AnchorHandleToggle that moves the handle.
/// </summary>
public class AudioToggleBinder : MonoBehaviour
{
    public enum AudioType
    {
        Music,
        Sfx
    }

    [Header("What does this toggle control?")]
    public AudioType type = AudioType.Music;

    [Header("Refs")]
    public Toggle toggle;

    private void Awake()
    {
        if (!toggle)
            toggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        if (!toggle) return;

        if (GlobalAudioSettings.Instance != null)
        {
            // Set initial state from saved settings
            switch (type)
            {
                case AudioType.Music:
                    toggle.isOn = GlobalAudioSettings.Instance.MusicOn;
                    GlobalAudioSettings.Instance.OnMusicChanged += HandleMusicChanged;
                    break;

                case AudioType.Sfx:
                    toggle.isOn = GlobalAudioSettings.Instance.SfxOn;
                    GlobalAudioSettings.Instance.OnSfxChanged += HandleSfxChanged;
                    break;
            }
        }

        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnDisable()
    {
        if (!toggle) return;

        toggle.onValueChanged.RemoveListener(OnToggleChanged);

        if (GlobalAudioSettings.Instance != null)
        {
            GlobalAudioSettings.Instance.OnMusicChanged -= HandleMusicChanged;
            GlobalAudioSettings.Instance.OnSfxChanged -= HandleSfxChanged;
        }
    }

    private void OnToggleChanged(bool value)
    {
        if (GlobalAudioSettings.Instance == null) return;

        if (type == AudioType.Music)
            GlobalAudioSettings.Instance.SetMusic(value);
        else
            GlobalAudioSettings.Instance.SetSfx(value);
    }

    private void HandleMusicChanged(bool on)
    {
        if (type != AudioType.Music || !toggle) return;

        if (toggle.isOn != on)
            toggle.isOn = on;
    }

    private void HandleSfxChanged(bool on)
    {
        if (type != AudioType.Sfx || !toggle) return;

        if (toggle.isOn != on)
            toggle.isOn = on;
    }
}
