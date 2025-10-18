using UnityEngine;

public class PerLevelSelector : MonoBehaviour
{
    [Header("Profiles per difficulty")]
    public PerLevelFocus easy;
    public PerLevelFocus normal;
    public PerLevelFocus hard;
    public PerLevelFocus advanced;
    public PerLevelFocus expert;

    // Use this from other scripts
    public PerLevelFocus CurrentProfile
    {
        get
        {
            switch (SmartLadderSession.SelectedDifficulty)
            {
                case LadderDifficulty.Easy: return easy;
                case LadderDifficulty.Normal: return normal;
                case LadderDifficulty.Hard: return hard;
                case LadderDifficulty.Advanced: return advanced;
                case LadderDifficulty.Expert: return expert;
                default: return easy;
            }
        }
    }
}
