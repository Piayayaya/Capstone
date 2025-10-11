using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmartLadderSceneSetup : MonoBehaviour
{
    [Header("Scene Refs")]
    [Tooltip("Parent containing Level 1 .. Level 50 and the chests")]
    public RectTransform levelsGroup;        // e.g., Content/LevelsGroup  (MUST be RectTransform)
    [Tooltip("ScrollRect of the ladder view (for snapping to bottom)")]
    public ScrollRect scroll;
    [Tooltip("Content RectTransform (Scroll View -> Viewport -> Content)")]
    public RectTransform content;

    [Header("Header (optional)")]
    public TMP_Text headerLabel;

    [Header("Level Names")]
    public string levelPrefix = "Level ";
    public int maxLevelsInScene = 50;

    [Header("Levels per difficulty")]
    public int easyCount = 10;
    public int normalCount = 15;
    public int hardCount = 25;
    public int advancedCount = 35;
    public int expertCount = 50;

    [Header("Content height per difficulty (pixels)")]
    public float easyHeight = 6000f;
    public float normalHeight = 8000f;
    public float hardHeight = 11000f;
    public float advancedHeight = 14000f;
    public float expertHeight = 17000f;

    [Header("Bottom alignment")]
    [Tooltip("How far above the very bottom Level 1 should sit (pixels in Content space).")]
    public float bottomMargin = 180f;

    [Header("Manual chests (toggle only)")]
    public GameObject chestEasy;
    public GameObject chestNormal;
    public GameObject chestHard;
    public GameObject chestAdvanced;
    public GameObject chestExpert;

    void Start()
    {
        if (!levelsGroup || !content)
        {
            Debug.LogError("SmartLadderSceneSetup: Assign levelsGroup and content.");
            return;
        }

        // 1) Update header text
        if (headerLabel) headerLabel.text = DifficultyTitle(SmartLadderSession.SelectedDifficulty);

        // 2) How many levels to keep
        int keep = CountFor(SmartLadderSession.SelectedDifficulty);

        // 3) Enable Level 1..keep, disable the rest
        for (int i = 1; i <= maxLevelsInScene; i++)
        {
            var level = levelsGroup.Find(levelPrefix + i);
            if (!level) continue;
            level.gameObject.SetActive(i <= keep);
        }

        // 4) Set content height for this difficulty
        content.sizeDelta = new Vector2(content.sizeDelta.x, HeightFor(SmartLadderSession.SelectedDifficulty));

        // 5) Align Level 1 to the bottom margin
        AlignLevel1ToBottom();

        // 6) Toggle the correct chest
        ToggleChestsFor(SmartLadderSession.SelectedDifficulty);

        // 7) Snap scroll to bottom (so player starts in view)
        if (scroll) scroll.verticalNormalizedPosition = 0f;
    }

    void AlignLevel1ToBottom()
    {
        // Find Level 1
        var level1 = levelsGroup.Find(levelPrefix + 1) as RectTransform;
        if (!level1)
        {
            Debug.LogWarning("SmartLadderSceneSetup: Could not find 'Level 1'.");
            return;
        }

        // Take the bottom of the Level 1 circle in WORLD space, then convert to CONTENT space.
        // This makes it robust no matter what anchors/pivots you used.
        Vector3 level1BottomWorld = level1.TransformPoint(new Vector3(0f, level1.rect.yMin, 0f));
        float level1BottomY_InContent = content.InverseTransformPoint(level1BottomWorld).y;

        // We want that bottom to sit at `bottomMargin` (in Content space).
        float deltaY = bottomMargin - level1BottomY_InContent;

        // Move the whole levelsGroup by that delta (Content space).
        var pos = levelsGroup.anchoredPosition;
        pos.y += deltaY;
        levelsGroup.anchoredPosition = pos;
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

    float HeightFor(LadderDifficulty d)
    {
        switch (d)
        {
            case LadderDifficulty.Easy: return easyHeight;
            case LadderDifficulty.Normal: return normalHeight;
            case LadderDifficulty.Hard: return hardHeight;
            case LadderDifficulty.Advanced: return advancedHeight;
            case LadderDifficulty.Expert: return expertHeight;
            default: return easyHeight;
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
