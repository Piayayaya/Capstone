// SmartLadderSceneSetup.cs
using TMPro;
using UnityEngine;

public class SmartLadderSceneSetup : MonoBehaviour
{
    [Header("Scene Refs")]
    [Tooltip("Parent containing Level 1 .. Level 50 and the chests")]
    public Transform levelsGroup;            // e.g., Content/LevelsGroup

    [Header("Header (optional)")]
    [Tooltip("Drag the TMP text that shows 'EASY MODE' / 'NORMAL MODE' etc.")]
    public TMP_Text headerLabel;

    [Header("Level Names")]
    [Tooltip("Prefix used by level parents, e.g. 'Level ' so it finds 'Level 1', 'Level 2', ...")]
    public string levelPrefix = "Level ";
    [Tooltip("How many total levels exist in this scene (max)")]
    public int maxLevelsInScene = 50;

    [Header("Levels per difficulty")]
    public int easyCount = 10;
    public int normalCount = 15;
    public int hardCount = 25;
    public int advancedCount = 35;
    public int expertCount = 50;

    [Header("Manual chests (toggle only)")]
    public GameObject chestEasy;
    public GameObject chestNormal;
    public GameObject chestHard;
    public GameObject chestAdvanced;
    public GameObject chestExpert;

    void Start()
    {
        if (levelsGroup == null)
        {
            Debug.LogError("SmartLadderSceneSetup: Assign 'levelsGroup'.");
            return;
        }

        // 1) Update header text safely
        if (headerLabel != null)
        {
            headerLabel.text = DifficultyTitle(SmartLadderSession.SelectedDifficulty);
            headerLabel.gameObject.SetActive(true);   // ensure it’s visible
        }

        // 2) Trim levels for the selected difficulty
        int keep = CountFor(SmartLadderSession.SelectedDifficulty);

        for (int i = 1; i <= maxLevelsInScene; i++)
        {
            var level = levelsGroup.Find(levelPrefix + i);
            if (level == null) continue;

            bool enable = i <= keep;
            level.gameObject.SetActive(enable);
        }

        // 3) Toggle the correct chest (you placed them manually)
        ToggleChestsFor(SmartLadderSession.SelectedDifficulty);
    }

    string DifficultyTitle(LadderDifficulty d)
    {
        switch (d)
        {
            case LadderDifficulty.Easy: return "EASY MODE";
            case LadderDifficulty.Normal: return "NORMAL MODE";
            case LadderDifficulty.Hard: return "HARD MODE";
            case LadderDifficulty.Advanced: return "ADVANCED MODE";
            case LadderDifficulty.Expert: return "EXPERT MODE";
            default: return "SMART LADDER";
        }
    }

    int CountFor(LadderDifficulty d)
    {
        switch (d)
        {
            case LadderDifficulty.Easy: return Mathf.Min(easyCount, maxLevelsInScene);
            case LadderDifficulty.Normal: return Mathf.Min(normalCount, maxLevelsInScene);
            case LadderDifficulty.Hard: return Mathf.Min(hardCount, maxLevelsInScene);
            case LadderDifficulty.Advanced: return Mathf.Min(advancedCount, maxLevelsInScene);
            case LadderDifficulty.Expert: return Mathf.Min(expertCount, maxLevelsInScene);
            default: return Mathf.Min(easyCount, maxLevelsInScene);
        }
    }

    void ToggleChestsFor(LadderDifficulty d)
    {
        if (chestEasy) chestEasy.SetActive(d == LadderDifficulty.Easy);
        if (chestNormal) chestNormal.SetActive(d == LadderDifficulty.Normal);
        if (chestHard) chestHard.SetActive(d == LadderDifficulty.Hard);
        if (chestAdvanced) chestAdvanced.SetActive(d == LadderDifficulty.Advanced);
        if (chestExpert) chestExpert.SetActive(d == LadderDifficulty.Expert);
    }
}
