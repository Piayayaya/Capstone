using UnityEngine;

public enum QuestGoalType { Counter /* generic progress: reach Target */ }

[CreateAssetMenu(fileName = "QuestDefinition", menuName = "BrainyMe/Quests/Definition")]
public class QuestDefinition : ScriptableObject
{
    [Header("Identity")]
    public string questId;                 // unique, stable (e.g., "answer_10_easy")
    public string title;                   // "Answer 10 questions"
    [TextArea] public string description;  // "Answer 10 questions in any mode."

    [Header("Logic")]
    public QuestGoalType goalType = QuestGoalType.Counter;
    public string progressTag;             // e.g., "answers_any", "ladder_wins", "login"
    public int target = 10;                // required amount

    [Header("Rewards")]
    public int coinReward = 100;

    [Header("Availability")]
    public bool eligibleForDaily = true;   // toggle if you want to exclude from daily rotation
}
    