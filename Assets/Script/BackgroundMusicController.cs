using UnityEngine;

/// <summary>
/// Controls background music:
/// - Keeps the object alive across scenes.
/// - Ensures only ONE instance exists (prevents double music).
/// - Mutes/unmutes when Music setting changes.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicController : MonoBehaviour
{
    public static BackgroundMusicController Instance;

    private AudioSource source;

    private void Awake()
    {
        // ----- SINGLETON GUARD: avoid duplicate music when reloading scenes -----
        if (Instance != null && Instance != this)
        {
            // Another BackgroundMusicController already exists → kill this one
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);   // keep BGM when changing scenes
        // ------------------------------------------------------------------------

        source = GetComponent<AudioSource>();

        // optional: make sure it's playing & looping
        if (source != null)
        {
            source.loop = true;
            if (!source.isPlaying)
                source.Play();
        }
    }

    private void OnEnable()
    {
        if (GlobalAudioSettings.Instance != null)
        {
            // apply current setting immediately
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
