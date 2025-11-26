using SQLite4Unity3d;

[Table("GameModes")]
public class LocalGamemode
{
    [PrimaryKey] public string id { get; set; }   // Firebase key of the gamemode
    public string gameModeName { get; set; }
    public string gameInstruc { get; set; }
    public string updated_at { get; set; }        // optional
}
