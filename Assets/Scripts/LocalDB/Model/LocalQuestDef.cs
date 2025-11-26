using SQLite4Unity3d;

/// <summary>
/// Local SQLite mirror of tblQuests from Firebase.
/// </summary>
[Table("LocalQuests")]
public class LocalQuestDef
{
    // Same as questId from Firebase
    [PrimaryKey]
    public string id { get; set; }

    public string title { get; set; }
    public string description { get; set; }
    public string progressTag { get; set; }

    public int target { get; set; }
    public int coinReward { get; set; }

    // Optional metadata (can be empty in JSON, no problem)
    public string createdAt { get; set; }
    public string updatedAt { get; set; }
    public string createdBy { get; set; }
    public string updatedBy { get; set; }
}
