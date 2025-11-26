using UnityEngine;

/// <summary>
/// Controls background music:
/// - Keeps the object alive across scenes.
/// - Mutes/unmutes when Music setting changes.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicController : MonoBehaviour
{
    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);   // keep BGM when changing scenes
    }

    private void OnEnable()
    {
        if (GlobalAudioSettings.Instance != null)
        {
            Apply(GlobalAudioSettings.Instance.MusicOn);
            GlobalAudioSettings.Instance.OnMusicChanged += Apply;
        }
    }

    private void OnDisable()
    {
        if (GlobalAudioSettings.Instance != null)
        {
            GlobalAudioSettings.Instance.OnMusicChanged -= Apply;
        }
    }

    private void Apply(bool on)
    {
        if (source)
            source.mute = !on;
    }
}
