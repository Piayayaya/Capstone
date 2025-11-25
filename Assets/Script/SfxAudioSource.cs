using UnityEngine;

/// <summary>
/// Makes this AudioSource follow the global SFX setting.
/// - If Sounds is OFF, source.mute = true.
/// - If Sounds is ON, source.mute = false.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SfxAudioSource : MonoBehaviour
{
    private AudioSource src;

    private void Awake()
    {
        src = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if (GlobalAudioSettings.Instance != null)
        {
            Apply(GlobalAudioSettings.Instance.SfxOn);
            GlobalAudioSettings.Instance.OnSfxChanged += Apply;
        }
    }

    private void OnDisable()
    {
        if (GlobalAudioSettings.Instance != null)
        {
            GlobalAudioSettings.Instance.OnSfxChanged -= Apply;
        }
    }

    private void Apply(bool on)
    {
        if (src)
            src.mute = !on;
    }
}
