using UnityEngine;

public class ChestSwitcher : MonoBehaviour
{
    [Header("Chests (place them exactly where you want)")]
    public GameObject chestEasy;
    public GameObject chestNormal;
    public GameObject chestHard;
    public GameObject chestAdvanced;
    public GameObject chestExpert;

    void Awake()
    {
        // Pick the one chest for the selected difficulty and disable the rest.
        var target = GetFor(SmartLadderSession.SelectedDifficulty);
        SetOnlyActive(target);
    }

    GameObject GetFor(LadderDifficulty d)
    {
        switch (d)
        {
            case LadderDifficulty.Easy: return chestEasy;
            case LadderDifficulty.Normal: return chestNormal;
            case LadderDifficulty.Hard: return chestHard;
            case LadderDifficulty.Advanced: return chestAdvanced;
            case LadderDifficulty.Expert: return chestExpert;
            default: return chestEasy;
        }
    }

    void SetOnlyActive(GameObject target)
    {
        if (chestEasy) chestEasy.SetActive(chestEasy == target);
        if (chestNormal) chestNormal.SetActive(chestNormal == target);
        if (chestHard) chestHard.SetActive(chestHard == target);
        if (chestAdvanced) chestAdvanced.SetActive(chestAdvanced == target);
        if (chestExpert) chestExpert.SetActive(chestExpert == target);
    }
}
