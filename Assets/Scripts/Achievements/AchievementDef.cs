using UnityEngine;

[CreateAssetMenu(menuName = "BrainyMe/Achievements/AchievementDef")]
public class AchievementDef : ScriptableObject
{
    [Header("Identity")]
    public string id;                    // e.g., "answers_10"
    public string displayName;           // e.g., "Quick Thinker"
    [TextArea] public string description;

    [Header("Visuals")]
    public Sprite icon;                  // shown in list

    [Header("Progress")]
    public int target = 1;               // e.g., 10 correct answers
    public bool showAsCounter = true;    // progress label "3/10", or just a checkmark when done

    [Header("Optional")]
    public bool hiddenUntilStarted = false;  // keeps list tidy if you like

    [Header("Reward")]
    public int coinReward = 0;          // coins to give
    public bool autoGrantReward = false; // manual claim flow

}
