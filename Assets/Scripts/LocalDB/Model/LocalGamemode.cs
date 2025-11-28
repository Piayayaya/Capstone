using SQLite4Unity3d;

public class LocalGamemode
{
    // PK – this matches the Firebase key 7001, 7002, etc.
    [PrimaryKey]
    public int id { get; set; }          // 7001

    public string gameModeName { get; set; }  // "Smart Ladder"
    public string gameInstruc { get; set; }  // long description text

    // Audit fields – match what MasterSqliteSync expects
    public string created_at { get; set; }   // e.g. "2025-11-25T00:00:00Z"
    public string updated_at { get; set; }
}
