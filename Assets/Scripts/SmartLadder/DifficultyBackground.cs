using UnityEngine;
using UnityEngine.UI;

public class DifficultyBackground : MonoBehaviour
{
    [Header("Target (assign ONE of these)")]
    public Image uiImage;                 // e.g., Canvas -> Fullscreen Image
    public SpriteRenderer spriteRenderer; // or a world-space background sprite

    [Header("Sprites per Difficulty (optional)")]
    public Sprite bgEasy;
    public Sprite bgNormal;
    public Sprite bgHard;
    public Sprite bgAdvanced;
    public Sprite bgExpert;

    [Header("Prebuilt Background Objects (optional)")]
    [Tooltip("If you prefer to have 5 different background objects in the scene and only enable one, assign them here.")]
    public GameObject goEasy;
    public GameObject goNormal;
    public GameObject goHard;
    public GameObject goAdvanced;
    public GameObject goExpert;

    [Header("Options")]
    public bool preserveAspectForUIImage = true;

    void OnEnable()
    {
        Apply();
    }

    // Call this if the difficulty changes while the scene is active
    public void Apply()
    {
        var d = SmartLadderSession.SelectedDifficulty; // comes from your selection scene

        // 1) Set sprite on Image/SpriteRenderer (if assigned)
        Sprite s = GetSpriteFor(d);
        if (uiImage != null)
        {
            uiImage.sprite = s;
            uiImage.enabled = (s != null);
            if (preserveAspectForUIImage) uiImage.preserveAspect = true;
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.sprite = s;
            spriteRenderer.enabled = (s != null);
        }

        // 2) Toggle prebuilt background GameObjects (if you use this route)
        if (AnyGOAssigned())
        {
            if (goEasy) goEasy.SetActive(d == LadderDifficulty.Easy);
            if (goNormal) goNormal.SetActive(d == LadderDifficulty.Normal);
            if (goHard) goHard.SetActive(d == LadderDifficulty.Hard);
            if (goAdvanced) goAdvanced.SetActive(d == LadderDifficulty.Advanced);
            if (goExpert) goExpert.SetActive(d == LadderDifficulty.Expert);
        }
    }

    Sprite GetSpriteFor(LadderDifficulty d)
    {
        switch (d)
        {
            case LadderDifficulty.Easy: return bgEasy;
            case LadderDifficulty.Normal: return bgNormal;
            case LadderDifficulty.Hard: return bgHard;
            case LadderDifficulty.Advanced: return bgAdvanced;
            case LadderDifficulty.Expert: return bgExpert;
            default: return bgEasy;
        }
    }

    bool AnyGOAssigned()
    {
        return goEasy || goNormal || goHard || goAdvanced || goExpert;
    }
}
