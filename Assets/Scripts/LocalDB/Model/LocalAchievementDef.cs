using SQLite4Unity3d; // same namespace you use for LocalQuestion, LocalQuestDef, etc.

public class LocalAchievementDef
{
    [PrimaryKey]
    public string id { get; set; }            // achievementId

    public string title { get; set; }
    public string description { get; set; }

    public string progressTag { get; set; }   // e.g. "answers_any"
    public int target { get; set; }
    public int coinReward { get; set; }

    public string iconId { get; set; }        // e.g. "ach_rising_star"

    public string createdAt { get; set; }
    public string updatedAt { get; set; }
    public string createdBy { get; set; }
    public string updatedBy { get; set; }
}
