using UnityEngine;

/// <summary>
/// Global SFX manager.
/// - Lives on SfxPlayer in the first scene.
/// - Plays button click sounds on demand.
/// - Respects GlobalAudioSettings (SfxOn).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    [Header("SFX Clips")]
    public AudioClip buttonClickClip;

    private AudioSource src;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);  // keep this across scenes

        src = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Play the default button click sound.
    /// </summary>
    public void PlayButtonClick()
    {
        // Respect the global SFX toggle
        if (GlobalAudioSettings.Instance != null && !GlobalAudioSettings.Instance.SfxOn)
            return;

        if (src != null && buttonClickClip != null)
        {
            src.PlayOneShot(buttonClickClip);
        }
    }
}
